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

        public async Task<RepositoryResult<RSSFeed>> UpdateRSSFeed(Guid feedGUID, RSSFeed feed)
        {
            var toUpdate = await GetRSSFeedByGUID(feedGUID);
            if (toUpdate is NotFoundError<RSSFeed>)
            {
                return new NotFoundError<RSSFeed>();
            }

            toUpdate.Data.Name = feed.Name;
            toUpdate.Data.Description = feed.Description;
            toUpdate.Data.SourceUrl = feed.SourceUrl;
            toUpdate.Data.PollingInterval = feed.PollingInterval;

            try
            {
                await SaveAsync();
            }
            catch (DbUpdateConcurrencyException) when (!RSSFeedExists(feedGUID).Data)
            {
                return new NotFoundError<RSSFeed>();
            }

            return new Success<RSSFeed>(toUpdate.Data);
        }

        public RepositoryResult<bool> RSSFeedExists(Guid feedGUID)
        {
            bool result = _data.Any(e => e.Guid == feedGUID);
            return new Success<bool>(result);
        }
    }
}
