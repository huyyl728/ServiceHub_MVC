# ServiceHub MVC

Website đặt lịch và quản lý dịch vụ trực tuyến. Stack: ASP.NET Core 10 MVC, EF Core 10, SQL Server, Identity, Razor Views/Bootstrap.

## Chạy dự án

Hướng dẫn **cài đặt Mac/Windows, database tự migration, tài khoản demo và dữ liệu mẫu**:
[docs/DEV_SETUP.md](docs/DEV_SETUP.md).

Trong Development, khi đã cấu hình SQL Server và bật Seeder, dự án tạo:
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
