using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RSSFeedify.Models;
using System.Reflection.Emit;

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
            options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection"));
        }

        public DbSet<RSSFeed> RSSFeeds { get; set; }
        public DbSet<RSSFeedItem> RSSFeedsItems { get; set; }
    }
}


