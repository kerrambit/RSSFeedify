using ClientNetLib.Business.Errors;
using ClientNetLib.Types;

namespace ClientNetLib.Services.EnvironmentUtils
{
    public static class ConfigDirectoryService
    {
        public static Result<string, DetailedError> GetConfigFilesDirectory()
        {
            string configFilesDirectory = string.Empty;
            try
            {
                configFilesDirectory = AppDomain.CurrentDomain.BaseDirectory;
            }
            catch (Exception e) when (e is AppDomainUnloadedException)
            {
                return Result.Error<string, DetailedError>(new(Error.ConfigFileDirectoryLoadingError, e.Message));
            }

            return Result.Ok<string, DetailedError>(configFilesDirectory);
        }

        public static Result<string, DetailedError> GetEnvironmentFilePath()
        {
            var configFilesDirectoryResult = GetConfigFilesDirectory();
            if (configFilesDirectoryResult.IsSuccess)
            {
                string envFilePath = string.Empty;
                try
                {
                    envFilePath = Path.Combine(configFilesDirectoryResult.GetValue, ".env");
                    return Result.Ok<string, DetailedError>(envFilePath);
                }
                catch (Exception e) when (e is ArgumentException || e is ArgumentNullException)
                {
                    return Result.Error<string, DetailedError>(new(Error.EnvironmentFileLoadingError, e.Message));
                }
            }

            return configFilesDirectoryResult;
        }

        public static Result<string, DetailedError> GetEnvironmentFilePath(string basePath)
        {
            string envFilePath = string.Empty;
            try
            {
                envFilePath = Path.Combine(basePath, ".env");
                return Result.Ok<string, DetailedError>(envFilePath);
            }
            catch (Exception e) when (e is ArgumentException || e is ArgumentNullException)
            {
                return Result.Error<string, DetailedError>(new(Error.EnvironmentFileLoadingError, e.Message));
            }
        }
    }
}
