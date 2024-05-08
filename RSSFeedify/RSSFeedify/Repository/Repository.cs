using Microsoft.EntityFrameworkCore;
using PostgreSQL.Data;
using RSSFeedify.Repository.Types;

namespace RSSFeedify.Repositories
{
    public class Repository<T> : IRepository<T> where T : Reposable
    {
        protected DbContext _context;
        protected DbSet<T> _data;

        public Repository(ApplicationDbContext context, DbSet<T> data)
        {
            _context = context;
            _data = data;
        }

        public async Task<RepositoryResult<T>> DeleteAsync(Guid guid)
        {
            var result = await GetAsync(guid);
            var batch = result.Data;

            if (batch == null)
            {
                return new NotFoundError<T>();
            }

            _data.Remove(batch);
            return new Success<T>(batch);
        }

        public async Task<RepositoryResult<T>> GetAsync(Guid guid)
        {
            var batch = await _data.FindAsync(guid);
            if (batch == null)
            {
                return new NotFoundError<T>();
            }
            return new Success<T>(batch);
        }

        public async Task<RepositoryResult<IEnumerable<T>>> GetAsync()
        {
            var batches = await _data.ToListAsync();
            return new Success<IEnumerable<T>>(batches);
        }

        public RepositoryResult<T> Insert(T batch)
        {
            batch.CreatedAt = DateTime.UtcNow;
            batch.UpdatedAt = batch.CreatedAt;
            _data.Add(batch);
            return new Created<T>("GetRSSFeed", batch, batch.Guid);
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<RepositoryResult<T>> UpdateAsync(Guid guid, T batch)
        {
            using (var dbContextTransaction = _context.Database.BeginTransaction())
            {
                var original = await DeleteAsync(guid);
                Guid originalGuid = original.Data.Guid;
                DateTime originalCreatedAt = original.Data.CreatedAt;
                batch.Guid = originalGuid;
                batch.UpdatedAt = DateTime.UtcNow;

                var result = Insert(batch);

                batch.CreatedAt = originalCreatedAt;
                await SaveAsync();

                dbContextTransaction.Commit();
                return result;
            }
        }

        public RepositoryResult<bool> Exists(Guid guid)
        {
            bool result = _data.Any(e => e.Guid == guid);
            return new Success<bool>(result);
        }
    }
}
