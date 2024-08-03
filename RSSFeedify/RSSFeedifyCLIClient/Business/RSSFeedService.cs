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

            var requestResult = await _httpService.GetAsync(Endpoints.BuildUri(Endpoints.EndPoint.RSSFeeds, new List<(string, string)>() { ("page", $"{_pages["RSSFeeds"]}"), ("pageSize", $"{PageSize}") }));
            if (!requestResult.IsError)
            {
                _errorWriter.RenderErrorMessage(ApplicationError.Network, requestResult.GetError);
                return;
            }

            var response = requestResult.GetValue;
            if (HTTPService.RetrieveContentType(response) != HTTPService.ContentType.AppJson)
            {
                _errorWriter.RenderErrorMessage(ApplicationError.DataType);
                return;
            }

            var result = await JsonFromHttpResponseReader.ReadJson<List<RSSFeed>>(response);
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
            var requestResult = await _httpService.DeleteAsync(Endpoints.BuildUri(Endpoints.EndPoint.RSSFeeds, guid));
            if (requestResult.IsError)
            {
                _errorWriter.RenderErrorMessage(ApplicationError.Network, requestResult.GetError);
                return;
            }

            var response = requestResult.GetValue;
            if (HTTPService.RetrieveContentType(response) != HTTPService.ContentType.AppJson)
            {
                _errorWriter.RenderErrorMessage(ApplicationError.DataType);
                return;
            }

            var result = await JsonFromHttpResponseReader.ReadJson<RSSFeed>(response);
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

            var requestResult = await _httpService.GetAsync(Endpoints.BuildUri(Endpoints.EndPoint.RSSFeeds, guid));
            if (requestResult.IsError)
            {
                _errorWriter.RenderErrorMessage(ApplicationError.Network, requestResult.GetError);
                return;
            }

            var response = requestResult.GetValue;
            var result = await JsonFromHttpResponseReader.ReadJson<RSSFeed>(response);
            if (result.IsError)
            {
                _errorWriter.RenderErrorMessage(result.GetError);
                return;
            }

            var originalFeed = result.GetValue;

            RSSFeedDTO feed = new(parameters[1].String, parameters[2].String, originalFeed.SourceUrl, parameters[3].Double);
            requestResult = await _httpService.PutAsync(Endpoints.BuildUri(Endpoints.EndPoint.RSSFeeds, guid), JsonConvertor.ConvertObjectToJsonString(feed));
            if (requestResult.IsError)
            {
                _errorWriter.RenderErrorMessage(ApplicationError.Network, requestResult.GetError);
                return;
            }

            response = requestResult.GetValue;
            result = await JsonFromHttpResponseReader.ReadJson<RSSFeed>(response);
            if (result.IsSuccess)
            {
                _writer.RenderBareText("RSSFeed was successfully edited:");
                RenderRSSFeed(result.GetValue);
            }
        }

        public async Task ReadArticle(IList<ParameterResult> parameters)
        {
            AuthenticationType authenticationType = new BearerToken("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJqdGkiOiJkZDNiYTljOS1kYmE2LTRmNWMtODkwNC02MDc4MjM2OGM2Y2UiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjMxMTMyZTY4LTYxZTQtNGI1YS1iZTQyLTMzNmVmZDVhNDI3MyIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IlJlZ3VsYXJVc2VyIiwiZXhwIjoxNzIyNjg4MjI4LCJpc3MiOiJodHRwczovL2dpdGh1Yi5jb20va2VycmFtYml0L1JTU0ZlZWRpZnkiLCJhdWQiOiJSU1NGZWVkaWZ5IEFQSSJ9.3Psy4kkyaGNa4Dn2U51ywQJMaWRymzpnl9E6VY_4su0");
            IAuthenticationHeader authenticationHeader = new AuthenticationHeader(authenticationType);

            string guid = parameters[0].String;
            guid = "2995bf42-b3e9-4981-b54a-914bec4d0b60"; // TEMP
            var requestResult = await _httpService.GetAsync(Endpoints.BuildUri(Endpoints.EndPoint.RSSFeedItems, guid), authenticationHeader);
            if (requestResult.IsError)
            {
                _errorWriter.RenderErrorMessage(ApplicationError.Network, requestResult.GetError);
                return;
            }

            var response = requestResult.GetValue;
            Console.WriteLine(response);
            string responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine(responseContent);
            if (HTTPService.RetrieveContentType(response) != HTTPService.ContentType.AppJson)
            {
                _errorWriter.RenderErrorMessage(ApplicationError.DataType);
                return;
            }
            var result = await JsonFromHttpResponseReader.ReadJson<RSSFeedItem>(response);
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

            var requestResult = await _httpService.GetAsync(Endpoints.BuildUri(Endpoints.EndPoint.RSSFeedItems, new List<(string, string)>() { ("byRSSFeedGuid", guid), ("page", $"{_pages["RSSFeedItems"]}"), ("pageSize", $"{PageSize}") }));
            if (requestResult.IsError)
            {
                _errorWriter.RenderErrorMessage(ApplicationError.Network, requestResult.GetError);
                return;
            }

            var response = requestResult.GetValue;
            if (HTTPService.RetrieveContentType(response) != HTTPService.ContentType.AppJson)
            {
                _errorWriter.RenderErrorMessage(ApplicationError.DataType);
                return;
            }

            var result = await JsonFromHttpResponseReader.ReadJson<List<RSSFeedItem>>(response);
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
            var requestResult = await _httpService.PostAsync(Endpoints.BuildUri(Endpoints.EndPoint.RSSFeeds), JsonConvertor.ConvertObjectToJsonString(feed));
            if (requestResult.IsError)
            {
                _errorWriter.RenderErrorMessage(ApplicationError.Network, requestResult.GetError);
                return;
            }

            var response = requestResult.GetValue;
            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                _errorWriter.RenderErrorMessage(await response.Content.ReadAsStringAsync());
                return;
            }

            var result = await JsonFromHttpResponseReader.ReadJson<RSSFeed>(response);
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

        private async Task<Result<int, DetailedApplicationError>> RetrieveCount(Endpoints.EndPoint endpoint, (string key, string value) queryString)
        {
            var count = await _httpService.GetAsync(Endpoints.BuildUri(endpoint, "count", queryString));
            return await ParseRetrievedCountResponse(count);
        }

        private async Task<Result<int, DetailedApplicationError>> RetrieveCount(Endpoints.EndPoint endpoint)
        {
            var count = await _httpService.GetAsync(Endpoints.BuildUri(endpoint, "count"));
            return await ParseRetrievedCountResponse(count);
        }

        private async Task<Result<int, DetailedApplicationError>> ParseRetrievedCountResponse(Result<HttpResponseMessage, string> count)
        {
            if (count.IsError)
            {
                return Result.Error<int, DetailedApplicationError>(new(ApplicationError.Network, count.GetError));
            }

            if (HTTPService.RetrieveContentType(count.GetValue) != HTTPService.ContentType.AppJson)
            {
                return Result.Error<int, DetailedApplicationError>(new(ApplicationError.DataType, string.Empty));
            }

            var result = await JsonFromHttpResponseReader.ReadJson<int>(count.GetValue);
            if (result.IsError)
            {
                return Result.Error<int, DetailedApplicationError>(result.GetError.ConvertToDetailedApplicationError());
            }

            return Result.Ok<int, DetailedApplicationError>(result.GetValue);
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
