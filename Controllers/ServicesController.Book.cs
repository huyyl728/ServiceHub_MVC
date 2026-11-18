using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceHubMVC.Models;
using ServiceHubMVC.Services;
using ServiceHubMVC.ViewModels;

namespace ServiceHubMVC.Controllers;

// Action đặt lịch dùng chung phần truy vấn công khai ở ServicesController.
public partial class ServicesController
{
    [HttpPost]
    [Authorize(Roles = "Customer")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Book(
        int id,
        [FromForm] string? startTimeLocal,
        [FromServices] BookingService bookingService,
        [FromServices] UserManager<ApplicationUser> userManager,
        CancellationToken cancellationToken)
    {
        var customerId = userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(customerId))
            return Challenge();

        var detail = await AvailableServices()
            .Where(s => s.Id == id)
            .Select(s => new ServiceDetailViewModel
            {
                Service = new ServiceCardViewModel
                {
                    Id = s.Id,
                    Name = s.Name,
                    Category = s.Category,
                    BusinessName = s.Business.Name,
                    Address = s.Business.Address,
                    Price = s.Price,
                    DurationMinutes = s.DurationMinutes
                },
                Description = s.Description
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (detail is null)
            return NotFound();

        var model = new ServiceDetailViewModel
        {
            Service = detail.Service,
            Description = detail.Description,
            StartTimeLocal = startTimeLocal
        };

        var formats = new[] { "yyyy-MM-dd'T'HH:mm", "yyyy-MM-dd'T'HH:mm:ss" };
        if (string.IsNullOrWhiteSpace(startTimeLocal) ||
            !DateTime.TryParseExact(
                startTimeLocal.Trim(),
                formats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var localTime))
        {
            ModelState.AddModelError(
                nameof(ServiceDetailViewModel.StartTimeLocal),
                "Vui lòng chọn ngày giờ hợp lệ.");
            return View("Details", model);
        }

        DateTimeOffset startTime;
        try
        {
            startTime = new DateTimeOffset(
                DateTime.SpecifyKind(localTime, DateTimeKind.Unspecified),
                TimeSpan.FromHours(7));
        }
        catch (ArgumentOutOfRangeException)
        {
            ModelState.AddModelError(
                nameof(ServiceDetailViewModel.StartTimeLocal),
                "Thời gian đặt lịch không hợp lệ.");
            return View("Details", model);
        }

        // Giá, thời lượng và quyền đặt được xác minh trong BookingService.
        var result = await bookingService.CreateBookingAsync(
            customerId, id, startTime);

        if (result.Success && result.BookingId is int bookingId)
        {
            TempData["SuccessMessage"] = result.Message;
            return RedirectToAction("Details", "Bookings", new { id = bookingId });
        }

        if (result.Code == BookingResultCode.Forbidden)
            return Forbid();

        if (result.Code is BookingResultCode.ServiceNotFound
            or BookingResultCode.ServiceUnavailable)
            return NotFound();

        if (result.Code == BookingResultCode.TimeConflict)
            Response.StatusCode = StatusCodes.Status409Conflict;

        if (result.Code == BookingResultCode.Busy)
            Response.StatusCode = StatusCodes.Status503ServiceUnavailable;

        ModelState.AddModelError(
            nameof(ServiceDetailViewModel.StartTimeLocal),
            result.Message);

        return View("Details", model);
    }
}
