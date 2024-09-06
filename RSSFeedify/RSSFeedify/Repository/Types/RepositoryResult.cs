
namespace RSSFeedify.Repository.Types
{
    public abstract class RepositoryResult<T>
    {
        public T Data { get; }

        public RepositoryResult() { }

        public RepositoryResult(T data)
        {
            Data = data;
        }
    }

    public class Success<T> : RepositoryResult<T>
    {
        public Success() { }

        public Success(T data) : base(data) { }
    }

    public class Created<T> : RepositoryResult<T>
    {
        public string GetEndPoint { get; }
        public Guid Guid { get; }

        public Created(T data, Guid guid, string getEndPoint) : base(data)
        {
            GetEndPoint = getEndPoint;
            Guid = guid;
        }

        public Created(T data, Guid guid) : base(data)
        {
            GetEndPoint = string.Empty;
            Guid = guid;
        }
    }

    public class NotFoundError<T> : RepositoryResult<T>
    {
        public NotFoundError() { }
    }

    public class Duplicate<T> : RepositoryResult<T>
    {
        public string Info { get; }

        public Duplicate(string info)
        {
            Info = info;
        }
    }

    public class RepositoryConcurrencyError<T> : RepositoryResult<T>
    {
        public RepositoryConcurrencyError() { }
    }
}
