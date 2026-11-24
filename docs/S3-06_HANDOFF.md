# S3-06 — Hồ sơ bàn giao ServiceHub MVP

**Trạng thái:** bản khung để nhóm rà soát, 24/11/2026. **Mã đã đối chiếu:** `ea2e9237c56a161c3b451d57598d8a998d23885e` (`main`, sau PR #28). Đây không phải biên bản nghiệm thu #15. Liên kết công việc: [issue #16](https://github.com/huyyl728/ServiceHub_MVC/issues/16).

## 1. Dựng lại trên máy thành viên

1. Cài Git, .NET SDK 10 và SQL Server phù hợp; kiểm tra `git --version`, `dotnet --info`. Windows có thể dùng SQL Server LocalDB.
2. Clone bằng `git clone --branch main https://github.com/huyyl728/ServiceHub_MVC.git ServiceHub_MVC`; chạy `cd ServiceHub_MVC`, `git rev-parse HEAD` để ghi lại commit.
3. Nhận `appsettings.Development.local.json` qua kênh riêng từ trưởng nhóm, đặt cạnh `ServiceHubMVC.csproj`. Điền kết nối SQL Server và tài khoản thử theo [DEV_SETUP.md](DEV_SETUP.md); không commit file này, không dùng dữ liệu người thật. Trên Windows, có thể để kết nối rỗng để dùng LocalDB theo cấu hình ứng dụng. Trên Mac/Linux phải điền kết nối SQL Server.
4. Chạy `dotnet build ServiceHubMVC.csproj` rồi `dotnet run --project ServiceHubMVC.csproj`; mở `http://localhost:5247` với launch profile Development mặc định. Ứng dụng tự áp dụng migration trong Development và seed dữ liệu thử nếu cấu hình tương ứng hợp lệ. Không cần lệnh migration riêng khi chạy app.
5. Theo hướng dẫn chi tiết trong [DEV_SETUP.md](DEV_SETUP.md), kiểm tra cơ sở/dịch vụ mẫu, đăng nhập theo vai trò, đặt và xử lý lịch; khởi động lại để kiểm tra dữ liệu giữ nguyên. Ghi hệ điều hành, phiên bản .NET, SQL engine, commit và kết quả thực tế.

**Chưa được xác nhận trong tài liệu này:** một thành viên khác đã tự dựng lại bản bàn giao trên Windows/LocalDB, chạy đủ luồng và ghi bằng chứng. Kết quả build/seed trên Mac tại PR #27 thuộc một commit cũ, không được áp sang commit nêu trên.

## 2. Nội dung có trong mã ở commit đã đối chiếu

| Phần | Điểm kiểm tra trong mã | Trạng thái bàn giao |
| --- | --- | --- |
| Đăng ký, đăng nhập, hồ sơ | `Controllers/AccountController.cs` | Có mã; chờ nghiệm thu toàn luồng #15 |
| Quản lý cơ sở/dịch vụ của Owner | `Controllers/BusinessesController.cs`, `Controllers/OwnerServicesController.cs` | Có mã; chờ kết quả #14/#15 |
| Tìm kiếm, lọc, xem dịch vụ, đặt lịch MVC | `Controllers/ServicesController.cs`, `Controllers/ServicesController.Book.cs` | Có mã; chờ kết quả #14/#15 |
| Xem, hủy, xác nhận, từ chối, hoàn thành lịch | `Controllers/BookingsController.cs` | Có mã; chờ kết quả #15 |
| Dashboard Owner/Admin | `Controllers/DashboardController.cs`; [PR #28](https://github.com/huyyl728/ServiceHub_MVC/pull/28) đã merge | Có mã; chưa suy diễn issue #11 đã nghiệm thu toàn bộ ca |
| REST API JSON | Không có controller API hay route `api/*` trong `main` tại commit này | **Chưa tích hợp vào bản bàn giao**; [issue #10](https://github.com/huyyl728/ServiceHub_MVC/issues/10) đóng không chứng minh API có trên `main` |
| Giao diện cuối | [PR #29](https://github.com/huyyl728/ServiceHub_MVC/pull/29) đang Draft tại lần rà soát | Chưa đưa thay đổi PR này vào tài liệu cho `main` |

Sơ đồ và luồng đọc tại [S3-06_ARCHITECTURE.md](S3-06_ARCHITECTURE.md); tình trạng ví dụ API tại [S3-06_API_STATUS.md](S3-06_API_STATUS.md). Các tài liệu cũ ghi “chưa có controller” hoặc “PR #27 còn Draft” phản ánh **thời điểm được viết**; khi trích dẫn hiện trạng cần dùng bảng này và kiểm tra lại commit mới nhất.

## 3. Phân công và bằng chứng

| Thành viên | Phần được giao theo #16 | Đầu ra cần xác nhận |
| --- | --- | --- |
| Đoàn Trần Gia Hưng (`@Hung2k7n`) | Tổng hợp hồ sơ, liên kết, đối chiếu chức năng và rubric | Ba tài liệu S3-06 trong PR này; kết quả #14 và liên kết minh chứng thật cần bổ sung |
| Đỗ Thành Huy (`@huyyl728`) | Xác nhận kỹ thuật/rubric, nghiệm thu tích hợp và rà soát cuối | [#15](https://github.com/huyyl728/ServiceHub_MVC/issues/15), rubric chấm thực tế và ý kiến review |
| Nguyễn Đắc Tuấn Hưng (`@tuanhungnguyendac-netizen`) | Slide, video và màn hình demo | Slide/video thực tế, kết quả [#12](https://github.com/huyyl728/ServiceHub_MVC/issues/12) |
| Bùi Quang Huy (`@huybui330`) | Xác thực, vai trò, ca kiểm thử | Kết quả [#13](https://github.com/huyyl728/ServiceHub_MVC/issues/13), log/ảnh đã lọc bí mật |

| Bằng chứng có thể dẫn | Phạm vi chứng minh | Không suy diễn thành |
| --- | --- | --- |
| [PR #27](https://github.com/huyyl728/ServiceHub_MVC/pull/27), [nhật ký PR #27](PR27_VALIDATION.md) | Kết quả build, migration/seed, đăng nhập trên môi trường được ghi ngày 18/11 | Đã kiểm chứng Windows và commit `ea2e923` |
| [Sprint3_TestCases.md](Sprint3_TestCases.md), [PR #26](https://github.com/huyyl728/ServiceHub_MVC/pull/26) | Các ca phân quyền được tác giả ghi nhận ở phiên bản nêu trong tài liệu | Toàn bộ #13/#15 đã đạt ở bản cuối |
| [PR #28](https://github.com/huyyl728/ServiceHub_MVC/pull/28) | Dashboard đã được đưa vào `main` | Đã đối chiếu số liệu dashboard trên bộ dữ liệu cuối |
| [#14](https://github.com/huyyl728/ServiceHub_MVC/issues/14), [#15](https://github.com/huyyl728/ServiceHub_MVC/issues/15), [#16](https://github.com/huyyl728/ServiceHub_MVC/issues/16) | Danh sách công việc và điều kiện nghiệm thu | Bằng chứng đã chạy nếu checkbox còn mở |

**Mẫu ghi ca còn thiếu:** `ngày giờ (GMT+7) | người chạy | OS/.NET/SQL | commit | dữ liệu thử | bước chạy | mong đợi | kết quả thật | PASS/FAIL/BLOCKED/CHƯA CHẠY | liên kết log/ảnh đã che dữ liệu`. Không ghi tài khoản thật, mật khẩu, cookie, connection string hoặc token vào kho.

## 4. Kịch bản slide và quay video (chưa ghi hình)

| Cảnh | Thao tác trên dữ liệu thử | Lời dẫn gợi ý | Điều kiện trước khi quay |
| --- | --- | --- | --- |
| 1. Giới thiệu | Hiện commit, kiến trúc, vai trò | “ServiceHub kết nối khách với cơ sở dịch vụ và lưu lịch đặt.” | Mã và SQL Server khớp #15 |
| 2. Owner | Đăng nhập, tạo cơ sở và dịch vụ | “Chủ cơ sở quản lý dữ liệu thuộc mình.” | Xác nhận dữ liệu thử và quyền |
| 3. Customer | Tìm/lọc dịch vụ, xem chi tiết, đặt lịch | “Giá và thời lượng lấy từ dịch vụ; thời gian được kiểm tra ở máy chủ.” | Luồng chạy thật trên cùng commit |
| 4. Xử lý | Owner xem và xác nhận; Customer xem/hủy khi trạng thái cho phép | “Mỗi vai trò chỉ xử lý lịch mình được phép.” | Dùng lịch/role riêng cho demo |
| 5. Trường hợp lỗi | Thử khung giờ giao nhau và truy cập trái quyền | “Hệ thống báo xung đột hoặc từ chối truy cập theo tình huống.” | Đã kiểm tra thông báo và kết quả SQL trên #15 |
| 6. Tổng kết | Hiện ma trận PASS/FAIL/BLOCKED, hạn chế | “Các phần chưa nghiệm thu được ghi rõ để nhóm xử lý.” | Có liên kết bằng chứng thật |

Nguyễn Đắc Tuấn Hưng dựng slide/quay video sau khi nhóm chốt phiên bản chạy; **chưa có slide, tệp video hoặc kết quả quay được xác nhận trong PR tài liệu này**. Không đưa API vào video trước khi nó xuất hiện và được kiểm thử trên commit trình diễn.

## 5. Đối chiếu rubric và chốt bàn giao

Tệp mẫu báo cáo được chia sẻ trong nhóm có yêu cầu hình thức trình bày; phần bảng rubric là mẫu chưa điền tiêu chí và trọng số cho dự án này. **Cần Đỗ Thành Huy cung cấp rubric gốc của môn/đề để đối chiếu điểm, tiêu chí bắt buộc.** Chưa thể khẳng định dự án đạt thang điểm nào.

| Tiêu chí rubric nguyên văn | Trọng số | Chức năng/issue | Bằng chứng cùng commit | Thiếu sót và người xử lý | Kết luận |
| --- | --- | --- | --- | --- | --- |
| Chờ rubric gốc | Chờ | Chờ đối chiếu #3–#16 | Chờ kết quả #15 | Đỗ Thành Huy cung cấp nguồn | **BLOCKED** |

Trước khi bàn giao: (1) xác nhận `main` và commit sau các PR cuối; (2) có người khác dựng lại và thử trên SQL Server; (3) điền ca #14/#15, API nếu được tích hợp; (4) dẫn slide/video thật; (5) điền từng dòng rubric theo bản gốc; (6) Đỗ Thành Huy review cuối. **Issue #16 để mở tới khi có bằng chứng và review.**
