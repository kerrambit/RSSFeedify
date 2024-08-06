using System.ComponentModel.DataAnnotations;

namespace RSSFeedifyCLIClient.Models
{
    public class LogoutDTO
    {
        [Required]
        public string JWT { get; set; }
    }
}
