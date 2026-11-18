# ServiceHub MVC

Website đặt lịch và quản lý dịch vụ trực tuyến. Stack: ASP.NET Core 10 MVC, EF Core 10, SQL Server, Identity, Razor Views/Bootstrap.

## Chạy dự án

Hướng dẫn **cài đặt Mac/Windows, database tự migration, tài khoản demo và dữ liệu mẫu**:
[docs/DEV_SETUP.md](docs/DEV_SETUP.md).

**PR #27:** clone nhánh `feature/shared-dev-bootstrap`, không dùng ZIP cũ:

```sh
git clone --branch feature/shared-dev-bootstrap --single-branch https://github.com/huyyl728/ServiceHub_MVC.git ServiceHub_MVC-pr27
cd ServiceHub_MVC-pr27
```

Nhận `appsettings.Development.local.json` từ trưởng nhóm, đặt cạnh `.csproj`,
đảm bảo SQL Server đã chạy, rồi `dotnet run`. File chứa kết nối và email/mật khẩu
nội bộ, được ignore và không đi vào publish. Mẫu không có bí mật:
[`appsettings.Development.local.example.json`](appsettings.Development.local.example.json).

Trong Development, dự án tự migration rồi tạo:
- Roles `Admin`, `Owner`, `Customer`.
- 4 Admin, 1 Owner, 2 Customer demo (email cố định; mật khẩu được cấp riêng).
- 4 cơ sở, 16 dịch vụ mẫu, không tạo trùng khi chạy lại.

**Không đưa mật khẩu, file chứa tài khoản nội bộ hoặc database thật lên GitHub.**

## Thành viên nhóm

- Đỗ Thành Huy
- Nguyễn Đắc Tuấn Hưng
- Bùi Quang Huy
- Đoàn Trần Gia Hưng

## Kế hoạch

- Sprint 1: Khởi tạo dự án, database, authentication, UI.
- Sprint 2: Services, bookings, search, REST API.
- Sprint 3: Dashboard, testing, documentation.
