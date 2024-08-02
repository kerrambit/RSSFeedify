using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSSFeedifyCLIClient.Models
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
