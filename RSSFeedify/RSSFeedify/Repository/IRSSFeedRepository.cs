using RSSFeedify.Models;

namespace RSSFeedify.Repositories
{
    public interface IRSSFeedRepository : IDisposable
    {
        IEnumerable<RSSFeed> GetStudents();
        RSSFeed GetRSSFeedByGUID(Guid studentGUID);
        void InsertRSSFeed(RSSFeed student);
        Task<RepositoryResult> DeleteRSSFeedAsync(Guid studentGUID);
        void UpdateRSSFeed(RSSFeed student);
        Task SaveAsync();
    }
}
