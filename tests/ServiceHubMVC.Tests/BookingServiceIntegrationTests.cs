using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ServiceHubMVC.Data;
using ServiceHubMVC.Models;
using ServiceHubMVC.Services;
using Xunit;

namespace ServiceHubMVC.Tests;

public class BookingServiceIntegrationTests
{
    [Fact]
    public async Task DatLichHopLe_VaTuChoiLichTrung()
    {
        // Chỉ cho phép kiểm thử trên database riêng
        var password = Environment.GetEnvironmentVariable(
            "SERVICEHUB_TEST_SQL_PASSWORD");

        var configuredConnection = Environment.GetEnvironmentVariable("SERVICEHUB_TEST_CONNECTION");
        Assert.True(!string.IsNullOrWhiteSpace(configuredConnection) || !string.IsNullOrWhiteSpace(password),
            "Set SERVICEHUB_TEST_CONNECTION to the isolated ServiceHubDB_Test database.");
        var connectionBuilder = string.IsNullOrWhiteSpace(configuredConnection)
            ? new SqlConnectionStringBuilder
            {
                DataSource = "localhost,1433", InitialCatalog = "ServiceHubDB_Test",
                UserID = "sa", Password = password, TrustServerCertificate = true
            }
            : new SqlConnectionStringBuilder(configuredConnection);
        Assert.Equal("ServiceHubDB_Test", connectionBuilder.InitialCatalog, ignoreCase: true);

        var services = new ServiceCollection();

        services.AddLogging();

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionBuilder.ConnectionString));

        services.AddIdentityCore<ApplicationUser>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

        services.AddScoped<BookingService>();

        using var provider = services.BuildServiceProvider();

        using var scope = provider.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<ApplicationUser>>();

        var bookingService = scope.ServiceProvider
            .GetRequiredService<BookingService>();

        // Xác nhận kết nối tới database kiểm thử
        Assert.True(await db.Database.CanConnectAsync());

        // Tìm Owner đã khởi tạo trong database kiểm thử
        var ownerId = await (
            from user in db.Users
            join userRole in db.UserRoles
                on user.Id equals userRole.UserId
            join role in db.Roles
                on userRole.RoleId equals role.Id
            where role.Name == "Owner"
            select user.Id
        ).FirstOrDefaultAsync();

        Assert.False(string.IsNullOrEmpty(ownerId));

        // Tạo Customer thử nghiệm không có mật khẩu đăng nhập
        var email = $"test-{Guid.NewGuid():N}@servicehub.test";

        var customer = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = "Khách hàng kiểm thử"
        };

        int? businessId = null;
        int? serviceId = null;
        bool customerCreated = false;

        try
        {
            var createResult =
                await userManager.CreateAsync(customer);

            Assert.True(
                createResult.Succeeded,
                string.Join("; ",
                    createResult.Errors.Select(e => e.Description)));

            customerCreated = true;

            var roleResult = await userManager.AddToRoleAsync(
                customer, "Customer");

            Assert.True(roleResult.Succeeded);

            // Tạo cơ sở và dịch vụ kiểm thử
            var business = new Business
            {
                Name = "Cơ sở kiểm thử Booking",
                Address = "Địa chỉ kiểm thử",
                OwnerId = ownerId!,
                IsActive = true
            };

            db.Businesses.Add(business);
            await db.SaveChangesAsync();
            businessId = business.Id;

            var service = new ServiceHubMVC.Models.Service
            {
                Name = "Dịch vụ kiểm thử",
                Category = "Kiểm thử",
                Price = 150000,
                DurationMinutes = 60,
                IsAvailable = true,
                BusinessId = business.Id
            };

            db.Services.Add(service);
            await db.SaveChangesAsync();
            serviceId = service.Id;

            // Thử đặt lịch hợp lệ
            var startTime = DateTimeOffset.UtcNow.AddDays(2);

            var firstResult =
                await bookingService.CreateBookingAsync(
                    customer.Id, service.Id, startTime);

            Assert.Equal(
                BookingResultCode.Success,
                firstResult.Code);

            Assert.NotNull(firstResult.BookingId);

            // Thử đặt trùng chính khung giờ đó
            var secondResult =
                await bookingService.CreateBookingAsync(
                    customer.Id, service.Id, startTime);

            Assert.Equal(
                BookingResultCode.TimeConflict,
                secondResult.Code);

            // Kiểm tra SQL Server chỉ có một lịch giữ chỗ
            var bookings = await db.Bookings
                .AsNoTracking()
                .Where(b => b.ServiceId == service.Id)
                .ToListAsync();

            Assert.Single(bookings);
            Assert.Equal(BookingStatus.Pending, bookings[0].Status);
            Assert.Equal(150000m, bookings[0].PriceAtBooking);
            Assert.Equal(
                startTime.AddMinutes(60),
                bookings[0].EndTime);
                // Kiểm thử hai yêu cầu đặt lịch đồng thời
// bằng hai phạm vi DbContext độc lập
var concurrentStartTime = startTime.AddDays(1);

using var scopeA = provider.CreateScope();
using var scopeB = provider.CreateScope();

var bookingServiceA = scopeA.ServiceProvider
    .GetRequiredService<BookingService>();

var bookingServiceB = scopeB.ServiceProvider
    .GetRequiredService<BookingService>();

// Đồng bộ thời điểm bắt đầu hai yêu cầu
var startSignal = new TaskCompletionSource<bool>(
    TaskCreationOptions.RunContinuationsAsynchronously);

async Task<BookingResult> DatLichDongThoiAsync(
    BookingService bookingService)
{
    await startSignal.Task;

    return await bookingService.CreateBookingAsync(
        customer.Id,
        service.Id,
        concurrentStartTime);
}

// Chuẩn bị hai yêu cầu đặt cùng một dịch vụ
var taskA = DatLichDongThoiAsync(bookingServiceA);
var taskB = DatLichDongThoiAsync(bookingServiceB);

// Cho phép cả hai yêu cầu bắt đầu
startSignal.SetResult(true);

var results = await Task.WhenAll(taskA, taskB);

// Chỉ một yêu cầu được đặt lịch thành công
Assert.Equal(
    1,
    results.Count(r =>
        r.Code == BookingResultCode.Success));

// Yêu cầu còn lại phải bị từ chối hoặc yêu cầu thử lại
Assert.All(results, result =>
    Assert.Contains(
        result.Code,
        new[]
        {
            BookingResultCode.Success,
            BookingResultCode.TimeConflict,
            BookingResultCode.Busy
        }));

// Kiểm tra trực tiếp dữ liệu đã lưu trong SQL Server
var concurrentBookings = await db.Bookings
    .AsNoTracking()
    .Where(b =>
        b.ServiceId == service.Id
        && b.StartTime == concurrentStartTime
        && (b.Status == BookingStatus.Pending
            || b.Status == BookingStatus.Confirmed))
    .ToListAsync();

Assert.Single(concurrentBookings);
// KIỂM THỬ 1: Đặt lịch sát giờ kết thúc của lịch trước
var adjacentStartTime = startTime.AddMinutes(60);

var adjacentResult =
    await bookingService.CreateBookingAsync(
        customer.Id,
        service.Id,
        adjacentStartTime);

Assert.Equal(
    BookingResultCode.Success,
    adjacentResult.Code);


// KIỂM THỬ 2: Lịch đã hủy không còn giữ chỗ
var originalBooking = await db.Bookings
    .FindAsync(firstResult.BookingId!.Value);

Assert.NotNull(originalBooking);

// Hủy lịch đặt đầu tiên
originalBooking.Status = BookingStatus.Cancelled;

await db.SaveChangesAsync();

// Đặt lại đúng khung giờ của lịch đã hủy
var afterCancelResult =
    await bookingService.CreateBookingAsync(
        customer.Id,
        service.Id,
        startTime);

Assert.Equal(
    BookingResultCode.Success,
    afterCancelResult.Code);


// KIỂM THỬ 3: Không cho đặt dịch vụ ngừng hoạt động
service.IsAvailable = false;

await db.SaveChangesAsync();

var unavailableResult =
    await bookingService.CreateBookingAsync(
        customer.Id,
        service.Id,
        startTime.AddDays(4));

Assert.Equal(
    BookingResultCode.ServiceUnavailable,
    unavailableResult.Code);


// KIỂM THỬ 4: Không cho đặt lịch trong quá khứ
var pastResult =
    await bookingService.CreateBookingAsync(
        customer.Id,
        service.Id,
        DateTimeOffset.UtcNow.AddDays(-1));

Assert.Equal(
    BookingResultCode.InvalidTime,
    pastResult.Code);
        }
        finally
        {
            // Chỉ xóa dữ liệu do bài kiểm thử tạo ra
            db.ChangeTracker.Clear();

            if (serviceId.HasValue)
            {
                var bookings = await db.Bookings
                    .Where(b => b.ServiceId == serviceId.Value)
                    .ToListAsync();

                db.Bookings.RemoveRange(bookings);

                var service = await db.Services
                    .FindAsync(serviceId.Value);

                if (service != null)
                    db.Services.Remove(service);

                await db.SaveChangesAsync();
            }

            if (businessId.HasValue)
            {
                var business = await db.Businesses
                    .FindAsync(businessId.Value);

                if (business != null)
                {
                    db.Businesses.Remove(business);
                    await db.SaveChangesAsync();
                }
            }

            if (customerCreated)
            {
                var savedCustomer =
                    await userManager.FindByIdAsync(customer.Id);

                if (savedCustomer != null)
                    await userManager.DeleteAsync(savedCustomer);
            }
        }
    }
}
