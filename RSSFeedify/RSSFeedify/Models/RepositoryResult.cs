using System;

namespace RSSFeedify.Models
{
    public abstract class RepositoryResult<T> // Define RepositoryResult as a generic class
    {
        public T Data { get; } // Data property is of type T

        public RepositoryResult() { }

        public RepositoryResult(T data)
        {
            Data = data;
        }
    }

    public class Success<T> : RepositoryResult<T> // Success is a generic class derived from RepositoryResult
    {
        public Success() { }

        public Success(T data) : base(data) { }
    }

    public class Created<T> : RepositoryResult<T> // Created is a generic class derived from RepositoryResult
    {
        public string GetEndPoint { get; }
        public Guid Guid { get; }

        public Created(string getEndPoint, T data, Guid guid) : base(data)
        {
            GetEndPoint = getEndPoint;
            Guid = guid;
        }
    }

    public class NotFoundError<T> : RepositoryResult<T> // NotFoundError is a generic class derived from RepositoryResult
    {
        public NotFoundError() { }
    }
}
