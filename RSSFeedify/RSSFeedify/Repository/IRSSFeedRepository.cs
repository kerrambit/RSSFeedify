using RSSFeedify.Models;
using RSSFeedify.Repositories;
using RSSFeedify.Repository.Types;

namespace RSSFeedify.Repository
{
    public interface IRSSFeedRepository : IRepository<RSSFeed>
    {
        Task UpdatePollingTimeAsync(Guid guid, bool successfullPolling);
    }
}
