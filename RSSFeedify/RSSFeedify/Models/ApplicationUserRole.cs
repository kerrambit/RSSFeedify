namespace RSSFeedify.Models
{
    public enum ApplicationUserRole
    {
        SuperUser,
        Admin,
        RegularUser
    }

    public static class ApplicationUserRoleExtensions
    {
        public static string[] ConvertApplicationUserRoleExtensionsToStringArray()
        {
            ApplicationUserRole[] roles = (ApplicationUserRole[])Enum.GetValues(typeof(ApplicationUserRole));
            return Array.ConvertAll(roles, role => role.ToString());
        }
    }
}
