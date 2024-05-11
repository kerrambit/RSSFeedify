using Microsoft.EntityFrameworkCore;
using System.Collections;

namespace RSSFeedify.Repository.Types.Pagination
{
    /// <summary>
    /// The implementation was inspired by the article, see https://steven-giesel.com/blogPost/09285b33-79e6-4879-95e0-35aeae5fbcc6.
    /// </summary>
    public class PagedList<T> : IEnumerable<T>
    {
        public int PageNumber { get; }
        public int PageSize { get; }
        public int Count => _data.Count;

        private readonly IList<T> _data;

        public PagedList(IEnumerable<T> items, int pageNumber, int pageSize)
        {
            PageNumber = pageNumber;
            PageSize = pageSize;
            _data = items as IList<T> ?? new List<T>(items);
        }

        public T this[int index] => _data[index];

        public IEnumerator<T> GetEnumerator()
        {
            return _data.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _data.GetEnumerator();
        }
    }

    public static class PagedListQueryableExtensions
    {
        public static async Task<PagedList<T>> ToPagedListAsync<T>(this IQueryable<T> source, int page, int pageSize, CancellationToken token = default)
        {
            var count = await source.CountAsync(token);
            if (count > 0)
            {
                var items = await source
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(token);
                return new PagedList<T>(items, page, pageSize);
            }

            return new([], 0, 0);
        }
    }
}
