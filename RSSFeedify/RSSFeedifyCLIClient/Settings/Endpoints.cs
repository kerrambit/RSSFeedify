using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSSFeedifyCLIClient.Settings
{
    public static class Endpoints
    {
        public static Uri BaseUrl { get; set; } = new(@"https://localhost:32776/api/");

        public enum EndPoint
        {
            RSSFeeds
        };

        public static Uri BuildUri(EndPoint endPoint)
        {
            UriBuilder uriBuilder = new UriBuilder(BaseUrl);

            switch (endPoint)
            {
                case EndPoint.RSSFeeds:
                    uriBuilder.Path += "RSSFeeds";
                    break;
            }

            return uriBuilder.Uri;
        }
    }
}
