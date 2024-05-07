using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using RSSFeedify.Models;
using Created = RSSFeedify.Models.Created;

namespace RSSFeedify.Services
{
    public static class RepositoryResultToActionResultConvertor
    {
        public static readonly string Controllername = "RSSFeeds";

        public static ActionResult<RSSFeed> Convert(RepositoryResult repositoryResult)
        {
            switch (repositoryResult)
            {
                case Success success:
                    return new OkObjectResult(success.Data);
                case Created create:
                    return new CreatedAtActionResult(create.GetEndPoint, Controllername, new { guid = create.Guid }, create.Data);
                default:
                    return new NotFoundResult();
            }
        }
    }
}
