using System.Net;
using System.Text;

namespace RSSFeedifyCLIClient.Services
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

        public async Task<(bool success, HttpResponseMessage response)> GetAsync(Uri uri)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(uri.ToString());
                return (true, response);
            }
            catch (Exception ex)
            {
                return (false, new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent($"Failed to fetch data from {uri}: {ex.Message}")
                });
            }
        }

        public async Task<(bool success, HttpResponseMessage response)> DeleteAsync(Uri uri)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.DeleteAsync(uri.ToString());
                return (true, response);
            }
            catch (Exception ex)
            {
                return (false, new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent($"Failed to fetch data from {uri}: {ex.Message}")
                });
            }
        }

        public async Task<(bool success, HttpResponseMessage response)> PutAsync(Uri uri, string payload, ContentType contentType = ContentType.AppJson)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.PutAsync(uri.ToString(), new StringContent(payload, Encoding.UTF8, StringifyContentType(contentType)));
                return (true, response);
            }
            catch (Exception ex)
            {
                return (false, new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent($"Failed to post data to {uri}: {ex.Message}")
                });
            }
        }

        public async Task<(bool success, HttpResponseMessage response)> PostAsync(Uri uri, string payload, ContentType contentType = ContentType.AppJson)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.PostAsync(uri.ToString(), new StringContent(payload, Encoding.UTF8, StringifyContentType(contentType)));
                return (true, response);
            }
            catch (Exception ex)
            {
                return (false, new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent($"Failed to post data to {uri}: {ex.Message}")
                });
            }
        }

        public static ContentType GetContentType(HttpResponseMessage response)
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

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}
