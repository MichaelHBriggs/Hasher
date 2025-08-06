using hasher.Messages;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace hasher.Threads
{
    public abstract class EventHandlingThread
    {

        private bool _cancellationPending;
        private bool _isRunning;
        private bool _canCancelWorker = true;


        protected readonly Thread _thread;
        private readonly ILogger _logger;

        protected static SemaphoreSlim DatabaseSemaphore { get; } = new(1, 1);
        protected ConcurrentQueue<IThreadMessage> _runResultsQueue { get; private set; } = new();

        public void SendMessage(IThreadMessage message)
        {
            if (_isRunning)
            {
                _runResultsQueue.Enqueue(message);
            }
        }


        public EventHandlingThread(ILogger logger)
        {
            _thread = new(() => DoWork());
            _logger= logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected void SetThreadName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Thread name cannot be null or empty", nameof(name));
            }
            _thread.Name = name;
        }

        public void Start()
        {
            if (_isRunning)
            {
                throw new InvalidOperationException("Thread is already running");
            }
            _logger.LogDebug($"[{Thread.CurrentThread.ManagedThreadId} - {_thread.Name}] Starting thread [{_thread.Name}]");
            _isRunning = true;
            _thread.Start();
        }

        private void DoWork()
        {
            _logger.LogInformation($"[{Thread.CurrentThread.ManagedThreadId} - {_thread.Name}] Thread has started running.");
            while (!CancellationPending)
            {
                if (_runResultsQueue.TryDequeue(out IThreadMessage? message))
                {
                    if (message == null)
                    {
                        continue;
                    }
                    try
                    {
                        IThreadMessage? response = OnMessageRecieved(message).Result;
                        if (response != null )
                        {
                            message.SendResponseMessage?.Invoke(response);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Handle exceptions as needed, e.g., log them
                        _logger.LogError(ex, $"[{Thread.CurrentThread.ManagedThreadId} - {_thread.Name}] Error processing message {message.MessageId}: {ex.Message}");
                    }
                }
                Thread.Sleep(10);
            }
            _isRunning = false;
            _logger.LogInformation($"[{Thread.CurrentThread.ManagedThreadId} - {_thread.Name}] Thread has stopped running.");
        }

        protected abstract Task<IThreadMessage?> OnMessageRecieved(IThreadMessage? data);


        public bool CancellationPending
        {
            get
            {
                return _cancellationPending;
            }
        }

        public void CancelAsync()
        {
            if (!WorkerSupportsCancellation)
            {
                throw new InvalidOperationException("Thread does not support cancellation");
            }
            _logger.LogDebug($"[{Thread.CurrentThread.ManagedThreadId} - {_thread.Name}] Canceling thread [{_thread.Name}]");
            _cancellationPending = true;
        }

        public bool WorkerSupportsCancellation
        {
            get
            {
                return _canCancelWorker;
            }

            set
            {
                _canCancelWorker = value;
            }
        }

        public bool IsBusy { get { return _isRunning; } }
    }
}
