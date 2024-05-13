﻿using PostgreSQL.Data;
using RSSFeedify.Models;
using RSSFeedify.Repositories;
using RSSFeedify.Repository.Types;
using RSSFeedify.Repository.Types.Pagination;
using RSSFeedify.Repository.Types.PaginationQuery;

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
                    var result = await GetAsync(guid);
                    if (result is Success<RSSFeed>)
                    {
                        result.Data.LastPoll = DateTime.UtcNow;
                        if (successfullPolling)
                        {
                            result.Data.LastSuccessfullPoll = result.Data.LastPoll;
                        }
                    }
                
                    await SaveAsync(context);
                    dbContextTransaction.Commit();
                }
            }
        }
    }
}
