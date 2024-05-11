using RSSFeedify.Repository.Types;
using RSSFeedify.Repository.Types.PaginationQuery;

namespace RSSFeedify.Repositories
{
    public interface IRepository<T> where T : Reposable
    {
        Task<RepositoryResult<IEnumerable<T>>> GetAsync();
        Task<RepositoryResult<IEnumerable<T>>> GetAsync(PaginationQuery paginationQuery);
        Task<RepositoryResult<int>> GetTotalCountAsync();
        Task<RepositoryResult<T>> GetAsync(Guid feedGUID);
        RepositoryResult<T> Insert(T feed);
        Task<RepositoryResult<T>> DeleteAsync(Guid feedGUID);
        Task<RepositoryResult<T>> UpdateAsync(Guid feedGUID, T feed);
        Task SaveAsync();
        RepositoryResult<bool> Exists(Guid feedGUID);
    }
}
