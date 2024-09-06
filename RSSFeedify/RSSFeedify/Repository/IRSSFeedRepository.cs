using RSSFeedify.Repositories;
using RSSFeedify.Repository.Types;
using RSSFeedify.Repository.Types.PaginationQuery;
using RSSFeedifyCommon.Models;

using RSSFeed = RSSFeedify.Models.RSSFeed;

namespace RSSFeedify.Repository
{
    public interface IRSSFeedRepository : IRepository<RSSFeed>
    {
        Task UpdatePollingTimeAsync(Guid guid, bool successfullPolling);
        Task<RepositoryResult<IEnumerable<RSSFeed>>> GetSortedByNameAsync(PaginationQuery paginationQuery);
        Task<RepositoryResult<RSSFeed>> UpdateAsync(Guid guid, RSSFeedDTO batch);
        new Task<RepositoryResult<RSSFeed>> InsertAsync(RSSFeed batch);
        Task<RepositoryResult<RSSFeed>> UpdateAsync(RSSFeed batch);
    }
}
