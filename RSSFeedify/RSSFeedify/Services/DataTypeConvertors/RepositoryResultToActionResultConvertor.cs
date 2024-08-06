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
                    if (IsSimpleType(typeof(T)))
                    {
                        return new ContentResult
                        {
                            StatusCode = 200,
                            Content = success.Data is null ? "" : success.Data.ToString(),
                            ContentType = "text/plain"
                        };
                    }
                    return new OkObjectResult(success.Data);
                case Created<T> create:
                    return new CreatedAtActionResult(create.GetEndPoint, Controllername, new { guid = create.Guid }, create.Data);
                default:
                    return new ContentResult
                    {
                        StatusCode = 404,
                        Content = "Repository query was not successful. Requested resource was not found.",
                        ContentType = "text/plain"
                    };
            }
        }

        private static bool IsSimpleType(Type type)
        {
            return
                type.IsPrimitive ||
                type.IsEnum ||
                type == typeof(string) ||
                type == typeof(decimal);
        }
    }
}
