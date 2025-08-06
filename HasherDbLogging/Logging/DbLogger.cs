using HasherDataObjects.Models;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Reflection;

namespace HasherDbLogging.Logging
{
    public class DbLogger : ILogger
    {
        private readonly DbLoggingProvider _provider;
        public DbLogger(DbLoggingProvider provider)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider), "DbLoggingProvider cannot be null");
        }
        public IDisposable BeginScope<TState>(TState state) where TState : notnull
        {
            return null; // No scope management implemented
        }
        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel != LogLevel.None;
        }
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        { 
            if (!IsEnabled(logLevel))
            {
                return;
            }
            var threadId = Thread.CurrentThread.ManagedThreadId;

            StackTrace stackTrace = new();
            StackFrame? frame = stackTrace.GetFrame(7); // Get the caller's frame
            MethodBase? callingMethod = frame?.GetMethod();
            string? className = callingMethod?.DeclaringType?.FullName;

            using (var context =  _provider.DbContext)
            {
                LogEntry logEntry = new()
                {
                    LogLevel = logLevel.ToString(),
                    ThreadId = threadId,
                    EventId = eventId.Id,
                    EventName = eventId.Name ?? string.Empty,
                    Message = formatter(state, exception),
                    Exception = exception?.Message,
                    StackTrace = exception?.StackTrace,
                    SourceClass = className ?? "Unknown",
                    SourceMethod = callingMethod?.Name ?? "Unknown",
                };
                context.Logging.Add(logEntry);
                context.SaveChanges();
            }
        }
    }

}
