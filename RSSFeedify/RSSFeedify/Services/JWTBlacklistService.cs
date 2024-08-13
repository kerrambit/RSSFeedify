using StackExchange.Redis;

using RSSFeed = RSSFeedify.Models.RSSFeed;
using RSSFeedItem = RSSFeedify.Models.RSSFeedItem;

namespace RSSFeedify.Services
{
    public class JWTBlacklistService
    {
        private readonly RequestDelegate _next;
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _db;
        private readonly ILogger<JWTBlacklistService> _logger;

        public JWTBlacklistService(RequestDelegate next, IConnectionMultiplexer redis, ILogger<JWTBlacklistService> logger)
        {
            _next = next;
            _redis = redis;
            _db = _redis.GetDatabase();
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                var token = authHeader.ToString().Split(" ").Last();
                var isBlacklisted = await _db.KeyExistsAsync(token);

                _logger.LogInformation("New request occured and was parsed.");
                if (isBlacklisted)
                {
                    _logger.LogInformation("JWT was found in the blacklist database. Responding with 'Unauthorized' (401).");

                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.ContentType = "text/plain";
                    await context.Response.WriteAsync("You've been logged out. You will need to log in again to gain access.");
                    return;
                }
                _logger.LogInformation("JWT was not found in the blacklist database. HttpContext is passed on in the pipeline.");
            }

            await _next(context);
        }
    }
}
