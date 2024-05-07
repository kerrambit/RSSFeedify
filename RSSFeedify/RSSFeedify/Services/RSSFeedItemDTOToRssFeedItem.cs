using RSSFeedify.Models;

namespace RSSFeedify.Services
{
    public static class RSSFeedItemDTOToRssFeedItem
    {
        public static RSSFeedItem Convert(RSSFeedItemDTO rSSFeedItemDTO)
        {
            var rSSFeed = new RSSFeedItem
            {
                Title = rSSFeedItemDTO.Title,
                Summary = rSSFeedItemDTO.Summary,
                PublishDate = rSSFeedItemDTO.PublishDate,
                Links = rSSFeedItemDTO.Links,
                Categories = rSSFeedItemDTO.Categories,
                Authors = rSSFeedItemDTO.Authors,
                Contributors = rSSFeedItemDTO.Contributors,
                Content = rSSFeedItemDTO.Content,
                Id = rSSFeedItemDTO.Id,
                RSSFeedId = rSSFeedItemDTO.RSSFeedId,
            };

            return rSSFeed;
        }
    }
}
