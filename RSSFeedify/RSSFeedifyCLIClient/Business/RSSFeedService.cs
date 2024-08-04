using CommandParsonaut.Core.Types;
using CommandParsonaut.Interfaces;
using RSSFeedifyCLIClient.IO;
using RSSFeedifyCLIClient.Models;
using RSSFeedifyClientCore.Business.Errors;
using RSSFeedifyClientCore.Services;
using RSSFeedifyClientCore.Services.Networking;
using RSSFeedifyClientCore.Types;
using System.Diagnostics;

namespace RSSFeedifyCLIClient.Business
{
    public class RSSFeedService
    {
        private IWriter _writer;
        private readonly ApplicationErrorWriter _errorWriter;
        private HTTPService _httpService;
        private AccountService _accountService;

        private readonly int PageSize = 5;
        private IDictionary<string, int> _pages = new Dictionary<string, int>();
        private (ListingPage page, IList<ParameterResult> parameters) _currentListingPage = (ListingPage.None, []);

        private HttpResponseMessageValidator _httpResponseMessageValidatorJson = new HttpResponseMessageValidatorBuilder()
            .AddStatusCodeCheck(HTTPService.StatusCode.OK)
            .AddContentTypeCheck(HTTPService.ContentType.AppJson)
            .Build();

        private HttpResponseMessageValidator _httpResponseMessageValidatorCreate = new HttpResponseMessageValidatorBuilder()
            .AddStatusCodeCheck(HTTPService.StatusCode.Created)
            .AddContentTypeCheck(HTTPService.ContentType.AppJson)
            .Build();

        private HttpResponseMessageValidator _httpResponseMessageValidatorTxt = new HttpResponseMessageValidatorBuilder()
            .AddStatusCodeCheck(HTTPService.StatusCode.OK)
            .AddContentTypeCheck(HTTPService.ContentType.TxtPlain)
            .Build();

        private enum ListingPage
        {
            RSSFeeds,
            RSSFeedItems,
            None
        }

        public RSSFeedService(IWriter writer, ApplicationErrorWriter errorWriter, HTTPService hTTPService, AccountService accountService)
        {
            _writer = writer;
            _errorWriter = errorWriter;
            _httpService = hTTPService;
            _accountService = accountService;
            _pages["RSSFeeds"] = 1;
            _pages["RSSFeedItems"] = 1;
        }

