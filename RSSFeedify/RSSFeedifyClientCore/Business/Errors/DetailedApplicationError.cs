﻿namespace RSSFeedifyClientCore.Business.Errors
{
    public record DetailedApplicationError(ApplicationError Error, string Details)
    {
        public bool HasDetails()
        {
            return Details.Length != 0;
        }
    }
}
