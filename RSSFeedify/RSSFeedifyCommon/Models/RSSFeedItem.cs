using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSSFeedifyCommon.Models
{
    public interface IRSSFeedItem
    {
        Guid Guid { get; set; }
        DateTime CreatedAt { get; set; }
        DateTime UpdatedAt { get; set; }
        string Hash { get; set; }
        string Title { get; set; }
        string Summary { get; set; }
        DateTime PublishDate { get; set; }
        List<Uri> Links { get; set; }
        List<string> Categories { get; set; }
        List<string> Authors { get; set; }
        List<string> Contributors { get; set; }
        string Content { get; set; }
        string Id { get; set; }
        Guid RSSFeedId { get; set; }
    }

    public class RSSFeedItem
    {
        public Guid Guid { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string Hash { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        public DateTime PublishDate { get; set; }
        public List<Uri> Links { get; set; }
        public List<string> Categories { get; set; }
        public List<string> Authors { get; set; }
        public List<string> Contributors { get; set; }
        public string Content { get; set; }
        public string Id { get; set; }
        public Guid RSSFeedId { get; set; }
    }
}
