using Microsoft.AspNetCore.Identity;

namespace Boookify.Web.Seeds
{
    public static class DefaultUsers
    {
        public static async Task SeedAdminUser(UserManager<ApplicationUser> userManager)
        {
            ApplicationUser admin=new(){
                FullName="admin",
                Email="Admin@Bookify.com",
                UserName="admin",
                EmailConfirmed=true,
            };

            var user = await userManager.FindByEmailAsync(admin.Email);
            if (user is null)
            {
               await userManager.CreateAsync(admin,"P@ssword123");
               await userManager.AddToRoleAsync(admin,AppRoles.Admin);
            }
        }
    }
}
