using RSSFeedify.Models;

namespace RSSFeedify.Repositories
{
    public interface IRSSFeedRepository : IDisposable
    {
        Task<RepositoryResult<IEnumerable<RSSFeed>>> GetRSSFeeds();
        Task<RepositoryResult<RSSFeed>> GetRSSFeedByGUID(Guid feedGUID);
        RepositoryResult<RSSFeed> InsertRSSFeed(RSSFeed feed);
        Task<RepositoryResult<RSSFeed>> DeleteRSSFeedAsync(Guid feedGUID);
        void UpdateRSSFeed(RSSFeed feed);
        Task SaveAsync();
    }
}
