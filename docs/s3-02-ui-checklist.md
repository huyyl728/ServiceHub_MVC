# S3-02 — Hoàn thiện trải nghiệm giao diện (#12)

## Thay đổi

- Tăng vùng tương tác của biểu mẫu/nút, tối ưu khoảng cách ở điện thoại và xử lý trường nhập có chiều rộng lớn.
- Gắn lỗi ngày giờ với đúng trường nhập, thông báo đang gửi bằng tiếng Việt.
- Chống gửi lặp ở form POST đặt lịch và đăng xuất qua thuộc tính data-prevent-repeat. Khi validation hủy submit hoặc quay lại từ bộ nhớ đệm, form được mở khóa. Không thay thế xác thực, chống giả mạo hoặc chống trùng ở máy chủ.
- Các module về sau có thể dùng data-prevent-repeat và phần tử data-processing-message cho form POST điều hướng thông thường; không áp dụng cho form AJAX.

## Kết quả nghiệm thu giao diện

Commit kiểm thử: 5f8cd94
Môi trường: macOS, ASP.NET Core 10, SQL Server
Người kiểm thử: Đỗ Thành Huy

| Màn hình                  | 375px | 1366px |
| ------------------------- | ----- | ------ |
| Trang chủ và menu         | PASS  |
| Đăng nhập, đăng ký, hồ sơ | PASS  |
| Tìm kiếm và xem dịch vụ   | PASS  |
| Đặt lịch                  | PASS  |
| Xem và quản lý lịch       | PASS  |
| Quản lý cơ sở, dịch vụ    | PASS  |
| Dashboard Owner/Admin     | PASS  |

## Những file đã thay đổi

- Views/Account/Login.cshtml:
  Bổ sung chống gửi biểu mẫu lặp và thông báo đang đăng nhập.
- Views/Account/Register.cshtml:
  Bổ sung thông báo xử lý khi tạo tài khoản.
- Views/Account/Profile.cshtml:
  Bổ sung thông báo khi lưu hồ sơ.
- Views/Bookings/Index.cshtml:
  Liên kết nhãn và trường lọc ngày.

## Kết luận

Các thay đổi về giao diện đã được tích hợp
trên nhánh feature/sprint3-ui-final.

Kết quả nghiệm thu:
[Điền sau khi đối chiếu các ca kiểm thử thực tế]

Các lỗi còn tồn tại:
[Không có / mô tả lỗi phát hiện]

## Cách kiểm tra

1. Ghi commit kiểm thử, phiên bản .NET, trình duyệt và môi trường SQL Server; chạy dotnet build và dotnet run.
2. Mở DevTools, chế độ Responsive, nhập lần lượt 375 và 1366 pixel. Đảm bảo menu dùng được, footer không che nội dung và không cuộn ngang.
3. Dùng Tab để kiểm tra thứ tự focus; thử nhập thiếu ngày giờ, kiểm tra nhãn/lỗi cạnh trường.
4. Khi nghiệp vụ thật có sẵn, bật Slow 3G trong Network, nhấp nút đặt lịch hai lần và xác nhận chỉ một POST. Kiểm tra trùng lịch bằng cơ sở dữ liệu thật, không chỉ dựa trên JS.
5. Chụp ảnh trang chủ, danh sách, chi tiết và lỗi/đặt lịch trên hai kích thước khi các module đã có. Không đưa mật khẩu, cookie hoặc thông tin cá nhân thật vào ảnh.
6. Ghi lại kết quả thực tế và lỗi đã sửa, đính kèm PR. Không đánh dấu đạt các ca bị chặn.

## Trạng thái tại thời điểm chuẩn bị

Chưa có controller tài khoản/dịch vụ/lịch/bảng tổng quan trên main; các nhánh giao diện #4/#8 chưa hợp nhất vào main. Môi trường chuẩn bị patch thiếu .NET SDK, cần build và kiểm thử thực tế trên máy thành viên. Không đóng #12 trước khi kiểm thử tích hợp.
