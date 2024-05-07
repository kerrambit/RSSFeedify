using RSSFeedify.Repository.Types;

namespace RSSFeedify.Models
{
    public class RSSFeed : Reposable
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public Uri SourceUrl {  get; set; }
        public double PollingInterval { get; set; }
        public DateTime LastPoll { get; set; }
        public DateTime LastSuccessfullPoll { get; set; }
    }
}
