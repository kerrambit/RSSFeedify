using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RSSFeedify.Models;

namespace PostgreSQL.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        protected readonly IConfiguration Configuration;

        public ApplicationDbContext(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseNpgsql(Environment.GetEnvironmentVariable("RSSFEEDIFY_PG_DB"));
        }

        public DbSet<RSSFeed> RSSFeeds { get; set; }
        public DbSet<RSSFeedItem> RSSFeedsItems { get; set; }
    }
}


