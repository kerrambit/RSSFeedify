using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RSSFeedify.Models
{
    public class RSSFeedDTO
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public Uri SourceUrl { get; set; }
        public double PollingInterval { get; set; }
    }
}
