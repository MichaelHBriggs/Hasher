using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HasherDataObjects.Configuration
{
    public class DbLoggingOptions
    {
        public Dictionary<string, string> LogLevel { get; init; } = new();
    }
}
