
using HasherDataObjects.Configuration;
using HasherDataObjects.Models;
using HasherDbLogging.Logging;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace HasherWeb
{
    public class Program
    {
        public static void Main(string[] args)
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

            var builder = WebApplication.CreateBuilder(args);

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
            builder.Services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                options.KnownProxies.Add(IPAddress.Parse("127.0.0.1"));
            });
            builder.Services.AddSingleton<IConfiguration>(x => config);
            builder.Services.AddSingleton<Settings>(x => config!.GetSection("AppSettings")!.Get<Settings>()!);
            builder.Services.AddDbContext<HasherContext>(options =>
            {
                options.UseSqlServer(config.GetValue<string>("AppSettings:HasherDBConnectionString"));
                options.UseLoggerFactory(factory);
                options.EnableSensitiveDataLogging(false);
                options.EnableDetailedErrors(true);
            });

            // Add services to the container.

            builder.Services.AddControllers();
            
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            app.UseDefaultFiles();
            app.UseStaticFiles();

            // Configure the HTTP request pipeline.
            if (true || app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseForwardedHeaders();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
