using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ServiceHubMVC.Models;

namespace ServiceHubMVC.Data;

// Chỉ chạy Development khi bộ tài khoản demo được bật.
// Kiểm tra khóa nghiệp vụ để không tạo trùng và không sửa dữ liệu hiện có.
public static class DemoDataSeeder
{
    private sealed record DemoService(
        string Name, string Category, decimal Price, int DurationMinutes);

    private sealed record DemoBusiness(
        string Name, string Address, DemoService[] Services);

    public static async Task InitializeAsync(
        IServiceProvider services,
        IConfiguration configuration)
    {
        if (!configuration.GetValue<bool>("SeedAccounts:SharedDemo:Enabled"))
            return;

        var db = services.GetRequiredService<ApplicationDbContext>();
        var users = services.GetRequiredService<UserManager<ApplicationUser>>();

        var owner = await users.FindByEmailAsync(SharedDemoAccounts.Read(configuration).Single(a => a.Role == "Owner").Email)
            ?? throw new InvalidOperationException(
                "Không tìm thấy Owner demo để khởi tạo dữ liệu cơ sở.");

        if (!await users.IsInRoleAsync(owner, "Owner"))
            throw new InvalidOperationException("Tài khoản Owner demo chưa có quyền Owner.");

        var fixtures = new[]
        {
            new DemoBusiness("[DEMO] Sân thể thao",
                "Địa chỉ minh họa – khu thể thao, Hà Nội",
                new[]
                {
                    new DemoService("Đặt sân cầu lông", "Thể thao", 90000m, 60),
                    new DemoService("Đặt sân bóng đá", "Thể thao", 250000m, 90),
                    new DemoService("Đặt sân pickleball", "Thể thao", 150000m, 60),
                    new DemoService("Đặt sân bóng rổ", "Thể thao", 120000m, 60)
                }),
            new DemoBusiness("[DEMO] Salon tóc",
                "Địa chỉ minh họa – khu salon, Hà Nội",
                new[]
                {
                    new DemoService("Cắt tóc", "Làm đẹp", 80000m, 30),
                    new DemoService("Gội đầu", "Làm đẹp", 60000m, 30),
                    new DemoService("Tạo kiểu tóc", "Làm đẹp", 180000m, 60),
                    new DemoService("Chăm sóc tóc", "Làm đẹp", 250000m, 90)
                }),
            new DemoBusiness("[DEMO] Spa thư giãn",
                "Địa chỉ minh họa – khu spa, Hà Nội",
                new[]
                {
                    new DemoService("Massage thư giãn", "Spa", 350000m, 60),
                    new DemoService("Chăm sóc da mặt", "Spa", 300000m, 60),
                    new DemoService("Ngâm chân", "Spa", 150000m, 30),
                    new DemoService("Xông hơi", "Spa", 180000m, 45)
                }),
            new DemoBusiness("[DEMO] Nhà hàng",
                "Địa chỉ minh họa – khu nhà hàng, Hà Nội",
                new[]
                {
                    new DemoService("Đặt bàn 2 người", "Nhà hàng", 0m, 90),
                    new DemoService("Đặt bàn 4 người", "Nhà hàng", 0m, 90),
                    new DemoService("Đặt bàn nhóm", "Nhà hàng", 0m, 120),
                    new DemoService("Đặt phòng riêng", "Nhà hàng", 0m, 120)
                })
        };

        foreach (var sample in fixtures)
        {
            var business = await db.Businesses
                .SingleOrDefaultAsync(x => x.OwnerId == owner.Id && x.Name == sample.Name);

            if (business is null)
            {
                business = new Business
                {
                    Name = sample.Name,
                    Address = sample.Address,
                    Description = "Cơ sở giả lập phục vụ kiểm thử nội bộ.",
                    OwnerId = owner.Id,
                    IsActive = true
                };
                db.Businesses.Add(business);
                await db.SaveChangesAsync();
            }

            foreach (var service in sample.Services)
            {
                if (await db.Services.AnyAsync(
                    x => x.BusinessId == business.Id && x.Name == service.Name))
                    continue;

                db.Services.Add(new Service
                {
                    BusinessId = business.Id,
                    Name = service.Name,
                    Category = service.Category,
                    Description = "Dịch vụ demo, không nhận đặt lịch ngoài đời thực.",
                    Price = service.Price,
                    DurationMinutes = service.DurationMinutes,
                    IsAvailable = true
                });
            }

            await db.SaveChangesAsync();
        }
    }
}
