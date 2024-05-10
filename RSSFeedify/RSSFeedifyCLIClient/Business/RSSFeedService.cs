﻿using CommandParsonaut.Core.Types;
using CommandParsonaut.Interfaces;
using RSSFeedifyCLIClient.Models;
using RSSFeedifyCLIClient.Services;
using RSSFeedifyCLIClient.Settings;
using System.Net.Http.Json;
using System.Text.Json;

namespace RSSFeedifyCLIClient.Business
{
    public class RSSFeedService
    {
        private IWriter _writer;
        private HTTPService _httpService;

        private enum Error
        {
            General,
            Network,
            DataType,
            InvalidJsonFormat
        }

        public RSSFeedService(IWriter writer, HTTPService hTTPService)
        {
            _writer = writer;
            _httpService = hTTPService;
        }

        public async Task GetFeedsAsync()
        {
            var data = await _httpService.Get(Endpoints.BuildUri(Endpoints.EndPoint.RSSFeeds));
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
                return;
            }
            foreach (var feed in feeds)
            {
                RenderRSSFeed(feed);
            }
        }

        public async Task GetFeedItemsAsync(IList<ParameterResult> parameters)
        {
            string guid = parameters[0].String;
            var data = await _httpService.Get(Endpoints.BuildUri(Endpoints.EndPoint.RSSFeedItems, guid));
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
        }

        public async Task AddNewFeed(IList<ParameterResult> parameters)
        {
            RSSFeedDTO feed = new(parameters[0].String, parameters[1].String, new Uri(parameters[2].String), parameters[3].Double);
            var data = await _httpService.Post(Endpoints.BuildUri(Endpoints.EndPoint.RSSFeeds), JsonConvertor.ConvertObjectToJsonString(feed));
            if (!data.success)
            {
                RenderErrorMessage(Error.Network);
                return;
            }

            _writer.RenderBareText("\nData were sent, new RSSFeed was registered:");
            await ParseRSSFeedInResponse(data.response);
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

        private async Task ParseRSSFeedInResponse(HttpResponseMessage response)
        {
            try
            {
                var feed = await response.Content.ReadFromJsonAsync<RSSFeed>();
                RenderRSSFeed(feed);
            }
            catch (JsonException)
            {
                RenderErrorMessage(Error.InvalidJsonFormat);
            }
            catch (Exception)
            {
                RenderErrorMessage();
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
                           $"    Authors: {((item.Authors.Count == 0) ? "[]" : string.Join('\n', item.Authors))}\n" +
                           $"    Categories: {((item.Categories.Count == 0) ? "[]" : string.Join('\n', item.Categories))}\n");
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
    }
}
