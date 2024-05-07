using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using RSSFeedify.Models;

namespace RSSFeedify.Services
{
    public static class RepositoryResultToActionResultConvertor<T>
    {
        public static readonly string Controllername = "RSSFeeds";

        public static ActionResult<RSSFeed> Convert(RepositoryResult<T> repositoryResult)
        {
            switch (repositoryResult)
            {
                case Success<T> success:
                    return new OkObjectResult(success.Data);
                case Models.Created<T> create:
                    return new CreatedAtActionResult(create.GetEndPoint, Controllername, new { guid = create.Guid }, create.Data);
                default:
                    return new NotFoundResult();
            }
        }
    }
}
