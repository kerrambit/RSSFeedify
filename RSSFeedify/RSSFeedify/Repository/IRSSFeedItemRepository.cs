using RSSFeedify.Models;
using RSSFeedify.Repositories;
using RSSFeedify.Repository.Types;

namespace RSSFeedify.Repository
{
    public interface IRSSFeedItemRepository : IRepository<RSSFeedItem>
    {
        Task<RepositoryResult<IEnumerable<RSSFeedItem>>> GetFilteredByForeignKeyAsync(Guid guid);
    }
}