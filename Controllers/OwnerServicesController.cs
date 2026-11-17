using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceHubMVC.Data;
using ServiceHubMVC.Models;
using ServiceHubMVC.ViewModels;

namespace ServiceHubMVC.Controllers;

[Authorize(Roles = "Owner")]
[AutoValidateAntiforgeryToken]
public sealed class OwnerServicesController(ApplicationDbContext db, UserManager<ApplicationUser> users) : Controller
{
    private IQueryable<Business> OwnedBusinesses()
    {
        var ownerId = users.GetUserId(User);
        return db.Businesses.Where(b => ownerId != null && b.OwnerId == ownerId);
    }

    private IQueryable<Service> OwnedServices()
    {
        var ownerId = users.GetUserId(User);
        return db.Services.Where(s => ownerId != null && s.Business.OwnerId == ownerId);
    }

    [HttpGet]
    public async Task<IActionResult> Index(int businessId)
    {
        var business = await OwnedBusinesses().AsNoTracking().Include(b => b.Services).SingleOrDefaultAsync(b => b.Id == businessId);
        return business is null ? NotFound() : View(business);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var service = await OwnedServices().AsNoTracking().Include(s => s.Business).SingleOrDefaultAsync(s => s.Id == id);
        return service is null ? NotFound() : View(service);
    }

    [HttpGet]
    public async Task<IActionResult> Create(int businessId)
    {
        if (!await OwnedBusinesses().AnyAsync(b => b.Id == businessId)) return NotFound();
        ViewData["BusinessId"] = businessId;
        return View(new OwnerServiceFormViewModel { DurationMinutes = 30 });
    }

    [HttpPost]
    public async Task<IActionResult> Create(int businessId, OwnerServiceFormViewModel model)
    {
        if (!await OwnedBusinesses().AnyAsync(b => b.Id == businessId)) return NotFound();
        ViewData["BusinessId"] = businessId;
        if (!ModelState.IsValid) return View(model);
        db.Services.Add(new Service { BusinessId = businessId, Name = model.Name.Trim(), Category = model.Category.Trim(), Description = model.Description?.Trim(), Price = model.Price, DurationMinutes = model.DurationMinutes });
        await db.SaveChangesAsync();
        TempData["Success"] = "Đã thêm dịch vụ.";
        return RedirectToAction(nameof(Index), new { businessId });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var service = await OwnedServices().AsNoTracking().SingleOrDefaultAsync(s => s.Id == id);
        if (service is null) return NotFound();
        ViewData["ResourceId"] = id;
        ViewData["BusinessId"] = service.BusinessId;
        return View(new OwnerServiceFormViewModel { Name = service.Name, Category = service.Category, Description = service.Description, Price = service.Price, DurationMinutes = service.DurationMinutes });
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, OwnerServiceFormViewModel model)
    {
        var service = await OwnedServices().SingleOrDefaultAsync(s => s.Id == id);
        if (service is null) return NotFound();
        ViewData["ResourceId"] = id;
        ViewData["BusinessId"] = service.BusinessId;
        if (!ModelState.IsValid) return View(model);
        service.Name = model.Name.Trim();
        service.Category = model.Category.Trim();
        service.Description = model.Description?.Trim();
        service.Price = model.Price;
        service.DurationMinutes = model.DurationMinutes;
        await db.SaveChangesAsync();
        TempData["Success"] = "Đã cập nhật dịch vụ.";
        return RedirectToAction(nameof(Index), new { businessId = service.BusinessId });
    }

    [HttpPost]
    public async Task<IActionResult> SetAvailable(int id, bool available)
    {
        var service = await OwnedServices().SingleOrDefaultAsync(s => s.Id == id);
        if (service is null) return NotFound();
        if (!ModelState.IsValid) return BadRequest();
        service.IsAvailable = available;
        await db.SaveChangesAsync();
        TempData["Success"] = available ? "Đã mở lại dịch vụ." : "Đã tạm ngừng dịch vụ.";
        return RedirectToAction(nameof(Index), new { businessId = service.BusinessId });
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var service = await OwnedServices().SingleOrDefaultAsync(s => s.Id == id);
        if (service is null) return NotFound();
        if (await db.Bookings.AnyAsync(b => b.ServiceId == id))
        {
            TempData["OwnerError"] = "Không thể xóa dịch vụ đã có lịch đặt. Hãy tạm ngừng dịch vụ.";
            return RedirectToAction(nameof(Index), new { businessId = service.BusinessId });
        }
        db.Services.Remove(service);
        try { await db.SaveChangesAsync(); TempData["Success"] = "Đã xóa dịch vụ."; }
        catch (DbUpdateException) { TempData["OwnerError"] = "Không thể xóa dịch vụ vì dữ liệu đã thay đổi hoặc có liên kết."; }
        return RedirectToAction(nameof(Index), new { businessId = service.BusinessId });
    }
}
