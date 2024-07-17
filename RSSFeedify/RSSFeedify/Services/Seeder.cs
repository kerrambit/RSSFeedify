using Microsoft.AspNetCore.Identity;
using PostgreSQL.Data;
using RSSFeedify.Models;

namespace RSSFeedify.Services
{
    public class Seeder
    {
        public static async Task Run(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetService<ApplicationDbContext>();

                string[] roles = ApplicationUserRoleExtensions.ConvertApplicationUserRoleExtensionsToStringArray();

                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                foreach (string role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                    {
                        await roleManager.CreateAsync(new IdentityRole(role));
                    }
                }

                await context.SaveChangesAsync();
            }
        }

    }
}
