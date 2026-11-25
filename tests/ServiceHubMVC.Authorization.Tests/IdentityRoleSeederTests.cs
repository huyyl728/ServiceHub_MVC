
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ServiceHubMVC.Data;
using ServiceHubMVC.Models;
using Xunit;

namespace ServiceHubMVC.Authorization.Tests;
[Collection("AuthorizationDatabase")]

public class IdentityRoleSeederTests
{
    [Fact]
    public async Task DbSeeder_TaoDuBaVaiTroVaKhongBiTrungLap()
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
        await DbSeeder.InitializeAsync(provider);

        using var scope = provider.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        var roleNames = await db.Roles
            .Select(r => r.Name!)
            .ToListAsync();

        Assert.Contains("Customer", roleNames);
        Assert.Contains("Owner", roleNames);
        Assert.Contains("Admin", roleNames);

        Assert.Equal(3, roleNames.Count);
    }
}