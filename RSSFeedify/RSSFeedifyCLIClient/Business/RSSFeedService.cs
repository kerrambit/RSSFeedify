using CommandParsonaut.Core.Types;
using CommandParsonaut.Interfaces;
using RSSFeedifyCLIClient.IO;
using RSSFeedifyCLIClient.Models;
using RSSFeedifyClientCore;
using System.Diagnostics;

namespace RSSFeedifyCLIClient.Business
{
    public class RSSFeedService
    {
        private IWriter _writer;
        private readonly ApplicationErrorWriter _errorWriter;
        private HTTPService _httpService;

        private readonly int PageSize = 5;
        private IDictionary<string, int> _pages = new Dictionary<string, int>();
        private (ListingPage page, IList<ParameterResult> parameters) _currentListingPage = (ListingPage.None, []);

        private enum ListingPage
        {
            RSSFeeds,
            RSSFeedItems,
            None
        }

        public RSSFeedService(IWriter writer, ApplicationErrorWriter errorWriter, HTTPService hTTPService)
        {
            _writer = writer;
            _errorWriter = errorWriter;
            _httpService = hTTPService;
            _pages["RSSFeeds"] = 1;
            _pages["RSSFeedItems"] = 1;
        }

        public async Task GetFeedsAsync()
        {
            var countResult = await RetrieveCount(Endpoints.EndPoint.RSSFeeds);
            if (countResult.IsError)
            {
                _errorWriter.RenderErrorMessage(countResult.GetError);
                return;
            }

            var count = countResult.GetValue;
            if (_pages["RSSFeeds"] > ComputeTotalPagesCount(count))
            {
                _pages["RSSFeeds"] = 1;
            }

            var data = await _httpService.GetAsync(Endpoints.BuildUri(Endpoints.EndPoint.RSSFeeds, new List<(string, string)>() { ("page", $"{_pages["RSSFeeds"]}"), ("pageSize", $"{PageSize}") }));
            if (!data.success)
            {
                _errorWriter.RenderErrorMessage(ApplicationError.Network);
                return;
            }

            if (HTTPService.GetContentType(data.response) != HTTPService.ContentType.AppJson)
            {
                _errorWriter.RenderErrorMessage(ApplicationError.DataType);
                return;
            }

            var result = await JsonFromHttpResponseReader.ReadJson<List<RSSFeed>>(data.response);
            if (result.IsError)
            {
                _errorWriter.RenderErrorMessage(result.GetError);
                return;
            }
            foreach (var feed in result.GetValue)
            {
                RenderRSSFeed(feed);
            }
            _writer.RenderBareText($"\t\t   {_pages["RSSFeeds"]} / {ComputeTotalPagesCount(count)}");

            _pages["RSSFeeds"]++;
            _currentListingPage = (ListingPage.RSSFeeds, []);
        }

        public async Task DeleteFeedAsync(IList<ParameterResult> parameters)
        {
            string guid = parameters[0].String;
            var data = await _httpService.DeleteAsync(Endpoints.BuildUri(Endpoints.EndPoint.RSSFeeds, guid));
            if (!data.success)
            {
                _errorWriter.RenderErrorMessage(ApplicationError.Network);
                return;
            }

            if (HTTPService.GetContentType(data.response) != HTTPService.ContentType.AppJson)
            {
                _errorWriter.RenderErrorMessage(ApplicationError.DataType);
                return;
            }

            var result = await JsonFromHttpResponseReader.ReadJson<RSSFeed>(data.response);
            if (result.IsError)
            {
                _errorWriter.RenderErrorMessage(result.GetError);
                return;
            }

            var feed = result.GetValue;

            _writer.RenderBareText($"RSSFeed '{feed.Name}' with GUID '{feed.Guid}' was successfully deleted!");
        }

