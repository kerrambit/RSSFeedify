using System.Net;
using System.Text;

namespace RSSFeedifyClientCore
{
    public class HTTPService : IDisposable
    {
        private HttpClient _httpClient;

        public enum ContentType
        {
            AppJson,
            AppXml,
            AudioMpeg,
            ImgGif,
            TxtHtml,
            Unkown
        }

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
            switch (response.Content.Headers.ContentType?.MediaType)
            {
                case "application/json":
                    return ContentType.AppJson;
                default:
                    return ContentType.Unkown;
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
