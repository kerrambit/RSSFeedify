using Microsoft.AspNetCore.Mvc;
using RSSFeedify.Repository.Types;

namespace RSSFeedify.Services.DataTypeConvertors
{
    public static class RepositoryResultToActionResultConvertor<T>
    {
        public static readonly string Controllername = "RSSFeeds";

        public static ActionResult<T> Convert(RepositoryResult<T> repositoryResult)
        {
            switch (repositoryResult)
            {
                case Success<T> success:
                    return new OkObjectResult(success.Data);
                case Created<T> create:
                    return new CreatedAtActionResult(create.GetEndPoint, Controllername, new { guid = create.Guid }, create.Data);
                default:
                    return new NotFoundResult();
            }
        }
    }
}
