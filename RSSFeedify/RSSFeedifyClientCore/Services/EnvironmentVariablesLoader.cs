using RSSFeedifyCommon.Types;

namespace RSSFeedifyClientCore.Services
{
    public static class EnvironmentVariablesLoader
    {
        public static Result<string, string> LoadEnvironmentVariable(string path, string key)
        {
            DotNetEnv.Env.Load(path);
            var variable = Environment.GetEnvironmentVariable(key);
            if (variable == null)
            {
                return Result.Error<string, string>($"Key {key} does not exist or the path {path} is invalid.");
            }

            return Result.Ok<string, string>(variable);
        }
    }
}
