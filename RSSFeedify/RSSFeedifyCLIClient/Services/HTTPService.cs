using System.Net;

namespace RSSFeedifyCLIClient.Services
{
    public class HTTPService
    {
        private HttpClient _httpClient;

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
    }
}
