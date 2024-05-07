using Microsoft.EntityFrameworkCore;
using PostgreSQL.Data;
using RSSFeedify.Repository.Types;

namespace RSSFeedify.Repositories
{
    public class Repository<T> : IRepository<T> where T : Reposable
    {
        private DbContext _context;
        private DbSet<T> _data;

        public Repository(ApplicationDbContext context, DbSet<T> data)
        {
            _context = context;
            _data = data;
        }

        public async Task<RepositoryResult<T>> DeleteAsync(Guid studentGUID)
        {
            var result = await GetAsync(studentGUID);
            var rSSFeed = result.Data;

            if (rSSFeed == null)
            {
                return new NotFoundError<T>();
            }

            _data.Remove(rSSFeed);
            return new Success<T>(rSSFeed);
        }

        public async Task<RepositoryResult<T>> GetAsync(Guid studentGUID)
        {
            var rSSFeed = await _data.FindAsync(studentGUID);
            if (rSSFeed == null)
            {
                return new NotFoundError<T>();
            }
            return new Success<T>(rSSFeed);
        }

        public async Task<RepositoryResult<IEnumerable<T>>> GetAsync()
        {
            var rSSFeeds = await _data.ToListAsync();
            return new Success<IEnumerable<T>>(rSSFeeds);
        }

        public RepositoryResult<T> Insert(T feed)
        {
            _data.Add(feed);
            return new Created<T>("GetRSSFeed", feed, feed.Guid);
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<RepositoryResult<T>> UpdateAsync(Guid feedGUID, T feed)
        {
            using (var dbContextTransaction = _context.Database.BeginTransaction())
            {
                var original = await DeleteAsync(feedGUID);
                Guid guid = original.Data.Guid;
                feed.Guid = guid;

                var result = Insert(feed);
                await SaveAsync();

                dbContextTransaction.Commit();
                return result;
            }
        }

        public RepositoryResult<bool> Exists(Guid feedGUID)
        {
            bool result = _data.Any(e => e.Guid == feedGUID);
            return new Success<bool>(result);
        }
    }
}
