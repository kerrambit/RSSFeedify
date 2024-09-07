using ClientNetLib.Business.Errors;
using ClientNetLib.Types;

namespace ClientNetLib.Services.EnvironmentUtils
{
    public static class EnvironmentVariablesLoader
    {
        public static Result<string, DetailedError> LoadEnvironmentVariable(string path, string key)
        {
            DotNetEnv.Env.Load(path);
            var variable = Environment.GetEnvironmentVariable(key);
            if (variable == null)
            {
                return Result.Error<string, DetailedError>(new(Error.UnkownEnvironmentVariable, $"Key: {key}, path: {path}"));
            }

            return Result.Ok<string, DetailedError>(variable);
        }
    }
}
