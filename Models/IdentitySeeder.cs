
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
                    var roleResult =
                        await roleManager.CreateAsync(new IdentityRole(role));

                    if (!roleResult.Succeeded)
                    {
                        throw new InvalidOperationException(
                            $"Role creation failed: {role}");
                    }
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

            var existingUser =
                await userManager.FindByEmailAsync(superAdminEmail);

            if (existingUser == null)
            {
                existingUser = new ApplicationUser
                {
                    UserName = superAdminEmail,
                    Email = superAdminEmail,
                    FullName = superAdminName,
                    SocietyId = null,
                    IsActive = true,
                    EmailConfirmed = true
                };

                var createResult = await userManager.CreateAsync(
                    existingUser,
                    superAdminPassword);

                if (!createResult.Succeeded)
                {
                    var errors = string.Join(
                        "; ",
                        createResult.Errors.Select(e => e.Description));

                    throw new InvalidOperationException(
                        $"SuperAdmin creation failed: {errors}");
                }
            }
            else
            {
                if (!await userManager.CheckPasswordAsync(
                    existingUser, superAdminPassword))
                {
                    var token =
                        await userManager.GeneratePasswordResetTokenAsync(
                            existingUser);

                    var resetResult =
                        await userManager.ResetPasswordAsync(
                            existingUser,
                            token,
                            superAdminPassword);

                    if (!resetResult.Succeeded)
                    {
                        var errors = string.Join(
                            "; ",
                            resetResult.Errors.Select(e => e.Description));

                        throw new InvalidOperationException(
                            $"SuperAdmin password reset failed: {errors}");
                    }
                }
            }

            if (!await userManager.IsInRoleAsync(
                existingUser, "SuperAdmin"))
            {
                var roleResult = await userManager.AddToRoleAsync(
                    existingUser, "SuperAdmin");

                if (!roleResult.Succeeded)
                {
                    throw new InvalidOperationException(
                        "Failed to assign SuperAdmin role.");
                }
            }
        }
    }
}
