using RSSFeedify.Models;
using RSSFeedify.Repositories;
using RSSFeedify.Repository.Types;
using RSSFeedify.Repository.Types.PaginationQuery;

namespace RSSFeedify.Repository
{
    public interface IRSSFeedRepository : IRepository<RSSFeed>
    {
        Task UpdatePollingTimeAsync(Guid guid, bool successfullPolling);
        Task<RepositoryResult<IEnumerable<RSSFeed>>> GetSortedByNameAsync(PaginationQuery paginationQuery);
    }
}