        public async Task EditFeedAsync(IList<ParameterResult> parameters)
        {
            string guid = parameters[0].String;

            var original = await _httpService.GetAsync(Endpoints.BuildUri(Endpoints.EndPoint.RSSFeeds, guid));
            if (!original.success)
            {
                _errorWriter.RenderErrorMessage(ApplicationError.Network);
                return;
            }

            var result = await JsonFromHttpResponseReader.ReadJson<RSSFeed>(original.response);
            if (result.IsError)
            {
                _errorWriter.RenderErrorMessage(result.GetError);
                return;
            }

            var originalFeed = result.GetValue;

            RSSFeedDTO feed = new(parameters[1].String, parameters[2].String, originalFeed.SourceUrl, parameters[3].Double);
            var data = await _httpService.PutAsync(Endpoints.BuildUri(Endpoints.EndPoint.RSSFeeds, guid), JsonConvertor.ConvertObjectToJsonString(feed));
            if (!data.success)
            {
                _errorWriter.RenderErrorMessage(ApplicationError.Network);
                return;
            }

            result = await JsonFromHttpResponseReader.ReadJson<RSSFeed>(data.response);
            if (result.IsSuccess)
            {
                _writer.RenderBareText("RSSFeed was successfully edited:");
                RenderRSSFeed(result.GetValue);
            }
        }

        public async Task ReadArticle(IList<ParameterResult> parameters)
        {
            string guid = parameters[0].String;
            var data = await _httpService.GetAsync(Endpoints.BuildUri(Endpoints.EndPoint.RSSFeedItems, guid));
            if (!data.success)
            {
                _errorWriter.RenderErrorMessage(ApplicationError.Network);
                return;
            }

            if (HTTPService.GetContentType(data.response) != HTTPService.ContentType.AppJson)
            {
                _errorWriter.RenderErrorMessage(ApplicationError.DataType);
                return;
            }
            var result = await JsonFromHttpResponseReader.ReadJson<RSSFeedItem>(data.response);
            if (result.IsError)
            {
                _errorWriter.RenderErrorMessage(result.GetError);
                return;
            }

            var article = result.GetValue;

            try
            {
                string link = article.Id;
                if (!IsValidUrl(link))
                {
                    _errorWriter.RenderInvalidUrlWarningMessage(link);
                    return;
                }

                Process.Start(new ProcessStartInfo
                {
                    FileName = link,
                    UseShellExecute = true
                });
            }
            catch (System.ComponentModel.Win32Exception noBrowserException)
            {
                _errorWriter.RenderErrorMessage(noBrowserException.Message);
            }
            catch (Exception other)
            {
                _errorWriter.RenderErrorMessage(other.Message);
            }
        }

        public async Task GetFeedItemsAsync(IList<ParameterResult> parameters)
        {
            string guid = parameters[0].String;

            var countResult = await RetrieveCount(Endpoints.EndPoint.RSSFeedItems, ("byRSSFeedGuid", guid));
            if (countResult.IsError)
            {
                _errorWriter.RenderErrorMessage(countResult.GetError);
                return;
            }


            var count = countResult.GetValue;
            if (_pages["RSSFeedItems"] > ComputeTotalPagesCount(count))
            {
                _pages["RSSFeedItems"] = 1;
            }

            var data = await _httpService.GetAsync(Endpoints.BuildUri(Endpoints.EndPoint.RSSFeedItems, new List<(string, string)>() { ("byRSSFeedGuid", guid), ("page", $"{_pages["RSSFeedItems"]}"), ("pageSize", $"{PageSize}") }));
            if (!data.success)
            {
                _errorWriter.RenderErrorMessage(ApplicationError.Network);
                return;
            }

            if (HTTPService.GetContentType(data.response) != HTTPService.ContentType.AppJson)
            {
                _errorWriter.RenderErrorMessage(ApplicationError.DataType);
                return;
            }

            var result = await JsonFromHttpResponseReader.ReadJson<List<RSSFeedItem>>(data.response);
            if (result.IsError)
            {
                return;
            }

            var items = result.GetValue;
            foreach (var item in items)
            {
                RenderRSSFeedItem(item);
            }
            _writer.RenderBareText($"\t\t   {_pages["RSSFeedItems"]} / {ComputeTotalPagesCount(count)}");

            _pages["RSSFeedItems"]++;
            _currentListingPage = (ListingPage.RSSFeedItems, parameters);
        }

