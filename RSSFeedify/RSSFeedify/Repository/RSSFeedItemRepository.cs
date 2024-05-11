using Microsoft.EntityFrameworkCore;
using PostgreSQL.Data;
using RSSFeedify.Models;
using RSSFeedify.Repositories;
using RSSFeedify.Repository.Types;
using RSSFeedify.Repository.Types.Pagination;
using RSSFeedify.Repository.Types.PaginationQuery;

namespace RSSFeedify.Repository
{
    public class RSSFeedItemRepository : Repository<RSSFeedItem>, IRSSFeedItemRepository
    {
        public RSSFeedItemRepository(ApplicationDbContext context, DbSet<RSSFeedItem> data) : base(context, data) { }

        public async Task<RepositoryResult<IEnumerable<RSSFeedItem>>> GetFilteredByForeignKeyAsync(Guid guid)
        {
            var entities = await _data.Where(e => e.RSSFeedId == guid).ToListAsync();
            if (entities == null)
            {
                return new NotFoundError<IEnumerable<RSSFeedItem>>();
            }
            return new Success<IEnumerable<RSSFeedItem>>(entities);
        }

        public async Task<RepositoryResult<IEnumerable<RSSFeedItem>>> GetFilteredByForeignKeyAsync(Guid guid, PaginationQuery paginationQuery)
        {
            var entities = await _data.Where(e => e.RSSFeedId == guid).ToPagedListAsync(paginationQuery.Page, paginationQuery.PageSize);
            if (entities == null)
            {
                return new NotFoundError<IEnumerable<RSSFeedItem>>();
            }
            return new Success<IEnumerable<RSSFeedItem>>(entities);
        }

        public async Task<RepositoryResult<int>> GetTotalCountAsync(Guid guid)
        {
            var result = await _data.Where(item => item.RSSFeedId == guid).CountAsync();
            return new Success<int>(result);
        }
    }
}