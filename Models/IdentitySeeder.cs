
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

            // SUPER ADMIN CONFIGURATION
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

            var superAdmin =
                await userManager.FindByEmailAsync(superAdminEmail);

            if (superAdmin == null)
            {
                superAdmin = new ApplicationUser
                {
                    UserName = superAdminEmail,
                    Email = superAdminEmail,
                    FullName = superAdminName,
                    SocietyId = null,
                    IsActive = true,
                    EmailConfirmed = true
                };

                var createResult = await userManager.CreateAsync(
                    superAdmin,
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
            else if (!await userManager.CheckPasswordAsync(
                superAdmin, superAdminPassword))
            {
                var token =
                    await userManager.GeneratePasswordResetTokenAsync(
                        superAdmin);

                var resetResult =
                    await userManager.ResetPasswordAsync(
                        superAdmin,
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

            if (!await userManager.IsInRoleAsync(
                superAdmin, "SuperAdmin"))
            {
                var roleResult = await userManager.AddToRoleAsync(
                    superAdmin, "SuperAdmin");

                if (!roleResult.Succeeded)
                {
                    throw new InvalidOperationException(
                        "Failed to assign SuperAdmin role.");
                }
            }

            // SOCIETY ADMIN CONFIGURATION
            var societyAdminEmail = configuration["SocietyAdmin:Email"];
            var societyAdminPassword = configuration["SocietyAdmin:Password"];

            if (!string.IsNullOrWhiteSpace(societyAdminEmail) &&
                !string.IsNullOrWhiteSpace(societyAdminPassword))
            {
                var societyAdminName =
                    configuration["SocietyAdmin:FullName"]
                    ?? "Society Admin";

                var societyAdmin =
                    await userManager.FindByEmailAsync(societyAdminEmail);

                if (societyAdmin == null)
                {
                    societyAdmin = new ApplicationUser
                    {
                        UserName = societyAdminEmail,
                        Email = societyAdminEmail,
                        FullName = societyAdminName,
                        SocietyId = 1,
                        IsActive = true,
                        EmailConfirmed = true
                    };

                    var createResult = await userManager.CreateAsync(
                        societyAdmin,
                        societyAdminPassword);

                    if (!createResult.Succeeded)
                    {
                        var errors = string.Join(
                            "; ",
                            createResult.Errors.Select(e => e.Description));

                        throw new InvalidOperationException(
                            $"SocietyAdmin creation failed: {errors}");
                    }

                    Console.WriteLine(
                        "[Seeder] SocietyAdmin account created.");
                }
                else
                {
                    societyAdmin.FullName = societyAdminName;
                    societyAdmin.SocietyId = 1;
                    societyAdmin.IsActive = true;
                    societyAdmin.EmailConfirmed = true;

                    var updateResult =
                        await userManager.UpdateAsync(societyAdmin);

                    if (!updateResult.Succeeded)
                    {
                        var errors = string.Join(
                            "; ",
                            updateResult.Errors.Select(e => e.Description));

                        throw new InvalidOperationException(
                            $"SocietyAdmin update failed: {errors}");
                    }

                    if (!await userManager.CheckPasswordAsync(
                        societyAdmin, societyAdminPassword))
                    {
                        var token =
                            await userManager.GeneratePasswordResetTokenAsync(
                                societyAdmin);

                        var resetResult =
                            await userManager.ResetPasswordAsync(
                                societyAdmin,
                                token,
                                societyAdminPassword);

                        if (!resetResult.Succeeded)
                        {
                            var errors = string.Join(
                                "; ",
                                resetResult.Errors.Select(e => e.Description));

                            throw new InvalidOperationException(
                                $"SocietyAdmin password reset failed: {errors}");
                        }
                    }
                }

                if (!await userManager.IsInRoleAsync(
                    societyAdmin, "SocietyAdmin"))
                {
                    var roleResult = await userManager.AddToRoleAsync(
                        societyAdmin, "SocietyAdmin");

                    if (!roleResult.Succeeded)
                    {
                        throw new InvalidOperationException(
                            "Failed to assign SocietyAdmin role.");
                    }
                }

                Console.WriteLine(
                    "[Seeder] SocietyAdmin setup completed.");
            }
            else
            {
                Console.WriteLine(
                    "[Seeder] SocietyAdmin configuration missing. Skipped.");
            }
        }
    }
}
