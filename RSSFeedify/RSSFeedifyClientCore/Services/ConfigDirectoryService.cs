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
                configFilesDirectory = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName;
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException || e is ArgumentException || e is PathTooLongException || e is FileNotFoundException || e is DirectoryNotFoundException || e is NotSupportedException || e is System.Security.SecurityException)
            {
                return Result.Error<string, string>($"Exception occured when loading environment variable(s): '{e.Message}'.");
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
