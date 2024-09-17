using Microsoft.AspNetCore.Identity;

namespace Boookify.Web.Seeds
{
    public class DefaultRoles
    {
        public static async Task SeedRoles(RoleManager<IdentityRole> roleManager)
        {
            if (!roleManager.Roles.Any()) 
            {
               await roleManager.CreateAsync(new IdentityRole(AppRoles.Admin));
               await roleManager.CreateAsync(new IdentityRole(AppRoles.Archive));
               await roleManager.CreateAsync(new IdentityRole(AppRoles.Reception));
            }
        }
    }
}
