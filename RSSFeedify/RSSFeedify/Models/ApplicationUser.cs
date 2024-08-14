
using Microsoft.AspNetCore.Identity;

namespace RSSFeedify.Models
{
    public class ApplicationUser : IdentityUser
    {
        public List<RSSFeed> WatchedList { get; set; }
    }
}
