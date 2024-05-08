using RSSFeedify.Models;
using RSSFeedify.Repositories;

namespace RSSFeedify.Repository
{
    public interface IRSSFeedRepository : IRepository<RSSFeed>
    {
        Task UpdatePollingTimeAsync(Guid guid, bool successfullPolling);
    }
}
