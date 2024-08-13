using RSSFeedify.Models;
using RSSFeedify.Repository;
using RSSFeedify.Repository.Types;
using RSSFeedify.Services.DataTypeConvertors;
using RSSFeedify.Types;
using System.ServiceModel.Syndication;
using System.Xml;

namespace RSSFeedify.Services
{
    public sealed class RSSFeedPollingService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly TimeSpan _pollingInterval = TimeSpan.FromMinutes(10);
        private readonly ILogger<RSSFeedPollingService> _logger;

        public struct HashedRSSFeedItemDTO
        {
            public string Hash;
            public RSSFeedItemDTO Item;

            public HashedRSSFeedItemDTO(string hash, RSSFeedItemDTO item)
            {
                Hash = hash;
                Item = item;
            }
        }

        public RSSFeedPollingService(IServiceScopeFactory scopeFactory, ILogger<RSSFeedPollingService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            bool firstRun = true;

            while (!stoppingToken.IsCancellationRequested)
            {
                if (!firstRun)
                {
                    await Task.Delay(_pollingInterval, stoppingToken);
                }

                firstRun = false;
                using (var scope = _scopeFactory.CreateScope())
                {
                    _logger.LogInformation("Service wakes up.");

                    var rSSRepository = scope.ServiceProvider.GetRequiredService<IRSSFeedRepository>();
                    var rSSItemsRepository = scope.ServiceProvider.GetRequiredService<IRSSFeedItemRepository>();

                    var feeds = await rSSRepository.GetAsync();
                    if (feeds is not Success<IEnumerable<RSSFeed>>)
                    {
                        _logger.LogError("Unable to get access to RSSFeeds repository!");
                        continue;
                    }

                    foreach (var feed in feeds.Data)
                    {
                        if (!CheckTimeStampsForUpdate(feed))
                        {
                            _logger.LogInformation("Feed '{Guid}' won't be updated.", feed.Guid);
                            continue;
                        }
                        _logger.LogInformation("Feed '{Guid}' will be updated.", feed.Guid);

                        var result = LoadRSSFeedItemsFromUri(feed.SourceUrl);
                        if (result.IsError)
                        {
                            _logger.LogWarning("Poll from '{Url}' was not successfull. Only 'LastPoll' timestamp will be updated. Failure details: '{Details}'", feed.SourceUrl, result.GetError);
                            await rSSRepository.UpdatePollingTimeAsync(feed.Guid, successfullPolling: false);
                            continue;
                        }

                        if (feed.LastPoll > result.GetValue.lastUpdate)
                        {
                            _logger.LogInformation("Nothing new to poll from '{Url}'. Only timestamps will be updated.", feed.SourceUrl);
                            await rSSRepository.UpdatePollingTimeAsync(feed.Guid, successfullPolling: true);
                            continue;
                        }

                        var originalItems = await rSSItemsRepository.GetFilteredByForeignKeyAsync(feed.Guid);
                        if (originalItems is not Success<IEnumerable<RSSFeedItem>>)
                        {
                            _logger.LogError("Unable to get access to RSSFeedsItems repository for feed '{Guid}'!", feed.Guid);
                            await rSSRepository.UpdatePollingTimeAsync(feed.Guid, successfullPolling: false);
                            continue;
                        }

                        UpdateRSSFeedItems(rSSItemsRepository, feed, result.GetValue.items, originalItems);
                        await rSSRepository.UpdatePollingTimeAsync(feed.Guid, successfullPolling: true);
                        _logger.LogInformation("Feed '{Guid}' was succesfully updated from '{Url}'.", feed.Guid, feed.SourceUrl);
                    }
                }
            }
        }

