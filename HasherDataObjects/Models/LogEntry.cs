using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HasherDataObjects.Models
{
    public class LogEntry: AuditableThing
    {
        public string LogLevel { get; set; } = string.Empty;
        public int ThreadId { get; set; } = 0;
        public int EventId { get; set; } = 0;
        public string EventName { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? Exception { get; set; } = null;
        public string? StackTrace { get; set; } = null;
        public string? ExceptionSource { get; set; } = null;
        public string SourceClass { get; set; } = string.Empty;
        public string SourceMethod { get; set; } = string.Empty;
        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
