# SPRINT 3 – MA TRẬN KIỂM THỬ XÁC THỰC VÀ PHÂN QUYỀN

## 1. Thông tin chung

| Thuộc tính | Nội dung |
|---|---|
| Dự án | ServiceHub MVC – ASP.NET Core 10 |
| Sprint | Sprint 3 |
| Nhiệm vụ | S3-03 – Kiểm thử xác thực, phân quyền và quyền sở hữu lịch |
| GitHub Issue | [#13](https://github.com/huyyl728/ServiceHub_MVC/issues/13) |
| Người phụ trách | Bùi Quang Huy (`huybui330`) |
| Thời gian kiểm thử | 18/11/2026 và 24/11/2026 |
| Công cụ | Windows, .NET 10, SQL Server LocalDB, xUnit, Chrome, PowerShell |
| Nhánh báo cáo và kiểm thử Authorization | `feature/sprint3-authorization-tests` |
| Nhánh kiểm thử Booking tích hợp local (18/11) | `test/sprint3-booking-integration` |
| Nhánh bổ sung bằng chứng (24/11) | `feature/sprint3-issue13-evidence-20261124` |
| Tình trạng | **PARTIAL – Chờ nghiệm thu các mục chưa có bằng chứng** |

### 1.1. Phiên bản và phạm vi đã kiểm thử

- **Account/Profile:** mã nguồn từ PR #21, commit `ea554b2`; kiểm thử tại `http://127.0.0.1:5087`, database `ServiceHubMVC_AccountHttpTest`.
- **Booking:** bản tích hợp **local** từ `main` tại commit `aef0a8f` và mã Booking Management của nhánh PR #22 (`f5e4323`); kiểm thử tại `http://127.0.0.1:5088`, database `ServiceHubMVC_BookingHttpTest`.
- **xUnit Authorization:** project `tests/ServiceHubMVC.Authorization.Tests`, database riêng `ServiceHubMVC_AuthTest`.
- **xUnit Booking Management:** project `tests/ServiceHubMVC.BookingManagement.Tests`, database riêng `ServiceHubMVC_BookingTest`.
- **Tình trạng tích hợp GitHub:** PR #24 đã đưa mã Booking Management vào `main`; PR #22 đóng mà không merge trực tiếp vì nội dung đã được tích hợp bằng PR #24. Kết quả ở mục 4–5 thuộc đợt kiểm thử local ngày 18/11; mục 6–8 tổng hợp kết quả của cả hai đợt và ghi rõ phạm vi. **Đợt kiểm thử ngày 24/11 sử dụng mã từ `main` tại commit `ea2e923`**, trên worktree riêng, với bằng chứng chi tiết ở mục 12. Không suy diễn các kết quả này sang commit mới hơn.

## 2. Mục tiêu kiểm thử

Xác minh hệ thống ServiceHub:

- Xác thực đúng người dùng, từ chối mật khẩu sai và kết thúc phiên sau đăng xuất.
- Phân quyền Guest, Customer, Owner và Admin ở phía máy chủ.
- Chỉ cho phép đọc hoặc cập nhật tài nguyên thuộc quyền sở hữu của tài khoản đăng nhập.
- Không cho phép tự nâng quyền Admin hoặc Owner thông qua dữ liệu đăng ký.
- Chặn yêu cầu ghi thiếu hoặc sai antiforgery token khi sử dụng xác thực bằng cookie.
- Xử lý an toàn chuyển hướng đăng nhập và chống Open Redirect.
- Trả mã lỗi và phản hồi JSON đúng hợp đồng đối với API khi module API sẵn sàng.
- Không cho phép cập nhật trạng thái lịch trái quyền hoặc hai thao tác xung đột ghi đè nhau.

## 3. Tài khoản và dữ liệu kiểm thử

Toàn bộ là tài khoản giả, chỉ sử dụng trong database local.

| Ký hiệu | Email kiểm thử | Vai trò |
|---|---|---|
| Guest | Không đăng nhập | Guest |
| Customer A | `book-customer-a@example.test` | Customer |
| Customer B | `book-customer-b@example.test` | Customer |
| Owner A | `book-owner-a@example.test` | Owner |
| Owner B | `book-owner-b@example.test` | Owner |
| Admin | `book-admin-test@example.test` | Admin |

Dữ liệu Booking HTTP được tạo cho hai cơ sở, hai dịch vụ và hai lịch:

| Booking | Khách hàng | Cơ sở | Chủ cơ sở | Trạng thái ban đầu |
|---|---|---|---|---|
| `#1` | Customer A | `Booking HTTP - Business A` | Owner A | Pending (`0`) |
| `#2` | Customer B | `Booking HTTP - Business B` | Owner B | Pending (`0`) |

**Đợt 18/11:** Ban đầu có 4 tài khoản, 2 cơ sở, 2 dịch vụ và 2 lịch trong `ServiceHubMVC_BookingHttpTest`. Sau khi tạo Admin để kiểm thử, số tài khoản dự kiến là 5. **Đợt 24/11:** dữ liệu mới gồm Owner A, Owner B, hai Customer và ba Booking trong `ServiceHubMVC_Issue13_MainTest` (xem mục 12.3). Hai bộ dữ liệu này **độc lập**; không so sánh ID của các database như thể là một tập dữ liệu.

**Bảo mật:** Không đưa mật khẩu, cookie, mã CSRF hoặc thông tin cá nhân thật vào tài liệu.

## 4. Ma trận kiểm thử xác thực (đợt 18/11)

| Mã | Vai trò | Tình huống | Kết quả mong đợi | Kết quả thực tế / bằng chứng | Trạng thái |
|---|---|---|---|---|---|
| AUTH-01 | Guest | Vào Profile sau đăng xuất | Chuyển đến Login | Không truy cập được Profile khi chưa đăng nhập | **PASS** |
| AUTH-02 | Guest | Đăng ký tài khoản hợp lệ | Tạo Customer | Đăng ký thành công; SQL xác nhận vai trò Customer | **PASS** |
| AUTH-03 | Guest | Đăng ký bằng email đã tồn tại | Từ chối trùng email | Bài xUnit kiểm tra `DuplicateEmail` thành công; chưa ghi nhận kiểm thử HTTP riêng | **PASS (xUnit)** |
| AUTH-04 | Guest | Đăng nhập bằng mật khẩu sai | Không tạo phiên | Giao diện báo email/mật khẩu không chính xác; không đăng nhập | **PASS** |
| AUTH-05 | Customer | Đăng nhập hợp lệ | Đăng nhập thành công | Mở được Profile sau đăng nhập | **PASS** |
| AUTH-06 | Customer | Đăng xuất | Phiên bị kết thúc | Đăng xuất thành công, truy cập lại Profile bị yêu cầu đăng nhập; đăng nhập lại thành công | **PASS** |
| AUTH-07 | Guest | Gửi thêm `Role=Admin`, `IsAdmin=true` khi đăng ký | Chỉ có quyền Customer | `customer-d-test@example.test` vẫn có vai trò Customer trong SQL Server | **PASS** |

## 5. Ma trận kiểm thử phân quyền và quyền sở hữu Booking (đợt 18/11)

| Mã | Vai trò | Tình huống | Kết quả mong đợi | Kết quả thực tế / bằng chứng | Trạng thái |
|---|---|---|---|---|---|
| ROLE-01 | Guest | Truy cập `/Bookings` và `/Bookings/Owner` | Chuyển Login | Cả hai trả `302` đến `/Account/Login` | **PASS** |
| ROLE-02 | Customer A | Truy cập `/Bookings/Owner`, `/Bookings/OwnerDetails/1` | Từ chối vai trò | Không xem được các trang dành cho Owner | **PASS** |
| ROLE-03 | Customer A | Xem danh sách lịch cá nhân | Chỉ thấy lịch A | Danh sách chỉ có Booking `#1` | **PASS** |
| ROLE-04 | Owner A | Xem danh sách lịch cơ sở A | Chỉ thấy lịch cơ sở A | Danh sách chỉ có Booking `#1` | **PASS** |
| ROLE-05 | Owner A | Đổi ID để xem Booking B | Không đọc được Booking B | `/Bookings/OwnerDetails/2` trả `404`; Owner B đăng nhập riêng xem được Booking `#2` | **PASS** |
| ROLE-06 | Customer A | Đổi ID để xem Booking B | Không đọc được Booking B | `/Bookings/Details/2` trả `404`; Customer A xem được Booking `#1` | **PASS** |
| ROLE-07 | Customer A | POST hủy Booking B | Không thay đổi lịch B | HTTP cuối `404` sau chuyển hướng; SQL Booking `#2` vẫn `Pending (0)` | **PASS** |
| ROLE-08 | Owner A | POST Confirm/Reject Booking B | Không thay đổi lịch B | Cả hai yêu cầu có HTTP cuối `404` sau chuyển hướng; SQL Booking `#2` vẫn `Pending (0)` | **PASS** |

**Lưu ý về kết quả `404`:** Các bài POST trái quyền có chuyển hướng đến trang chi tiết bị chặn; mã `404` là phản hồi **cuối sau chuyển hướng**, không phải mã trạng thái POST ban đầu. Kết luận không cập nhật trái quyền dựa trên việc đối chiếu **SQL Server trước và sau yêu cầu**, không dựa riêng vào `404`.

### 5.1. Kiểm thử sai vai trò bổ sung

| Mã | Tình huống | Kết quả thực tế | Trạng thái |
|---|---|---|---|
| ROLE-09 | Owner A truy cập `/Bookings` | Bị từ chối | **PASS** |
| ROLE-10 | Owner A truy cập `/Bookings/Details/1` | Bị từ chối | **PASS** |
| ROLE-11 | Customer A gửi POST Confirm Booking `#1` bằng CSRF token hợp lệ | Bị chuyển đến AccessDenied; SQL không thay đổi | **PASS** |
| ROLE-12 | Owner A gửi POST Cancel Booking `#1` bằng CSRF token hợp lệ | Bị chuyển đến AccessDenied; SQL không thay đổi | **PASS** |

### 5.2. Kiểm thử Admin trên endpoint hiện có

Tài khoản Admin chỉ mang vai trò `Admin`, đã đăng xuất và đăng nhập lại sau khi đổi vai trò trong database kiểm thử.

| Mã | Tình huống | Kết quả mong đợi | Kết quả thực tế | Trạng thái |
|---|---|---|---|---|
| ADM-01 | Admin truy cập `/Account/Profile` | Xem hồ sơ của mình | Xem được Profile | **PASS** |
| ADM-02 | Admin truy cập `/Bookings` | Không có quyền Customer | Bị từ chối | **PASS** |
| ADM-03 | Admin truy cập `/Bookings/Owner` | Không có quyền Owner | Bị từ chối | **PASS** |
| ADM-04 | Admin truy cập `/Bookings/Details/1` | Không có quyền Customer | Bị từ chối | **PASS** |
| ADM-05 | Admin truy cập `/Bookings/OwnerDetails/2` | Không có quyền Owner | Bị từ chối | **PASS** |

**Giới hạn:** Năm bài Admin chỉ kiểm thử phân quyền ở các endpoint hiện có; **không chứng minh đã có hoặc đã nghiệm thu trang quản trị Admin**.

## 6. Ma trận kiểm thử CSRF, dữ liệu thừa và API (cập nhật 24/11)

| Mã | Tình huống | Kết quả mong đợi | Kết quả thực tế / phạm vi | Trạng thái |
|---|---|---|---|---|
| SEC-01 | POST chuyển trạng thái Booking **thiếu** antiforgery token | Từ chối yêu cầu | **24/11:** Owner A POST `/Bookings/Confirm/1` không gửi token, HTTP `400`; SQL Booking #1 vẫn Pending (`0`) | **PASS** |
| SEC-02 | POST chuyển trạng thái Booking với token **sai** | Từ chối yêu cầu | **24/11:** Owner A POST `/Bookings/Confirm/1` với token giả, HTTP `400`; SQL Booking #1 vẫn Pending (`0`) | **PASS** |
| SEC-03 | Gửi trường thừa nhằm sửa quyền hoặc dữ liệu người khác | Không được nâng quyền/ghi chéo | Đăng ký kèm `Role=Admin`, `IsAdmin=true` vẫn là Customer; Profile gửi `UserId` của người khác chỉ cập nhật hồ sơ đang đăng nhập | **PASS** |
| API-01 | Gọi API bảo vệ khi chưa đăng nhập | Mã `401`, JSON đúng hợp đồng | Chưa có kết quả thử API thật của Issue #10; không dùng redirect MVC thay cho mã API | **BLOCKED** |
| API-02 | Gọi API khi không đủ quyền | `403` hoặc mã phù hợp hợp đồng, không lộ dữ liệu | Chưa có kết quả thử API thật của Issue #10 | **BLOCKED** |

### 6.1. Bằng chứng HTTP đã có cho Account và Profile

| Tình huống | Kết quả | Trạng thái |
|---|---|---|
| POST `/Account/Login` thiếu token | `400 Bad Request` | **PASS** |
| POST `/Account/Login` với token giả | `400 Bad Request` | **PASS** |
| POST `/Account/Profile` thiếu token trong phiên hợp lệ | `400 Bad Request`, dữ liệu không đổi | **PASS** |
| Open Redirect sau Login | Chuyển đến URL nội bộ `/Account/Profile`, không chuyển sang URL bên ngoài | **PASS** |
| GET Profile bằng ID hoặc `userId` của Customer A khi đăng nhập Customer D | Vẫn hiển thị Profile Customer D | **PASS** |
| POST Profile kèm `UserId` của Customer A | Chỉ thay đổi FullName của Customer D; Customer A giữ nguyên | **PASS** |

## 7. Kiểm thử nghiệp vụ lịch đặt và chuyển trạng thái

### 7.1. Phạm vi nghiệp vụ cần kiểm tra

- Customer chỉ được hủy lịch Pending/Confirmed trước giờ bắt đầu.
- Owner chỉ được Confirm/Reject lịch Pending thuộc cơ sở mình.
- Owner chỉ được Complete lịch Confirmed sau giờ kết thúc.
- Lịch đã ở trạng thái kết thúc không được chuyển lại trái quy tắc.
- Hai yêu cầu chuyển trạng thái cạnh tranh không được ghi đè nhau.

### 7.2. Kết quả và giới hạn

| Hạng mục | Kết quả | Trạng thái |
|---|---|---|
| xUnit Booking Management trên SQL Server riêng | Tổng cộng **13/13 PASS**: kiểm tra metadata phân quyền/CSRF và nghiệp vụ BookingStatusService | **PASS** |
| Customer A hủy lịch B trái quyền | SQL Booking B giữ nguyên Pending (`0`) | **PASS** |
| Owner A xác nhận/từ chối lịch B trái quyền | SQL Booking B giữ nguyên Pending (`0`) | **PASS** |
| Customer POST Confirm / Owner POST Cancel sai vai trò | Bị từ chối; SQL hai lịch giữ nguyên Pending (`0`) | **PASS** |
| Thử hai yêu cầu HTTP thực sự cạnh tranh / hai kết nối SQL giao nhau | Chưa có bằng chứng tái hiện tranh chấp độc lập, lặp lại trên tích hợp | **CHƯA CHẠY / CẦN BỔ SUNG** |
| Chạy lại xUnit và các luồng Booking trọng yếu trên mã từ `main` commit `ea2e923` | 24/11: Authorization 4/4, Booking Management 13/13; quyền sở hữu và CSRF HTTP đạt kỳ vọng (mục 12) | **PASS (phạm vi đã chạy)** |

Không lấy 13/13 xUnit thay thế cho kết quả kiểm thử đồng thời HTTP thực tế.

## 8. Tổng hợp kết quả thực tế và bằng chứng

### 8.1. Tổng kết

| Nhóm kiểm thử | Kết quả thực tế | Tình trạng |
|---|---|---|
| Authorization xUnit | 4/4 thành công, 0 thất bại | **PASS** |
| Booking Management xUnit | 13/13 thành công, 0 thất bại | **PASS** |
| Guest truy cập Booking | 2/2 tình huống bị chuyển Login | **PASS** |
| Customer đọc/hủy lịch trái quyền | Không xem/hủy được Booking B | **PASS** |
| Owner đọc/xác nhận/từ chối lịch trái quyền | Không xem hoặc cập nhật được Booking B | **PASS** |
| Owner B đọc Booking B | Xem được danh sách và chi tiết trên phiên ẩn danh riêng | **PASS** |
| Customer/Owner truy cập sai vai trò | 4/4 GET bị từ chối | **PASS** |
| Customer/Owner POST sai vai trò | 2/2 bị từ chối, SQL không đổi | **PASS** |
| Admin kiểm thử trên endpoint hiện có | 5/5 đúng kỳ vọng | **PASS** |
| CSRF HTTP trên POST Booking | 24/11: thiếu token và token giả đều HTTP `400`; SQL không đổi | **PASS (2/2)** |
| API JSON / mã lỗi chuẩn | Chờ API thực tế của Issue #10 | **BLOCKED** |
| Kiểm thử cạnh tranh HTTP/SQL thực tế | Chưa hoàn tất | **CHƯA CHẠY** |
| Chạy lại trên mã `main` commit `ea2e923` | 24/11: xUnit 17/17; kiểm thử lại các luồng ROLE-05–08 và SEC-01–02 | **PASS (phạm vi đã chạy)** |

### 8.2. Ghi chú về kết quả bất thường

Lần đầu Owner B xem Booking B từng ghi nhận FAIL. Sau khi mở phiên Chrome ẩn danh riêng và đăng nhập Owner B, danh sách chỉ hiển thị Booking `#2` và trang `/Bookings/OwnerDetails/2` mở thành công. **Kết quả đối chứng đã PASS; chưa xác định nguyên nhân chính xác của lần FAIL ban đầu**, vì vậy không khẳng định lỗi đã được sửa trong mã nguồn.

### 8.3. Các dấu vết đã đối chiếu

- Log Terminal xUnit xác nhận Authorization `4/4` và Booking Management `13/13`.
- Kết quả `curl.exe` xác nhận Guest nhận `302` với `Location` về Login.
- Ảnh Chrome xác nhận danh sách/chi tiết lịch của Customer và Owner.
- Console Chrome xác nhận các yêu cầu POST trái quyền bị chuyển hướng; SQL xác nhận trạng thái Booking không đổi.
- Truy vấn SQL Server ngày 18/11 xác nhận Booking `#1` và `#2` vẫn Pending (`0`) trong `ServiceHubMVC_BookingHttpTest`; đợt 24/11 xác nhận cả Booking `#1`, `#2`, `#3` đều Pending (`0`) trong `ServiceHubMVC_Issue13_MainTest`.
- Kết quả Admin gồm năm trường hợp do người kiểm thử xác nhận.

**Lưu ý bằng chứng:** Tài liệu này mô tả các log và ảnh đã xem trong quá trình kiểm thử. Nếu nộp ảnh chụp màn hình trong repository/PR, chỉ đính kèm tệp thực sự có và đã xóa/che mọi thông tin nhạy cảm; không tự ghi tên tệp bằng chứng chưa được lưu.

**Quy ước:** `PASS` – đáp ứng kỳ vọng trên phạm vi đã thử; `FAIL` – hành vi sai kỳ vọng; `BLOCKED` – thiếu module/phụ thuộc; `CHƯA CHẠY` – chưa có bằng chứng thực thi.

## 9. Điều kiện hoàn thành Issue #13

- [x] Có ma trận Guest/Customer/Owner/Admin và dữ liệu kiểm thử riêng.
- [x] Có kết quả thực tế cho truy cập chéo, thay ID, nâng quyền bằng trường thừa và cập nhật trái quyền ở các endpoint đã triển khai.
- [x] Đã kiểm tra đăng xuất, đăng nhập và Open Redirect.
- [x] Có bằng chứng CSRF tại Account/Profile.
- [x] Có kiểm thử xUnit trên SQL Server và HTTP quyền sở hữu lịch.
- [x] Hoàn thành kiểm thử HTTP CSRF thiếu/sai token trên POST Booking (`400`, SQL không đổi, ngày 24/11).
- [ ] Kiểm thử API JSON đúng mã lỗi khi Issue #10 sẵn sàng.
- [ ] Bổ sung kiểm thử đồng thời có cạnh tranh thực tế.
- [x] Chạy lại build, hai bộ xUnit và các luồng Booking trọng yếu trên mã lấy từ `main` tại commit `ea2e923` (24/11).
- [ ] Nếu `main` có commit mới sau `ea2e923`, đối chiếu thay đổi và quyết định phạm vi kiểm thử lại trước nghiệm thu cuối.
- [x] Đã có PR #26 của `feature/sprint3-authorization-tests` liên kết Issue #13.
- [ ] Tạo PR bổ sung bằng chứng từ `feature/sprint3-issue13-evidence-20261124` để trưởng nhóm review; chưa xác nhận review/merge.

**Không đóng Issue khi còn các hạng mục bắt buộc BLOCKED hoặc CHƯA CHẠY.**

## 10. Phụ thuộc, hạn chế và bảo mật

- Phụ thuộc vào các module Account/Identity, Business/Services, Booking Management và API (Issue #10).
- Chức năng Admin riêng chưa được nghiệm thu; không suy diễn năm bài ADM thành bằng chứng về toàn bộ trang Admin.
- `ServiceHubMVC_BookingHttpTest`, `ServiceHubMVC_AccountHttpTest`, `ServiceHubMVC_Issue13_MainTest`, `ServiceHubMVC_AuthTest`, `ServiceHubMVC_BookingTest` là database phục vụ thử nghiệm; không sử dụng database sản xuất.
- Không đẩy mật khẩu, chuỗi kết nối có bí mật, cookie, antiforgery token, database local, `bin/` hoặc `obj/` lên GitHub.
- Ngày 24/11 đã chạy lại trên một worktree dựa trên `main` commit `ea2e923`. Đây là kiểm thử local trên mã lấy từ `main`, không chứng minh tự động cho những commit mới hơn hoặc môi trường triển khai khác.

## 11. Kết luận và bàn giao

Trong phạm vi các ca đã chạy, ServiceHub MVC **không bộc lộ quyền đọc hoặc cập nhật lịch của người khác**; việc phân quyền Guest, Customer, Owner và Admin tại các endpoint được kiểm thử đáp ứng kỳ vọng.

**Bộ kiểm thử tự động đạt 4/4 Authorization và 13/13 Booking Management.** Các tình huống POST trái quyền được đối chiếu thêm trong SQL Server, không chỉ dựa vào mã phản hồi cuối cùng trên trình duyệt.

**Trạng thái báo cáo: PARTIAL – Chưa đủ bằng chứng để kết luận hoàn tất mọi tiêu chí của Issue #13.** Những hạng mục còn thiếu được ghi ở mục 9; đợt kiểm thử lại ngày 24/11 đã hoàn thành ROLE-05–08, SEC-01–02 và 17/17 xUnit (xem mục 12).

**Liên kết:** [Issue #13](https://github.com/huyyl728/ServiceHub_MVC/issues/13) · [PR #26 – báo cáo gốc](https://github.com/huyyl728/ServiceHub_MVC/pull/26) · [PR #24 – tích hợp Booking Management](https://github.com/huyyl728/ServiceHub_MVC/pull/24) · [PR #22 – đã đóng, không merge trực tiếp](https://github.com/huyyl728/ServiceHub_MVC/pull/22)

## 12. Kiểm thử lại trên main ngày 24/11/2026

### 12.1. Môi trường kiểm thử

- Commit nền của worktree: `ea2e923` (mã lấy từ `origin/main` ngày 24/11/2026)
- Nhánh kiểm thử: `feature/sprint3-issue13-evidence-20261124`
- Hệ điều hành: Windows
- Framework: ASP.NET Core 10
- Database HTTP: `ServiceHubMVC_Issue13_MainTest`
- Database xUnit: `ServiceHubMVC_AuthTest`, `ServiceHubMVC_BookingTest`
- Website: `http://127.0.0.1:5093`
- Ghi chú: Đợt này dùng các tài khoản/dữ liệu độc lập với đợt 18/11; các Booking trong phần 12 thuộc database `ServiceHubMVC_Issue13_MainTest`.

### 12.2. Kết quả kiểm thử tự động

| Bộ kiểm thử | Tổng số | PASS | FAIL | Skipped |
|---|---:|---:|---:|---:|
| Authorization | 4 | 4 | 0 | 0 |
| Booking Management | 13 | 13 | 0 | 0 |
| **Tổng cộng** | **17** | **17** | **0** | **0** |

Kết luận: 17/17 bài xUnit đạt yêu cầu trên phiên bản được kiểm thử.

Database `ServiceHubMVC_BookingTest` được cập nhật migration trước khi chạy lại. Lượt trước có 5 FAIL; sau cập nhật đạt 13/13 PASS. Kết quả không đồng nghĩa đã kiểm thử đồng thời qua HTTP.

### 12.3. Kiểm thử phân quyền HTTP

| Mã | Tình huống | Kết quả thực tế | Trạng thái |
|---|---|---|---|
| ROLE-01 | Guest truy cập Booking | HTTP 302 đến Login | PASS |
| ROLE-02 | Customer truy cập trang Owner | Access Denied | PASS |
| ROLE-05 | Owner B xem danh sách của mình; Owner A xem danh sách và cố mở Booking #3 | Owner B chỉ thấy #3; Owner A chỉ thấy #1, #2; GET `/Bookings/OwnerDetails/3` trả `404` với Owner A | PASS (phạm vi đã thử) |
| ROLE-06 | Hai Customer xem chéo Booking | 4/4 trường hợp đúng quyền | PASS |
| ROLE-07 | Customer 1/2 lần lượt POST Cancel Booking của tài khoản kia | HTTP cuối `404` sau chuyển hướng; SQL cả hai Booking vẫn Pending (`0`) | PASS |
| ROLE-08 | Owner A POST Confirm/Reject Booking #3 thuộc Owner B | HTTP cuối `404` sau chuyển hướng; SQL Booking #3 vẫn Pending (`0`) sau từng thao tác | PASS |

Dữ liệu kiểm thử:

- Booking #1: `customer1@servicehub.test`, thuộc cơ sở Owner A (`owner@servicehub.test`).
- Booking #2: `customer2@servicehub.test`, thuộc cơ sở Owner A (`owner@servicehub.test`).
- Booking #3: `customer2@servicehub.test`, thuộc `ISSUE13 Business B`, Owner B (`owner-b-issue13@example.test`).

Các POST trái quyền đã dùng CSRF token hợp lệ của phiên đăng nhập hiện tại, nhưng cố truy cập Booking của tài khoản/cơ sở khác. Với `fetch` theo chuyển hướng, HTTP cuối `404` là phản hồi GET sau chuyển hướng, **không phải bằng chứng rằng POST trực tiếp trả `404`**. Kết quả được đối chiếu thêm bằng SQL Server.

### 12.4. Kiểm thử CSRF trên Booking

| Mã | Tình huống | HTTP | SQL | Trạng thái |
|---|---|---|---|---|
| SEC-01 | POST Confirm thiếu CSRF token | 400 | Không đổi | PASS |
| SEC-02 | POST Confirm với CSRF token giả | 400 | Không đổi | PASS |

Sau các lượt kiểm thử:

| Booking ID | Status |
|---|---|
| 1 | Pending (0) |
| 2 | Pending (0) |
| 3 | Pending (0) |

### 12.5. Phạm vi và hạn chế

- Các trường hợp quyền sở hữu Booking và CSRF đã được kiểm thử lại trên commit `ea2e923`.
- Kiểm thử API JSON của Issue #10 chưa có bằng chứng HTTP để đánh dấu PASS.
- Chưa có bằng chứng tái hiện đồng thời hai yêu cầu HTTP cạnh tranh.
- Chưa kiểm thử lại đầy đủ các chức năng Admin trên commit này; năm ca Admin ở mục 5.2 thuộc đợt 18/11.
- Ảnh và log được quan sát tại máy Windows trong quá trình kiểm thử; chưa xác nhận đã lưu các ảnh làm tệp minh chứng và đưa vào PR.
- Không suy diễn các kết quả từ commit này sang các commit mới hơn.

### 12.6. Kết luận

Kết quả kiểm thử lại xác nhận các endpoint Booking đã kiểm tra không cho phép Customer hoặc Owner đọc, hủy, xác nhận hay từ chối lịch thuộc quyền sở hữu của người khác.

Hai trường hợp CSRF đều bị từ chối với HTTP 400.

Bộ kiểm thử tự động đạt **17/17 PASS**.

Trạng thái báo cáo cho Issue #13: **PARTIAL**. Các mục API JSON, cạnh tranh HTTP/SQL thực tế và việc nghiệm thu PR còn chưa có đủ bằng chứng; đối chiếu mục 9. Không tự đánh dấu Issue đã hoàn thành.
