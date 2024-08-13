using Microsoft.EntityFrameworkCore;
using PostgreSQL.Data;
using RSSFeedify.Repository.Types;
using RSSFeedify.Repository.Types.Pagination;
using RSSFeedify.Repository.Types.PaginationQuery;

using RSSFeed = RSSFeedify.Models.RSSFeed;
using RSSFeedItem = RSSFeedify.Models.RSSFeedItem;

namespace RSSFeedify.Repositories
{
    public class Repository<T> : IRepository<T> where T : Reposable
    {
        protected IConfiguration _configuration;

        public Repository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<RepositoryResult<T>> DeleteAsync(Guid guid)
        {
            using (var context = new ApplicationDbContext(_configuration))
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    var result = await GetAsync(guid);
                    var batch = result.Data;

                    if (batch == null)
                    {
                        return new NotFoundError<T>();
                    }

                    context.Set<T>().Remove(batch);
                    await SaveAsync(context);

                    transaction.Commit();
                    return new Success<T>(batch);
                }
            }
        }

        public async Task<RepositoryResult<T>> GetAsync(Guid guid)
        {
            using (var context = new ApplicationDbContext(_configuration))
            {
                var batch = await context.Set<T>().FindAsync(guid);
                if (batch == null)
                {
                    return new NotFoundError<T>();
                }
                return new Success<T>(batch);
            }
        }

        public async Task<RepositoryResult<IEnumerable<T>>> GetAsync()
        {
            using (var context = new ApplicationDbContext(_configuration))
            {
                var batches = await context.Set<T>().ToListAsync();
                return new Success<IEnumerable<T>>(batches);
            }
        }

        public async Task<RepositoryResult<IEnumerable<T>>> GetAsync(PaginationQuery paginationQuery)
        {
            using (var context = new ApplicationDbContext(_configuration))
            {
                var batches = await context.Set<T>().ToPagedListAsync(paginationQuery.Page, paginationQuery.PageSize);
                return new Success<IEnumerable<T>>(batches);
            }
        }

        public async Task<RepositoryResult<T>> InsertAsync(T batch)
        {
            using (var context = new ApplicationDbContext(_configuration))
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    batch.CreatedAt = DateTime.UtcNow;
                    batch.UpdatedAt = batch.CreatedAt;
                    context.Set<T>().Add(batch);
                    await SaveAsync(context);

                    transaction.Commit();
                    return new Created<T>("GetRSSFeed", batch, batch.Guid);
                }
            }
        }

        public async Task SaveAsync(ApplicationDbContext context)
        {
            await context.SaveChangesAsync();
        }

        public RepositoryResult<bool> Exists(Guid guid)
        {
            using (var context = new ApplicationDbContext(_configuration))
            {
                bool result = context.Set<T>().Any(e => e.Guid == guid);
                return new Success<bool>(result);
            }
        }

        public async Task<RepositoryResult<int>> GetTotalCountAsync()
        {
            using (var context = new ApplicationDbContext(_configuration))
            {
                var result = await context.Set<T>().CountAsync();
                return new Success<int>(result);
            }
        }
    }
}
