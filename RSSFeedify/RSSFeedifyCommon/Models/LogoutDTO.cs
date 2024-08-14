using System.ComponentModel.DataAnnotations;

namespace RSSFeedifyCommon.Models
{
    public class LogoutDTO
    {
        [Required]
        public string JWT { get; set; }
    }
}
