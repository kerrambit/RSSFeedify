using Microsoft.EntityFrameworkCore;
using PostgreSQL.Data;
using RSSFeedify.Models;
using RSSFeedify.Repositories;
using RSSFeedify.Repository.Types;
using RSSFeedify.Repository.Types.Pagination;
using RSSFeedify.Repository.Types.PaginationQuery;
using System.Linq;

namespace RSSFeedify.Repository
{
    public class RSSFeedRepository : Repository<RSSFeed>, IRSSFeedRepository
    {
        public RSSFeedRepository(ApplicationDbContext context, DbSet<RSSFeed> data) : base(context, data) { }

        public async Task<RepositoryResult<IEnumerable<RSSFeed>>> GetSortedByNameAsync(PaginationQuery paginationQuery)
        {
            var batches = await _data.OrderBy(u => u.Name).ToPagedListAsync(paginationQuery.Page, paginationQuery.PageSize);
            return new Success<IEnumerable<RSSFeed>>(batches);
        }

        public async Task UpdatePollingTimeAsync(Guid guid, bool successfullPolling)
        {
            using (var dbContextTransaction = _context.Database.BeginTransaction())
            {
                var result = await GetAsync(guid);
                if (result is Success<RSSFeed>)
                {
                    result.Data.LastPoll = DateTime.UtcNow;
                    if (successfullPolling)
                    {
                        result.Data.LastSuccessfullPoll = result.Data.LastPoll;
                    }
                }
                
                await SaveAsync();
                dbContextTransaction.Commit();
            }
        }
    }
}
