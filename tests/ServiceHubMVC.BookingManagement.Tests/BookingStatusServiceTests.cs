using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ServiceHubMVC.Data;
using ServiceHubMVC.Models;
using ServiceHubMVC.Services;
using Xunit;

namespace ServiceHubMVC.BookingManagement.Tests;

public class BookingStatusServiceTests
{
    [Fact]
    public async Task Customer_HuyLichPendingTruocGioBatDau_ThanhCong()
    {
        var connectionString = Environment.GetEnvironmentVariable(
            "SERVICEHUB_BOOKING_TEST_CONNECTION");

        Assert.False(string.IsNullOrWhiteSpace(connectionString));

        var connectionBuilder = new SqlConnectionStringBuilder(
            connectionString);

        Assert.Equal(
        "ServiceHubMVC_BookingTest",
        connectionBuilder.InitialCatalog,
        ignoreCase: true);

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(connectionBuilder.ConnectionString)
            .Options;

        await using var db = new ApplicationDbContext(options);

        Assert.True(await db.Database.CanConnectAsync());

        await using var transaction =
            await db.Database.BeginTransactionAsync();

        var suffix = Guid.NewGuid().ToString("N");

        var customer = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = $"customer-{suffix}",
            Email = $"customer-{suffix}@example.test",
            FullName = "Khách hàng kiểm thử"
        };

