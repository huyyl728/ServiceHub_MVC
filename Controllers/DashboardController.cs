
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceHubMVC.Data;
using ServiceHubMVC.Models;
using ServiceHubMVC.ViewModels;

namespace ServiceHubMVC.Controllers;

[Authorize(Roles = "Owner,Admin")]
public sealed class DashboardController(
    ApplicationDbContext db,
    UserManager<ApplicationUser> userManager
) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userId = userManager.GetUserId(User);

        if (string.IsNullOrEmpty(userId))
            return Challenge();

        bool isAdmin = User.IsInRole("Admin");

        // Xây dựng các truy vấn từ database.
        var businesses = db.Businesses.AsNoTracking();
        var services = db.Services.AsNoTracking();
        var bookings = db.Bookings.AsNoTracking();

        // Owner chỉ được thống kê dữ liệu của mình.
        if (!isAdmin)
        {
            businesses = businesses.Where(
                b => b.OwnerId == userId);

            services = services.Where(
                s => s.Business.OwnerId == userId);

            bookings = bookings.Where(
                b => b.Service.Business.OwnerId == userId);
        }

        // Thống kê số lịch theo trạng thái trên SQL Server.
        var statusCounts = await bookings
            .GroupBy(b => b.Status)
            .Select(group => new
            {
                Status = group.Key,
                Total = group.Count()
            })
            .ToDictionaryAsync(x => x.Status, x => x.Total);

        int GetCount(BookingStatus status) =>
            statusCounts.GetValueOrDefault(status);

        var model = new DashboardViewModel
        {
            IsAdmin = isAdmin,

            BusinessCount = await businesses.CountAsync(),
            ServiceCount = await services.CountAsync(),
            BookingCount = statusCounts.Values.Sum(),

            PendingCount = GetCount(BookingStatus.Pending),
            ConfirmedCount = GetCount(BookingStatus.Confirmed),
            CompletedCount = GetCount(BookingStatus.Completed),
            CancelledCount = GetCount(BookingStatus.Cancelled),
            RejectedCount = GetCount(BookingStatus.Rejected),

            // Hiển thị tối đa 20 cơ sở trong bảng tổng quan.
            Businesses = await businesses
                .OrderBy(b => b.Name)
                .ThenBy(b => b.Id)
                .Take(20)
                .Select(b => new DashboardBusinessItemViewModel
                {
                    Id = b.Id,
                    Name = b.Name,

                    ServiceCount = db.Services.Count(
                        s => s.BusinessId == b.Id),

                    BookingCount = db.Bookings.Count(
                        booking =>
                            booking.Service.BusinessId == b.Id)
                })
                .ToListAsync()
        };

        return View(model);
    }
}
