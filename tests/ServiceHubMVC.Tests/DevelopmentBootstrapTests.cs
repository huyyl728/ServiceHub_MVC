using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using ServiceHubMVC.Data;
using ServiceHubMVC.Models;

namespace ServiceHubMVC.Tests;

public class BootstrapSqlFactAttribute : FactAttribute
{
    public BootstrapSqlFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("SERVICEHUB_BOOTSTRAP_TEST_CONNECTION")))
            Skip = "Requires SERVICEHUB_BOOTSTRAP_TEST_CONNECTION to an isolated SQL Server test instance.";
    }
}

public sealed class DevelopmentBootstrapTests
{
    private sealed class TestEnvironment(string name) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = name;
        public string ApplicationName { get; set; } = "ServiceHubMVC";
        public string ContentRootPath { get; set; } = Directory.GetCurrentDirectory();
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }

    private static ServiceProvider Provider(string connection)
    {
        var services = new ServiceCollection().AddLogging();
        services.AddDbContext<ApplicationDbContext>(o => o.UseSqlServer(connection));
        services.AddIdentityCore<ApplicationUser>(options =>
        {
            options.User.RequireUniqueEmail = true;
            options.Password.RequiredLength = 8;
        }).AddRoles<IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>();
        return services.BuildServiceProvider();
    }

    [Fact]
    public async Task Production_bootstrap_does_not_resolve_database_or_seed_services()
    {
        using var provider = new ServiceCollection().BuildServiceProvider();
        await DevelopmentBootstrapper.InitializeAsync(provider,
            DevelopmentConfigurationTests.DemoConfiguration(), new TestEnvironment("Production"));
    }

    [BootstrapSqlFact]
    public async Task Fresh_database_repeat_concurrent_start_and_conflicts_preserve_data()
    {
        var connection = new SqlConnectionStringBuilder(
            Environment.GetEnvironmentVariable("SERVICEHUB_BOOTSTRAP_TEST_CONNECTION"))
        {
            InitialCatalog = "ServiceHubBootstrapTest_" + Guid.NewGuid().ToString("N")
        };
        using var provider = Provider(connection.ConnectionString);
        var config = DevelopmentConfigurationTests.DemoConfiguration("custom-owner@servicehub.test");
        var environment = new TestEnvironment("Development");
        using var scope = provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        try
        {
            await DevelopmentBootstrapper.InitializeAsync(provider, config, environment);
            Assert.False(db.Database.HasPendingModelChanges());
            Assert.Empty(await db.Database.GetPendingMigrationsAsync());
            Assert.Equal(7, await db.Users.CountAsync());
            Assert.Equal(4, await db.Businesses.CountAsync());
            Assert.Equal(16, await db.Services.CountAsync());
            var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            foreach (var account in SharedDemoAccounts.Read(config))
            {
                var user = await users.FindByEmailAsync(account.Email);
                Assert.NotNull(user);
                Assert.True(await users.CheckPasswordAsync(user, account.Password));
                Assert.Equal(new[] { account.Role }, await users.GetRolesAsync(user));
            }
            var owner = await users.FindByEmailAsync("custom-owner@servicehub.test");
            Assert.All(await db.Businesses.ToListAsync(), b => Assert.Equal(owner!.Id, b.OwnerId));
            var business = await db.Businesses.FirstAsync();
            business.PhoneNumber = "0900000000";
            business.Description = "Keep user edit";
            var service = await db.Services.FirstAsync();
            var customer = await users.FindByEmailAsync("customer1@servicehub.test");
            var booking = new Booking
            {
                CustomerId = customer!.Id, ServiceId = service.Id,
                StartTime = DateTimeOffset.UtcNow.AddDays(1), EndTime = DateTimeOffset.UtcNow.AddDays(1).AddHours(1),
                PriceAtBooking = service.Price
            };
            db.Bookings.Add(booking);
            await db.SaveChangesAsync();
            var userIds = await db.Users.OrderBy(u => u.Id).Select(u => u.Id).ToArrayAsync();

            await Task.WhenAll(Enumerable.Range(0, 2).Select(_ =>
                DevelopmentBootstrapper.InitializeAsync(provider, config, environment)));
            db.ChangeTracker.Clear();
            Assert.Equal(userIds, await db.Users.OrderBy(u => u.Id).Select(u => u.Id).ToArrayAsync());
            Assert.Equal(4, await db.Businesses.CountAsync());
            Assert.Equal(16, await db.Services.CountAsync());
            Assert.Equal("Keep user edit", (await db.Businesses.FindAsync(business.Id))!.Description);
            Assert.Equal("0900000000", (await db.Businesses.FindAsync(business.Id))!.PhoneNumber);
            Assert.True(await db.Bookings.AnyAsync(b => b.Id == booking.Id));

            // A failure after creating an earlier account must roll back that new account.
            var changed = new ConfigurationBuilder().AddConfiguration(config).AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["SeedAccounts:SharedDemo:AdminEmails:0"] = "new-admin@servicehub.test",
                    ["SeedAccounts:SharedDemo:AdminEmails:1"] = "admin2@servicehub.test",
                    ["SeedAccounts:SharedDemo:AdminEmails:2"] = "admin3@servicehub.test",
                    ["SeedAccounts:SharedDemo:AdminEmails:3"] = "admin4@servicehub.test",
                    ["SeedAccounts:SharedDemo:OwnerPassword"] = "Aa1!" + Guid.NewGuid().ToString("N")
                }).Build();
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                DevelopmentBootstrapper.InitializeAsync(provider, changed, environment));
            Assert.False(await db.Users.AnyAsync(u => u.Email == "new-admin@servicehub.test"));
            Assert.Equal(7, await db.Users.CountAsync());
            Assert.True(await db.Bookings.AnyAsync(b => b.Id == booking.Id));

            var conflicting = new ConfigurationBuilder().AddConfiguration(config).AddInMemoryCollection(
                new Dictionary<string, string?> { ["SeedAccounts:SharedDemo:OwnerEmail"] = "admin1@servicehub.test",
                    ["SeedAccounts:SharedDemo:AdminEmails:0"] = "another-admin@servicehub.test",
                    ["SeedAccounts:SharedDemo:AdminEmails:1"] = "admin2@servicehub.test",
                    ["SeedAccounts:SharedDemo:AdminEmails:2"] = "admin3@servicehub.test",
                    ["SeedAccounts:SharedDemo:AdminEmails:3"] = "admin4@servicehub.test" }).Build();
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                DevelopmentBootstrapper.InitializeAsync(provider, conflicting, environment));
            Assert.Equal(7, await db.Users.CountAsync());
        }
        finally
        {
            // Only this test's random database is removed; never touch configured user databases.
            await db.Database.EnsureDeletedAsync();
        }
    }
}
