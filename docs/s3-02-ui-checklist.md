# S3-02 — Hoàn thiện trải nghiệm giao diện (#12)

## Thay đổi

- Tăng vùng tương tác của biểu mẫu/nút, tối ưu khoảng cách ở điện thoại và xử lý trường nhập có chiều rộng lớn.
- Gắn lỗi ngày giờ với đúng trường nhập, thông báo đang gửi bằng tiếng Việt.
- Chống gửi lặp ở form POST đặt lịch và đăng xuất qua thuộc tính data-prevent-repeat. Khi validation hủy submit hoặc quay lại từ bộ nhớ đệm, form được mở khóa. Không thay thế xác thực, chống giả mạo hoặc chống trùng ở máy chủ.
- Các module về sau có thể dùng data-prevent-repeat và phần tử data-processing-message cho form POST điều hướng thông thường; không áp dụng cho form AJAX.

## Bảng nghiệm thu (điền kết quả thật)

| Trang/ca kiểm tra | 375px | 1366px | Bằng chứng |
| --- | --- | --- | --- |
| Trang chủ, menu mở/đóng, quyền riêng tư | Chờ kiểm tra | Chờ kiểm tra | Ảnh màn hình |
| Đăng ký/đăng nhập/hồ sơ/đăng xuất | Bị chặn #3 | Bị chặn #3 | Chờ tích hợp |
| Quản lý cơ sở/dịch vụ | Bị chặn #5 | Bị chặn #5 | Chờ tích hợp |
| Danh sách, lọc, phân trang, chi tiết dịch vụ | Bị chặn #6 | Bị chặn #6 | Chờ tích hợp |
| Đặt lịch hợp lệ, lỗi trùng, giữ ngày giờ, gửi lặp | Bị chặn #7/#8 | Bị chặn #7/#8 | Chờ tích hợp |
| Danh sách lịch đặt | Bị chặn #9 | Bị chặn #9 | Chờ tích hợp |
| Bảng tổng quan | Bị chặn #11 | Bị chặn #11 | Chờ tích hợp |

## Cách kiểm tra

1. Ghi commit kiểm thử, phiên bản .NET, trình duyệt và môi trường SQL Server; chạy dotnet build và dotnet run.
2. Mở DevTools, chế độ Responsive, nhập lần lượt 375 và 1366 pixel. Đảm bảo menu dùng được, footer không che nội dung và không cuộn ngang.
3. Dùng Tab để kiểm tra thứ tự focus; thử nhập thiếu ngày giờ, kiểm tra nhãn/lỗi cạnh trường.
4. Khi nghiệp vụ thật có sẵn, bật Slow 3G trong Network, nhấp nút đặt lịch hai lần và xác nhận chỉ một POST. Kiểm tra trùng lịch bằng cơ sở dữ liệu thật, không chỉ dựa trên JS.
5. Chụp ảnh trang chủ, danh sách, chi tiết và lỗi/đặt lịch trên hai kích thước khi các module đã có. Không đưa mật khẩu, cookie hoặc thông tin cá nhân thật vào ảnh.
6. Ghi lại kết quả thực tế và lỗi đã sửa, đính kèm PR. Không đánh dấu đạt các ca bị chặn.

## Trạng thái tại thời điểm chuẩn bị

Chưa có controller tài khoản/dịch vụ/lịch/bảng tổng quan trên main; các nhánh giao diện #4/#8 chưa hợp nhất vào main. Môi trường chuẩn bị patch thiếu .NET SDK, cần build và kiểm thử thực tế trên máy thành viên. Không đóng #12 trước khi kiểm thử tích hợp.
