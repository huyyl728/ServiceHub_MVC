
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ServiceHubMVC.Data;
using ServiceHubMVC.Models;
using Xunit;

namespace ServiceHubMVC.Authorization.Tests;
[Collection("AuthorizationDatabase")]
public class IdentityPasswordPolicyTests
{
    [Fact]
    public async Task Identity_KiemTraMatKhauVaEmailTrung()
    {
        var connectionString = Environment.GetEnvironmentVariable(
            "SERVICEHUB_AUTH_TEST_CONNECTION");

        Assert.False(string.IsNullOrWhiteSpace(connectionString));

        var connectionBuilder = new SqlConnectionStringBuilder(
            connectionString);

        Assert.Equal(
            "ServiceHubMVC_AuthTest",
            connectionBuilder.InitialCatalog,
            ignoreCase: true);

        var services = new ServiceCollection();

        services.AddLogging();

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionBuilder.ConnectionString));

        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.User.RequireUniqueEmail = true;

            options.Password.RequiredLength = 8;
            options.Password.RequireDigit = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireNonAlphanumeric = true;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        using var provider = services.BuildServiceProvider();

        using var scope = provider.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<ApplicationUser>>();

        Assert.True(await db.Database.CanConnectAsync());

        await using var transaction =
            await db.Database.BeginTransactionAsync();

        var suffix = Guid.NewGuid().ToString("N");
        var email = $"test-{suffix}@example.test";

        var invalidUser = new ApplicationUser
        {
            UserName = $"invalid-{suffix}",
            Email = $"invalid-{suffix}@example.test",
            FullName = "Tài khoản kiểm thử"
        };

        var invalidResult = await userManager.CreateAsync(
            invalidUser,
            "abc");

        Assert.False(invalidResult.Succeeded);

        Assert.Contains(
            invalidResult.Errors,
            error => error.Code == "PasswordTooShort");

        var validUser = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = "Khách hàng kiểm thử"
        };

        var validResult = await userManager.CreateAsync(
            validUser,
            "Abcdef12!");

        Assert.True(
            validResult.Succeeded,
            string.Join("; ",
                validResult.Errors.Select(e => e.Description)));

        var duplicateUser = new ApplicationUser
        {
            UserName = $"duplicate-{suffix}",
            Email = email,
            FullName = "Tài khoản trùng email"
        };

        var duplicateResult = await userManager.CreateAsync(
            duplicateUser,
            "Abcdef12!");

        Assert.False(duplicateResult.Succeeded);

        Assert.Contains(
            duplicateResult.Errors,
            error => error.Code == "DuplicateEmail");

        await transaction.RollbackAsync();
    }
    [Fact]
    public async Task TaiKhoanMoi_KhongTuDongCoQuyenAdminHoacOwner()
    {
        var connectionString = Environment.GetEnvironmentVariable(
            "SERVICEHUB_AUTH_TEST_CONNECTION");

        Assert.False(string.IsNullOrWhiteSpace(connectionString));

        var connectionBuilder = new SqlConnectionStringBuilder(
            connectionString);

        Assert.Equal(
            "ServiceHubMVC_AuthTest",
            connectionBuilder.InitialCatalog,
            ignoreCase: true);

        var services = new ServiceCollection();

        services.AddLogging();

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionBuilder.ConnectionString));

        services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        using var provider = services.BuildServiceProvider();

        await DbSeeder.InitializeAsync(provider);

        using var scope = provider.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<ApplicationUser>>();

        await using var transaction =
            await db.Database.BeginTransactionAsync();

        var suffix = Guid.NewGuid().ToString("N");

        var email = $"customer-{suffix}@example.test";

        var customer = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = "Khách hàng kiểm thử"
        };

        var createResult = await userManager.CreateAsync(
            customer,
            "Abcdef12!");

        Assert.True(createResult.Succeeded);

        Assert.False(
            await userManager.IsInRoleAsync(customer, "Admin"));

        Assert.False(
            await userManager.IsInRoleAsync(customer, "Owner"));

        var roleResult = await userManager.AddToRoleAsync(
            customer,
            "Customer");

        Assert.True(roleResult.Succeeded);

        Assert.True(
            await userManager.IsInRoleAsync(customer, "Customer"));

        Assert.False(
            await userManager.IsInRoleAsync(customer, "Admin"));

        Assert.False(
            await userManager.IsInRoleAsync(customer, "Owner"));

        await transaction.RollbackAsync();
    }

    [Fact]
    public async Task Identity_XacMinhMatKhauVaTuChoiMatKhauSai()
    {
        var connectionString = Environment.GetEnvironmentVariable(
            "SERVICEHUB_AUTH_TEST_CONNECTION");

        Assert.False(string.IsNullOrWhiteSpace(connectionString));

        var connectionBuilder = new SqlConnectionStringBuilder(
            connectionString);

        Assert.Equal(
            "ServiceHubMVC_AuthTest",
            connectionBuilder.InitialCatalog,
            ignoreCase: true);

        var services = new ServiceCollection();

        services.AddLogging();

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionBuilder.ConnectionString));

        services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<ApplicationUser>>();

        Assert.True(await db.Database.CanConnectAsync());

        await using var transaction =
            await db.Database.BeginTransactionAsync();

        var suffix = Guid.NewGuid().ToString("N");
        var email = $"password-test-{suffix}@example.test";

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = "Tài khoản kiểm thử mật khẩu"
        };

        const string correctPassword = "Abcdef12!";
        const string wrongPassword = "WrongPassword12!";

        var createResult = await userManager.CreateAsync(
            user,
            correctPassword);

        Assert.True(
            createResult.Succeeded,
            string.Join("; ",
                createResult.Errors.Select(e => e.Description)));

        Assert.False(string.IsNullOrWhiteSpace(user.PasswordHash));
        Assert.NotEqual(correctPassword, user.PasswordHash);

        var correctResult = await userManager.CheckPasswordAsync(
            user,
            correctPassword);

        var wrongResult = await userManager.CheckPasswordAsync(
            user,
            wrongPassword);

        Assert.True(correctResult);
        Assert.False(wrongResult);

        await transaction.RollbackAsync();
    }

}