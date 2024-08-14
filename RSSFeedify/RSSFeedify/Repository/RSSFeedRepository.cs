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
    }
}
