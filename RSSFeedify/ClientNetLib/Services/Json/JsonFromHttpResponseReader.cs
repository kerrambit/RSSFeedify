using ClientNetLib.Business.Errors;
using ClientNetLib.Types;
using System.Net.Http.Json;
using System.Text.Json;

namespace ClientNetLib.Services.Json
{
    public static class JsonFromHttpResponseReader
    {
        public static async Task<Result<T, Error>> ReadJson<T>(HttpResponseMessage response)
        {
            try
            {
                var data = await response.Content.ReadFromJsonAsync<T>();
                if (data is null)
                {
                    return Result.Error<T, Error>(Error.InvalidJsonFormat);
                }
                return Result.Ok<T, Error>(data);
            }
            catch (JsonException)
            {
                return Result.Error<T, Error>(Error.InvalidJsonFormat);
            }
            catch (Exception)
            {
                return Result.Error<T, Error>(Error.General);
            }
        }
    }
}
