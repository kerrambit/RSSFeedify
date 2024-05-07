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

        public async Task<RepositoryResult> DeleteRSSFeedAsync(Guid studentGUID)
        {
            var rSSFeed = await _data.FindAsync(studentGUID);
            if (rSSFeed == null)
            {
                return new NotFoundError();
            }

            _data.Remove(rSSFeed);
            return new Success(rSSFeed);
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

        public RSSFeed GetRSSFeedByGUID(Guid studentGUID)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<RSSFeed> GetStudents()
        {
            throw new NotImplementedException();
        }

        public void InsertRSSFeed(RSSFeed student)
        {
            throw new NotImplementedException();
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
