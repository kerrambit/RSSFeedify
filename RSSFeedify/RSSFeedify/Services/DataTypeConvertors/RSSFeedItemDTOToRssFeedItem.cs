using RSSFeedify.Models;

namespace RSSFeedify.Services.DataTypeConvertors
{
    public static class RSSFeedItemDTOToRssFeedItem
    {
        public static RSSFeedItem Convert(RSSFeedItemDTO rSSFeedItemDTO, string additionalHash)
        {
            var rSSFeed = new RSSFeedItem
            {
                Hash = additionalHash,
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
