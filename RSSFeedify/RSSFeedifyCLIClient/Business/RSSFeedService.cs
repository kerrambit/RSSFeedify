using CommandParsonaut.Core.Types;
using CommandParsonaut.Interfaces;
using RSSFeedifyCLIClient.Models;
using RSSFeedifyCLIClient.Services;
using RSSFeedifyCLIClient.Settings;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;

namespace RSSFeedifyCLIClient.Business
{
    public class RSSFeedService
    {
        private IWriter _writer;
        private HTTPService _httpService;

        private readonly int PageSize = 5;
        private IDictionary<string, int> _pages = new Dictionary<string, int>();
        private (ListingPage page, IList<ParameterResult> parameters) _currentListingPage = (ListingPage.None, []);

        private enum Error
        {
            General,
            Network,
            DataType,
            InvalidJsonFormat
        }

        private enum ListingPage
        {
            RSSFeeds,
            RSSFeedItems,
            None
        }

        public RSSFeedService(IWriter writer, HTTPService hTTPService)
        {
            _writer = writer;
            _httpService = hTTPService;
            _pages["RSSFeeds"] = 1;
            _pages["RSSFeedItems"] = 1;
        }

        public async Task GetFeedsAsync()
        {
            int count = await RetrieveCount(Endpoints.EndPoint.RSSFeeds);
            if (count == -1)
            {
                return;
            }
            if (_pages["RSSFeeds"] > ComputeTotalPagesCount(count))
            {
                _pages["RSSFeeds"] = 1;
            }

            var data = await _httpService.GetAsync(Endpoints.BuildUri(Endpoints.EndPoint.RSSFeeds, new List<(string, string)>() { ("page", $"{_pages["RSSFeeds"]}"), ("pageSize", $"{PageSize}") }));
            if (!data.success)
            {
                RenderErrorMessage(Error.Network);
                return;
            }

            if (HTTPService.GetContentType(data.response) != HTTPService.ContentType.AppJson)
            {
                RenderErrorMessage(Error.DataType);
                return;
            }

            var feeds = await ReadJson<List<RSSFeed>>(data.response);
            if (feeds is null)
            {
                RenderErrorMessage(Error.InvalidJsonFormat);
                return;
            }
            foreach (var feed in feeds)
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
                RenderErrorMessage(Error.Network);
                return;
            }

            if (HTTPService.GetContentType(data.response) != HTTPService.ContentType.AppJson)
            {
                RenderErrorMessage(Error.DataType);
                return;
            }

            var feed = await ReadJson<RSSFeed>(data.response);
            if (feed is null)
            {
                RenderErrorMessage(Error.InvalidJsonFormat);
                return;
            }

            _writer.RenderBareText($"RSSFeed '{feed.Name}' with GUID '{feed.Guid}' was successfully deleted!");
        }

        public async Task EditFeedAsync(IList<ParameterResult> parameters)
        {
            string guid = parameters[0].String;

            var original = await _httpService.GetAsync(Endpoints.BuildUri(Endpoints.EndPoint.RSSFeeds, guid));
            if (!original.success)
            {
                RenderErrorMessage(Error.Network);
                return;
            }

            var originalFeed = await ReadJson<RSSFeed>(original.response);
            if (originalFeed is null)
            {
                RenderErrorMessage(Error.InvalidJsonFormat);
                return;
            }

            RSSFeedDTO feed = new(parameters[1].String, parameters[2].String, originalFeed.SourceUrl, parameters[3].Double);
            var data = await _httpService.PutAsync(Endpoints.BuildUri(Endpoints.EndPoint.RSSFeeds, guid), JsonConvertor.ConvertObjectToJsonString(feed));
            if (!data.success)
            {
                RenderErrorMessage(Error.Network);
                return;
            }

            var result = await ReadJson<RSSFeed>(data.response);
            if (result is not null)
            {
                _writer.RenderBareText("RSSFeed was successfully edited:");
                RenderRSSFeed(result);
            }
        }

        public async Task ReadArticle(IList<ParameterResult> parameters)
        {
            string guid = parameters[0].String;
            var data = await _httpService.GetAsync(Endpoints.BuildUri(Endpoints.EndPoint.RSSFeedItems, guid));
            if (!data.success)
            {
                RenderErrorMessage(Error.Network);
                return;
            }

            if (HTTPService.GetContentType(data.response) != HTTPService.ContentType.AppJson)
            {
                RenderErrorMessage(Error.DataType);
                return;
            }
            var article = await ReadJson<RSSFeedItem>(data.response);
            if (article is null)
            {
                RenderErrorMessage(Error.InvalidJsonFormat);
                return;
            }

            try
            {
                string link = article.Id;
                if(!IsValidUrl(link))
                {
                    RenderInvalidUrlWarningMessage(link);
                    return;
                }

                Process.Start(new ProcessStartInfo
                {
                    FileName = link,
                    UseShellExecute = true
                });

            }
            catch (System.ComponentModel.Win32Exception noBrowser)
            {
                _writer.RenderErrorMessage(noBrowser.Message);
            }
            catch (Exception other)
            {
                _writer.RenderErrorMessage(other.Message);
            }
        }

