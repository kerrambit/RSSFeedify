using System.Text;

namespace RSSFeedifyClientCore
{
    public class HTTPService : IDisposable
    {
        private HttpClient _httpClient;

        public enum ContentType
        {
            Empty,
            AppJson,
            AppXml,
            AudioMpeg,
            ImgGif,
            TxtHtml,
            TxtPlain,
            Unkown
        }

        public enum StatusCode
        {
            OK = 200,
            Created = 201,
            BadRequest = 400,
            Unauthorized = 401,
            Forbidden = 403,
            NotFound = 404,
            MethodNotAllowed = 405,
            InternalServerError = 500
        }

        public record HttpServiceResponseMessageMetaData(ContentType ContentType, StatusCode StatusCode);

        public HTTPService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Result<HttpResponseMessage, string>> GetAsync(Uri uri)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(uri.ToString());
                return Result.Ok<HttpResponseMessage, string>(response);
            }
            catch (Exception ex)
            {
                return Result.Error<HttpResponseMessage, string>($"Failed to fetch data from {uri}: {ex.Message}.");
            }
        }

        public async Task<Result<HttpResponseMessage, string>> GetAsync(Uri uri, IAuthenticationHeader authenticationHeader)
        {
            try
            {
                AddAuthenticationHeader(authenticationHeader);
                HttpResponseMessage response = await _httpClient.GetAsync(uri.ToString());
                return Result.Ok<HttpResponseMessage, string>(response);
            }
            catch (Exception ex)
            {
                return Result.Error<HttpResponseMessage, string>($"Failed to fetch data from {uri}: {ex.Message}.");
            }
        }

        public async Task<Result<HttpResponseMessage, string>> DeleteAsync(Uri uri)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.DeleteAsync(uri.ToString());
                return Result.Ok<HttpResponseMessage, string>(response);
            }
            catch (Exception ex)
            {
                return Result.Error<HttpResponseMessage, string>($"Failed to fetch data from {uri}: {ex.Message}.");
            }
        }

        public async Task<Result<HttpResponseMessage, string>> DeleteAsync(Uri uri, IAuthenticationHeader authenticationHeader)
        {
            try
            {
                AddAuthenticationHeader(authenticationHeader);
                HttpResponseMessage response = await _httpClient.DeleteAsync(uri.ToString());
                return Result.Ok<HttpResponseMessage, string>(response);
            }
            catch (Exception ex)
            {
                return Result.Error<HttpResponseMessage, string>($"Failed to fetch data from {uri}: {ex.Message}.");
            }
        }

        public async Task<Result<HttpResponseMessage, string>> PutAsync(Uri uri, string payload, ContentType contentType = ContentType.AppJson)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.PutAsync(uri.ToString(), new StringContent(payload, Encoding.UTF8, StringifyContentType(contentType)));
                return Result.Ok<HttpResponseMessage, string>(response);
            }
            catch (Exception ex)
            {
                return Result.Error<HttpResponseMessage, string>($"Failed to fetch data from {uri}: {ex.Message}.");
            }
        }

        public async Task<Result<HttpResponseMessage, string>> PutAsync(Uri uri, string payload, IAuthenticationHeader authenticationHeader, ContentType contentType = ContentType.AppJson)
        {
            try
            {
                AddAuthenticationHeader(authenticationHeader);
                HttpResponseMessage response = await _httpClient.PutAsync(uri.ToString(), new StringContent(payload, Encoding.UTF8, StringifyContentType(contentType)));
                return Result.Ok<HttpResponseMessage, string>(response);
            }
            catch (Exception ex)
            {
                return Result.Error<HttpResponseMessage, string>($"Failed to fetch data from {uri}: {ex.Message}.");
            }
        }

        public async Task<Result<HttpResponseMessage, string>> PostAsync(Uri uri, string payload, ContentType contentType = ContentType.AppJson)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.PostAsync(uri.ToString(), new StringContent(payload, Encoding.UTF8, StringifyContentType(contentType)));
                return Result.Ok<HttpResponseMessage, string>(response);
            }
            catch (Exception ex)
            {
                return Result.Error<HttpResponseMessage, string>($"Failed to fetch data from {uri}: {ex.Message}.");
            }
        }

        public async Task<Result<HttpResponseMessage, string>> PostAsync(Uri uri, string payload, IAuthenticationHeader authenticationHeader, ContentType contentType = ContentType.AppJson)
        {
            try
            {
                AddAuthenticationHeader(authenticationHeader);
                HttpResponseMessage response = await _httpClient.PostAsync(uri.ToString(), new StringContent(payload, Encoding.UTF8, StringifyContentType(contentType)));
                return Result.Ok<HttpResponseMessage, string>(response);
            }
            catch (Exception ex)
            {
                return Result.Error<HttpResponseMessage, string>($"Failed to fetch data from {uri}: {ex.Message}.");
            }
        }

        public static ContentType RetrieveContentType(HttpResponseMessage response)
        {
            if (response.Content == null || response.Content.Headers.ContentType == null)
            {
                return ContentType.Empty;
            }

            switch (response.Content.Headers.ContentType?.MediaType)
            {
                case "application/json":
                    return ContentType.AppJson;
                case "text/plain":
                    return ContentType.TxtPlain;
                default:
                    return ContentType.Unkown;
            }
        }

        public static async Task<string> RetrieveAndStringifyContent(HttpResponseMessage response)
        {
            return await response.Content.ReadAsStringAsync();
        }

        public static StatusCode RetrieveStatusCode(HttpResponseMessage response)
        {
            switch (response.StatusCode)
            {
                case System.Net.HttpStatusCode.OK:
                    return StatusCode.OK;
                case System.Net.HttpStatusCode.BadRequest:
                    return StatusCode.BadRequest;
                case System.Net.HttpStatusCode.Forbidden:
                    return StatusCode.Forbidden;
                case System.Net.HttpStatusCode.Created:
                    return StatusCode.Created;
                case System.Net.HttpStatusCode.Unauthorized:
                    return StatusCode.Unauthorized;
                case System.Net.HttpStatusCode.InternalServerError:
                    return StatusCode.InternalServerError;
                case System.Net.HttpStatusCode.MethodNotAllowed:
                    return StatusCode.MethodNotAllowed;
                case System.Net.HttpStatusCode.NotFound:
                    return StatusCode.NotFound;
                default:
                    return StatusCode.BadRequest;
            }
        }

        public static string StringifyContentType(ContentType contentType)
        {
            switch (contentType)
            {
                case ContentType.AppJson:
                    return "application/json";
                default:
                    return "text/plain";
            }
        }

        private void AddAuthenticationHeader(IAuthenticationHeader authenticationHeader)
        {
            if (authenticationHeader.AuthSchemeType == AuthenticationTypeName.NoAuth)
            {
                return;
            }

            _httpClient.DefaultRequestHeaders.Authorization = authenticationHeader.ConvertToDotNetHttpHeader();
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}
