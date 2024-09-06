using PostgreSQL.Data;
using RSSFeedify.Repositories;
using RSSFeedify.Repository.Types;
using RSSFeedify.Repository.Types.Pagination;
using RSSFeedify.Repository.Types.PaginationQuery;
using RSSFeedifyCommon.Models;
using RSSFeed = RSSFeedify.Models.RSSFeed;

namespace RSSFeedify.Repository
{
    public class RSSFeedRepository : Repository<RSSFeed>, IRSSFeedRepository
    {
        public RSSFeedRepository(IConfiguration configuration) : base(configuration) { }

        public async Task<RepositoryResult<IEnumerable<RSSFeed>>> GetSortedByNameAsync(PaginationQuery paginationQuery)
        {
            using (var context = new ApplicationDbContext(_configuration))
            {
                var batches = await context.Set<RSSFeed>().OrderBy(u => u.Name).ToPagedListAsync(paginationQuery.Page, paginationQuery.PageSize);
                return new Success<IEnumerable<RSSFeed>>(batches);
            }
        }

        public async Task UpdatePollingTimeAsync(Guid guid, bool successfullPolling)
        {
            using (var context = new ApplicationDbContext(_configuration))
            {
                using (var dbContextTransaction = context.Database.BeginTransaction())
                {
                    var result = context.RSSFeeds.SingleOrDefault(feed => feed.Guid == guid);
                    if (result is not null)
                    {
                        result.LastPoll = DateTime.UtcNow;
                        if (successfullPolling)
                        {
                            result.LastSuccessfullPoll = result.LastPoll;
                        }
                    }

                    await SaveAsync(context);
                    dbContextTransaction.Commit();
                }
            }
        }

        public async Task<RepositoryResult<RSSFeed>> UpdateAsync(Guid guid, RSSFeedDTO batch)
        {
            using (var context = new ApplicationDbContext(_configuration))
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    var feed = context.RSSFeeds.SingleOrDefault(feed => feed.Guid == guid);
                    if (feed is not null)
                    {
                        feed.Name = batch.Name;
                        feed.Description = batch.Description;
                        feed.SourceUrl = batch.SourceUrl;
                        feed.PollingInterval = batch.PollingInterval;

                        await SaveAsync(context);

                        transaction.Commit();
                        return new Success<RSSFeed>(feed);
                    }
                    else
                    {
                        return new NotFoundError<RSSFeed>();
                    }
                }
            }
        }

        public async Task<RepositoryResult<RSSFeed>> UpdateAsync(RSSFeed batch)
        {
            using (var context = new ApplicationDbContext(_configuration))
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    var feed = context.RSSFeeds.SingleOrDefault(feed => feed.Guid == batch.Guid);
                    if (feed is not null)
                    {
                        feed.Name = batch.Name;
                        feed.Description = batch.Description;
                        feed.SourceUrl = batch.SourceUrl;
                        feed.PollingInterval = batch.PollingInterval;
                        feed.LastPoll = batch.LastPoll;
                        feed.LastSuccessfullPoll = batch.LastSuccessfullPoll;

                        feed.UpdatedAt = DateTime.UtcNow;

                        await SaveAsync(context);

                        transaction.Commit();
                        return new Success<RSSFeed>(feed);
                    }
                    else
                    {
                        return new NotFoundError<RSSFeed>();
                    }
                }
            }
        }

        public new async Task<RepositoryResult<RSSFeed>> InsertAsync(RSSFeed batch)
        {
            using (var context = new ApplicationDbContext(_configuration))
            {
                return await RepositoryConcurrentReplyExecutor.ExecuteWithRetryAsync<RSSFeed>(async (context) =>
                {
                    var count = context.Set<RSSFeed>().Where(feed => feed.SourceUrl == batch.SourceUrl).Count();
                    if (count != 0)
                    {
                        return new Duplicate<RSSFeed>(Controllers.Helpers.ControllersHelper.GetMessageForDuplicatedSourcerUr(batch.SourceUrl));
                    }
                    batch.CreatedAt = DateTime.UtcNow;
                    batch.UpdatedAt = batch.CreatedAt;
                    Thread.Sleep(4000);
                    context.Set<RSSFeed>().Add(batch);
                    await SaveAsync(context);

                    return new Created<RSSFeed>(batch, batch.Guid, "GetRSSFeed");
                }, context);
            }
        }
    }
}