        public async Task AddNewFeed(IList<ParameterResult> parameters)
        {
            RSSFeedDTO feed = new(parameters[0].String, parameters[1].String, parameters[2].Uri, parameters[3].Double);
            var data = await _httpService.PostAsync(Endpoints.BuildUri(Endpoints.EndPoint.RSSFeeds), JsonConvertor.ConvertObjectToJsonString(feed));
            if (!data.success)
            {
                _errorWriter.RenderErrorMessage(ApplicationError.Network);
                return;
            }
            if (data.response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                _errorWriter.RenderErrorMessage(await data.response.Content.ReadAsStringAsync());
                return;
            }

            var result = await JsonFromHttpResponseReader.ReadJson<RSSFeed>(data.response);
            if (result.IsSuccess)
            {
                _writer.RenderBareText("New RSSFeed was registered:");
                RenderRSSFeed(result.GetValue);
            }
        }

        public async Task Next()
        {
            switch (_currentListingPage.page)
            {
                case ListingPage.RSSFeeds:
                    await GetFeedsAsync();
                    break;
                case ListingPage.RSSFeedItems:
                    await GetFeedItemsAsync(_currentListingPage.parameters);
                    break;
                case ListingPage.None:
                    _writer.RenderWarningMessage("Nothing to show.");
                    break;
            }
        }

        public void Settings()
        {
            _writer.RenderBareText("\n>General");
            _writer.RenderBareText($"\t>KEY: 'ssl-host-port'\t\tVALUE: '{Endpoints.BaseUrl}'");
        }

        private int ComputeTotalPagesCount(int count)
        {
            return (int)Math.Ceiling(count / (double)PageSize);
        }

        private async Task<Result<int, ApplicationError>> RetrieveCount(Endpoints.EndPoint endpoint, (string key, string value) queryString)
        {
            var count = await _httpService.GetAsync(Endpoints.BuildUri(endpoint, "count", queryString));
            return await ParseRetrievedCountResponse(count);
        }

        private async Task<Result<int, ApplicationError>> RetrieveCount(Endpoints.EndPoint endpoint)
        {
            var count = await _httpService.GetAsync(Endpoints.BuildUri(endpoint, "count"));
            return await ParseRetrievedCountResponse(count);
        }

        private async Task<Result<int, ApplicationError>> ParseRetrievedCountResponse((bool success, HttpResponseMessage response) count)
        {
            if (!count.success)
            {
                return Result.Error<int, ApplicationError>(ApplicationError.Network);
            }

            if (HTTPService.GetContentType(count.response) != HTTPService.ContentType.AppJson)
            {
                return Result.Error<int, ApplicationError>(ApplicationError.DataType);
            }

            return await JsonFromHttpResponseReader.ReadJson<int>(count.response);
        }

        private void RenderRSSFeed(RSSFeed feed)
        {
            _writer.RenderBareText($"RSSFeed [{feed.Guid}]:\n" +
                           $"    Title: '{feed.Name}'\n" +
                           $"    Description: '{feed.Description}'\n" +
                           $"    Link: {feed.SourceUrl}\n" +
                           $"    PollingInterval: {feed.PollingInterval} minutes\n" +
                           $"    LastSuccessfulPolling: {feed.LastSuccessfullPoll}\n");
        }

        private void RenderRSSFeedItem(RSSFeedItem item)
        {
            _writer.RenderBareText($"RSSFeedItem [{item.Guid}]:\n" +
                           $"    Title: '{item.Title}'\n" +
                           $"    Summary: '{item.Summary}'\n" +
                           $"    Publish Date: {item.PublishDate}\n" +
                           $"    Id: {item.Id}\n" +
                           $"    Authors: {((item.Authors is null || item.Authors.Count == 0) ? "[]" : string.Join(", ", item.Authors))}\n" +
                           $"    Categories: {((item.Categories is null || item.Categories.Count == 0) ? "[]" : string.Join(", ", item.Categories))}\n");
        }

        bool IsValidUrl(string url)
        {
            Uri? uriResult;
            return Uri.TryCreate(url, UriKind.Absolute, out uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }
    }
}
