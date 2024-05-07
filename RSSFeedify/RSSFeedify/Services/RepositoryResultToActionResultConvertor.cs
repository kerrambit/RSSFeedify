using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using RSSFeedify.Models;

namespace RSSFeedify.Services
{
    public static class RepositoryResultToActionResultConvertor
    {
        public static ActionResult<RSSFeed> Convert(RepositoryResult repositoryResult)
        {
            switch (repositoryResult)
            {
                case Success success:
                    return new OkObjectResult(success.Data);
                default:
                    return new NotFoundResult();
            }
        }
    }
}