        private void UpdateRSSFeedItems(IRSSFeedItemRepository rSSItemsRepository, RSSFeed feed, List<HashedRSSFeedItemDTO> items, RepositoryResult<IEnumerable<RSSFeedItem>> originalItems)
        {
            foreach (var newFeedItem in items)
            {
                bool isUnique = true;
                foreach (var feedItem in originalItems.Data)
                {
                    if (newFeedItem.Hash == feedItem.Hash)
                    {
                        isUnique = false;
                        break;
                    }
                }

                if (isUnique)
                {
                    var newRSSFeedItem = RSSFeedItemDTOToRssFeedItem.Convert(newFeedItem.Item, newFeedItem.Hash);
                    newRSSFeedItem.RSSFeedId = feed.Guid;
                    _ = rSSItemsRepository.InsertAsync(newRSSFeedItem);
                    _logger.LogInformation("New item '{Title}' from feed '{Guid}' will be inserted into database.", newRSSFeedItem.Title, feed.Guid);
                }
            }
        }

        private bool CheckTimeStampsForUpdate(RSSFeed feed)
        {
            _logger.LogDebug("Checking timestamps of '{Guid}'; 'LastPoll': {LastPoll}, 'LastSuccessfullPoll': {LastSuccessfullPoll}, 'PollingInterval': {PollingInterval}.", feed.Guid, feed.LastPoll, feed.LastSuccessfullPoll, feed.PollingInterval);

            bool readyToUpdate = false;
            if (feed.LastPoll == DateTime.MinValue && feed.LastSuccessfullPoll == DateTime.MinValue || feed.LastSuccessfullPoll != feed.LastPoll)
            {
                readyToUpdate = true;
                if (feed.LastPoll == DateTime.MinValue && feed.LastSuccessfullPoll == DateTime.MinValue)
                {
                    _logger.LogDebug("Polling times for feed '{Guid}' were not ititialized.", feed.Guid);
                }
                else
                {
                    _logger.LogDebug("Polling times for feed '{Guid}' are not equal. The last poll was not successfull.", feed.Guid);
                }
            }
            else if (DateTime.UtcNow - feed.LastSuccessfullPoll >= TimeSpan.FromMinutes(feed.PollingInterval))
            {
                readyToUpdate = true;
                _logger.LogDebug("Last polling time for feed '{Guid}' is greater than the polling interval. See {TimeElapsed} >= {PollingInterval}.", feed.Guid, (int)(DateTime.UtcNow - feed.LastSuccessfullPoll).TotalMinutes, (int)TimeSpan.FromMinutes(feed.PollingInterval).TotalMinutes);
            }

            return readyToUpdate;
        }

        public static string GenerateRSSFeedItemHash(string title, DateTime publishDate)
        {
            return HashingService.GetHash($"{title},{publishDate}");
        }

        public static Result<(DateTime lastUpdate, List<HashedRSSFeedItemDTO> items), string> LoadRSSFeedItemsFromUri(Uri uri)
        {
            XmlReader? reader = null;
            SyndicationFeed feed;
            try
            {
                reader = XmlReader.Create(uri.ToString());
                feed = SyndicationFeed.Load(reader);
            }
            catch (Exception e)
            {
                return Result.Error<(DateTime lastUpdate, List<HashedRSSFeedItemDTO> items), string>(e.Message);
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
            }

            List<HashedRSSFeedItemDTO> items = [];
            foreach (SyndicationItem item in feed.Items)
            {
                RSSFeedItemDTO newItem = new RSSFeedItemDTO
                {
                    Title = item.Title.Text,
                    Summary = (item.Summary == null) ? "" : item.Summary.Text,
                    PublishDate = DateTime.SpecifyKind(item.PublishDate.DateTime, DateTimeKind.Utc),
                    Links = item.Links.Select(link => link.Uri).ToList(),
                    Categories = item.Categories.Select(category => category.Name).ToList(),
                    Authors = item.Authors.Select(author => author.Name).ToList(),
                    Contributors = item.Contributors.Select(contributor => contributor.Name).ToList(),
                    Content = ((item.Content is null) ? "" : (item.Content?.ToString())),
                    Id = item.Id
                };

                string hash = GenerateRSSFeedItemHash(newItem.Title, newItem.PublishDate);
                items.Add(new(hash, newItem));
            }

            return Result.Ok<(DateTime lastUpdate, List<HashedRSSFeedItemDTO> items), string>((feed.LastUpdatedTime.DateTime, items));
        }
    }
}
