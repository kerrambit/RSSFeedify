using Microsoft.EntityFrameworkCore;
using PostgreSQL.Data;
using RSSFeedify.Models;
using System;

namespace RSSFeedify.Repositories
{
    public class RSSFeedRepository : IRSSFeedRepository
    {
        private DbContext _context;
        private DbSet<RSSFeed> _data;
        private bool _disposed = false;

        public RSSFeedRepository(ApplicationDbContext context)
        {
            _context = context;
            _data = context.RSSFeeds;
        }

        public async Task<RepositoryResult<RSSFeed>> DeleteRSSFeedAsync(Guid studentGUID)
        {
            var result = await GetRSSFeedByGUID(studentGUID);
            var rSSFeed = result.Data;

            if (rSSFeed == null)
            {
                return new NotFoundError<RSSFeed>();
            }

            _data.Remove(rSSFeed);
            return new Success<RSSFeed>(rSSFeed);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }
            this._disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public async Task<RepositoryResult<RSSFeed>> GetRSSFeedByGUID(Guid studentGUID)
        {
            var rSSFeed = await _data.FindAsync(studentGUID);
            if (rSSFeed == null)
            {
                return new NotFoundError<RSSFeed>();
            }
            return new Success<RSSFeed>(rSSFeed);
        }

        public async Task<RepositoryResult<IEnumerable<RSSFeed>>> GetRSSFeeds()
        {
            var rSSFeeds = await _data.ToListAsync();
            return new Success<IEnumerable<RSSFeed>>(rSSFeeds);
        }

        public RepositoryResult<RSSFeed> InsertRSSFeed(RSSFeed feed)
        {
            _data.Add(feed);
            return new Created<RSSFeed>("GetRSSFeed", feed, feed.Guid);
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        public void UpdateRSSFeed(RSSFeed student)
        {
            throw new NotImplementedException();
        }
    }
}
