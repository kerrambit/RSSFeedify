using RSSFeedify.Repository.Types;

namespace RSSFeedify.Models
{
    public class RSSFeedItem : Reposable
    {
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
