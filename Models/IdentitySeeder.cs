using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using MTSS.Models;

namespace MTSS.Data
{
    public static class IdentitySeeder
    {
        public static async Task SeedRolesAsync(
            IServiceProvider serviceProvider,
            IConfiguration configuration)
        {
            var roleManager = serviceProvider
                .GetRequiredService<RoleManager<IdentityRole>>();

            var userManager = serviceProvider
                .GetRequiredService<UserManager<ApplicationUser>>();

            string[] roles =
            {
                "SuperAdmin",
                "SocietyAdmin",
                "Resident"
            };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            var superAdminEmail = configuration["SuperAdmin:Email"];
            var superAdminPassword = configuration["SuperAdmin:Password"];
            var superAdminName = configuration["SuperAdmin:FullName"];

            if (string.IsNullOrWhiteSpace(superAdminEmail) ||
                   string.IsNullOrWhiteSpace(superAdminPassword) ||
                string.IsNullOrWhiteSpace(superAdminName))
            {
                throw new InvalidOperationException(
                    "SuperAdmin configuration is missing.");
            }

            var existingUser = await userManager.FindByEmailAsync(superAdminEmail);

            if (existingUser == null)
            {
                var superAdmin = new ApplicationUser
                {
                    UserName = superAdminEmail,
                    Email = superAdminEmail,
                    FullName = superAdminName,
                    SocietyId = null,
                    IsActive = true,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(
                    superAdmin,
                    superAdminPassword);

                if (!result.Succeeded)
                {
                    var errors = string.Join(
                        "; ",
                        result.Errors.Select(e => e.Description));

                    throw new InvalidOperationException(
                        $"SuperAdmin creation failed: {errors}");
                }

                await userManager.AddToRoleAsync(
                    superAdmin,
                    "SuperAdmin");
            }
        }
    }
}