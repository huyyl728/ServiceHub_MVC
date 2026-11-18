
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceHubMVC.Data;
using ServiceHubMVC.Models;
using ServiceHubMVC.Services;
using ServiceHubMVC.ViewModels;

namespace ServiceHubMVC.Controllers;

[Authorize]
public class BookingsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly BookingStatusService _bookingStatusService;

    public BookingsController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        BookingStatusService bookingStatusService)
    {
        _context = context;
        _userManager = userManager;
        _bookingStatusService = bookingStatusService;
    }

    [HttpGet]
    [Authorize(Roles = "Customer")]
    public Task<IActionResult> Index(
        DateOnly? date,
        BookingStatus? status,
        int page = 1)
    {
        return GetBookingsAsync(false, date, status, page);
    }

    [HttpGet]
    [Authorize(Roles = "Owner")]
    public Task<IActionResult> Owner(
        DateOnly? date,
        BookingStatus? status,
        int page = 1)
    {
        return GetBookingsAsync(true, date, status, page);
    }

    private async Task<IActionResult> GetBookingsAsync(
        bool ownerMode,
        DateOnly? date,
        BookingStatus? status,
        int page)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest();
        }

        if (status.HasValue && !Enum.IsDefined(status.Value))
        {
            return BadRequest();
        }

        var userId = _userManager.GetUserId(User);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        const int pageSize = 10;

        if (page < 1)
        {
            page = 1;
        }

        IQueryable<Booking> query = _context.Bookings
            .AsNoTracking();

        if (ownerMode)
        {
            query = query.Where(b =>
                b.Service.Business.OwnerId == userId);
        }
        else
        {
            query = query.Where(b =>
                b.CustomerId == userId);
        }

        if (status.HasValue)
        {
            query = query.Where(b =>
                b.Status == status.Value);
        }

        if (date.HasValue)
        {
            var start = new DateTimeOffset(
                date.Value.ToDateTime(TimeOnly.MinValue),
                TimeSpan.FromHours(7));

            var end = start.AddDays(1);

            query = query.Where(b =>
                b.StartTime >= start &&
                b.StartTime < end);
        }

        var totalItems = await query.CountAsync();

        var totalPages = Math.Max(
            1,
            (int)Math.Ceiling((double)totalItems / pageSize));

        if (page > totalPages)
        {
            page = totalPages;
        }

        var bookings = await query
            .OrderByDescending(b => b.StartTime)
            .ThenByDescending(b => b.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(b => new BookingListItemViewModel
            {
                Id = b.Id,
                ServiceName = b.Service.Name,
                BusinessName = b.Service.Business.Name,
                CustomerName = b.Customer.FullName,
                StartTime = b.StartTime,
                EndTime = b.EndTime,
                PriceAtBooking = b.PriceAtBooking,
                Status = b.Status
            })
            .ToListAsync();

        var model = new BookingListPageViewModel
        {
            Items = bookings,
            Date = date,
            Status = status,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            IsOwner = ownerMode
        };

        return View("Index", model);
    }

    [HttpGet]
    [Authorize(Roles = "Customer")]
    public Task<IActionResult> Details(int id)
    {
        return GetBookingDetailsAsync(id, false);
    }

    [HttpGet]
    [Authorize(Roles = "Owner")]
    public Task<IActionResult> OwnerDetails(int id)
    {
        return GetBookingDetailsAsync(id, true);
    }

    private async Task<IActionResult> GetBookingDetailsAsync(
        int id,
        bool ownerMode)
    {
        if (id <= 0)
        {
            return NotFound();
        }

        var userId = _userManager.GetUserId(User);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        IQueryable<Booking> query = _context.Bookings
            .AsNoTracking()
            .Where(b => b.Id == id);

        if (ownerMode)
        {
            query = query.Where(b =>
                b.Service.Business.OwnerId == userId);
        }
        else
        {
            query = query.Where(b =>
                b.CustomerId == userId);
        }

        var booking = await query
            .Select(b => new BookingListItemViewModel
            {
                Id = b.Id,
                ServiceName = b.Service.Name,
                BusinessName = b.Service.Business.Name,
                CustomerName = b.Customer.FullName,
                StartTime = b.StartTime,
                EndTime = b.EndTime,
                PriceAtBooking = b.PriceAtBooking,
                Status = b.Status,
                BusinessAddress = b.Service.Business.Address,
                BusinessPhoneNumber = b.Service.Business.PhoneNumber,
                CustomerPhoneNumber = ownerMode ? b.Customer.PhoneNumber : null
            })
            .FirstOrDefaultAsync();

        if (booking is null)
        {
            return NotFound();
        }

        ViewData["IsOwner"] = ownerMode;

        return View("Details", booking);
    }

    [HttpPost]
    [Authorize(Roles = "Customer")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var userId = _userManager.GetUserId(User);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        var success = await _bookingStatusService.CancelAsync(
            id, userId);

        TempData[success ? "SuccessMessage" : "ErrorMessage"] =
            success
                ? "Hủy lịch đặt thành công."
                : "Không thể hủy lịch. Vui lòng kiểm tra quyền, thời gian và trạng thái.";

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [Authorize(Roles = "Owner")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Confirm(int id)
    {
        var userId = _userManager.GetUserId(User);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        var success = await _bookingStatusService.ConfirmAsync(
            id, userId);

        TempData[success ? "SuccessMessage" : "ErrorMessage"] =
            success
                ? "Xác nhận lịch đặt thành công."
                : "Không thể xác nhận lịch. Vui lòng kiểm tra quyền và trạng thái.";

        return RedirectToAction(nameof(OwnerDetails), new { id });
    }

    [HttpPost]
    [Authorize(Roles = "Owner")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id)
    {
        var userId = _userManager.GetUserId(User);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        var success = await _bookingStatusService.RejectAsync(
            id, userId);

        TempData[success ? "SuccessMessage" : "ErrorMessage"] =
            success
                ? "Từ chối lịch đặt thành công."
                : "Không thể từ chối lịch. Vui lòng kiểm tra quyền và trạng thái.";

        return RedirectToAction(nameof(OwnerDetails), new { id });
    }

    [HttpPost]
    [Authorize(Roles = "Owner")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(int id)
    {
        var userId = _userManager.GetUserId(User);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        var success = await _bookingStatusService.CompleteAsync(
            id, userId);

        TempData[success ? "SuccessMessage" : "ErrorMessage"] =
            success
                ? "Đánh dấu hoàn thành lịch đặt thành công."
                : "Không thể hoàn thành lịch. Vui lòng kiểm tra quyền, thời gian và trạng thái.";

        return RedirectToAction(nameof(OwnerDetails), new { id });
    }
}
