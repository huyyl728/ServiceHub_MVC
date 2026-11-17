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
public sealed class BusinessesController(ApplicationDbContext db, UserManager<ApplicationUser> users) : Controller
{
    // Mọi truy vấn đều giới hạn theo chủ sở hữu lấy từ Identity.
    private IQueryable<Business> Owned()
    {
        var ownerId = users.GetUserId(User);
        return db.Businesses.Where(b => ownerId != null && b.OwnerId == ownerId);
    }

    [HttpGet]
    public async Task<IActionResult> Index() => View(await Owned().AsNoTracking().OrderBy(b => b.Name).ThenBy(b => b.Id).ToListAsync());

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var business = await Owned().AsNoTracking().SingleOrDefaultAsync(b => b.Id == id);
        return business is null ? NotFound() : View(business);
    }

    [HttpGet]
    public IActionResult Create() => View(new BusinessFormViewModel());

    [HttpPost]
    public async Task<IActionResult> Create(BusinessFormViewModel model)
    {
        var ownerId = users.GetUserId(User);
        if (ownerId is null) return Forbid();
        if (!ModelState.IsValid) return View(model);
        db.Businesses.Add(new Business { Name = model.Name.Trim(), Address = model.Address.Trim(), Description = model.Description?.Trim(), OwnerId = ownerId });
        await db.SaveChangesAsync();
        TempData["Success"] = "Đã thêm cơ sở.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var business = await Owned().AsNoTracking().SingleOrDefaultAsync(b => b.Id == id);
        if (business is null) return NotFound();
        ViewData["ResourceId"] = id;
        return View(new BusinessFormViewModel { Name = business.Name, Address = business.Address, Description = business.Description });
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, BusinessFormViewModel model)
    {
        var business = await Owned().SingleOrDefaultAsync(b => b.Id == id);
        if (business is null) return NotFound();
        ViewData["ResourceId"] = id;
        if (!ModelState.IsValid) return View(model);
        business.Name = model.Name.Trim();
        business.Address = model.Address.Trim();
        business.Description = model.Description?.Trim();
        await db.SaveChangesAsync();
        TempData["Success"] = "Đã cập nhật cơ sở.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> SetActive(int id, bool active)
    {
        var business = await Owned().SingleOrDefaultAsync(b => b.Id == id);
        if (business is null) return NotFound();
        if (!ModelState.IsValid) return BadRequest();
        business.IsActive = active;
        await db.SaveChangesAsync();
        TempData["Success"] = active ? "Đã mở lại cơ sở." : "Đã tạm ngừng cơ sở.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var business = await Owned().SingleOrDefaultAsync(b => b.Id == id);
        if (business is null) return NotFound();
        if (await db.Services.AnyAsync(s => s.BusinessId == id))
        {
            TempData["OwnerError"] = "Không thể xóa cơ sở còn dịch vụ. Hãy tạm ngừng cơ sở nếu cần.";
            return RedirectToAction(nameof(Index));
        }
        db.Businesses.Remove(business);
        try { await db.SaveChangesAsync(); TempData["Success"] = "Đã xóa cơ sở."; }
        catch (DbUpdateException) { TempData["OwnerError"] = "Không thể xóa cơ sở vì dữ liệu đã thay đổi hoặc có liên kết."; }
        return RedirectToAction(nameof(Index));
    }
}
