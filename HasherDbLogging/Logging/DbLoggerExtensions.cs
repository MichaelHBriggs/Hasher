using HasherDataObjects.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HasherDbLogging.Logging
{
    public static class DbLoggerExtensions
    {
        public static ILoggingBuilder AddDbLogger(this ILoggingBuilder builder, Action<DbLoggingOptions> configure)
        {
            builder.Services.AddSingleton<ILoggerProvider, DbLoggingProvider>();
            builder.Services.Configure(configure);
            return builder;
        }
    }
}
