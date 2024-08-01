using System.ComponentModel.DataAnnotations;

namespace RSSFeedify.Repository.Types.PaginationQuery
{
    public class PaginationQuery
    {
        public int Page { get; }
        public int PageSize { get; }

        public PaginationQuery(int page, int pageSize)
        {
            Page = page;
            PageSize = pageSize;
        }
    }
}
