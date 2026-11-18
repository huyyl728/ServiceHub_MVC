# Kết quả xác minh PR #27 — 18/11/2026

## Đã chạy trong phiên hoàn thiện bootstrap

- macOS 26.5, Apple Silicon; .NET SDK 10.0.403 / runtime 10.0.14.
- Database thử nghiệm: container **Azure SQL Edge** riêng, bind localhost cổng
  riêng; không dùng container/database đang có của người dùng. Đây là engine
  sẵn có để kiểm thử, không thay thế việc xác minh SQL Server/LocalDB của nhóm.
- Build và Release publish thành công, không báo warning/error.
- `ServiceHubMVC.Tests`: **7/7 passed**, gồm test cấu hình, bootstrap SQL và booking.
- `ServiceHubMVC.BookingManagement.Tests`: **13/13 passed**, gồm kiểm tra thuộc tính
  phân quyền/CSRF và các chuyển trạng thái booking.
- DB mới được migration đầy đủ, không còn pending migration/model change;
  đúng 4 Admin, 1 Owner, 2 Customer, 4 cơ sở, 16 dịch vụ.
- Kiểm tra mật khẩu/role cả 7 tài khoản, Owner email tùy chỉnh; hai bootstrap
  đồng thời không nhân bản; giữ phone/description của cơ sở và booking đã tạo.
- Mật khẩu/role xung đột làm seed rollback (kể cả tài khoản tạo trước khi lỗi),
  không thay mật khẩu cũ, không nâng quyền.
- Khởi động thực tế bằng `dotnet run --no-build` với file JSON nội bộ chứa bí mật
  ngẫu nhiên dành riêng cho test; login HTTP form (gồm antiforgery) thành công cho
  cả 7 tài khoản trước và sau khi khởi động lại.
- File JSON thật hiện diện trong lúc publish nhưng không vào `bin`/publish;
  `git check-ignore` nhận diện file và `git ls-files` không liệt kê file.
- Production bỏ qua file local (kể cả JSON lỗi) và bỏ qua bootstrap demo.
- File cấu hình sai được báo lỗi mà không đính kèm nội dung/inner exception.

Môi trường chạy kiểm thử này cần `DOTNET_USE_POLLING_FILE_WATCHER=1` để tránh
file watcher mặc định của .NET bị treo trong môi trường chạy có giới hạn quyền.
Đây là thiết lập của phiên kiểm thử, không được thêm vào cấu hình app của nhóm.

## Còn cần kiểm thử trước khi merge

- Clone sạch và chạy trên **Windows + LocalDB**.
- Chạy trên SQL Server x64 được hỗ trợ (phiên này dùng Azure SQL Edge).
- Toàn bộ luồng giao diện Customer đặt lịch / Owner sửa cơ sở và xem chi tiết;
  kiểm tra truy cập chéo giữa hai Owner qua HTTP. Các test thuộc tính phân quyền
  hiện có không thay thế kiểm thử truy cập thực tế này.
- Trưởng nhóm điền **mật khẩu hiện có** từ tài liệu nội bộ vào file riêng và gửi
  đúng thành viên; phiên này chỉ dùng mật khẩu test ngẫu nhiên, không tạo lại hoặc
  đổi mật khẩu dùng chung của nhóm.

PR tiếp tục **Draft**. **Không merge main** trong phiên này. ZIP cũ không phải
nguồn mã để kiểm thử bản hoàn thiện; xem `DEV_SETUP.md` để clone đúng nhánh.
