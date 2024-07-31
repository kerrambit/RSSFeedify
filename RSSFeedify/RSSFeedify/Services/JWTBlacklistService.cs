using StackExchange.Redis;

namespace RSSFeedify.Services
{
    public class JWTBlacklistService
    {
        private readonly RequestDelegate _next;
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _db;

        public JWTBlacklistService(RequestDelegate next, IConnectionMultiplexer redis)
        {
            _next = next;
            _redis = redis;
            _db = _redis.GetDatabase();
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                Console.WriteLine($"[JWTBlacklistService]: received new HTTP context.");

                var token = authHeader.ToString().Split(" ").Last();
                var isBlacklisted = await _db.KeyExistsAsync(token);

                Console.WriteLine("[JWTBlacklistService]: parsed JWT bearer.");

                if (isBlacklisted)
                {
                    Console.WriteLine("[JWTBlacklistService]: JWT bearer is blacklisted.");

                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("You have been logged off.");
                    return;
                }

                Console.WriteLine("[JWTBlacklistService]: JWT bearer was not blacklisted.");
            }

            await _next(context);
        }
    }
}
