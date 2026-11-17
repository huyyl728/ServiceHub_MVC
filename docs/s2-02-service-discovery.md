# S2-02 — Danh sách, tìm kiếm, lọc và phân trang dịch vụ (#6)

## Phạm vi và hợp đồng giao diện

- `GET /Services/Index?keyword=&category=&page=1`: trả `ServiceListViewModel` theo đúng ViewModel của #8; không tạo ViewModel hay Razor View trùng lặp.
- `GET /Services/Details/{id}`: trả `ServiceDetailViewModel`; dịch vụ không tồn tại hoặc ngừng hoạt động trả view `NotFound` với HTTP 404.
- Chỉ lấy dịch vụ có `IsAvailable = true` và cơ sở có `IsActive = true`.
- Tìm theo tên dịch vụ hoặc tên cơ sở; kết hợp lọc danh mục. So khớp chuỗi theo collation SQL Server đang cấu hình.
- Mỗi trang tối đa 12 dịch vụ, sắp xếp theo tên rồi ID. `Count`, `Skip` và `Take` được thực thi tại database.
- Nếu trang nhỏ hơn 1, dùng trang đầu; nếu vượt tổng số trang, dùng trang cuối; khi không có kết quả, giữ bộ lọc và hiển thị trạng thái rỗng.
- Dùng dữ liệu thật từ `ApplicationDbContext`, không sửa model, migration, cấu hình, code giao diện hay nghiệp vụ đặt lịch.
- Hai action GET công khai không yêu cầu đăng nhập. POST đặt lịch không thuộc phạm vi #6; phối hợp chủ nhiệm #7/#8 để bổ sung riêng và không tự phát triển lại nghiệp vụ.

## Kiểm thử tích hợp trên SQL Server

Chuẩn bị dữ liệu bằng chức năng Owner của #5: ít nhất hai cơ sở, hai danh mục và 13 dịch vụ hoạt động cùng bộ lọc, một dịch vụ tạm ngừng và một cơ sở tạm ngừng. Chỉ dùng dữ liệu thử nghiệm.

| Ca kiểm thử | Kết quả mong đợi | Trạng thái |
| --- | --- | --- |
| Người chưa đăng nhập mở danh sách và chi tiết | Xem được dịch vụ/cơ sở đang hoạt động | Chưa chạy |
| Tìm theo tên dịch vụ hoặc tên cơ sở | Kết quả đúng theo từ khóa | Chưa chạy |
| Kết hợp từ khóa và danh mục | Chỉ hiển thị bản ghi thỏa cả hai | Chưa chạy |
| Bộ lọc có 13 bản ghi | Trang đầu 12, trang sau 1, không trùng ID | Chưa chạy |
| Bấm trang tiếp theo | Từ khóa và danh mục giữ nguyên | Chưa chạy |
| `page=0`, `page=-1`, `page=9999` | Điều chỉnh về trang hợp lệ | Chưa chạy |
| Từ khóa/danh mục không khớp | Danh sách rỗng, thông báo tiếng Việt | Chưa chạy |
| Tạm ngừng dịch vụ hoặc cơ sở từ #5 | Không xuất hiện trong danh sách; chi tiết HTTP 404 | Chưa chạy |
| Mở lại tài nguyên | Xuất hiện lại khi cả hai trạng thái đều hoạt động | Chưa chạy |
| Truy cập ID không tồn tại | View NotFound, HTTP 404 | Chưa chạy |

## Trạng thái và phụ thuộc

Mã mới phụ thuộc ViewModels và Razor Views ở #8 (PR #19), dữ liệu thật từ #5 (PR #23). Nhánh triển khai tách từ nhánh giao diện để không sao chép code của thành viên khác. Chưa chạy build hoặc kiểm thử SQL Server ở môi trường chuẩn bị mã; cần ghép đúng các phiên bản phụ thuộc, kiểm thử thực tế và ghi minh chứng trước khi nghiệm thu. Không đóng issue khi chưa được nhóm hợp nhất và xác minh.
