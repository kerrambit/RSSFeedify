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
        public RSSFeedItemRepository(IConfiguration configuration) : base(configuration) { }

        public async Task<RepositoryResult<IEnumerable<RSSFeedItem>>> GetFilteredByForeignKeyAsync(Guid guid)
        {
            using (var context = new ApplicationDbContext(_configuration))
            {
                var entities = await context.Set<RSSFeedItem>().Where(e => e.RSSFeedId == guid).ToListAsync();
                if (entities == null)
                {
                    return new NotFoundError<IEnumerable<RSSFeedItem>>();
                }
                return new Success<IEnumerable<RSSFeedItem>>(entities);
            }
        }

        public async Task<RepositoryResult<IEnumerable<RSSFeedItem>>> GetFilteredByForeignKeyAsync(Guid guid, PaginationQuery paginationQuery)
        {
            using (var context = new ApplicationDbContext(_configuration))
            {
                var entities = await context.Set<RSSFeedItem>().Where(item => item.RSSFeedId == guid).OrderByDescending(item => item.PublishDate).ToPagedListAsync(paginationQuery.Page, paginationQuery.PageSize);
                if (entities == null)
                {
                    return new NotFoundError<IEnumerable<RSSFeedItem>>();
                }
                return new Success<IEnumerable<RSSFeedItem>>(entities);
            }
        }

        public async Task<RepositoryResult<int>> GetTotalCountAsync(Guid guid)
        {
            using (var context = new ApplicationDbContext(_configuration))
            {
                var result = await context.Set<RSSFeedItem>().Where(item => item.RSSFeedId == guid).CountAsync();
                return new Success<int>(result);
            }
        }

        public async Task<RepositoryResult<int>> InsertMultipleAsync(IList<RSSFeedItem> rSSFeedItems)
        {
            using (var context = new ApplicationDbContext(_configuration))
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    int insertedEntities = 0;
                    foreach (var item in rSSFeedItems)
                    {
                        context.Set<RSSFeedItem>().Add(item);
                        insertedEntities++;
                    }

                    await SaveAsync(context);
                    transaction.Commit();

                    return new Success<int>(insertedEntities);
                }
            }
        }
    }
}