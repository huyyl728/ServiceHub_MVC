using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ServiceHubMVC.Models;

namespace ServiceHubMVC.Data;

public static class DevelopmentBootstrapper
{
    public static async Task InitializeAsync(IServiceProvider services, IConfiguration configuration,
        IHostEnvironment environment)
    {
        if (!environment.IsDevelopment()) return;

        // Validate the whole demo configuration before any schema/data changes.
        var accounts = SharedDemoAccounts.Read(configuration);
        using var scope = services.CreateScope();
        var scoped = scope.ServiceProvider;
        var users = scoped.GetRequiredService<UserManager<ApplicationUser>>();
        await SharedDemoAccounts.ValidatePasswordsAsync(users, accounts);
        var db = scoped.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync();

        // EF serializes migrations; serialize the following seed phase as well.
        // All Identity and fixture writes share this transaction and roll back together.
        await using var transaction = await db.Database.BeginTransactionAsync();
        await db.Database.ExecuteSqlRawAsync("""
            DECLARE @result int;
            EXEC @result = sys.sp_getapplock
                @Resource = N'ServiceHub.DevelopmentBootstrap',
                @LockMode = 'Exclusive', @LockOwner = 'Transaction', @LockTimeout = 15000;
            IF @result < 0 THROW 51000, 'Development bootstrap is busy. Retry startup.', 1;
            """);
        await DbSeeder.InitializeAsync(scoped);
        await DevAccountSeeder.InitializeAsync(scoped, configuration);
        await DemoDataSeeder.InitializeAsync(scoped, configuration);
        await transaction.CommitAsync();
    }
}
