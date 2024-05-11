namespace RSSFeedifyCLIClient.Settings
{
    public static class Endpoints
    {
        public static Uri BaseUrl { get; set; } = new(@"https://localhost:32770/api/");

        public enum EndPoint
        {
            RSSFeeds,
            RSSFeedItems
        };

        public static Uri BuildUri(EndPoint endPoint)
        {
            UriBuilder uriBuilder = new UriBuilder(BaseUrl);

            switch (endPoint)
            {
                case EndPoint.RSSFeeds:
                    uriBuilder.Path += "RSSFeeds";
                    break;
                case EndPoint.RSSFeedItems:
                    uriBuilder.Path += "RSSFeedItems";
                    break;
            }

            return uriBuilder.Uri;
        }

        public static Uri BuildUri(EndPoint endpoint, string resourcePath)
        {
            string baseUriString = BuildUri(endpoint).ToString();
            string completeUriString = baseUriString + "/" + resourcePath;
            return new Uri(completeUriString);
        }

        public static Uri BuildUri(EndPoint endpoint, (string key, string value) queryString)
        {
            string baseUriString = BuildUri(endpoint).ToString();
            string completeUriString = baseUriString + $"?{queryString.key}={queryString.value}";
            return new Uri(completeUriString);
        }

        public static Uri BuildUri(EndPoint endpoint, string resourcePath, (string key, string value) queryString)
        {
            string baseUriString = BuildUri(endpoint).ToString();
            string completeUriString = baseUriString + "/" + resourcePath + $"?{queryString.key}={queryString.value}";
            return new Uri(completeUriString);
        }
    }
}
