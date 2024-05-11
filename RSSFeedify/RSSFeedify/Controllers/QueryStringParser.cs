namespace RSSFeedify.Controllers
{
    public static class QueryStringParser
    {
        public static bool ParseGuid(string queryString, out Guid guid)
        {
            if (!Guid.TryParse(queryString, out guid))
            {
                return false;
            }
            return true;
        }
    }
}
