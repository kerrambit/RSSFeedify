using System.Net.Http.Json;
using System.Text.Json;
using RSSFeedifyClientCore.Business.Errors;
using RSSFeedifyClientCore.Types;

namespace RSSFeedifyClientCore.Services.Networking
{
    public static class JsonFromHttpResponseReader
    {
        public static async Task<Result<T, ApplicationError>> ReadJson<T>(HttpResponseMessage response)
        {
            try
            {
                var data = await response.Content.ReadFromJsonAsync<T>();
                if (data is null)
                {
                    return Result.Error<T, ApplicationError>(ApplicationError.InvalidJsonFormat);
                }
                return Result.Ok<T, ApplicationError>(data);
            }
            catch (JsonException)
            {
                return Result.Error<T, ApplicationError>(ApplicationError.InvalidJsonFormat);
            }
            catch (Exception)
            {
                return Result.Error<T, ApplicationError>(ApplicationError.General);
            }
        }
    }
}
