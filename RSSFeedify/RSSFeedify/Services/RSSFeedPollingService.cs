using RSSFeedify.Models;
using RSSFeedify.Repository;
using RSSFeedify.Repository.Types;
using RSSFeedify.Services.DataTypeConvertors;
using System.ServiceModel.Syndication;
using System.Xml;

namespace RSSFeedify.Services
{
    public sealed class RSSFeedPollingService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(20);

        public RSSFeedPollingService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(_pollingInterval, stoppingToken);
                using (var scope = _scopeFactory.CreateScope())
                {
                    Console.WriteLine("[RSSFeedPollingService]: service wakes up...");

                    var rSSRepository = scope.ServiceProvider.GetRequiredService<IRSSFeedRepository>();
                    var rSSItemsRepository = scope.ServiceProvider.GetRequiredService<IRSSFeedItemRepository>();

                    var feeds = await rSSRepository.GetAsync();
                    if (feeds is not Success<IEnumerable<RSSFeed>>)
                    {
                        Console.WriteLine("[RSSFeedPollingService]: failure. Unable to get access to RSSFeeds!");

                    } else
                    {
                        foreach (var feed in feeds.Data)
                        {
                            if (!CheckTimeStampsForUpdate(feed))
                            {
                                Console.WriteLine($"[RSSFeedPollingService]: feed must be updated: {false}");
                                continue;
                            }

                            Console.WriteLine($"[RSSFeedPollingService]: feed must be updated: {true}");
                            (bool success, DateTime lastUpdate, List<(string hash, RSSFeedItemDTO dto)> items) = LoadRSSFeedItemsFromUri(feed.SourceUrl);
                            if (!success)
                            {
                                Console.WriteLine($"[RSSFeedPollingService]: poll was not successfull so only LastPoll time stamp will be updated.");
                                await rSSRepository.UpdatePollingTimeAsync(feed.Guid, successfullPolling: false);
                                continue;
                            }
                            
                            if (feed.LastPoll > lastUpdate)
                            {
                                Console.WriteLine($"[RSSFeedPollingService]: feed was not updated so our RSSFeed can be left as it is and only time stamps are going to be updated.");
                                await rSSRepository.UpdatePollingTimeAsync(feed.Guid, successfullPolling: true);
                                continue;
                            }

                            Console.WriteLine($"[RSSFeedPollingService]: feed was updated so the database must be also updated ({feed.LastPoll} < {lastUpdate})");

                            var originalItems = await rSSItemsRepository.GetAsyncFilteredByForeignKey(feed.Guid);
                            if (originalItems is not Success<IEnumerable<RSSFeedItem>>)
                            {
                                Console.WriteLine($"[RSSFeedPollingService]: failure. Unable to get access to RSSFeedsItems for feed: {feed.Guid}!");
                                await rSSRepository.UpdatePollingTimeAsync(feed.Guid, successfullPolling: false);
                                continue;
                            }

                            UpdateRSSFeedItems(rSSItemsRepository, feed, items, originalItems);
                            await rSSRepository.UpdatePollingTimeAsync(feed.Guid, successfullPolling: true);
                        }
                    }
                }
            }
        }

        private static void UpdateRSSFeedItems(IRSSFeedItemRepository rSSItemsRepository, RSSFeed feed, List<(string hash, RSSFeedItemDTO dto)> items, RepositoryResult<IEnumerable<RSSFeedItem>> originalItems)
        {
            foreach (var newFeedItem in items)
            {
                bool isUnique = true;
                foreach (var feedItem in originalItems.Data)
                {
                    if (newFeedItem.hash == feedItem.Hash)
                    {
                        isUnique = false;
                        break;
                    }
                }
                if (isUnique)
                {
                    var newRSSFeedItem = RSSFeedItemDTOToRssFeedItem.Convert(newFeedItem.dto, newFeedItem.hash);
                    newRSSFeedItem.RSSFeedId = feed.Guid;
                    _ = rSSItemsRepository.Insert(newRSSFeedItem);
                    Console.WriteLine($"[RSSFeedPollingService]: new RSSFeedItem should be inserted {newRSSFeedItem.Title}.");
                }
            }
        }

        private static bool CheckTimeStampsForUpdate(RSSFeed feed)
        {
            bool readyToUpdate = false;
            Console.WriteLine($"\n\n{feed.Guid} - {feed.Name}, LastPoll: {feed.LastPoll}, LastSuccessfullPoll: {feed.LastSuccessfullPoll}, PollingInterval: {feed.PollingInterval}");
            if (feed.LastPoll == DateTime.MinValue && feed.LastSuccessfullPoll == DateTime.MinValue || feed.LastSuccessfullPoll != feed.LastPoll)
            {
                readyToUpdate = true;
                if (feed.LastPoll == DateTime.MinValue && feed.LastSuccessfullPoll == DateTime.MinValue)
                {
                    Console.WriteLine("[RSSFeedPollingService]: polling times were not initialized.");
                }
                else
                {
                    Console.WriteLine("[RSSFeedPollingService]: polling times are not equal. It seems that the last poll was not successfull.");
                }
            }
            else if (DateTime.UtcNow - feed.LastSuccessfullPoll >= TimeSpan.FromMinutes(feed.PollingInterval))
            {
                readyToUpdate = true;
                Console.WriteLine($"[RSSFeedPollingService]: last polling time is greater than the polling interval. See {DateTime.UtcNow - feed.LastSuccessfullPoll} >= {TimeSpan.FromMinutes(feed.PollingInterval)}.");
            }

            return readyToUpdate;
        }

        public static string GenerateRSSFeedItemHash(string title, DateTime publishDate)
        {
            return HashingService.GetHash($"{title},{publishDate}");
        }

        public static (bool success, DateTime lastUpdate, List<(string hash, RSSFeedItemDTO dto)> items) LoadRSSFeedItemsFromUri(Uri uri)
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
                Console.WriteLine($"[RSSFeedPollingService]: failure. {e.Message}");
                return (false, new DateTime(), []);
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
            }
     
            List<(string hash, RSSFeedItemDTO dto)> items = [];
            foreach (SyndicationItem item in feed.Items)
            {
                RSSFeedItemDTO newItem = new RSSFeedItemDTO
                {
                    Title = item.Title.Text,
                    Summary = item.Summary.Text,
                    PublishDate = DateTime.SpecifyKind(item.PublishDate.DateTime, DateTimeKind.Utc),
                    Links = item.Links.Select(link => link.Uri).ToList(),
                    Categories = item.Categories.Select(category => category.Name).ToList(),
                    Authors = item.Authors.Select(author => author.Name).ToList(),
                    Contributors = item.Contributors.Select(contributor => contributor.Name).ToList(),
                    Content = ((item.Content is null) ? "" : (item.Content?.ToString())),
                    Id = item.Id
                };

                string hash = GenerateRSSFeedItemHash(newItem.Title, newItem.PublishDate);
                items.Add((hash, newItem));
            }

            return (true, feed.LastUpdatedTime.DateTime, items);
        }
    }
}
