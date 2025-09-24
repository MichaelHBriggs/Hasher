using HasherDataObjects.Configuration;
using hasher.Messages;
using HasherDataObjects.Models;
using hasher.Threads;
using hasher.Workers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using hasher.Models;

namespace hasher
{
    public class HostedApplication : IHostedService, IHostedLifecycleService
    {
        private readonly ILogger<HostedApplication> _logger;
        private readonly IServiceProvider _services;
        private Guid? ActiveRunId { get; set; }
        private readonly IHostApplicationLifetime _appLifetime;
        private readonly List<EventHandlingThread> _workers = [];

        public HostedApplication(ILogger<HostedApplication> logger,
                                IHostApplicationLifetime appLifetime,
                                IServiceProvider services)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger), "Logger cannot be null");
            _services = services ?? throw new ArgumentNullException(nameof(services), "Services cannot be null");
            _appLifetime = appLifetime ?? throw new ArgumentNullException(nameof(appLifetime), "Application Lifetime cannot be null");

            appLifetime.ApplicationStarted.Register(OnStarted);
            appLifetime.ApplicationStopping.Register(OnStopping);
            appLifetime.ApplicationStopped.Register(OnStopped);


        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"[{Thread.CurrentThread.ManagedThreadId}] 2. StartAsync has been called.");

            return Task.CompletedTask;
        }

        public Task StartedAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"[{Thread.CurrentThread.ManagedThreadId}] 3. StartedAsync has been called.");

            return Task.CompletedTask;
        }

        public Task StartingAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"[{Thread.CurrentThread.ManagedThreadId}] 1. StartingAsync has been called.");

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"[{Thread.CurrentThread.ManagedThreadId}] 7. StopAsync has been called.");
            _logger.LogInformation($"[{Thread.CurrentThread.ManagedThreadId}] Cancelling all workers.");
            if (_workers.Any(w => w.IsBusy))
            {
                _logger.LogInformation($"[{Thread.CurrentThread.ManagedThreadId}] Some Workers are busy, cancelling busy workers.");
                _workers.Where(w => w.IsBusy).ToList().ForEach(w => w.CancelAsync());
            }
            while (_workers.Any(w => w.IsBusy))
            {
                Task.Delay(100).Wait(); // Wait for all workers to complete
            }
            _logger.LogInformation($"[{Thread.CurrentThread.ManagedThreadId}] All workers have completed their tasks.");
            return Task.CompletedTask;
        }

        public Task StoppedAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"[{Thread.CurrentThread.ManagedThreadId}] 8. StoppedAsync has been called.");

            return Task.CompletedTask;
        }

        public Task StoppingAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"[{Thread.CurrentThread.ManagedThreadId}] 6. StoppingAsync has been called.");


            return Task.CompletedTask;
        }

        private async void OnStarted()
        {
            _logger.LogInformation($"[{Thread.CurrentThread.ManagedThreadId}] 4. OnStarted has been called.");

            try
            {
                ConcurrentDictionary<string, Tuple<string, string>> changedHashes = [];
                HasherContext hasherContext = _services.GetRequiredService<HasherContext>();

                //Ready to go after this point

                RunResults runResults = _services.GetRequiredService<HasherContext>().RunResults.Add(new RunResults()).Entity;
                runResults.IsActive = true;
                ActiveRunId = runResults.Id;
                hasherContext.SaveChanges();

                AuditableThingRequestThread thingRequestThread = _services.GetRequiredService<AuditableThingRequestThread>();
                thingRequestThread.Start();
                _workers.Add(thingRequestThread);
                Thread.Yield();

                AuditableThingUpdateThread thingUpdateThread = _services.GetRequiredService<AuditableThingUpdateThread>();
                thingUpdateThread.RunId = runResults.Id;
                thingUpdateThread.Start();
                _workers.Add(thingUpdateThread);
                Thread.Yield();

                _services.GetService<Settings>()?.Folders.ForEach(folder =>
                {
                    _logger.LogInformation($"[{Thread.CurrentThread.ManagedThreadId}] Folder Name: {folder.Name}, Path: {folder.RootFolder}");
                    _logger.LogInformation($"[{Thread.CurrentThread.ManagedThreadId}] Extensions: {string.Join(", ", folder.Extensions)}");
                    MainWorkerThread worker = new MainWorkerThread(_services.GetRequiredService<ILogger<MainWorkerThread>>(),
                                                                    _services, _services.GetRequiredService<Settings>());

                    JobInfo dbJobInfo = hasherContext.Jobs.Add(new JobInfo(folder)).Entity;
                    dbJobInfo.MostRecentRun = runResults;
                    dbJobInfo.ChunkSizePercent = (float)folder.chunkSizePercent / 100f;
                    hasherContext.SaveChanges();
                    worker.Start();
                    StartupData startupData = new StartupData()
                    {
                        JobInfo = dbJobInfo,
                        RunResultsId = runResults.Id,
                    };
                    startupData.OnChangedHashesUpdate = (fileName, newHash, oldHash) =>
                    {
                        return changedHashes.TryAdd(fileName, new(newHash, oldHash));
                    };
                    startupData.SendRequestThingForFileMessage = (message) =>
                    {
                        thingRequestThread.SendMessage(message);
                        return true;
                    };
                    startupData.SendThingForUpdateMessage = (message) =>
                    {
                        thingUpdateThread.SendMessage(message);
                        return true;
                    };

                    StartupMessage startupMessage = new(startupData, null);
                    Thread.Yield();
                    worker.SendMessage(startupMessage);
                    _workers.Add(worker);
                });

                while (_workers.Where(w => w.GetType() == typeof(MainWorkerThread)).Any(w => w.IsBusy))
                {
                    Thread.Yield();
                }

                _logger.LogInformation($"[{Thread.CurrentThread.ManagedThreadId}] All workers have completed their tasks.");
                if (changedHashes.Any())
                {
                    _logger.LogInformation($"[{Thread.CurrentThread.ManagedThreadId}] Changed hashes detected:");
                    foreach (var change in changedHashes)
                    {
                        _logger.LogInformation($"[{Thread.CurrentThread.ManagedThreadId}] File: {change.Key}, Name: {change.Value.Item1}, New Hash: {change.Value.Item2}");
                    }
                    // Here you would typically trigger an email notification or some other action
                }
                else
                {
                    _logger.LogInformation($"[{Thread.CurrentThread.ManagedThreadId}] No changed hashes detected.");
                }

                //Find files that were not touched in this run
                Message untouched = new(changedHashes, AuditableThingUpdateThread.MSG_HANDLE_UNTOUCHED_FILES, null);
                thingUpdateThread.SendMessage(untouched);
                thingRequestThread.CancelAsync();

                while (_workers.Any(w => w.GetType() != typeof(MainWorkerThread) && w.IsBusy))
                {
                    Thread.Yield(); // Wait for the "other than main worker" threads to complete
                }

                _workers.Where(w => w.GetType() != typeof(MainWorkerThread)).ToList().ForEach(w => w.CancelAsync());

                

            }
            catch (Exception ex)
            {
                _workers.ForEach(w => w.CancelAsync());
                _logger.LogError(ex, $"[{Thread.CurrentThread.ManagedThreadId}] An error occurred during the hosted service execution: {ex.Message}");
                while (_workers.Any(w => w.IsBusy))
                {
                    await Task.Delay(100); // Wait for all workers to complete
                }
            }
            _appLifetime.StopApplication();
        }

        private void OnStopping()
        {
            _logger.LogInformation($"[{Thread.CurrentThread.ManagedThreadId}] 5. OnStopping has been called.");
            RunResults? runResults = _services.GetRequiredService<HasherContext>().RunResults.FirstOrDefault(r => r.Id == ActiveRunId);
            if (runResults != null)
            {
                runResults.IsActive = false;
                _services.GetRequiredService<HasherContext>().UpdateThing(runResults);
                _services.GetRequiredService<HasherContext>().SaveChanges();
                _logger.LogInformation($"[{Thread.CurrentThread.ManagedThreadId}] Run completed. Results: {runResults}");
            }
        }

        private void OnStopped()
        {
            _logger.LogInformation($"[{Thread.CurrentThread.ManagedThreadId}] 9. OnStopped has been called.");
        }

        
    }
}
