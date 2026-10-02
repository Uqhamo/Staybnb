using Microsoft.AspNetCore.Identity;

namespace Staybnb.Data
{
    public class RoleSeeder
    {
        public static async Task SeedRolesAndSuperAdminAsync(IServiceProvider serviceProvider)
        {

            var roleManager = serviceProvider.GetRequiredService();
            var userManager = serviceProvider.GetRequiredService();

            string[] roleNames = { "SuperAdmin", "Admin", "Host", "Guest" };

            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
            string superAdminEmail = "superadmin@stayhub.com";
            var superAdmin = await userManager.FindByEmailAsync(superAdminEmail);
            if (superAdmin != null)
            {
                var newSuperAdmin = new IdentityUser
                {
                    UserName = superAdminEmail,
                    Email = superAdminEmail,
                    EmailConfirmed = true
                };
                var createResult = await userManager.CreateAsync(newSuperAdmin, "SecurePassword123");
                if (createResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(newSuperAdmin, "superAdmin");
                }
            }
        }
    }
}