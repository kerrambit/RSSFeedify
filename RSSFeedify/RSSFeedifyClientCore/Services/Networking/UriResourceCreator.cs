﻿using System.Text;

namespace ClientNetLib.Services.Networking
{
    public class UriResourceCreator
    {
        public Uri BaseUrl { get; private set; }

        public enum EndPoint
        {
            RSSFeeds,
            RSSFeedItems,
            ApplicationUser
        };

        public UriResourceCreator(string baseUrl)
        {
            BaseUrl = new(baseUrl);
        }

        public UriResourceCreator(Uri baseUrl)
        {
            BaseUrl = baseUrl;
        }

        public Uri BuildUri(EndPoint endPoint)
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
                case EndPoint.ApplicationUser:
                    uriBuilder.Path += "ApplicationUser";
                    break;
            }

            return uriBuilder.Uri;
        }

        public Uri BuildUri(EndPoint endpoint, string resourcePath)
        {
            string baseUriString = BuildUri(endpoint).ToString();
            string completeUriString = baseUriString + "/" + resourcePath;
            return new Uri(completeUriString);
        }

        public Uri BuildUri(EndPoint endpoint, (string key, string value) queryString)
        {
            string baseUriString = BuildUri(endpoint).ToString();
            string completeUriString = baseUriString + $"?{queryString.key}={queryString.value}";
            return new Uri(completeUriString);
        }

        public Uri BuildUri(EndPoint endpoint, string resourcePath, (string key, string value) queryString)
        {
            string baseUriString = BuildUri(endpoint).ToString();
            string completeUriString = baseUriString + "/" + resourcePath + $"?{queryString.key}={queryString.value}";
            return new Uri(completeUriString);
        }

        public Uri BuildUri(EndPoint endpoint, string resourcePath, IList<(string key, string value)> queryStrings)
        {
            string baseUriString = BuildUri(endpoint).ToString();
            StringBuilder completeUriString = new StringBuilder(baseUriString + "/" + resourcePath);

            if (queryStrings.Count == 0)
            {
                return new Uri(completeUriString.ToString());
            }

            completeUriString.Append("?");
            foreach (var queryString in queryStrings)
            {
                completeUriString.Append($"{queryString.key}={queryString.value}&");
            }

            completeUriString.Remove(completeUriString.Length - 1, 1);
            return new Uri(completeUriString.ToString());
        }

        public Uri BuildUri(EndPoint endpoint, IList<(string key, string value)> queryStrings)
        {
            return BuildUri(endpoint, "", queryStrings);
        }
    }
}
