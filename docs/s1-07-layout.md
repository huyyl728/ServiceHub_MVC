# S1-07 — Giao diện nền tảng (#4)

## Quy ước dùng lại

- Các Razor View sử dụng layout mặc định; đặt tiêu đề qua `ViewData["Title"]`.
- Dùng Bootstrap và các lớp `content-panel`, `feature-card`, `btn-primary`, `btn-outline-primary`.
- Thông báo sau chuyển hướng: `TempData["Success"]` hoặc `TempData["Error"]` với nội dung tiếng Việt an toàn cho người dùng.
- Khi truy vấn thật không có kết quả: `<partial name="_EmptyState" model='"Không tìm thấy dịch vụ phù hợp."' />`.
- Biểu mẫu ghi dữ liệu cần POST, mã chống giả mạo và kiểm tra quyền tại máy chủ.

## Điểm nối với module khác

Menu chỉ hiển thị khi action MVC tương ứng tồn tại. Tên action dưới đây là đề xuất tích hợp, cần đối chiếu với thành viên phụ trách trước khi nghiệm thu:

| Vai trò | Action |
| --- | --- |
| Khách chưa đăng nhập | Account/Login, Account/Register |
| Mọi vai trò | Home/Index, Services/Index |
| Đã đăng nhập | Account/Profile, Account/Logout (POST) |
| Customer | Bookings/MyBookings |
| Owner | Businesses/Index, Dashboard/Index |
| Admin | Dashboard/Index |

Không dùng việc ẩn liên kết thay cho Authorize và kiểm tra sở hữu dữ liệu.

## Kịch bản nghiệm thu

1. Chạy `dotnet build` với .NET SDK 10 và cấu hình SQL Server theo nhóm.
2. Chạy `dotnet run`; mở trang chủ và trang quyền riêng tư.
3. Kiểm tra 375px và 1366px: không cuộn ngang, mở/đóng menu, footer không che nội dung, điều hướng bàn phím.
4. Sau khi #3 tích hợp, kiểm tra khách, Customer, Owner, Admin; đăng nhập, đăng ký, hồ sơ, đăng xuất POST có mã chống giả mạo.
5. Sau khi các module nghiệp vụ tích hợp, xác nhận tên action, quyền truy cập và trạng thái rỗng trên dữ liệu thật.

## Trạng thái

- Đã triển khai mã layout, trang chủ, menu, footer, thông báo, trạng thái rỗng và trang lỗi.
- Chưa xác nhận build/runtime: môi trường triển khai hiện không có .NET SDK.
- Kiểm tra xác thực và điều hướng nghiệp vụ bị chặn bởi #3 và các module chưa có trên nhánh gốc.
- Không đóng issue trước khi PR được review, merge và xác minh.
