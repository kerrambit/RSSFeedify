using System.ComponentModel.DataAnnotations;

namespace RSSFeedify.Models
{
    public class LoginDTO
    {
        [Required]
        [EmailAddress(ErrorMessage = "The email adress is not valid.")]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
        public bool RememberMe { get; set; }
    }
}
