using hasher.Common;
using hasher.Messages;
using hasher.Models;
using HasherDataObjects.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace hasher.Threads
{
    public class AuditableThingRequestThread:
            EventHandlingThread
    {
        public const int MSG_REQUEST_THING_FOR_FILE = 1;
        private readonly IServiceProvider services;
        private readonly ILogger<AuditableThingUpdateThread> logger;

        public AuditableThingRequestThread(ILogger<AuditableThingUpdateThread> logger, IServiceProvider services) : 
                base(logger)
        {
            this.services = services ?? throw new ArgumentNullException(nameof(services), "Services cannot be null");
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger), "Logger cannot be null");
            SetThreadName("AuditableThingRequestThread");
        }

        protected override async Task<IThreadMessage?> OnMessageRecieved(IThreadMessage? data)
        {
            if (data == null)
            {
                return null;
            }
            HasherContext hasherContext = services.GetRequiredService<HasherContext>();
            if (data.MessageId == MSG_REQUEST_THING_FOR_FILE && data.Data != null && data.Data.GetType() == typeof(RequestThingForFileData ))
            {
                logger.LogDebug($"[{Thread.CurrentThread.ManagedThreadId} - {_thread.Name}] Getting Semaphore");
                await DatabaseSemaphore.WaitAsync();
                logger.LogDebug($"[{Thread.CurrentThread.ManagedThreadId} - {_thread.Name}] Got Semaphore");    
                RequestThingForFileData myData = (RequestThingForFileData)data.Data;
                RequestThingForFileResultData ? result = ConvertPathToHashableFile(myData.FolderPath, myData.CurrentHash);
                logger.LogDebug($"[{Thread.CurrentThread.ManagedThreadId} - {_thread.Name}] Done processing Request Thing for File Message");
                logger.LogDebug($"[{Thread.CurrentThread.ManagedThreadId} - {_thread.Name}] Releasing semaphore");  
                DatabaseSemaphore.Release();
                return new RequestThingForFileResultMessage(result, null);
            }
            return null;
        }

        private RequestThingForFileResultData? ConvertPathToHashableFile(string file, string currentHash)
        {
            logger.LogInformation($"[{Thread.CurrentThread.ManagedThreadId} - {_thread.Name}] Converting path to HashableFile: {file}, Current Hash: {currentHash}");
            if (!string.IsNullOrEmpty(file))
            {

                HasherContext hasherContext = services.GetRequiredService<HasherContext>();

                FileInfo fileInfo = new(file);
                HashableFile hashableFile = new()
                {
                    Name = fileInfo.Name,
                    Size = fileInfo.Length,
                    LastModified = fileInfo.LastWriteTimeUtc,
                    Hash = currentHash,
                    Extension = fileInfo.Extension.ToLowerInvariant(),
                };


                string[] pathChunks = file.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);
                bool isLocalDrive = pathChunks[0].Contains(':');

                NestableFolder? current = null;
                Drive? drive = null;
                for (int i = 0; i < pathChunks.Length - 1; i++)
                {
                    if (i == 0 && isLocalDrive)
                    {
                        //Create a drive entry if it doesn't exist
                        drive = hasherContext.Drives.Where(d => !d.IsDeleted).FirstOrDefault(d => d.Letter == pathChunks[i].Replace(":", string.Empty));
                        if (drive == null)
                        {
                            drive = new Drive
                            {
                                Letter = pathChunks[i].Replace(":", string.Empty)
                            };
                            hasherContext.Drives.Add(drive);
                            hasherContext.SaveChanges();
                        }
                        continue;
                    }
                    NestableFolder? folder = hasherContext.Folders
                                                            .Where(f => !f.IsDeleted)
                                                            .Where(f => f.Name == pathChunks[i])
                                                            .Where(f => f.Parent == current)
                                                            .FirstOrDefault();
                    if (folder == null)
                    {
                        folder = new NestableFolder
                        {
                            Name = pathChunks[i],
                            Parent = current,
                            Drive = null
                        };
                        if (current != null)
                        {
                            current.Children.Add(folder);
                            current.UpdatedAt = DateTime.UtcNow;
                        }
                        else
                        {
                            // If this is the first folder, it has no parent
                            folder.Parent = null;
                            folder.Drive = drive;
                        }
                        hasherContext.Folders.Add(folder);
                        hasherContext.SaveChanges();
                    }
                    current = folder;
                    hashableFile.Folder = folder;
                }
                HashableFile? existing = hasherContext.Files
                                                             .Where(f => !f.IsDeleted)
                                                             .Where(f => f.Name == hashableFile.Name)
                                                             .Where(f => f.Folder == current)
                                                             .FirstOrDefault();
                EnumResult enumResult = EnumResult.Created;
                if (existing == null)
                {
                    hasherContext.Files.Add(hashableFile);
                    hasherContext.SaveChanges();
                }
                else if (hashableFile.Hash != existing.Hash)
                {
                    hashableFile = existing;
                    enumResult = EnumResult.Different;
                }
                else
                {
                    hashableFile = existing;
                    enumResult = EnumResult.Same;
                }
                return new(hashableFile, enumResult);
            }
            return new(null, EnumResult.Missing);
        }

    }
}
