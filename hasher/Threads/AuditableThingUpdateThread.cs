using hasher.Messages;
using hasher.Models;
using hasher.Workers;
using HasherDataObjects.Models;
using HasherDbLogging.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace hasher.Threads
{
    public class AuditableThingUpdateThread:
                EventHandlingThread
    {
        public const int MSG_HASHABLE_FILE_UPDATE = 1;
        public const int MSG_NESTABLE_FOLDER_UPDATE = 2;
        public const int MSG_RUN_RESULT_UPDATE = 3;
        public const int MSG_JOB_INFO_UPDATE = 4;
        public const int MSG_HANDLE_UNTOUCHED_FILES = 5;
        private readonly IServiceProvider services;
        private readonly ILogger<AuditableThingUpdateThread> logger;

        public Guid? RunId { get; set; }

        public AuditableThingUpdateThread(ILogger<AuditableThingUpdateThread> logger, IServiceProvider services) :
                base(logger)
        {
            this.services = services ?? throw new ArgumentNullException(nameof(services), "Services cannot be null");
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger), "Logger cannot be null");
            SetThreadName("AuditableThingUpdateThread");
        }

        protected override async Task<IThreadMessage?> OnMessageRecieved(IThreadMessage? data)
        {
            if (data == null)
            {
                return null;
            }
            logger.LogDebug($"[{Thread.CurrentThread.ManagedThreadId} - {_thread.Name}] Getting Semaphore");
            await DatabaseSemaphore.WaitAsync();
            logger.LogDebug($"[{Thread.CurrentThread.ManagedThreadId} - {_thread.Name}] Got Semaphore");
            HasherContext hasherContext = services.GetRequiredService<HasherContext>();
            RunResults? runResults = hasherContext.RunResults.FirstOrDefault(r => r.Id == RunId && !r.IsDeleted && r.IsActive);
            if (runResults == null)
            {
                logger.LogWarning($"[{Thread.CurrentThread.ManagedThreadId} - {_thread.Name}] RunResults with ID {RunId} not found or is inactive.");
                return null;
            }

            switch (data.MessageId)
            {
                case MSG_HASHABLE_FILE_UPDATE:
                    if (data.Data.GetType() == typeof(HashableFile))
                    {
                        HashableFile? hashableFile = data.Data as HashableFile;
                        if (hashableFile != null)
                        {
                            logger.LogInformation($"[{Thread.CurrentThread.ManagedThreadId} - {_thread.Name}] Processing Hashable File Update: {hashableFile}");
                            hashableFile.LastRun = runResults;
                            hasherContext.UpdateThing(hashableFile);
                            hasherContext.Files.Update(hashableFile);
                            hasherContext.SaveChanges();
                        }
                    }
                    break;
                case MSG_NESTABLE_FOLDER_UPDATE:
                    if (data.Data.GetType() == typeof(NestableFolder))
                    {
                        NestableFolder? folder = data.Data as NestableFolder;
                        if (folder != null)
                        {
                            logger.LogInformation($"[{Thread.CurrentThread.ManagedThreadId} - {_thread.Name}] Processing Folder Update: {folder}");
                            hasherContext.UpdateThing(folder);
                            hasherContext.Folders.Update(folder);
                            hasherContext.SaveChanges();
                        }
                    }
                    break;
                case MSG_RUN_RESULT_UPDATE:
                    if (data.Data.GetType() == typeof(RunResults))
                    {
                        RunResults? runResultsUpdate = data.Data as RunResults;
                        if (runResultsUpdate != null && runResults.Id == runResultsUpdate.Id)
                        {
                            logger.LogInformation($"[{Thread.CurrentThread.ManagedThreadId} - {_thread.Name}] Processing Run Results Update: {runResultsUpdate}");
                            runResults += runResultsUpdate;
                            hasherContext.UpdateThing(runResults);
                            logger.LogInformation($"[{Thread.CurrentThread.ManagedThreadId} - {_thread.Name}] Updated Run Results: {runResults}");
                            hasherContext.RunResults.Update(runResults);
                            hasherContext.SaveChanges();
                        }
                    }
                    break;
                case MSG_JOB_INFO_UPDATE:
                    if (data.Data.GetType() == typeof(JobInfo))
                    {
                        JobInfo? jobInfo = data.Data as JobInfo;
                        if (jobInfo != null)
                        {
                            logger.LogInformation($"[{Thread.CurrentThread.ManagedThreadId} - {_thread.Name}] Processing Job Info Update: {jobInfo}");
                            hasherContext.UpdateThing(jobInfo);
                            hasherContext.Jobs.Update(jobInfo);
                            hasherContext.SaveChanges();
                        }
                    }
                    break;
                case MSG_HANDLE_UNTOUCHED_FILES:
                    if (data.Data != null)
                    {
                        ConcurrentDictionary<string, Tuple<string, string>> changedHashes = (ConcurrentDictionary<string, Tuple<string, string>>)( data.Data);
                        List<HashableFile> untouchedFiles = services.GetRequiredService<HasherContext>().Files
                            .Where(f => f.LastRun != null && f.LastRun.Id != runResults.Id)
                            .Where(f => f.DeletedAt == null && f.IsDeleted == false)
                            .ToList();
                        if (untouchedFiles.Any())
                        {

                            logger.LogInformation($"[{Thread.CurrentThread.ManagedThreadId}] Found {untouchedFiles.Count} files that were not touched in this run.");
                            //Soft delete these files
                            foreach (HashableFile file in untouchedFiles)
                            {
                                services.GetRequiredService<HasherContext>().SoftDeleteThing(file);
                                logger.LogInformation($"[{Thread.CurrentThread.ManagedThreadId}] Soft deleted file: {file.Name} with ID: {file.Id}");
                                runResults.DeletedFiles++;
                            }
                            services.GetRequiredService<HasherContext>().SaveChanges();
                            hasherContext.UpdateThing(runResults);
                            logger.LogInformation($"[{Thread.CurrentThread.ManagedThreadId} - {_thread.Name}] Updated Run Results: {runResults}");
                            hasherContext.RunResults.Update(runResults);
                            hasherContext.SaveChanges();

                            
                        }
                        WorkerEmailerData emailData = new()
                        {
                            ChangedHashes = changedHashes,
                            MissingFileList = untouchedFiles.Select(f => $"{GeneratePath(f.Folder)}\\{f.Name}").ToList(),
                            RunResults = runResults
                        };
                        WorkerEmailer emailer = services.GetRequiredService<WorkerEmailer>();
                        bool emailSent = await emailer.DoWork(emailData);
                        logger.LogDebug($"[{Thread.CurrentThread.ManagedThreadId}] Email sent: {emailSent}");
                    }
                    CancelAsync();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(data.MessageId), $"Unknown message ID: {data.MessageId}");
            }
            logger.LogDebug($"[{Thread.CurrentThread.ManagedThreadId} - {_thread.Name}] Releasing Semaphore");  
            DatabaseSemaphore.Release();
            return null;
        }

        private string GeneratePath(NestableFolder? folder)
        {
            if (folder == null)
            {
                return string.Empty;
            }
            else
            {
                return Path.Combine(folder.Parent != null ? GeneratePath(folder.Parent) : (folder?.Drive?.Letter ?? string.Empty), folder!.Name);
            }
        }

    }
}
