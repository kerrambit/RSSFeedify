using System.ComponentModel.DataAnnotations;

namespace RSSFeedifyCommon.Models
{
    public class RegisterDTO
    {
        [Required]
        [EmailAddress(ErrorMessage = "The email adress is not valid.")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at most {1} characters long.", MinimumLength = 8)]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
