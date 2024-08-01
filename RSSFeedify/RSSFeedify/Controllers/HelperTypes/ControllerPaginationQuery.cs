using System.ComponentModel.DataAnnotations;

namespace RSSFeedify.Controllers.HelperTypes
{
    public record ControllerPaginationQuery
    {
        [Range(1, int.MaxValue, ErrorMessage = "Page index size must be at least 1.")]
        public int Page { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Page size must be at least 1.")]
        public int PageSize { get; set; }
    }
}
