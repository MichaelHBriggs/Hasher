using HasherDataObjects.Configuration;
using HasherDataObjects.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HasherDbLogging.Logging
{
    [ProviderAlias("Database")]
    public class DbLoggingProvider(IOptions<DbLoggingOptions> options, Settings settings) : ILoggerProvider
    {
        public readonly DbLoggingOptions Options = options?.Value ?? throw new ArgumentNullException(nameof(options), "DbLoggerOptions cannot be null");
        public HasherContext DbContext
        {
            get
            {
                return CreateContext(settings);
            }
        }

        private static HasherContext CreateContext(Settings settings)
        {
            var optionsBuilder = new DbContextOptionsBuilder<HasherContext>();

            optionsBuilder.UseSqlServer(settings.HasherDBConnectionString);
            optionsBuilder.EnableSensitiveDataLogging(false);
            optionsBuilder.EnableDetailedErrors(true);
            return new HasherContext(optionsBuilder.Options);
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new DbLogger(this);
        }

        public void Dispose()
        {
        }
    }
}
