using RSSFeedify.Models;
using RSSFeedify.Repositories;
using RSSFeedify.Repository.Types;
using RSSFeedify.Repository.Types.PaginationQuery;

namespace RSSFeedify.Repository
{
    public interface IRSSFeedItemRepository : IRepository<RSSFeedItem>
    {
        Task<RepositoryResult<IEnumerable<RSSFeedItem>>> GetFilteredByForeignKeyAsync(Guid guid, PaginationQuery paginationQuery);
        Task<RepositoryResult<IEnumerable<RSSFeedItem>>> GetFilteredByForeignKeyAsync(Guid guid);
        Task<RepositoryResult<int>> GetTotalCountAsync(Guid guid);
        Task<RepositoryResult<int>> InsertMultipleAsync(IList<RSSFeedItem> rSSFeedItems);
    }
}