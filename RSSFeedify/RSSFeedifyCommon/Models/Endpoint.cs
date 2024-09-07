namespace RSSFeedifyCommon.Models
{
    public enum EndPoint
    {
        RSSFeeds,
        RSSFeedItems,
        ApplicationUser
    };

    public static class EndPointExtensions
    {
        public static string ConvertToString(this EndPoint endPoint)
        {
            switch (endPoint)
            {
                case EndPoint.RSSFeeds:
                    return "RSSFeeds";
                case EndPoint.RSSFeedItems:
                    return "RSSFeedItems";
                case EndPoint.ApplicationUser:
                    return "ApplicationUser";
                default:
                    return "RSSFeeds";
            }
        }
    }
}
