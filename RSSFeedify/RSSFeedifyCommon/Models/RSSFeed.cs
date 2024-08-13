using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSSFeedifyCommon.Models
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

    public interface IRSSFeed
    {
        Guid Guid { get; set; }
        DateTime CreatedAt { get; set; }
        DateTime UpdatedAt { get; set; }
        string Name { get; set; }
        string Description { get; set; }
        Uri SourceUrl { get; set; }
        double PollingInterval { get; set; }
        DateTime LastPoll { get; set; }
        DateTime LastSuccessfullPoll { get; set; }
    }
}
