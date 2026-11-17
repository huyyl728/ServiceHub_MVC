# S2-04 — Giao diện tìm dịch vụ và đặt lịch (#8)

## Phạm vi hiện tại

Mã Razor và ViewModel hiển thị, dựa trên layout #4. Không tạo controller hay dữ liệu giả thay cho #6 hoặc BookingService #7.

## Hợp đồng đề xuất, cần thống nhất với người phụ trách

- GET Services/Index?keyword=&category=&page=1: trả ServiceListViewModel; #6 truy vấn và phân trang tại SQL Server, chỉ dịch vụ/cơ sở hoạt động.
- GET Services/Details/{id}: trả ServiceDetailViewModel; nếu không tồn tại hoặc không hoạt động trả HTTP 404 cùng view NotFound.
- POST Services/Book/{id}: chỉ nhận StartTimeLocal (định dạng yyyy-MM-ddTHH:mm, giờ Việt Nam) và ID dịch vụ từ route. Kiểm tra chống giả mạo, Customer và ModelState tại máy chủ, lấy CustomerId từ Identity. Diễn giải thời gian với offset +07:00, chuyển UTC rồi gọi BookingService.CreateBookingAsync. Không nhận giá, thời lượng hay CustomerId từ trình duyệt.
- Khi lỗi: thêm thông báo tiếng Việt vào ModelState, tải lại thông tin dịch vụ từ truy vấn thật, trả Details giữ StartTimeLocal. Khi thành công: TempData["Success"] và chuyển hướng GET. Không ghi lỗi kỹ thuật nhạy cảm vào giao diện.
- Kiểm tra hợp đồng thực tế #3, #6, #7 và #9 trước khi nối; không đổi code nghiệp vụ của thành viên khác.

## Kiểm thử sau tích hợp

| Ca | Kết quả mong đợi |
| --- | --- |
| Khách chưa đăng nhập | Xem dịch vụ hoạt động, không gửi đặt lịch |
| Customer | Đặt lịch thật với giờ Việt Nam, máy chủ tính giá và thời lượng |
| Owner/Admin | Không đặt lịch bằng quyền Customer |
| Bộ lọc và phân trang | Giữ từ khóa/danh mục, kết quả đúng SQL Server |
| Không tìm thấy/dữ liệu rỗng | Thông báo tiếng Việt, HTTP 404 khi chi tiết không tồn tại |
| Giờ quá khứ/trùng lịch | Báo lỗi, giữ giờ nhập, không tạo bản ghi sai |
| POST không có mã chống giả mạo | Máy chủ từ chối |
| 375px/1366px | Không tràn ngang, form và nút dễ sử dụng |

## Trạng thái

Chưa chạy build tại môi trường chuẩn bị patch (thiếu SDK .NET). Chưa nghiệm thu tích hợp: #3 và #6 chưa có trên main; BookingService #7 đang ở nhánh riêng; điểm gọi MVC chưa có. Không đóng #8 trước khi tích hợp và kiểm thử thật.
