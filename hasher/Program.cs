using HasherDbLogging.Logging;
using hasher.Threads;
using hasher.Workers;
using HasherDataObjects.Configuration;
using HasherDataObjects.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace hasher
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            IConfiguration config = new ConfigurationBuilder()
                                        .AddJsonFile("AppSettings.json")
                                        .AddUserSecrets<Program>()
                                        .Build();

            ILoggerFactory factory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                IConfigurationSection loggingConfig = config.GetSection("Logging");
                if (loggingConfig.Exists())
                {
                    builder.AddConfiguration(loggingConfig);
                }
            });
            ILogger<Program> logger = factory.CreateLogger<Program>();
            logger.LogInformation("Starting Hasher Application...");
            HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
            builder.Services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.AddConsole();
                
                
                IConfigurationSection loggingConfig = config.GetSection("Logging");
                if (loggingConfig.Exists())
                {
                    loggingBuilder.AddConfiguration(loggingConfig);
                    DbLoggingOptions? dbLoggingOptions = config.GetSection("Logging").Get<DbLoggingOptions>();
                    if (dbLoggingOptions != null)
                    {
                        loggingBuilder.AddDbLogger(options =>
                        {
                            dbLoggingOptions.LogLevel.ToList().ForEach(kv => options.LogLevel.Add(kv.Key, kv.Value));
                        });
                    }
                }
                else
                {
                    logger.LogWarning("Logging configuration section not found in AppSettings.json. Using default console logging.");
                }
            });
            
            builder.Services.AddTransient<WorkerFileListGenerator>();
            builder.Services.AddTransient<WorkerHashGenerator>();
            builder.Services.AddTransient<MainWorkerThread>();
            builder.Services.AddSingleton<AuditableThingRequestThread>();
            builder.Services.AddSingleton<AuditableThingUpdateThread>();
            builder.Services.AddTransient<WorkerEmailer>();
            builder.Services.AddSingleton<IConfiguration>(x => config);
            builder.Services.AddSingleton<Settings>(x => config!.GetSection("AppSettings")!.Get<Settings>()!);
            builder.Services.AddDbContext<HasherContext>(options =>
            {
                Settings? settings = config.GetSection("AppSettings").Get<Settings>();
                _ = settings ?? throw new ArgumentNullException(nameof(settings), "Settings cannot be null");
                options.UseSqlServer(settings.HasherDBConnectionString, b => b.MigrationsAssembly("hasher"));
                options.UseLoggerFactory(factory);
                options.EnableSensitiveDataLogging(false);
                options.EnableDetailedErrors(true);
            });
            builder.Services.AddHostedService<HostedApplication>();

            IHost app = builder.Build();

            app.Services.GetRequiredService<HasherContext>().Database.Migrate();

            await app.RunAsync();
        }
    }
}
