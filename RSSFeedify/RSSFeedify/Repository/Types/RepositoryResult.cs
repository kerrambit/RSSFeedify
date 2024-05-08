
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

        public Created(string getEndPoint, T data, Guid guid) : base(data)
        {
            GetEndPoint = getEndPoint;
            Guid = guid;
        }
    }

    public class NotFoundError<T> : RepositoryResult<T>
    {
        public NotFoundError() { }
    }
}
