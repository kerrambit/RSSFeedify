using RSSFeedifyCommon.Types;

namespace RSSFeedifyClientCore.Services
{
    public static class ConfigDirectoryService
    {
        public static Result<string, string> GetConfigFilesDirectory()
        {
            string configFilesDirectory = string.Empty;
            try
            {
                configFilesDirectory = AppDomain.CurrentDomain.BaseDirectory;
            }
            catch (Exception e) when (e is AppDomainUnloadedException)
            {
                return Result.Error<string, string>($"Exception occured when loading config files directory: '{e.Message}'.");
            }

            return Result.Ok<string, string>(configFilesDirectory);
        }

        public static Result<string, string> GetEnvironmentFilePath()
        {
            var configFilesDirectoryResult = GetConfigFilesDirectory();
            if (configFilesDirectoryResult.IsSuccess)
            {
                string envFilePath = string.Empty;
                try
                {
                    envFilePath = Path.Combine(configFilesDirectoryResult.GetValue, ".env");
                    return Result.Ok<string, string>(envFilePath);
                }
                catch (Exception e) when (e is ArgumentException || e is ArgumentNullException)
                {
                    return Result.Error<string, string>($"Exception occured when loading environment variable(s): '{e.Message}'.");
                }
            }

            return configFilesDirectoryResult;
        }

        public static Result<string, string> GetEnvironmentFilePath(string basePath)
        {
            string envFilePath = string.Empty;
            try
            {
                envFilePath = Path.Combine(basePath, ".env");
                return Result.Ok<string, string>(envFilePath);
            }
            catch (Exception e) when (e is ArgumentException || e is ArgumentNullException)
            {
                return Result.Error<string, string>($"Exception occured when loading the .env file: '{e.Message}'.");
            }
        }
    }
}