        var owner = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = $"owner-{suffix}",
            Email = $"owner-{suffix}@example.test",
            FullName = "Chủ cơ sở kiểm thử"
        };

        var business = new Business
        {
            Name = "Cơ sở kiểm thử",
            Address = "Địa chỉ kiểm thử",
            Owner = owner
        };

        var service = new ServiceHubMVC.Models.Service
        {
            Name = "Dịch vụ kiểm thử",
            Category = "Khác",
            Price = 100000m,
            DurationMinutes = 60,
            Business = business
        };

        var startTime = DateTimeOffset.UtcNow.AddDays(1);

        var booking = new Booking
        {
            Customer = customer,
            Service = service,
            StartTime = startTime,
            EndTime = startTime.AddHours(1),
            PriceAtBooking = 100000m,
            Status = BookingStatus.Pending
        };

        db.Bookings.Add(booking);

        await db.SaveChangesAsync();

        var bookingStatusService = new BookingStatusService(db);

        var otherUserId = Guid.NewGuid().ToString();

        var wrongCustomerResult =
            await bookingStatusService.CancelAsync(
                booking.Id,
                otherUserId);

        Assert.False(wrongCustomerResult);

        var wrongOwnerResult =
            await bookingStatusService.ConfirmAsync(
                booking.Id,
                otherUserId);

        Assert.False(wrongOwnerResult);

        var statusBeforeCancel = await db.Bookings
            .AsNoTracking()
            .Where(b => b.Id == booking.Id)
            .Select(b => b.Status)
            .SingleAsync();

        Assert.Equal(BookingStatus.Pending, statusBeforeCancel);

        var result = await bookingStatusService.CancelAsync(
            booking.Id,
            customer.Id);

        var actualStatus = await db.Bookings
            .AsNoTracking()
            .Where(b => b.Id == booking.Id)
            .Select(b => b.Status)
            .SingleAsync();

        Assert.True(result);
        Assert.Equal(BookingStatus.Cancelled, actualStatus);

        var cancelAgain = await bookingStatusService.CancelAsync(
            booking.Id,
            customer.Id);

        var confirmAfterCancel = await bookingStatusService.ConfirmAsync(
            booking.Id,
            owner.Id);

        var rejectAfterCancel = await bookingStatusService.RejectAsync(
            booking.Id,
            owner.Id);

        var completeAfterCancel = await bookingStatusService.CompleteAsync(
            booking.Id,
            owner.Id);

        Assert.False(cancelAgain);
        Assert.False(confirmAfterCancel);
        Assert.False(rejectAfterCancel);
        Assert.False(completeAfterCancel);

        var finalStatus = await db.Bookings
            .AsNoTracking()
            .Where(b => b.Id == booking.Id)
            .Select(b => b.Status)
            .SingleAsync();

        Assert.Equal(BookingStatus.Cancelled, finalStatus);

        await transaction.RollbackAsync();
    }

    [Fact]
    public async Task Owner_XacNhanVaHoanThanhLich_ThanhCong()
    {
        var connectionString = Environment.GetEnvironmentVariable(
            "SERVICEHUB_BOOKING_TEST_CONNECTION");

        Assert.False(string.IsNullOrWhiteSpace(connectionString));

        var connectionBuilder = new SqlConnectionStringBuilder(
            connectionString);


        Assert.Equal(
            "ServiceHubMVC_BookingTest",
            connectionBuilder.InitialCatalog,
            ignoreCase: true);


        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(connectionBuilder.ConnectionString)
            .Options;

        await using var db = new ApplicationDbContext(options);

        Assert.True(await db.Database.CanConnectAsync());

        await using var transaction =
            await db.Database.BeginTransactionAsync();

        var suffix = Guid.NewGuid().ToString("N");

        var customer = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = $"customer-{suffix}",
            Email = $"customer-{suffix}@example.test",
            FullName = "Khách hàng kiểm thử"
        };

        var owner = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = $"owner-{suffix}",
            Email = $"owner-{suffix}@example.test",
            FullName = "Chủ cơ sở kiểm thử"
        };

        var business = new Business
        {
            Name = "Cơ sở kiểm thử Owner",
            Address = "Địa chỉ kiểm thử",
            Owner = owner
        };

        var service = new ServiceHubMVC.Models.Service
        {
            Name = "Dịch vụ kiểm thử Owner",
            Category = "Khác",
            Price = 150000m,
            DurationMinutes = 60,
            Business = business
        };

        var startTime = DateTimeOffset.UtcNow.AddHours(-2);

        var booking = new Booking
        {
            Customer = customer,
            Service = service,
            StartTime = startTime,
            EndTime = startTime.AddHours(1),
            PriceAtBooking = 150000m,
            Status = BookingStatus.Pending
        };

        db.Bookings.Add(booking);

        await db.SaveChangesAsync();

        var bookingStatusService = new BookingStatusService(db);

        var wrongOwnerId = Guid.NewGuid().ToString();

        var wrongOwnerResult =
            await bookingStatusService.ConfirmAsync(
                booking.Id,
                wrongOwnerId);

        Assert.False(wrongOwnerResult);

        var confirmResult =
            await bookingStatusService.ConfirmAsync(
                booking.Id,
                owner.Id);

        Assert.True(confirmResult);

        var confirmedStatus = await db.Bookings
            .AsNoTracking()
            .Where(b => b.Id == booking.Id)
            .Select(b => b.Status)
            .SingleAsync();

        Assert.Equal(BookingStatus.Confirmed, confirmedStatus);

        var completeResult =
            await bookingStatusService.CompleteAsync(
                booking.Id,
                owner.Id);

        Assert.True(completeResult);

        var completedStatus = await db.Bookings
            .AsNoTracking()
            .Where(b => b.Id == booking.Id)
            .Select(b => b.Status)
            .SingleAsync();

        Assert.Equal(BookingStatus.Completed, completedStatus);

        var rejectAfterComplete =
            await bookingStatusService.RejectAsync(
                booking.Id,
                owner.Id);

        Assert.False(rejectAfterComplete);

        await transaction.RollbackAsync();
    }

    [Fact]
    public async Task Owner_TuChoiLichPending_ThanhCong()
    {
        var connectionString = Environment.GetEnvironmentVariable(
            "SERVICEHUB_BOOKING_TEST_CONNECTION");

        Assert.False(string.IsNullOrWhiteSpace(connectionString));

        var connectionBuilder = new SqlConnectionStringBuilder(
            connectionString);


        Assert.Equal(
            "ServiceHubMVC_BookingTest",
            connectionBuilder.InitialCatalog,
            ignoreCase: true);


        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(connectionBuilder.ConnectionString)
            .Options;

        await using var db = new ApplicationDbContext(options);

        Assert.True(await db.Database.CanConnectAsync());

        await using var transaction =
            await db.Database.BeginTransactionAsync();

        var suffix = Guid.NewGuid().ToString("N");

        var customer = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = $"customer-{suffix}",
            Email = $"customer-{suffix}@example.test",
            FullName = "Khách hàng kiểm thử"
        };

        var owner = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = $"owner-{suffix}",
            Email = $"owner-{suffix}@example.test",
            FullName = "Chủ cơ sở kiểm thử"
        };

        var business = new Business
        {
            Name = "Cơ sở kiểm thử từ chối lịch",
            Address = "Địa chỉ kiểm thử",
            Owner = owner
        };

        var service = new ServiceHubMVC.Models.Service
        {
            Name = "Dịch vụ kiểm thử",
            Category = "Khác",
            Price = 120000m,
            DurationMinutes = 60,
            Business = business
        };

        var startTime = DateTimeOffset.UtcNow.AddDays(1);

        var booking = new Booking
        {
            Customer = customer,
            Service = service,
            StartTime = startTime,
            EndTime = startTime.AddHours(1),
            PriceAtBooking = 120000m,
            Status = BookingStatus.Pending
        };

        db.Bookings.Add(booking);

        await db.SaveChangesAsync();

        var bookingStatusService = new BookingStatusService(db);

        var wrongOwnerId = Guid.NewGuid().ToString();

        var wrongOwnerResult =
            await bookingStatusService.RejectAsync(
                booking.Id,
                wrongOwnerId);

        Assert.False(wrongOwnerResult);

        var rejectResult =
            await bookingStatusService.RejectAsync(
                booking.Id,
                owner.Id);

        Assert.True(rejectResult);

        var rejectedStatus = await db.Bookings
            .AsNoTracking()
            .Where(b => b.Id == booking.Id)
            .Select(b => b.Status)
            .SingleAsync();

        Assert.Equal(BookingStatus.Rejected, rejectedStatus);

        var confirmAfterReject =
            await bookingStatusService.ConfirmAsync(
                booking.Id,
                owner.Id);

        var cancelAfterReject =
            await bookingStatusService.CancelAsync(
                booking.Id,
                customer.Id);

        var completeAfterReject =
            await bookingStatusService.CompleteAsync(
                booking.Id,
                owner.Id);

        Assert.False(confirmAfterReject);
        Assert.False(cancelAfterReject);
        Assert.False(completeAfterReject);

        var finalStatus = await db.Bookings
            .AsNoTracking()
            .Where(b => b.Id == booking.Id)
            .Select(b => b.Status)
            .SingleAsync();

        Assert.Equal(BookingStatus.Rejected, finalStatus);

        await transaction.RollbackAsync();
    }

    [Fact]
    public async Task KhongChoPhepChuyenTrangThaiSaiThoiGian()
    {
        var connectionString = Environment.GetEnvironmentVariable(
            "SERVICEHUB_BOOKING_TEST_CONNECTION");

        Assert.False(string.IsNullOrWhiteSpace(connectionString));

        var connectionBuilder = new SqlConnectionStringBuilder(
            connectionString);

        Assert.Equal(
            "ServiceHubMVC_BookingTest",
            connectionBuilder.InitialCatalog,
            ignoreCase: true);

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(connectionBuilder.ConnectionString)
            .Options;

        await using var db = new ApplicationDbContext(options);

        Assert.True(await db.Database.CanConnectAsync());

        await using var transaction =
            await db.Database.BeginTransactionAsync();

        var suffix = Guid.NewGuid().ToString("N");

        var customer = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = $"customer-{suffix}",
            Email = $"customer-{suffix}@example.test",
            FullName = "Khách hàng kiểm thử"
        };

        var owner = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = $"owner-{suffix}",
            Email = $"owner-{suffix}@example.test",
            FullName = "Chủ cơ sở kiểm thử"
        };

        var business = new Business
        {
            Name = "Cơ sở kiểm thử thời gian",
            Address = "Địa chỉ kiểm thử",
            Owner = owner
        };

        var service = new ServiceHubMVC.Models.Service
        {
            Name = "Dịch vụ kiểm thử thời gian",
            Category = "Khác",
            Price = 100000m,
            DurationMinutes = 60,
            Business = business
        };

        var now = DateTimeOffset.UtcNow;

        var startedBooking = new Booking
        {
            Customer = customer,
            Service = service,
            StartTime = now.AddHours(-1),
            EndTime = now.AddHours(1),
            PriceAtBooking = 100000m,
            Status = BookingStatus.Pending
        };

        var futureBooking = new Booking
        {
            Customer = customer,
            Service = service,
            StartTime = now.AddHours(1),
            EndTime = now.AddHours(2),
            PriceAtBooking = 100000m,
            Status = BookingStatus.Confirmed
        };

        db.Bookings.AddRange(startedBooking, futureBooking);

        await db.SaveChangesAsync();

        var bookingStatusService = new BookingStatusService(db);

        var cancelStartedResult =
            await bookingStatusService.CancelAsync(
                startedBooking.Id,
                customer.Id);

        Assert.False(cancelStartedResult);

        var completeFutureResult =
            await bookingStatusService.CompleteAsync(
                futureBooking.Id,
                owner.Id);

        Assert.False(completeFutureResult);

        var startedStatus = await db.Bookings
            .AsNoTracking()
            .Where(b => b.Id == startedBooking.Id)
            .Select(b => b.Status)
            .SingleAsync();

        var futureStatus = await db.Bookings
            .AsNoTracking()
            .Where(b => b.Id == futureBooking.Id)
            .Select(b => b.Status)
            .SingleAsync();

        Assert.Equal(BookingStatus.Pending, startedStatus);
        Assert.Equal(BookingStatus.Confirmed, futureStatus);

        await transaction.RollbackAsync();
    }

    [Fact]
    public async Task HaiYeuCauChuyenTrangThai_KhongGhiDeNhau()
    {
        var connectionString = Environment.GetEnvironmentVariable(
            "SERVICEHUB_BOOKING_TEST_CONNECTION");

        Assert.False(string.IsNullOrWhiteSpace(connectionString));

        var connectionBuilder = new SqlConnectionStringBuilder(
            connectionString);

        Assert.Equal(
            "ServiceHubMVC_BookingTest",
            connectionBuilder.InitialCatalog,
            ignoreCase: true);

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(connectionBuilder.ConnectionString)
            .Options;

        var suffix = Guid.NewGuid().ToString("N");
        var customerId = Guid.NewGuid().ToString();
        var ownerId = Guid.NewGuid().ToString();

        int bookingId;
        int serviceId;
        int businessId;

        await using (var setupDb = new ApplicationDbContext(options))
        {
            Assert.True(await setupDb.Database.CanConnectAsync());

            var customer = new ApplicationUser
            {
                Id = customerId,
                UserName = $"customer-{suffix}",
                Email = $"customer-{suffix}@example.test",
                FullName = "Khách hàng kiểm thử"
            };

            var owner = new ApplicationUser
            {
                Id = ownerId,
                UserName = $"owner-{suffix}",
                Email = $"owner-{suffix}@example.test",
                FullName = "Chủ cơ sở kiểm thử"
            };

            var business = new Business
            {
                Name = "Cơ sở kiểm thử đồng thời",
                Address = "Địa chỉ kiểm thử",
                Owner = owner
            };

            var service = new ServiceHubMVC.Models.Service
            {
                Name = "Dịch vụ kiểm thử đồng thời",
                Category = "Khác",
                Price = 100000m,
                DurationMinutes = 60,
                Business = business
            };

            var startTime = DateTimeOffset.UtcNow.AddDays(1);

            var booking = new Booking
            {
                Customer = customer,
                Service = service,
                StartTime = startTime,
                EndTime = startTime.AddHours(1),
                PriceAtBooking = 100000m,
                Status = BookingStatus.Pending
            };

            setupDb.Bookings.Add(booking);
            await setupDb.SaveChangesAsync();

            bookingId = booking.Id;
            serviceId = service.Id;
            businessId = business.Id;
        }

        try
        {
            await using var firstDb =
                new ApplicationDbContext(options);

            await using var secondDb =
                new ApplicationDbContext(options);

            var firstService = new BookingStatusService(firstDb);
            var secondService = new BookingStatusService(secondDb);

            var confirmTask = firstService.ConfirmAsync(
                bookingId,
                ownerId);

            var rejectTask = secondService.RejectAsync(
                bookingId,
                ownerId);

            await Task.WhenAll(confirmTask, rejectTask);

            var confirmResult = await confirmTask;
            var rejectResult = await rejectTask;

            var successCount = new[]
            {
                confirmResult,
                rejectResult
            }.Count(result => result);

            Assert.Equal(1, successCount);

            await using var verifyDb =
                new ApplicationDbContext(options);

            var finalStatus = await verifyDb.Bookings
                .AsNoTracking()
                .Where(b => b.Id == bookingId)
                .Select(b => b.Status)
                .SingleAsync();

            var expectedStatus = confirmResult
                ? BookingStatus.Confirmed
                : BookingStatus.Rejected;

            Assert.Equal(expectedStatus, finalStatus);
        }
        finally
        {
            await using var cleanupDb =
                new ApplicationDbContext(options);

            await cleanupDb.Bookings
                .Where(b => b.Id == bookingId)
                .ExecuteDeleteAsync();

            await cleanupDb.Services
                .Where(s => s.Id == serviceId)
                .ExecuteDeleteAsync();

            await cleanupDb.Businesses
                .Where(b => b.Id == businessId)
                .ExecuteDeleteAsync();

            await cleanupDb.Users
                .Where(u =>
                    u.Id == customerId ||
                    u.Id == ownerId)
                .ExecuteDeleteAsync();
        }
    }

}