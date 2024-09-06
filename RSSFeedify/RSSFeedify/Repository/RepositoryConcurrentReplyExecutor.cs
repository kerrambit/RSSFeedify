using Microsoft.EntityFrameworkCore;
using Npgsql;
using PostgreSQL.Data;
using RSSFeedify.Repository.Types;

namespace RSSFeedify.Repository
{
    public static class RepositoryConcurrentReplyExecutor
    {
        public static async Task<RepositoryResult<T>> ExecuteWithRetryAsync<T>(Func<ApplicationDbContext, Task<RepositoryResult<T>>> work, ApplicationDbContext context, int maxRetry = 3, int delayInMs = 1000)
        {
            int retry = 0;
            while (retry < maxRetry)
            {
                try
                {
                    using (var transaction = context.Database.BeginTransaction(System.Data.IsolationLevel.Serializable))
                    {
                        var result = await work(context);
                        transaction.Commit();
                        return result;
                    }
                }
                catch (InvalidOperationException ex)
                {
                    if (ex.InnerException is not null && ex.InnerException is DbUpdateException && ex.InnerException.InnerException is not null && ex.InnerException.InnerException is PostgresException && ((PostgresException)(ex.InnerException.InnerException)).SqlState == "40001")
                    {
                        retry++;
                        await Task.Delay(delayInMs);
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return new RepositoryConcurrencyError<T>();
        }
    }
}
