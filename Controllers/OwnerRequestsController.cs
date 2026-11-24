using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ServiceHubMVC.Data;
using ServiceHubMVC.Models;
using ServiceHubMVC.ViewModels;

namespace ServiceHubMVC.Controllers;

[Authorize]
[AutoValidateAntiforgeryToken]
public sealed class OwnerRequestsController(
    ApplicationDbContext db,
    UserManager<ApplicationUser> users) : Controller
{
    private async Task<bool> CanApplyAsync()
{
    var user = await users.GetUserAsync(User);

    return user is not null
        && await users.IsInRoleAsync(user, "Customer")
        && !await users.IsInRoleAsync(user, "Owner")
        && !await users.IsInRoleAsync(user, "Admin");
}

    // Hiển thị biểu mẫu đăng ký.
    [HttpGet]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> Apply()
    {
        if (!await CanApplyAsync())
            return Forbid();

        var userId = users.GetUserId(User);

        var hasPending = await db.OwnerRequests.AnyAsync(x =>
            x.UserId == userId &&
            x.Status == OwnerRequestStatus.Pending);

        if (hasPending)
            return RedirectToAction(nameof(MyStatus));

        return View(new OwnerRequestFormViewModel());
    }

    // Tiếp nhận yêu cầu đăng ký Owner.
    [HttpPost]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> Apply(
        OwnerRequestFormViewModel model)
    {
        if (!await CanApplyAsync())
            return Forbid();

        if (!ModelState.IsValid)
            return View(model);

        var userId = users.GetUserId(User);

        if (string.IsNullOrEmpty(userId))
            return Challenge();

        var hasPending = await db.OwnerRequests.AnyAsync(x =>
            x.UserId == userId &&
            x.Status == OwnerRequestStatus.Pending);

        if (hasPending)
            return RedirectToAction(nameof(MyStatus));

        var request = new OwnerRequest
        {
            UserId = userId,
            BusinessName = model.BusinessName.Trim(),
            BusinessAddress = model.BusinessAddress.Trim(),
            Reason = model.Reason?.Trim(),
            Status = OwnerRequestStatus.Pending,
            CreatedAtUtc = DateTime.UtcNow
        };

        db.OwnerRequests.Add(request);

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (
            ex.InnerException is SqlException sqlException
            && sqlException.Number is 2601 or 2627)
        {
            TempData["Success"] =
                "Bạn đã có yêu cầu đang chờ xét duyệt.";

            return RedirectToAction(nameof(MyStatus));
        }

        TempData["Success"] =
            "Gửi yêu cầu thành công. Vui lòng chờ xét duyệt.";

        return RedirectToAction(nameof(MyStatus));
    }

    // Người dùng chỉ được xem yêu cầu của chính mình.
    [HttpGet]
    [Authorize(Roles = "Customer,Owner")]
    public async Task<IActionResult> MyStatus()
    {
        var userId = users.GetUserId(User);

        if (string.IsNullOrEmpty(userId))
            return Challenge();

        var request = await db.OwnerRequests
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ThenByDescending(x => x.Id)
            .FirstOrDefaultAsync();

        return View(request);
    }
    // Admin xem danh sách yêu cầu đang chờ xét duyệt.
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Index()
{
    var requests = await db.OwnerRequests
        .AsNoTracking()
        .Include(x => x.User)
        .Where(x => x.Status == OwnerRequestStatus.Pending)
        .OrderBy(x => x.CreatedAtUtc)
        .ToListAsync();

    return View(requests);
}
    // Admin phê duyệt và cấp quyền Owner.
[HttpPost]
[Authorize(Roles = "Admin")]
public async Task<IActionResult> Approve(int id)
{
    var adminId = users.GetUserId(User);

    if (string.IsNullOrEmpty(adminId))
        return Challenge();

    // Đảm bảo cập nhật yêu cầu và cấp quyền
    // thành công hoặc thất bại cùng nhau.
    await using var transaction =
        await db.Database.BeginTransactionAsync();

    var request = await db.OwnerRequests
        .AsNoTracking()
        .SingleOrDefaultAsync(x => x.Id == id);

    if (request is null)
        return NotFound();

    if (request.Status != OwnerRequestStatus.Pending)
        return Conflict("Yêu cầu đã được xử lý.");

    // Chỉ cập nhật nếu yêu cầu vẫn đang chờ duyệt.
    var updated = await db.OwnerRequests
        .Where(x => x.Id == id &&
                    x.Status == OwnerRequestStatus.Pending)
        .ExecuteUpdateAsync(setters => setters
            .SetProperty(
                x => x.Status,
                OwnerRequestStatus.Approved)
            .SetProperty(
                x => x.ReviewedAtUtc,
                DateTime.UtcNow)
            .SetProperty(
                x => x.ReviewedByUserId,
                adminId));

    if (updated != 1)
        return Conflict("Yêu cầu đã được xử lý.");

    var applicant = await users.FindByIdAsync(
        request.UserId);

    if (applicant is null)
        return NotFound();
if (!await users.IsInRoleAsync(applicant, "Customer"))
{
    return BadRequest(
        "Tài khoản đăng ký không có vai trò Customer.");
}

if (await users.IsInRoleAsync(applicant, "Owner"))
{
    return BadRequest(
        "Tài khoản này đã có quyền Owner.");
}

if (await users.IsInRoleAsync(applicant, "Admin"))
{
    return BadRequest(
        "Không thể cấp quyền Owner cho tài khoản Admin.");
}

    // Cấp thêm quyền Owner bằng ASP.NET Core Identity.
    var result = await users.AddToRoleAsync(
        applicant, "Owner");

    if (!result.Succeeded)
    {
        return StatusCode(
            500,
            "Không thể cấp quyền Owner.");
    }

    await transaction.CommitAsync();

    TempData["Success"] =
        "Đã phê duyệt yêu cầu và cấp quyền Owner thành công.";

    return RedirectToAction(nameof(Index));
}
    // Admin từ chối yêu cầu đăng ký Owner.
[HttpPost]
[Authorize(Roles = "Admin")]
public async Task<IActionResult> Reject(int id)
{
    var adminId = users.GetUserId(User);

    if (string.IsNullOrEmpty(adminId))
        return Challenge();

    var updated = await db.OwnerRequests
        .Where(x => x.Id == id &&
                    x.Status == OwnerRequestStatus.Pending)
        .ExecuteUpdateAsync(setters => setters
            .SetProperty(
                x => x.Status,
                OwnerRequestStatus.Rejected)
            .SetProperty(
                x => x.ReviewedAtUtc,
                DateTime.UtcNow)
            .SetProperty(
                x => x.ReviewedByUserId,
                adminId));

    if (updated != 1)
        return Conflict(
            "Yêu cầu không tồn tại hoặc đã được xử lý.");

    TempData["Success"] =
        "Đã từ chối yêu cầu đăng ký Owner.";

    return RedirectToAction(nameof(Index));
}
}