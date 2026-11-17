using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceHubMVC.Data;
using ServiceHubMVC.Models;
using ServiceHubMVC.ViewModels;

namespace ServiceHubMVC.Controllers;

// Phần truy vấn công khai; điểm gọi đặt lịch có thể được bổ sung ở file partial riêng.
public partial class ServicesController(ApplicationDbContext db) : Controller
{
    private const int PageSize = 12;

    private IQueryable<Service> AvailableServices() => db.Services
        .AsNoTracking()
        .Where(s => s.IsAvailable && s.Business.IsActive);

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> Index(string? keyword, string? category, int page = 1,
        CancellationToken cancellationToken = default)
    {
        keyword = string.IsNullOrWhiteSpace(keyword) ? null : keyword.Trim();
        category = string.IsNullOrWhiteSpace(category) ? null : category.Trim();

        var available = AvailableServices();

        // Danh mục công khai chỉ lấy từ dịch vụ có thể đặt, độc lập với bộ lọc hiện tại.
        var categories = await available.Select(s => s.Category)
            .Distinct().OrderBy(c => c).ToListAsync(cancellationToken);

        var query = available;
        if (keyword is not null)
        {
            query = query.Where(s => s.Name.Contains(keyword) || s.Business.Name.Contains(keyword));
        }

        if (category is not null)
        {
            query = query.Where(s => s.Category == category);
        }

        // Đếm trên SQL Server và giới hạn trang trước khi tính số bản ghi bỏ qua.
        var total = await query.CountAsync(cancellationToken);
        var totalPages = total == 0 ? 0 : (total - 1) / PageSize + 1;
        page = Math.Clamp(page, 1, Math.Max(1, totalPages));

        var items = await query
            .OrderBy(s => s.Name).ThenBy(s => s.Id)
            .Skip((page - 1) * PageSize).Take(PageSize)
            .Select(s => new ServiceCardViewModel
            {
                Id = s.Id,
                Name = s.Name,
                Category = s.Category,
                BusinessName = s.Business.Name,
                Address = s.Business.Address,
                Price = s.Price,
                DurationMinutes = s.DurationMinutes
            }).ToListAsync(cancellationToken);

        return View(new ServiceListViewModel
        {
            Items = items,
            Categories = categories,
            Keyword = keyword,
            Category = category,
            Page = page,
            TotalPages = totalPages
        });
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken = default)
    {
        var detail = await AvailableServices().Where(s => s.Id == id)
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
            }).SingleOrDefaultAsync(cancellationToken);

        if (detail is null)
        {
            Response.StatusCode = StatusCodes.Status404NotFound;
            return View("NotFound");
        }

        return View(detail);
    }
}
