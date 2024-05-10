namespace RSSFeedifyCLIClient.Models
{
    public class RSSFeed
    {
        public Guid Guid { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Uri SourceUrl { get; set; }
        public double PollingInterval { get; set; }
        public DateTime LastPoll { get; set; }
        public DateTime LastSuccessfullPoll { get; set; }
    }
}
