using Microsoft.AspNetCore.Identity;
using ServiceHubMVC.Models;

namespace ServiceHubMVC.Data;

public static class DevAccountSeeder
{
    public static async Task InitializeAsync(
        IServiceProvider services,
        IConfiguration configuration)
    {
        var userManager = services
            .GetRequiredService<UserManager<ApplicationUser>>();

        var accounts = SharedDemoAccounts.Read(configuration);
        if (accounts.Length > 0)
        {
            await SharedDemoAccounts.ValidatePasswordsAsync(userManager, accounts);
            foreach (var account in accounts)
                await CreateUserAsync(userManager, account.Email, account.Password,
                    account.FullName, account.Role, verifyPassword: true);
            return; // Shared demo replaces legacy seed settings, avoiding extra accounts.
        }

        foreach (var admin in configuration.GetSection("SeedAccounts:Admins").GetChildren())
            await CreateUserAsync(userManager, admin["Email"], admin["Password"],
                admin["FullName"] ?? "ServiceHub Admin", "Admin");
        await CreateUserAsync(userManager, configuration["SeedAccounts:Owner:Email"],
            configuration["SeedAccounts:Owner:Password"], "ServiceHub Owner", "Owner");
    }

    private static async Task CreateUserAsync(
        UserManager<ApplicationUser> userManager,
        string? email,
        string? password,
        string fullName,
        string role,
        bool verifyPassword = false)
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
            var roles = await userManager.GetRolesAsync(existingUser);
            if (!roles.Contains(role) || (verifyPassword && roles.Count != 1))
            {
                throw new InvalidOperationException(
                    $"Existing account does not have role {role}.");
            }

            if (verifyPassword && !await userManager.CheckPasswordAsync(existingUser, password))
                throw new InvalidOperationException(
                    $"Mật khẩu demo {role} không khớp tài khoản hiện có. Dùng đúng file nội bộ hoặc database dev mới; không tự đặt lại mật khẩu.");
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
