
using Microsoft.AspNetCore.Identity;

using RSSFeed = RSSFeedify.Models.RSSFeed;
using RSSFeedItem = RSSFeedify.Models.RSSFeedItem;

namespace RSSFeedify.Models
{
    public class ApplicationUser : IdentityUser
    {
        public List<RSSFeed> WatchedList { get; set; }
    }
}
