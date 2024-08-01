
using System.ComponentModel.DataAnnotations;

namespace RSSFeedify.Models
{
    public class RSSFeedDTO
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public Uri SourceUrl { get; set; }

        [Range(typeof(double), "10.0", "60.0",
        ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public double PollingInterval { get; set; }
    }
}
