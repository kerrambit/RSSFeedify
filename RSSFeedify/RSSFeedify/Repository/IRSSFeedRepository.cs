using RSSFeedify.Models;

namespace RSSFeedify.Repositories
{
    public interface IRSSFeedRepository : IDisposable
    {
        IEnumerable<RSSFeed> GetRSSFeeds();
        RSSFeed GetRSSFeedByGUID(Guid feedGUID);
        RepositoryResult InsertRSSFeed(RSSFeed feed);
        Task<RepositoryResult> DeleteRSSFeedAsync(Guid feedGUID);
        void UpdateRSSFeed(RSSFeed feed);
        Task SaveAsync();
    }
}
