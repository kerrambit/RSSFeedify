using RSSFeedify.Repository.Types;
using RSSFeedifyCommon.Models;

namespace RSSFeedify.Models
{
    public class RSSFeed : Reposable, IRSSFeed
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Uri SourceUrl { get; set; }
        public double PollingInterval { get; set; }
        public DateTime LastPoll { get; set; }
        public DateTime LastSuccessfullPoll { get; set; }

        public override string ToString()
        {
            return $"{base.ToString()}, Name: '{Name}', Description: '{Description}', SourceUrl: '{SourceUrl}', " +
                   $"PollingInterval: {PollingInterval}, LastPoll: {LastPoll}, LastSuccessfullPoll: {LastSuccessfullPoll}";
        }
    }
}
