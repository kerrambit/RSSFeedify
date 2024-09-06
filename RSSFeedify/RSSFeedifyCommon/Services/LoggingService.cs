using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Core;
using Serilog.Enrichers.WithCaller;

namespace RSSFeedifyCommon.Services
{
    public class LoggingService
    {
        public Logger Logger { get; private set; }

        public LoggingService(string basePath, string configFilePath = "loggingsettings.json")
        {
            Logger = Initialize(basePath);
        }

        public LoggingService(ConfigurationManager configuration)
        {
            Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.WithCaller()
            .CreateLogger();
        }

        private Logger Initialize(string basePath, string configFilePath = "loggingsettings.json")
        {
            IConfigurationRoot configuration;
            try
            {
                configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile(configFilePath, optional: false, reloadOnChange: true)
                .Build();

                var toReturn = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .Enrich.WithCaller()
                .CreateLogger();

                var logger = toReturn.ForContext<LoggingService>();
                logger.Information("Custom logger settings were loaded from '{Path}'.", Path.Combine(basePath, configFilePath));

                return toReturn;
            }
            catch (InvalidDataException e)
            {
                var toReturn = GetDefaultRescueLogger();

                var logger = toReturn.ForContext<LoggingService>();
                logger.Error("Custom logger settings could not be loaded. Detailed message: '{Message}'", e.Message);

                return toReturn;
            }
        }

        private Logger GetDefaultRescueLogger()
        {
            return new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day, outputTemplate: "[Warning: DefaultRescueLogger is being used.][{Timestamp:yyyy-MM-dd HH:mm:ss}][{Level:u3}][{SourceContext}]: {Message:lj}{NewLine}{Exception}")
            .CreateLogger();
        }
    }
}
