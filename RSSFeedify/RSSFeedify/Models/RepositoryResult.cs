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

    public class NotFoundError : RepositoryResult
    {
        public NotFoundError() { }
    }
}
