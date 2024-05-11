using Microsoft.AspNetCore.Mvc;

namespace RSSFeedify.Controllers
{
    public static class ControllersHelper
    {
        public static ActionResult<T> GetResultForInvalidGuid<T>()
        {
            return new BadRequestObjectResult("Invalid RSSFeedGuid format");
        }
    }
}
