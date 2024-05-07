namespace RSSFeedify.Models
{
    public abstract class RepositoryResult {}

    public class Success : RepositoryResult
    {
        public object? Data { get; }

        public Success() { }

        public Success(object? data)
        {
            Data = data;
        }
    }

    public class Created : RepositoryResult
    {
        public object? Data { get; }
        public string GetEndPoint { get; }

        public Guid Guid { get; }

        public Created(string getEndPoint, object? data, Guid guid)
        {
            Data = data;
            GetEndPoint = getEndPoint;
            Guid = guid;
        }
    }

    public class NotFoundError : RepositoryResult
    {
        public NotFoundError() { }
    }
}
