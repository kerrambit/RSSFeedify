using Microsoft.AspNetCore.Mvc;

namespace RSSFeedify.Controllers
{
    public static class ControllersHelper
    {
        public static ActionResult<T> GetResultForInvalidGuid<T>()
        {
            return new BadRequestObjectResult("Invalid RSSFeedGuid format.");
        }

        public static ActionResult<T> GetResultForDuplicatedSourcerUrl<T>(Uri uri)
        {
            return new BadRequestObjectResult($"Source URL has to be unique. Source URL '{uri}' is duplicated!");
        }

        public static ActionResult GetResultForInvalidLoginAttempt()
        {
            return new BadRequestObjectResult("Invalid login attempt.");
        }

        public static ActionResult GenerateBadRequest(string message)
        {
            return new BadRequestObjectResult(message);
        }

        public static ActionResult GetResultForSuccessfulLoggedOut()
        {
            return new OkObjectResult("You have been successfully logged out.");
        }

        public static ActionResult GetFormattedModelStateErrorMessage(ActionContext actionContext)
        {
            return new BadRequestObjectResult(string.Join("\n", actionContext.ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
        }
    }
}
