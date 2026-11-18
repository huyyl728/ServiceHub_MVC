using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using ServiceHubMVC.Models;

namespace ServiceHubMVC.Data;

public static class SharedDemoAccounts
{
    // Never use a record here: its generated ToString() would include the password.
    public sealed class Account(string email, string password, string fullName, string role)
    {
        public string Email { get; } = email;
        public string Password { get; } = password;
        public string FullName { get; } = fullName;
        public string Role { get; } = role;
    }

    public static Account[] Read(IConfiguration configuration)
    {
        var section = configuration.GetSection("SeedAccounts:SharedDemo");
        if (!section.GetValue<bool>("Enabled")) return [];

        var admins = section.GetSection("AdminEmails").Get<string[]>()
            ?? Enumerable.Range(1, 4).Select(n => $"admin{n}@servicehub.test").ToArray();
        var customers = section.GetSection("CustomerEmails").Get<string[]>()
            ?? Enumerable.Range(1, 2).Select(n => $"customer{n}@servicehub.test").ToArray();
        if (admins.Length != 4 || customers.Length != 2)
            throw new InvalidOperationException("SharedDemo cần đúng 4 AdminEmails và 2 CustomerEmails.");

        string Password(string key)
        {
            var value = section[key];
            if (string.IsNullOrWhiteSpace(value) || value.StartsWith("<", StringComparison.Ordinal))
                throw new InvalidOperationException($"Thiếu SeedAccounts:SharedDemo:{key}. Xem docs/DEV_SETUP.md.");
            return value;
        }

        var accounts = admins.Select((email, n) => new Account(email?.Trim() ?? "",
                Password("AdminPassword"), $"Quản trị viên demo {n + 1}", "Admin"))
            .Append(new Account(section["OwnerEmail"]?.Trim() ?? "owner@servicehub.test",
                Password("OwnerPassword"), "Chủ cơ sở demo", "Owner"))
            .Concat(customers.Select((email, n) => new Account(email?.Trim() ?? "",
                Password("CustomerPassword"), $"Khách hàng demo {n + 1}", "Customer")))
            .ToArray();

        if (accounts.Any(a => !new EmailAddressAttribute().IsValid(a.Email)) ||
            accounts.Select(a => a.Email).Distinct(StringComparer.OrdinalIgnoreCase).Count() != 7)
            throw new InvalidOperationException("SharedDemo cần 7 email hợp lệ, không trùng nhau.");
        return accounts;
    }

    public static async Task ValidatePasswordsAsync(UserManager<ApplicationUser> users, Account[] accounts)
    {
        foreach (var account in accounts)
        foreach (var validator in users.PasswordValidators)
        {
            var result = await validator.ValidateAsync(users,
                new ApplicationUser { UserName = account.Email, Email = account.Email }, account.Password);
            if (!result.Succeeded)
                throw new InvalidOperationException(
                    $"Mật khẩu SharedDemo cho {account.Role} không đạt chính sách Identity (tối thiểu 8 ký tự, hoa, thường, số, ký tự đặc biệt).");
        }
    }
}