        public async Task GetFeedItemsAsync(IList<ParameterResult> parameters)
        {
            string guid = parameters[0].String;

            int count = await RetrieveCount(Endpoints.EndPoint.RSSFeedItems, ("byRSSFeedGuid", guid));
            if (count == -1)
            {
                return;
            }
            if (_pages["RSSFeedItems"] > ComputeTotalPagesCount(count))
            {
                _pages["RSSFeedItems"] = 1;
            }

            var data = await _httpService.GetAsync(Endpoints.BuildUri(Endpoints.EndPoint.RSSFeedItems, new List<(string, string)>() { ("byRSSFeedGuid", guid), ("page", $"{_pages["RSSFeedItems"]}"), ("pageSize", $"{PageSize}") }));
            if (!data.success)
            {
                RenderErrorMessage(Error.Network);
                return;
            }

            if (HTTPService.GetContentType(data.response) != HTTPService.ContentType.AppJson)
            {
                RenderErrorMessage(Error.DataType);
                return;
            }

            var items = await ReadJson<List<RSSFeedItem>>(data.response);
            if (items is null)
            {
                return;
            }
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
            RSSFeedDTO feed = new(parameters[0].String, parameters[1].String, new Uri(parameters[2].String), parameters[3].Double);
            var data = await _httpService.PostAsync(Endpoints.BuildUri(Endpoints.EndPoint.RSSFeeds), JsonConvertor.ConvertObjectToJsonString(feed));
            if (!data.success)
            {
                RenderErrorMessage(Error.Network);
                return;
            }

            var result = await ReadJson<RSSFeed>(data.response);
            if (result is not null)
            {
                _writer.RenderBareText("New RSSFeed was registered:");
                RenderRSSFeed(result);
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

        private async Task<int> RetrieveCount(Endpoints.EndPoint endpoint, (string key, string value) queryString)
        {
            var count = await _httpService.GetAsync(Endpoints.BuildUri(endpoint, "count", queryString));
            if (!RetrieveCountInner(count))
            {
                return -1;
            }

            return await ReadJson<int>(count.response);
        }

        private async Task<int> RetrieveCount(Endpoints.EndPoint endpoint)
        {
            var count = await _httpService.GetAsync(Endpoints.BuildUri(endpoint, "count"));
            if (!RetrieveCountInner(count))
            {
                return -1;
            }

            return await ReadJson<int>(count.response);
        }

        private bool RetrieveCountInner((bool success, HttpResponseMessage response) count)
        {
            if (!count.success)
            {
                RenderErrorMessage(Error.Network);
                return false;
            }

            if (HTTPService.GetContentType(count.response) != HTTPService.ContentType.AppJson)
            {
                RenderErrorMessage(Error.DataType);
                return false;
            }

            return true;
        }

        private async Task<T?> ReadJson<T>(HttpResponseMessage response)
        {
            try
            {
                var data = await response.Content.ReadFromJsonAsync<T>();
                if (data is null)
                {
                    return default(T);
                }
                return data;
            }
            catch (JsonException)
            {
                RenderErrorMessage(Error.InvalidJsonFormat);
                return default(T);
            }
            catch (Exception)
            {
                RenderErrorMessage();
                return default(T);
            }
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
                           $"    Authors: {((item.Authors is null || item.Authors.Count == 0) ? "[]" : string.Join('\n', item.Authors))}\n" +
                           $"    Categories: {((item.Categories is null || item.Categories.Count == 0) ? "[]" : string.Join('\n', item.Categories))}\n");
        }

        private void RenderErrorMessage(Error error)
        {
            switch (error)
            {
                case Error.Network:
                    _writer.RenderErrorMessage($"Unable to send the request!");
                    break;
                case Error.DataType:
                    _writer.RenderErrorMessage("Wrong data type received!");
                    break;
                case Error.InvalidJsonFormat:
                    _writer.RenderErrorMessage("Invalid Json format!");
                    break;
                case Error.General:
                default:
                    _writer.RenderErrorMessage("Error occured!");
                    break;
            }
        }

        private void RenderErrorMessage()
        {
            RenderErrorMessage(Error.General);
        }

        private void RenderInvalidUrlWarningMessage(string link)
        {
            _writer.RenderWarningMessage($"Provided URL '{link}' does not seem to be valid URL and thus the link was refused to be opened in a browser.");
        }

        bool IsValidUrl(string url)
        {
            Uri? uriResult;
            return Uri.TryCreate(url, UriKind.Absolute, out uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }
    }
}
