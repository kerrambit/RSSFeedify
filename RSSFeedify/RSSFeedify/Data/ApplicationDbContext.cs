using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RSSFeedify.Models;

namespace PostgreSQL.Data
{
    public class ApplicationDbContext : DbContext
    {
        protected readonly IConfiguration Configuration;

        public ApplicationDbContext(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection"));
        }

        public DbSet<RSSFeed> RSSFeeds { get; set; }
    }
}