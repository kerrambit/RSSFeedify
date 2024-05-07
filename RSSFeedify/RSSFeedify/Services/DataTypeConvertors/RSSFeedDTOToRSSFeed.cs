using RSSFeedify.Models;

namespace RSSFeedify.Services.DataTypeConvertors
{
    public static class RSSFeedDTOToRSSFeed
    {
        public static RSSFeed Convert(RSSFeedDTO rSSFeedDTO)
        {
            var rSSFeed = new RSSFeed
            {
                Name = rSSFeedDTO.Name,
                Description = rSSFeedDTO.Description,
                SourceUrl = rSSFeedDTO.SourceUrl,
                PollingInterval = rSSFeedDTO.PollingInterval
            };

            return rSSFeed;
        }
    }
}
