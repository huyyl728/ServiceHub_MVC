# S3-06 — Tình trạng tài liệu API tại commit bàn giao đang rà soát

**Commit:** `ea2e9237c56a161c3b451d57598d8a998d23885e` (`main`, 24/11/2026).

Tại commit này, `Program.cs` đăng ký route MVC thông thường; danh sách `Controllers/` không có controller API và không có route `api/*`. Vì thế **chưa có API JSON đang chạy trên `main` để ghi ví dụ request/response thành công hoặc lỗi**. [Issue #10](https://github.com/huyyl728/ServiceHub_MVC/issues/10) đã đóng, nhưng cần kiểm tra nhánh/PR và merge module API trước khi đưa chức năng này vào nội dung nghiệm thu [#15](https://github.com/huyyl728/ServiceHub_MVC/issues/15). Không lấy đường dẫn MVC, trang HTML hay redirect đăng nhập làm ví dụ phản hồi JSON.

| Nội dung cần tài liệu hóa sau tích hợp | Điều phải kiểm tra trực tiếp |
| --- | --- |
| GET danh sách/chi tiết dịch vụ | Route thật, query lọc/phân trang, JSON thành công và 404 |
| POST tạo lịch | Đầu vào thực, xác thực và chống giả mạo theo cơ chế đã triển khai; JSON 201/400/409; giá và thời lượng do server quyết định |
| GET lịch của mình, yêu cầu hủy | Route thật, quyền sở hữu và trạng thái; JSON thành công và 401/403/404/409 theo chính sách thực tế |
| Đồng thời MVC/API | Hai request cạnh tranh cùng dịch vụ/khung giờ trên SQL Server, kết quả HTTP và số bản ghi sau khi chạy |

**Mẫu ghi cho từng endpoint khi đã có mã và kiểm thử:** `commit | method/path | role | headers bắt buộc | request thực tế đã lược bí mật | status | response JSON thực tế | ca lỗi | log/PR`. Không ghi ví dụ tự đặt trước khi quan sát API chạy. Nhờ Đỗ Thành Huy xác nhận vị trí PR/commit tích hợp API và Bùi Quang Huy liên kết kết quả kiểm thử quyền.

## Điểm tham chiếu MVC đang tồn tại

`GET /Services` nhận `keyword`, `category`, `page`; `GET /Services/Details/{id}` trả trang chi tiết. Customer gửi form `POST /Services/Book/{id}` với trường `startTimeLocal` và antiforgery token. Khi thành công, action chuyển hướng tới chi tiết lịch; khi thời gian nhập sai, hiển thị lại form; khi trùng giờ đặt, action trả trang với HTTP 409 và thông báo; khi hệ thống bận, HTTP 503. Đây là **hành vi MVC trả HTML**, được đọc từ `ServicesController.cs` và `ServicesController.Book.cs`, không phải hợp đồng API. Kiểm tra bằng HTTP thật trước khi ghi status phản hồi vào báo cáo nghiệm thu.
