using Microsoft.EntityFrameworkCore;
using PostgreSQL.Data;
using RSSFeedify.Models;
using RSSFeedify.Repositories;
using RSSFeedify.Repository.Types;

namespace RSSFeedify.Repository
{
    public class RSSFeedItemRepository : Repository<RSSFeedItem>, IRSSFeedItemRepository
    {
        public RSSFeedItemRepository(ApplicationDbContext context, DbSet<RSSFeedItem> data) : base(context, data) { }

        public async Task<RepositoryResult<IEnumerable<RSSFeedItem>>> GetAsyncFilteredByForeignKey(Guid guid)
        {
            var entities = await _data.Where(e => e.RSSFeedId == guid).ToListAsync();
            if (entities == null)
            {
                return new NotFoundError<IEnumerable<RSSFeedItem>>();
            }
            return new Success<IEnumerable<RSSFeedItem>>(entities);
        }
    }
}
