using Microsoft.AspNetCore.Mvc;
using RSSFeedify.Controllers.Helpers;

namespace RSSFeedifyTests.Controllers.Helpers
{
    public class ControllersHelperTest
    {
        [Fact]
        public void GetResultForInvalidGuid_ReturnsBadRequest()
        {
            // Act
            var result = ControllersHelper.GetResultForInvalidGuid();

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);

            var badRequestResult = result as BadRequestObjectResult;
            Assert.NotNull(badRequestResult);
            Assert.Equal("Invalid RSSFeedGuid format.", badRequestResult.Value);
        }
    }
}
