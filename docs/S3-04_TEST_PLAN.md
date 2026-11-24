# S3-04 — Dữ liệu thử và biên bản kiểm thử cơ sở, dịch vụ, tìm kiếm

Liên kết [issue #14](https://github.com/huyyl728/ServiceHub_MVC/issues/14). **Trạng thái: CHƯA CHẠY các ca trong bảng dưới đây.** Tài liệu này chuẩn bị dữ liệu và cách ghi bằng chứng; không chứng minh nghiệm thu. Ghi commit `git rev-parse HEAD`, ngày giờ Việt Nam, người chạy, phiên bản Windows/.NET/SQL Server và database thử trước khi đánh dấu PASS.

## Chuẩn bị trên Windows với SQL Server thử

1. Cài Git, .NET SDK 10, SQL Server Express hoặc SQL Server; với máy dùng `SQLEXPRESS01`, mở SQL Server Configuration Manager/SSMS để kiểm tra tên instance. LocalDB `MSSQLLocalDB` chỉ dùng nếu máy thực sự đã cài LocalDB. Làm theo [DEV_SETUP.md](DEV_SETUP.md) để clone, cấu hình file nội bộ (đã được `.gitignore`) và chạy `dotnet build ServiceHubMVC.csproj`.
2. Tạo **database thử mới** tên `ServiceHubDB_S3_04_Test`. Trong `appsettings.Development.local.json` đặt `ConnectionStrings:DefaultConnection` trỏ đúng instance và database này. Với xác thực Windows, ví dụ chuỗi kết nối giá trị JSON: `Server=.\\SQLEXPRESS01;Database=ServiceHubDB_S3_04_Test;Trusted_Connection=True;TrustServerCertificate=True`. Đổi `SQLEXPRESS01` theo instance của máy. Điền các mật khẩu demo do trưởng nhóm cấp trong file riêng; không commit hay chụp file.
3. Bật `SeedAccounts:SharedDemo:Enabled` và chạy `dotnet run --project ServiceHubMVC.csproj`. Development sẽ áp dụng migration, tạo 7 tài khoản, 4 cơ sở và 16 dịch vụ. Lưu mã commit và ảnh/log đã lọc bí mật. Kiểm tra dữ liệu trong đúng database trước khi chạy script.
4. Trong SSMS chọn `ServiceHubDB_S3_04_Test` rồi chạy [sql/s3-04-discovery-fixture.sql](sql/s3-04-discovery-fixture.sql). Script kiểm tra tên database và Owner demo, chỉ thêm 13 dịch vụ `[S3-04] Cầu lông mẫu 01`…`13` thuộc cơ sở `[DEMO] Sân thể thao`; giá 91.000–103.000, thời lượng 30/45/60 phút. Chạy lại phải vẫn có đúng 13 bản ghi. Nếu email Owner demo đã đổi, chỉ sửa giá trị email ở bản sao **local** của script, không đưa email thật vào PR.
5. Cần **Owner thứ hai** để thử đổi ID. Nhờ người quản lý tạo tài khoản thử Owner 2 bằng ASP.NET Identity theo quy trình cấp quyền của nhóm, trên database thử; tự đăng ký từ form chỉ được vai trò Customer. Không chèn trực tiếp vào `AspNetUsers` hoặc tự cấp role bằng SQL. Nếu chưa có Owner 2, đánh dấu các ca quyền sở hữu `BLOCKED`, không giả vờ kiểm thử.

`ServicesController.Index` dùng `PageSize = 12`, lọc `keyword` theo tên dịch vụ/cơ sở, so sánh `category` chính xác và kẹp `page` ngoài phạm vi về trang hợp lệ. Các ca tra cứu bên dưới dùng `keyword=Cầu lông mẫu` và `category=Thể thao`: đúng 13 dịch vụ cùng bộ lọc, trang 1 có 12, trang 2 có 1. Dùng `http://localhost:5247` nếu chạy theo launch profile mặc định; nếu console hiện URL khác thì dùng URL đó. Các ca thêm/sửa/xóa phải thao tác qua giao diện Owner và form có antiforgery; SQL chỉ dùng để tạo fixture và kiểm chứng.

## Ma trận ca cần chạy và ghi bằng chứng

| Mã  | Thực hiện trên dữ liệu thử                                                                      | Kết quả mong đợi                                                                                             | Trạng thái           |
| --- | ----------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------ | -------------------- |
| D01 | Chạy script hai lần; truy vấn dịch vụ `[S3-04]`; khởi động lại ứng dụng, tải lại trang          | Đúng 13 dịch vụ cùng bộ lọc, ID/giá/thời lượng giữ nguyên                                                    | PASS                 |
| B01 | Owner tạo cơ sở và dịch vụ mới; xem chi tiết, sửa tên/giá, tải lại                              | Dữ liệu mới đúng trong SQL; Owner thấy thay đổi sau tải lại                                                  | PASS                 |
| B02 | Tạo/sửa cơ sở, dịch vụ với tên rỗng/quá dài, giá âm, thời lượng 0                               | Form báo lỗi, không tạo/cập nhật bản ghi                                                                     | PASS                 |
| B03 | Xóa dịch vụ chưa có lịch; xóa cơ sở chưa có dịch vụ                                             | Xóa thành công, bản ghi khác giữ nguyên                                                                      | PASS                 |
| B04 | Customer đặt lịch tương lai cho một dịch vụ thử; Owner cố xóa dịch vụ và cơ sở chứa dịch vụ     | Báo ràng buộc, lịch và dữ liệu lịch sử còn nguyên                                                            | PASS                 |
| S01 | Mở `/Services?keyword=Cầu%20lông%20mẫu&category=Thể%20thao&page=1` rồi `page=2`                 | Trang 1 có 12, trang 2 có 1; bộ lọc còn trên link/phần nhập                                                  | PASS                 |
| S02 | Mở cùng URL với `page=999`, `page=0`, `page=-1`                                                 | Trang được kẹp 2, 1, 1; không lỗi và không mất bộ lọc                                                        | PASS                 |
| S03 | Tìm `keyword=khong-co-s3-04`, lọc `category=khong-co`                                           | Hiện trạng thái rỗng; không hiện dịch vụ lạ                                                                  | PASS                 |
| S04 | Tạm ngừng một dịch vụ, rồi cơ sở; tìm kiếm, xem URL chi tiết và thử đặt lịch mới; sau đó mở lại | Tài nguyên ngừng hoạt động biến khỏi danh sách, không xem/đặt lịch mới; lịch cũ giữ nguyên; mở lại xuất hiện | PASS                 |
| A01 | Owner 2 đổi `id`/`businessId` trên URL GET và gửi form POST sửa/xóa/trạng thái của Owner 1      | Trả từ chối/404, không thay đổi bản ghi; kiểm tra kết quả #13 cùng commit                                    | BLOCKED: cần Owner 2 |

**Lưu ý:** URL chi tiết tài nguyên không hoạt động có thể trả trang 404; ghi đúng HTTP và thông báo thực tế. Khi thử B04, dùng dịch vụ riêng ngoài 13 dịch vụ của ca phân trang để không làm thay đổi số liệu trang. Khi tạm ngừng một dịch vụ trong S04, kiểm tra lại S01 sau khi mở lại. Nếu khác mong đợi, ghi FAIL và issue xử lý; không sửa dữ liệu trực tiếp để che lỗi.

## Kiểm chứng SQL chỉ đọc, chạy trên database thử

```sql
SELECT COUNT(*) AS SoDichVuLoc
FROM dbo.Services s JOIN dbo.Businesses b ON b.Id = s.BusinessId
WHERE s.Name LIKE N'%Cầu lông mẫu%' AND s.Category = N'Thể thao'
  AND s.IsAvailable = 1 AND b.IsActive = 1;
-- Trước ca S04 và sau khi mở lại: 13. Trong lúc tạm ngừng: 12 hoặc 0.

SELECT b.Id AS BusinessId, b.Name, b.IsActive, s.Id AS ServiceId,
       s.Name AS ServiceName, s.IsAvailable, s.Price, s.DurationMinutes
FROM dbo.Businesses b LEFT JOIN dbo.Services s ON s.BusinessId = b.Id
WHERE b.Name LIKE N'[[]DEMO]%' OR b.Name LIKE N'[[]S3-04]%'
ORDER BY b.Id, s.Id;

SELECT bk.Id, bk.ServiceId, bk.StartTime, bk.EndTime, bk.PriceAtBooking, bk.Status
FROM dbo.Bookings bk JOIN dbo.Services s ON s.Id = bk.ServiceId
WHERE s.Name LIKE N'[[]S3-04]%'
ORDER BY bk.Id;
```

Không đưa tên/email khách hàng, cookie, token, mật khẩu hoặc chuỗi kết nối vào ảnh/log. Mã lịch và ID dữ liệu thử đủ để kiểm tra lịch không bị xóa; chỉ dùng database thử riêng. Nếu đã có lịch từ lần thử trước, dùng một database thử mới để có kết quả có thể lặp lại.

## Nhật ký kết quả (điền sau khi chạy)

| Mã ca   | Người chạy, giờ Việt Nam | OS / .NET / SQL | Commit + database thử | Kết quả thực tế, số hàng SQL | PASS / FAIL / BLOCKED | Ảnh/log/issue và người sửa |
| ------- | ------------------------ | --------------- | --------------------- | ---------------------------- | --------------------- | -------------------------- |
| D01–A01 | Chưa chạy                | Chưa ghi        | Chưa ghi              | Chưa có bằng chứng thực tế   | CHƯA CHẠY             | Chưa có                    |

Tổng hợp kết quả thật và liên kết tài liệu này vào [PR #30 cho #16](https://github.com/huyyl728/ServiceHub_MVC/pull/30) sau khi nhóm chạy; chỉ nhóm trưởng mới chốt #15 và nghiệm thu cuối.
