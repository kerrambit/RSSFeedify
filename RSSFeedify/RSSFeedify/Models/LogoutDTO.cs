using System.ComponentModel.DataAnnotations;

namespace RSSFeedify.Models
{
    public class LogoutDTO
    {
        [Required]
        public string JWT {  get; set; }
    }
}
