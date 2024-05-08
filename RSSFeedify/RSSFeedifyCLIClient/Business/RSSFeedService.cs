using CommandParsonaut.Core.Types;
using CommandParsonaut.Interfaces;
using RSSFeedifyCLIClient.Services;
using RSSFeedifyCLIClient.Settings;

namespace RSSFeedifyCLIClient.Business
{
    public class RSSFeedService
    {
        private IWriter _writer;
        private HTTPService _httpService;

        public RSSFeedService(IWriter writer, HTTPService hTTPService)
        {
            _writer = writer;
            _httpService = hTTPService;
        }

        public async Task AddNewFeed(IList<ParameterResult> parameters)
        {
            _writer.RenderBareText("Sending request...");
            var data = await _httpService.Get(Endpoints.BuildUri(Endpoints.EndPoint.RSSFeeds));
            if (!data.success)
            {
                _writer.RenderErrorMessage($"Unable to send the request or no request awaited. {data.response}");
                return;
            }

            _writer.RenderBareText(data.response.ToString());
        }
    }
}