        public async Task GetFeedsAsync()
        {
            var authResult = CreateAuthHeader();
            if (authResult.IsError)
            {
                _errorWriter.RenderErrorMessage(authResult.GetError);
                return;
            }
            var authenticationHeader = authResult.GetValue;

            var countResult = await RetrieveCount(Endpoints.EndPoint.RSSFeeds, authenticationHeader);
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

            var requestResult = await _httpService.GetAsync(Endpoints.BuildUri(Endpoints.EndPoint.RSSFeeds, new List<(string, string)>() { ("page", $"{_pages["RSSFeeds"]}"), ("pageSize", $"{PageSize}") }), authenticationHeader);
            if (requestResult.IsError)
            {
                _errorWriter.RenderErrorMessage(ApplicationError.NetworkGeneral, requestResult.GetError);
                return;
            }

            var response = requestResult.GetValue;

#if DEBUG
            // TODO: log
            Console.WriteLine(response);
            string responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine(responseContent);
#endif

            var validationResult = _httpResponseMessageValidatorJson.Validate(new HTTPService.HttpServiceResponseMessageMetaData(HTTPService.RetrieveContentType(response), HTTPService.RetrieveStatusCode(response)));
            if (validationResult.IsError)
            {
                _errorWriter.RenderErrorMessage(validationResult.GetError, await HTTPService.RetrieveAndStringifyContent(response));
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
            var authResult = CreateAuthHeader();
            if (authResult.IsError)
            {
                _errorWriter.RenderErrorMessage(authResult.GetError);
                return;
            }
            var authenticationHeader = authResult.GetValue;

            string guid = parameters[0].String;
            var requestResult = await _httpService.DeleteAsync(Endpoints.BuildUri(Endpoints.EndPoint.RSSFeeds, guid), authenticationHeader);
            if (requestResult.IsError)
            {
                _errorWriter.RenderErrorMessage(ApplicationError.NetworkGeneral, requestResult.GetError);
                return;
            }

            var response = requestResult.GetValue;

#if DEBUG
            // TODO: log
            Console.WriteLine(response);
            string responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine(responseContent);
#endif

            var validationResult = _httpResponseMessageValidatorJson.Validate(new HTTPService.HttpServiceResponseMessageMetaData(HTTPService.RetrieveContentType(response), HTTPService.RetrieveStatusCode(response)));
            if (validationResult.IsError)
            {
                _errorWriter.RenderErrorMessage(validationResult.GetError, await HTTPService.RetrieveAndStringifyContent(response));
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
            var authResult = CreateAuthHeader();
            if (authResult.IsError)
            {
                _errorWriter.RenderErrorMessage(authResult.GetError);
                return;
            }
            var authenticationHeader = authResult.GetValue;

            string guid = parameters[0].String;
            var requestResult = await _httpService.GetAsync(Endpoints.BuildUri(Endpoints.EndPoint.RSSFeeds, guid), authenticationHeader);
            if (requestResult.IsError)
            {
                _errorWriter.RenderErrorMessage(ApplicationError.NetworkGeneral, requestResult.GetError);
                return;
            }

            var response = requestResult.GetValue;

#if DEBUG
            // TODO: log
            Console.WriteLine(response);
            string responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine(responseContent);
#endif

            var validationResult = _httpResponseMessageValidatorJson.Validate(new HTTPService.HttpServiceResponseMessageMetaData(HTTPService.RetrieveContentType(response), HTTPService.RetrieveStatusCode(response)));
            if (validationResult.IsError)
            {
                _errorWriter.RenderErrorMessage(validationResult.GetError, await HTTPService.RetrieveAndStringifyContent(response));
                return;
            }

            var result = await JsonFromHttpResponseReader.ReadJson<RSSFeed>(response);
            if (result.IsError)
            {
                _errorWriter.RenderErrorMessage(result.GetError);
                return;
            }

            var originalFeed = result.GetValue;

            RSSFeedDTO feed = new(parameters[1].String, parameters[2].String, originalFeed.SourceUrl, parameters[3].Double);
            requestResult = await _httpService.PutAsync(Endpoints.BuildUri(Endpoints.EndPoint.RSSFeeds, guid), JsonConvertor.ConvertObjectToJsonString(feed), authenticationHeader);
            if (requestResult.IsError)
            {
                _errorWriter.RenderErrorMessage(ApplicationError.NetworkGeneral, requestResult.GetError);
                return;
            }

            response = requestResult.GetValue;

#if DEBUG
            // TODO: log
            Console.WriteLine(response);
            responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine(responseContent);
#endif

            validationResult = _httpResponseMessageValidatorJson.Validate(new HTTPService.HttpServiceResponseMessageMetaData(HTTPService.RetrieveContentType(response), HTTPService.RetrieveStatusCode(response)));
            if (validationResult.IsError)
            {
                _errorWriter.RenderErrorMessage(validationResult.GetError, await HTTPService.RetrieveAndStringifyContent(response));
                return;
            }

            result = await JsonFromHttpResponseReader.ReadJson<RSSFeed>(response);
            if (result.IsSuccess)
            {
                _writer.RenderBareText("RSSFeed was successfully edited:");
                RenderRSSFeed(result.GetValue);
            }
        }

        public async Task ReadArticle(IList<ParameterResult> parameters)
        {
            var authResult = CreateAuthHeader();
            if (authResult.IsError)
            {
                _errorWriter.RenderErrorMessage(authResult.GetError);
                return;
            }
            var authenticationHeader = authResult.GetValue;

            string guid = parameters[0].String;
            var requestResult = await _httpService.GetAsync(Endpoints.BuildUri(Endpoints.EndPoint.RSSFeedItems, guid), authenticationHeader);
            if (requestResult.IsError)
            {
                _errorWriter.RenderErrorMessage(ApplicationError.NetworkGeneral, requestResult.GetError);
                return;
            }

            var response = requestResult.GetValue;

#if DEBUG
            // TODO: log
            Console.WriteLine(response);
            string responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine(responseContent);
#endif

            var validationResult = _httpResponseMessageValidatorJson.Validate(new HTTPService.HttpServiceResponseMessageMetaData(HTTPService.RetrieveContentType(response), HTTPService.RetrieveStatusCode(response)));
            if (validationResult.IsError)
            {
                _errorWriter.RenderErrorMessage(validationResult.GetError, await HTTPService.RetrieveAndStringifyContent(response));
                return;
            }

            var jsonResult = await JsonFromHttpResponseReader.ReadJson<RSSFeedItem>(response);
            if (jsonResult.IsError)
            {
                _errorWriter.RenderErrorMessage(jsonResult.GetError);
                return;
            }

            var article = jsonResult.GetValue;
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
            var authResult = CreateAuthHeader();
            if (authResult.IsError)
            {
                _errorWriter.RenderErrorMessage(authResult.GetError);
                return;
            }
            var authenticationHeader = authResult.GetValue;

            string guid = parameters[0].String;
            var countResult = await RetrieveCount(Endpoints.EndPoint.RSSFeedItems, ("byRSSFeedGuid", guid), authenticationHeader);
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

            var requestResult = await _httpService.GetAsync(Endpoints.BuildUri(Endpoints.EndPoint.RSSFeedItems, new List<(string, string)>() { ("byRSSFeedGuid", guid), ("page", $"{_pages["RSSFeedItems"]}"), ("pageSize", $"{PageSize}") }), authenticationHeader);
            if (requestResult.IsError)
            {
                _errorWriter.RenderErrorMessage(ApplicationError.NetworkGeneral, requestResult.GetError);
                return;
            }

            var response = requestResult.GetValue;

#if DEBUG
            // TODO: log
            Console.WriteLine(response);
            string responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine(responseContent);
#endif

            var validationResult = _httpResponseMessageValidatorJson.Validate(new HTTPService.HttpServiceResponseMessageMetaData(HTTPService.RetrieveContentType(response), HTTPService.RetrieveStatusCode(response)));
            if (validationResult.IsError)
            {
                _errorWriter.RenderErrorMessage(validationResult.GetError, await HTTPService.RetrieveAndStringifyContent(response));
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
            var authResult = CreateAuthHeader();
            if (authResult.IsError)
            {
                _errorWriter.RenderErrorMessage(authResult.GetError);
                return;
            }
            var authenticationHeader = authResult.GetValue;

            RSSFeedDTO feed = new(parameters[0].String, parameters[1].String, parameters[2].Uri, parameters[3].Double);
            var requestResult = await _httpService.PostAsync(Endpoints.BuildUri(Endpoints.EndPoint.RSSFeeds), JsonConvertor.ConvertObjectToJsonString(feed), authenticationHeader);
            if (requestResult.IsError)
            {
                _errorWriter.RenderErrorMessage(ApplicationError.NetworkGeneral, requestResult.GetError);
                return;
            }

            var response = requestResult.GetValue;

#if DEBUG
            // TODO: log
            Console.WriteLine(response);
            string responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine(responseContent);
#endif

            var validationResult = _httpResponseMessageValidatorCreate.Validate(new HTTPService.HttpServiceResponseMessageMetaData(HTTPService.RetrieveContentType(response), HTTPService.RetrieveStatusCode(response)));
            if (validationResult.IsError)
            {
                _errorWriter.RenderErrorMessage(validationResult.GetError, await HTTPService.RetrieveAndStringifyContent(response));
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
            _writer.RenderBareText($"\t>KEY: 'http-host-port'\t\tVALUE: '{Endpoints.BaseUrl}'");
        }

        private Result<IAuthenticationHeader, ApplicationError> CreateAuthHeader()
        {
            var accessToken = _accountService.User.GetAccessToken();
            if (accessToken.IsError)
            {
                return Result.Error<IAuthenticationHeader, ApplicationError>(accessToken.GetError);
            }

            AuthenticationType authenticationType = new BearerToken(accessToken.GetValue);
            IAuthenticationHeader authenticationHeader = new AuthenticationHeader(authenticationType);
            return Result.Ok<IAuthenticationHeader, ApplicationError>(authenticationHeader);
        }

        private int ComputeTotalPagesCount(int count)
        {
            return (int)Math.Ceiling(count / (double)PageSize);
        }

        private async Task<Result<int, DetailedApplicationError>> RetrieveCount(Endpoints.EndPoint endpoint, (string key, string value) queryString, IAuthenticationHeader authenticationHeader)
        {
            var count = await _httpService.GetAsync(Endpoints.BuildUri(endpoint, "count", queryString), authenticationHeader);
            return await ParseRetrievedCountResponse(count);
        }

        private async Task<Result<int, DetailedApplicationError>> RetrieveCount(Endpoints.EndPoint endpoint, IAuthenticationHeader authenticationHeader)
        {
            var count = await _httpService.GetAsync(Endpoints.BuildUri(endpoint, "count"), authenticationHeader);
            return await ParseRetrievedCountResponse(count);
        }

        private async Task<Result<int, DetailedApplicationError>> ParseRetrievedCountResponse(Result<HttpResponseMessage, string> count)
        {
            if (count.IsError)
            {
                return Result.Error<int, DetailedApplicationError>(new(ApplicationError.NetworkGeneral, count.GetError));
            }
            var response = count.GetValue;

#if DEBUG
            // TODO: log
            Console.WriteLine(response);
            string responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine(responseContent);
#endif

            var validationResult = _httpResponseMessageValidatorTxt.Validate(new HTTPService.HttpServiceResponseMessageMetaData(HTTPService.RetrieveContentType(response), HTTPService.RetrieveStatusCode(response)));
            if (validationResult.IsError)
            {
                var error = new DetailedApplicationError(validationResult.GetError.Error, await HTTPService.RetrieveAndStringifyContent(response));
                return Result.Error<int, DetailedApplicationError>(error);
            }

            var result = await JsonFromHttpResponseReader.ReadJson<int>(response);
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
