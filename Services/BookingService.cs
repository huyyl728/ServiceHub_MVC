using System.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ServiceHubMVC.Data;
using ServiceHubMVC.Models;

namespace ServiceHubMVC.Services;

public class BookingService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public BookingService(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // Tạo một lịch đặt mới cho khách hàng
    public async Task<BookingResult> CreateBookingAsync(
        string customerId,
        int serviceId,
        DateTimeOffset startTime)
    {
        // Kiểm tra tài khoản và quyền khách hàng
        if (string.IsNullOrWhiteSpace(customerId))
        {
            return new BookingResult(
                BookingResultCode.Forbidden,
                "Bạn cần đăng nhập để đặt lịch.");
        }

        var customer = await _userManager
            .FindByIdAsync(customerId);

        if (customer == null ||
            !await _userManager.IsInRoleAsync(
                customer, "Customer"))
        {
            return new BookingResult(
                BookingResultCode.Forbidden,
                "Tài khoản không có quyền đặt lịch.");
        }

        // Chuẩn hóa thời gian về UTC
        startTime = startTime.ToUniversalTime();

        // Kiểm tra thời gian bắt đầu
        if (startTime <= DateTimeOffset.UtcNow)
        {
            return new BookingResult(
                BookingResultCode.InvalidTime,
                "Không thể đặt lịch trong quá khứ.");
        }

        try
        {
            // Sử dụng giao dịch để bảo vệ thao tác
            // kiểm tra và lưu lịch khi có nhiều yêu cầu đồng thời
            await using var transaction =
                await _context.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable);

            // Lấy thông tin dịch vụ và cơ sở kinh doanh
            var service = await _context.Services
                .AsNoTracking()
                .Include(s => s.Business)
                .FirstOrDefaultAsync(s => s.Id == serviceId);

            if (service == null)
            {
                return new BookingResult(
                    BookingResultCode.ServiceNotFound,
                    "Dịch vụ không tồn tại.");
            }

            // Kiểm tra trạng thái hoạt động của dịch vụ
            if (!service.IsAvailable ||
                !service.Business.IsActive)
            {
                return new BookingResult(
                    BookingResultCode.ServiceUnavailable,
                    "Dịch vụ hiện không khả dụng.");
            }

            // Kiểm tra thời lượng của dịch vụ
            if (service.DurationMinutes < 1 ||
                service.DurationMinutes > 1440)
            {
                return new BookingResult(
                    BookingResultCode.InvalidTime,
                    "Thời lượng dịch vụ không hợp lệ.");
            }

            // Backend tự tính thời gian kết thúc
            DateTimeOffset endTime;

            try
            {
                endTime = startTime.AddMinutes(
                    service.DurationMinutes);
            }
            catch (ArgumentOutOfRangeException)
            {
                return new BookingResult(
                    BookingResultCode.InvalidTime,
                    "Thời gian đặt lịch không hợp lệ.");
            }

            // Kiểm tra lịch giữ chỗ có giao nhau hay không
            bool isConflict = await _context.Bookings
                .AnyAsync(b =>
                    b.ServiceId == serviceId
                    && (b.Status == BookingStatus.Pending
                        || b.Status == BookingStatus.Confirmed)
                    && startTime < b.EndTime
                    && endTime > b.StartTime);

            if (isConflict)
            {
                return new BookingResult(
                    BookingResultCode.TimeConflict,
                    "Khung giờ này đã có người đặt.");
            }

            // Tạo bản ghi đặt lịch
            var booking = new Booking
            {
                CustomerId = customerId,
                ServiceId = serviceId,
                StartTime = startTime,
                EndTime = endTime,
                PriceAtBooking = service.Price,
                Status = BookingStatus.Pending,
                CreatedAt = DateTimeOffset.UtcNow
            };

            // Lưu lịch đặt vào SQL Server
            _context.Bookings.Add(booking);

            await _context.SaveChangesAsync();

            // Xác nhận giao dịch
            await transaction.CommitAsync();

            return new BookingResult(
                BookingResultCode.Success,
                "Đặt lịch thành công.",
                booking.Id);
        }
        catch (SqlException ex) when (ex.Number == 1205)
        {
            // Xử lý giao dịch bị chọn làm nạn nhân deadlock
            return new BookingResult(
                BookingResultCode.Busy,
                "Hệ thống đang bận. Vui lòng thử lại.");
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is SqlException
                { Number: 1205 })
        {
            // Xử lý deadlock phát sinh trong quá trình lưu
            return new BookingResult(
                BookingResultCode.Busy,
                "Hệ thống đang bận. Vui lòng thử lại.");
        }
    }
}