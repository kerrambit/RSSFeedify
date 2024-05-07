using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RSSFeedify.Models
{
    public class RSSFeed
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public Guid Guid { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public Uri SourceUrl {  get; set; }
        public double PollingInterval { get; set; }
    }
}
