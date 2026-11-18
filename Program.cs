using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ServiceHubMVC.Data;
using ServiceHubMVC.Models;
using ServiceHubMVC.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. MVC
builder.Services.AddControllersWithViews();
// Đăng ký dịch vụ xử lý nghiệp vụ đặt lịch
builder.Services.AddScoped<BookingService>();

// Dịch vụ quản lý và chuyển trạng thái lịch đặt
builder.Services.AddScoped<BookingStatusService>();

// 2. Database
var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection");

// Windows Development: ưu tiên SQL Server LocalDB nếu chưa thiết lập User Secrets.
// macOS/Linux: cấu hình DefaultConnection tới SQL Server của máy (ví dụ Docker).
if (string.IsNullOrWhiteSpace(connectionString) &&
    builder.Environment.IsDevelopment() &&
    OperatingSystem.IsWindows())
{
    connectionString =
        @"Server=(localdb)\MSSQLLocalDB;Database=ServiceHubDB;Trusted_Connection=True;TrustServerCertificate=True";
}

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Chưa cấu hình ConnectionStrings:DefaultConnection. Xem docs/DEV_SETUP.md.");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// 3. ASP.NET Core Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.User.RequireUniqueEmail = true;

    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// 4. Authentication Cookie
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.Cookie.HttpOnly = true;
});

var app = builder.Build();

// Chỉ Development được tự áp dụng migrations; không tự thay đổi schema Production.
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();
}

// 5. Error Handling
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

// 6. Authentication and Authorization
app.UseAuthentication();
app.UseAuthorization();

// 7. Static Assets
app.MapStaticAssets();

// 8. MVC Routing
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

await DbSeeder.InitializeAsync(app.Services);

if (app.Environment.IsDevelopment())
{
    await DevAccountSeeder.InitializeAsync(
        app.Services,
        app.Configuration);

    await DemoDataSeeder.InitializeAsync(
        app.Services,
        app.Configuration);
}

app.Run();