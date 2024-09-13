using ClientNetLib.Business.Errors;
using ClientNetLib.Services.Json;
using ClientNetLib.Services.Networking;
using CommandParsonaut.Core.Types;
using CommandParsonaut.Interfaces;
using RSSFeedifyCLIClient.IO;
using RSSFeedifyCommon.Models;
using RSSFeedifyCommon.Types;
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

        private UriResourceCreator _uriResourceCreator;

        private enum ListingPage
        {
            RSSFeeds,
            RSSFeedItems,
            None
        }

        public RSSFeedService(IWriter writer, ApplicationErrorWriter errorWriter, HTTPService hTTPService, AccountService accountService, Uri baseUri)
        {
            _writer = writer;
            _errorWriter = errorWriter;
            _httpService = hTTPService;
            _accountService = accountService;
            _uriResourceCreator = new(baseUri);

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

            var countResult = await RetrieveCount(EndPoint.RSSFeeds, authenticationHeader);
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

            var requestResult = await _httpService.GetAsync(_uriResourceCreator.BuildUri(EndPoint.RSSFeeds.ConvertToString(), new List<(string, string)>() { ("page", $"{_pages["RSSFeeds"]}"), ("pageSize", $"{PageSize}") }), authenticationHeader);
            if (requestResult.IsError)
            {
                _errorWriter.RenderErrorMessage(new ApplicationError(new DetailedError(Error.NetworkGeneral, requestResult.GetError)));
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
                _errorWriter.RenderErrorMessage(new ApplicationError(validationResult.GetError), await HTTPService.RetrieveAndStringifyContent(response));
                return;
            }

            var result = await JsonFromHttpResponseReader.ReadJson<List<RSSFeed>>(response);
            if (result.IsError)
            {
                _errorWriter.RenderErrorMessage(new ApplicationError(result.GetError));
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
            var requestResult = await _httpService.DeleteAsync(_uriResourceCreator.BuildUri(EndPoint.RSSFeeds.ConvertToString(), guid), authenticationHeader);
            if (requestResult.IsError)
            {
                _errorWriter.RenderErrorMessage(new ApplicationError(new DetailedError(Error.NetworkGeneral, requestResult.GetError)));
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
                _errorWriter.RenderErrorMessage(new ApplicationError(validationResult.GetError), await HTTPService.RetrieveAndStringifyContent(response));
                return;
            }

            var result = await JsonFromHttpResponseReader.ReadJson<RSSFeed>(response);
            if (result.IsError)
            {
                _errorWriter.RenderErrorMessage(new ApplicationError(result.GetError));
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
            var requestResult = await _httpService.GetAsync(_uriResourceCreator.BuildUri(EndPoint.RSSFeeds.ConvertToString(), guid), authenticationHeader);
            if (requestResult.IsError)
            {
                _errorWriter.RenderErrorMessage(new ApplicationError(new DetailedError(Error.NetworkGeneral, requestResult.GetError)));
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
                _errorWriter.RenderErrorMessage(new ApplicationError(validationResult.GetError), await HTTPService.RetrieveAndStringifyContent(response));
                return;
            }

            var result = await JsonFromHttpResponseReader.ReadJson<RSSFeed>(response);
            if (result.IsError)
            {
                _errorWriter.RenderErrorMessage(new ApplicationError(result.GetError));
                return;
            }

            var originalFeed = result.GetValue;

            RSSFeedDTO feed = new(parameters[1].String, parameters[2].String, originalFeed.SourceUrl, parameters[3].Double);
            requestResult = await _httpService.PutAsync(_uriResourceCreator.BuildUri(EndPoint.RSSFeeds.ConvertToString(), guid), JsonConvertor.ConvertObjectToJsonString(feed), authenticationHeader);
            if (requestResult.IsError)
            {
                _errorWriter.RenderErrorMessage(new ApplicationError(new DetailedError(Error.NetworkGeneral, requestResult.GetError)));
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
                _errorWriter.RenderErrorMessage(new ApplicationError(validationResult.GetError), await HTTPService.RetrieveAndStringifyContent(response));
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
            var requestResult = await _httpService.GetAsync(_uriResourceCreator.BuildUri(EndPoint.RSSFeedItems.ConvertToString(), guid), authenticationHeader);
            if (requestResult.IsError)
            {
                _errorWriter.RenderErrorMessage(new ApplicationError(new DetailedError(Error.NetworkGeneral, requestResult.GetError)));
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
                _errorWriter.RenderErrorMessage(new ApplicationError(validationResult.GetError), await HTTPService.RetrieveAndStringifyContent(response));
                return;
            }

            var jsonResult = await JsonFromHttpResponseReader.ReadJson<RSSFeedItem>(response);
            if (jsonResult.IsError)
            {
                _errorWriter.RenderErrorMessage(new ApplicationError(jsonResult.GetError));
                return;
            }

            var article = jsonResult.GetValue;
            try
            {
                string link = article.Id;
                if (!IsValidUrl(link))
                {
                    _errorWriter.RenderErrorMessage(new ApplicationError(Errors.Error.InvalidUrl, $"Provided invalid URL: '{link}'"));
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
            var countResult = await RetrieveCount(EndPoint.RSSFeedItems, ("byRSSFeedGuid", guid), authenticationHeader);
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

            var requestResult = await _httpService.GetAsync(_uriResourceCreator.BuildUri(EndPoint.RSSFeedItems.ConvertToString(), new List<(string, string)>() { ("byRSSFeedGuid", guid), ("page", $"{_pages["RSSFeedItems"]}"), ("pageSize", $"{PageSize}") }), authenticationHeader);
            if (requestResult.IsError)
            {
                _errorWriter.RenderErrorMessage(new ApplicationError(new DetailedError(Error.NetworkGeneral, requestResult.GetError)));
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
                _errorWriter.RenderErrorMessage(new ApplicationError(validationResult.GetError), await HTTPService.RetrieveAndStringifyContent(response));
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
            var requestResult = await _httpService.PostAsync(_uriResourceCreator.BuildUri(EndPoint.RSSFeeds.ConvertToString()), JsonConvertor.ConvertObjectToJsonString(feed), authenticationHeader);
            if (requestResult.IsError)
            {
                _errorWriter.RenderErrorMessage(new ApplicationError(new DetailedError(Error.NetworkGeneral, requestResult.GetError)));
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
                _errorWriter.RenderErrorMessage(new ApplicationError(validationResult.GetError), await HTTPService.RetrieveAndStringifyContent(response));
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
            _writer.RenderBareText($"\t>KEY: 'http-host-port'\t\tVALUE: '{_uriResourceCreator.BaseUrl}'");
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

        private async Task<Result<int, ApplicationError>> RetrieveCount(EndPoint endpoint, (string key, string value) queryString, IAuthenticationHeader authenticationHeader)
        {
            var count = await _httpService.GetAsync(_uriResourceCreator.BuildUri(endpoint.ConvertToString(), "count", queryString), authenticationHeader);
            return await ParseRetrievedCountResponse(count);
        }

        private async Task<Result<int, ApplicationError>> RetrieveCount(EndPoint endpoint, IAuthenticationHeader authenticationHeader)
        {
            var count = await _httpService.GetAsync(_uriResourceCreator.BuildUri(endpoint.ConvertToString(), "count"), authenticationHeader);
            return await ParseRetrievedCountResponse(count);
        }

        private async Task<Result<int, ApplicationError>> ParseRetrievedCountResponse(ClientNetLib.Types.Result<HttpResponseMessage, string> count)
        {
            if (count.IsError)
            {
                return Result.Error<int, ApplicationError>(new ApplicationError(new DetailedError(Error.NetworkGeneral, count.GetError)));
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
                var error = new DetailedError(validationResult.GetError.Error, await HTTPService.RetrieveAndStringifyContent(response));
                return Result.Error<int, ApplicationError>(new(error));
            }

            var result = await JsonFromHttpResponseReader.ReadJson<int>(response);
            if (result.IsError)
            {
                return Result.Error<int, ApplicationError>(new(result.GetError.ConvertToDetailedApplicationError()));
            }

            return Result.Ok<int, ApplicationError>(result.GetValue);
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
