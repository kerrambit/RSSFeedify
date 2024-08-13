using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSSFeedifyCommon.Models
{
    public class RSSFeedDTO
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Uri SourceUrl { get; set; }
        public double PollingInterval { get; set; }

        public RSSFeedDTO(string name, string description, Uri sourceUrl, double pollingInterval)
        {
            Name = name;
            Description = description;
            SourceUrl = sourceUrl;
            PollingInterval = pollingInterval;
        }
    }
}
