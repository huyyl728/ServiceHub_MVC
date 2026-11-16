using Microsoft.AspNetCore.Identity;
using ServiceHubMVC.Models;

namespace ServiceHubMVC.Data;

public static class DevAccountSeeder
{
    public static async Task InitializeAsync(
        IServiceProvider services,
        IConfiguration configuration)
    {
        using var scope = services.CreateScope();

        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<ApplicationUser>>();

        // Khởi tạo nhiều tài khoản Admin
        foreach (var admin in configuration
            .GetSection("SeedAccounts:Admins")
            .GetChildren())
        {
            await CreateUserAsync(
                userManager,
                admin["Email"],
                admin["Password"],
                admin["FullName"] ?? "ServiceHub Admin",
                "Admin");
        }

        // Khởi tạo tài khoản Owner
        await CreateUserAsync(
            userManager,
            configuration["SeedAccounts:Owner:Email"],
            configuration["SeedAccounts:Owner:Password"],
            "ServiceHub Owner",
            "Owner");
    }

    private static async Task CreateUserAsync(
        UserManager<ApplicationUser> userManager,
        string? email,
        string? password,
        string fullName,
        string role)
    {
        if (string.IsNullOrWhiteSpace(email) &&
            string.IsNullOrWhiteSpace(password))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException(
                $"Missing seed configuration for {role}.");
        }

        var existingUser =
            await userManager.FindByEmailAsync(email);

        if (existingUser != null)
        {
            if (!await userManager.IsInRoleAsync(existingUser, role))
            {
                throw new InvalidOperationException(
                    $"Existing account does not have role {role}.");
            }

            return;
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = fullName,
            EmailConfirmed = true
        };

        var createResult =
            await userManager.CreateAsync(user, password);

        EnsureSuccess(createResult);

        var roleResult =
            await userManager.AddToRoleAsync(user, role);

        EnsureSuccess(roleResult);
    }

    private static void EnsureSuccess(IdentityResult result)
    {
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                string.Join("; ",
                    result.Errors.Select(e => e.Description)));
        }
    }
}