using hasher.Common;
using hasher.Messages;
using hasher.Models;
using hasher.Workers;
using HasherDataObjects.Configuration;
using HasherDataObjects.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace hasher.Threads
{
    internal class FileInfoWithHash(string filePath, string hash)
    {
        public string FilePath => filePath;
        public string Hash => hash;
    }

    public class MainWorkerThread(ILogger<MainWorkerThread> logger, IServiceProvider serviceProvider, Settings settings) :
        EventHandlingThread(logger)
    {

        public const int MSG_START = 1;
        public const int MSG_RESPONSE_THING_FOR_FILE = 2;

        private JobInfo? _jobInfo;

        /// <summary>
        /// Function to report updates to changed hashs.
        /// T1 is the FQN of the file
        /// T2 is the new hash
        /// T3 is the old hash
        /// t$ is the return is bool, true if the update was successful, false otherwise.
        /// /// </summary>
        private Func<string, string, string, bool>? _OnChangedHashesUpdate;

        private Func<IThreadMessage, bool>? _sendRequestThingForFileMessage { get; set; }

        private Func<IThreadMessage, bool>? _sendThingForUpdateMessage { get; set; }

        private Guid? _runId;
        private List<FileInfoWithHash> _fileListWithHashes = [];

        protected override Task<IThreadMessage?> OnMessageRecieved(IThreadMessage? data)
        {
            switch (data?.MessageId)
            {
                case MSG_START:
                    if (data.Data is StartupData startupData)
                    {
                        return HandleStartupData(startupData);
                    }
                    else
                    {
                        logger.LogWarning($"[{Thread.CurrentThread.ManagedThreadId} - {_thread.Name}] Received MSG_START with invalid data type: {data.Data.GetType()}");
                        return Task.FromResult<IThreadMessage?>(null);
                    }
                case MSG_RESPONSE_THING_FOR_FILE:
                    if (data.Data is RequestThingForFileResultData resultData)
                    {
                        return ProcessThingResultAsync(resultData);
                    }
                    else
                    {
                        logger.LogWarning($"[{Thread.CurrentThread.ManagedThreadId} - {_thread.Name}] Received MSG_RESPONSE_THING_FOR_FILE with invalid data type: {data.Data.GetType()}");
                        return Task.FromResult<IThreadMessage?>(null);
                    }
                default:
                    logger.LogWarning($"[{Thread.CurrentThread.ManagedThreadId} - {_thread.Name}] Received unknown message ID: {data?.MessageId}");
                    return Task.FromResult<IThreadMessage?>(null);
            }
        }

        private async Task<IThreadMessage?> ProcessThingResultAsync(RequestThingForFileResultData resultData)
        {
            if (resultData.Thing != null && resultData.Thing.GetType() == typeof(HashableFile))
            {
                _ = _runId ?? throw new InvalidOperationException("RunId is not set. Cannot process file result without a valid RunId.");
                _ = _jobInfo ?? throw new InvalidOperationException("JobInfo is not set. Cannot process file result without a valid JobInfo.");

                HashableFile hashableFile = (HashableFile)resultData.Thing;
                FileInfoWithHash fileInfoWithHash = _fileListWithHashes.First();
                string hashResults = fileInfoWithHash.Hash;
                RunResults runResults = new RunResults() { Id = _runId.Value };
                _fileListWithHashes.RemoveAt(0);
                runResults.TotalFiles++;
                switch (resultData.Result)
                {
                    case EnumResult.Created:
                        logger.LogInformation($"[{Thread.CurrentThread.ManagedThreadId}] Hashable file created: {hashableFile.Name}, Hash: {hashableFile.Hash}");
                        runResults.AddedFiles++;
                        break;
                    case EnumResult.Different:
                        logger.LogInformation($"[{Thread.CurrentThread.ManagedThreadId}] Hashable file updated: {hashableFile.Name}");
                        //Capture the hash trivia for later complaint e-mail
                        _OnChangedHashesUpdate?.Invoke(fileInfoWithHash.FilePath, hashableFile.Hash, hashResults);
                        // Update the hash if it has changed
                        hashableFile.Hash = hashResults;
                        runResults.UpdatedFiles++;
                        // Use the E-mail Worker to complain about the hash change
                        logger.LogInformation($"[{Thread.CurrentThread.ManagedThreadId}] Updated hash for file: {hashableFile.Name} to {hashableFile.Hash}");
                        break;
                    case EnumResult.Same:
                        logger.LogInformation($"[{Thread.CurrentThread.ManagedThreadId}] Hash for file: {hashableFile.Name} is unchanged.");
                        runResults.UnchangedFiles++;
                        break;
                    case EnumResult.Missing:
                        logger.LogInformation($"[{Thread.CurrentThread.ManagedThreadId}] Hashable file not found, creating new one: {hashableFile.Name}");
                        break;
                }
                logger.LogDebug($"[{Thread.CurrentThread.ManagedThreadId} - {_thread.Name}] Getting Semaphore");

                // Ensure that the database semaphore is acquired before modifying shared resources
                await DatabaseSemaphore.WaitAsync();
                logger.LogDebug($"[{Thread.CurrentThread.ManagedThreadId} - {_thread.Name}] Got Semaphore");
                _jobInfo.Files.Add( hashableFile);
                hashableFile.LastJob = _jobInfo;
                _jobInfo.ProcessedFilesCount++;
                logger.LogDebug($"[{Thread.CurrentThread.ManagedThreadId} - {_thread.Name}] Releasing Semaphore");  
                DatabaseSemaphore.Release();


                logger.LogInformation($"[{Thread.CurrentThread.ManagedThreadId}] Updating job info: {_jobInfo}");
                _sendThingForUpdateMessage?.Invoke(new AuditableThingUpdateMessage(hashableFile, AuditableThingUpdateThread.MSG_HASHABLE_FILE_UPDATE));
                _sendThingForUpdateMessage?.Invoke(new AuditableThingUpdateMessage(_jobInfo, AuditableThingUpdateThread.MSG_JOB_INFO_UPDATE));
                _sendThingForUpdateMessage?.Invoke(new AuditableThingUpdateMessage(runResults, AuditableThingUpdateThread.MSG_RUN_RESULT_UPDATE));
            }
            if (_fileListWithHashes.Any())
            {
                // If there are more files to process, send the next request
                SendRequestForFileThing(_fileListWithHashes.First());
            }
            else
            {
                if (_jobInfo != null)
                {
                    await DatabaseSemaphore.WaitAsync();
                    logger.LogDebug($"[{Thread.CurrentThread.ManagedThreadId} - {_thread.Name}] Got Semaphore");
                    _jobInfo.isActive = false;
                    _sendThingForUpdateMessage?.Invoke(new AuditableThingUpdateMessage(_jobInfo, AuditableThingUpdateThread.MSG_JOB_INFO_UPDATE));
                    logger.LogDebug($"[{Thread.CurrentThread.ManagedThreadId} - {_thread.Name}] Releasing Semaphore");
                    DatabaseSemaphore.Release();
                }
                logger.LogInformation($"[{Thread.CurrentThread.ManagedThreadId} - {_thread.Name}] All files processed for job: {(_jobInfo != null ? _jobInfo.Name : "unknown")}");
                CancelAsync();
            }
            return null;
        }

        private async Task<IThreadMessage?> HandleStartupData(StartupData startupData)
        {
            _jobInfo = startupData.JobInfo ?? throw new ArgumentNullException(nameof(startupData.JobInfo), "JobInfo cannot be null");
            _OnChangedHashesUpdate = startupData.OnChangedHashesUpdate 
                ?? throw new ArgumentNullException(nameof(startupData.OnChangedHashesUpdate), "OnChangedHashesUpdate cannot be null");
            _runId = startupData.RunResultsId;
            _sendRequestThingForFileMessage = startupData.SendRequestThingForFileMessage 
                ?? throw new ArgumentNullException(nameof(startupData.SendRequestThingForFileMessage), "SendRequestThingForFileMessage cannot be null");
            _sendThingForUpdateMessage = startupData.SendThingForUpdateMessage ?? throw new ArgumentNullException(nameof(startupData.SendThingForUpdateMessage), "SendThingForUpdateMessage cannot be null");

            SetThreadName($"MainWorkerThread-{_jobInfo.Name}");
            logger.LogInformation($"[{Thread.CurrentThread.ManagedThreadId} - {_thread.Name}] Starting processing for job: {_jobInfo.Name} at {_jobInfo.RootFolder}");
            WorkerFileListGenerator fileListGenerator = serviceProvider.GetRequiredService<WorkerFileListGenerator>();
            IList<string> fileList = await fileListGenerator.DoWork(_jobInfo);

            if (settings.TakeSize > 0)
            {
                fileList = [.. fileList.Take(settings.TakeSize)];
            }
            _jobInfo.FoundFilesCount = fileList.Count;
            _sendThingForUpdateMessage.Invoke(new AuditableThingUpdateMessage(_jobInfo, AuditableThingUpdateThread.MSG_JOB_INFO_UPDATE));

            logger.LogInformation($"[{Thread.CurrentThread.ManagedThreadId} - {_thread.Name}] Found {fileList.Count} files in folder: {_jobInfo.Name}");
            WorkerHashGenerator hashGenerator = serviceProvider.GetRequiredService<WorkerHashGenerator>();
            ((List<string>)fileList).ForEach(file =>
            {
                if (!CancellationPending)
                {
                    logger.LogInformation($"[{Thread.CurrentThread.ManagedThreadId} - {_thread.Name}] Generating hash for file: {file}");
                    string hashResults = hashGenerator.DoWork(new Tuple<string,float>(file, _jobInfo.ChunkSizePercent)).Result;
                    if (!string.IsNullOrEmpty(hashResults))
                    {
                        _fileListWithHashes.Add(new FileInfoWithHash(file, hashResults));
                        _jobInfo.FilesHashedCount++;
                        _sendThingForUpdateMessage?.Invoke(new AuditableThingUpdateMessage(_jobInfo, AuditableThingUpdateThread.MSG_JOB_INFO_UPDATE));
                    }
                }
            });
            logger.LogInformation($"[{Thread.CurrentThread.ManagedThreadId} - {_thread.Name}] All hashes generated for job: {_jobInfo.Name}");
            if (!CancellationPending)
            {
                SendRequestForFileThing(_fileListWithHashes.First());
            }
            return null;
        }

        private void SendRequestForFileThing(FileInfoWithHash fileInfo)
        {
            if (_sendRequestThingForFileMessage == null)
            {
                logger.LogError($"[{Thread.CurrentThread.ManagedThreadId} - {_thread.Name}] RequestThingForFileMessage queue is not set.");
                return;
            }
            RequestThingForFileData requestData = new(fileInfo.FilePath, fileInfo.Hash);
            RequestThingForFileMessage requestMessage = new(requestData, (message) => { 
                _runResultsQueue.Enqueue(message); 
                return true; 
            });
            _sendRequestThingForFileMessage?.Invoke(requestMessage);
            
            logger.LogInformation($"[{Thread.CurrentThread.ManagedThreadId} - {_thread.Name}] Enqueued RequestThingForFileMessage for file: {fileInfo.FilePath}");
        }
    }
    
}
