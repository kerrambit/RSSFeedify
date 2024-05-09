using System.Net;

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

        public async Task<(bool success, HttpResponseMessage response)> Get(Uri uri)
        {
            using (_httpClient)
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
            Console.Write("I am deleting HTTP Client.");
            _httpClient.Dispose();
        }
    }
}
