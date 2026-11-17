# S2-01 — Quản lý cơ sở và dịch vụ (#5)

## Cách sử dụng

- Đăng nhập tài khoản Owner trong môi trường phát triển (cần module #3).
- Truy cập `/Businesses/Index`, tạo cơ sở, sau đó chọn Dịch vụ.
- Trang quản lý dịch vụ dùng `/OwnerServices/Index?businessId=<id>`, không trùng controller công khai `Services` của #6.
- Không cần migration mới: dùng bảng và quan hệ sẵn có. Không thêm quyền cho Customer/Admin.
- Khi cơ sở tạm ngừng, trạng thái riêng của các dịch vụ được giữ nguyên. #6 và #7 phải kiểm tra cả hai trạng thái trước khi hiển thị/đặt lịch.
- Không xóa cơ sở còn dịch vụ hoặc dịch vụ đã có lịch đặt; có thể tạm ngừng thay thế.

## Checklist kiểm thử trên SQL Server

| Ca kiểm thử | Kết quả mong đợi | Trạng thái |
| --- | --- | --- |
| Owner thêm, xem, sửa cơ sở và dịch vụ | Dữ liệu đúng ngay cả sau khi tải lại | Chưa chạy |
| Tên/danh mục rỗng, giá âm, thời lượng ngoài 1–1440 | Báo lỗi, không lưu | Chưa chạy |
| Owner thứ hai đổi ID trên URL hoặc POST | Không tìm thấy tài nguyên của Owner khác | Chưa chạy |
| Customer/Admin truy cập URL quản lý | Bị từ chối | Chưa chạy |
| Tạm ngừng/mở lại cơ sở và dịch vụ | Lưu trạng thái, không làm mất lịch đặt | Chưa chạy |
| Xóa dịch vụ chưa có lịch và cơ sở không còn dịch vụ | Xóa thành công | Chưa chạy |
| Xóa dịch vụ có lịch hoặc cơ sở còn dịch vụ | Thông báo rõ, dữ liệu được giữ | Chưa chạy |
| Gửi POST thiếu mã chống giả mạo | Bị từ chối | Chưa chạy |

## Trạng thái và phụ thuộc

Mã chỉ bổ sung file mới. Chưa xác nhận build hoặc kiểm thử SQL Server tại môi trường chuẩn bị mã do thiếu SDK .NET 10 và SQL Server. Module xác thực #3 cần được tích hợp để kiểm tra qua giao diện. Không đóng issue hoặc ghi nhận các ca trên là đạt trước khi review và kiểm thử thật.
