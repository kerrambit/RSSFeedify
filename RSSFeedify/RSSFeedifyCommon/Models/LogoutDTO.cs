using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSSFeedifyCommon.Models
{
    public class LogoutDTO
    {
        [Required]
        public string JWT { get; set; }
    }
}
