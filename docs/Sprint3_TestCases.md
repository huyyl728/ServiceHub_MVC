# SPRINT 3 – MA TRẬN KIỂM THỬ XÁC THỰC VÀ PHÂN QUYỀN

## 1. Thông tin chung

| Thuộc tính | Nội dung |
|---|---|
| Dự án | ServiceHub MVC – ASP.NET Core 10 |
| Sprint | Sprint 3 |
| Nhiệm vụ | S3-03 – Kiểm thử xác thực, phân quyền và quyền sở hữu lịch |
| GitHub Issue | [#13](https://github.com/huyyl728/ServiceHub_MVC/issues/13) |
| Người phụ trách | Bùi Quang Huy (`huybui330`) |
| Ngày thực hiện | 18/11/2026 |
| Công cụ | Windows, .NET 10, SQL Server LocalDB, xUnit, Chrome, PowerShell |
| Nhánh báo cáo và kiểm thử Authorization | `feature/sprint3-authorization-tests` |
| Nhánh kiểm thử Booking tích hợp local | `test/sprint3-booking-integration` |
| Tình trạng | **PARTIAL – Chưa đủ điều kiện đóng Issue #13** |

### 1.1. Phiên bản và phạm vi đã kiểm thử

- **Account/Profile:** mã nguồn từ PR #21, commit `ea554b2`; kiểm thử tại `http://127.0.0.1:5087`, database `ServiceHubMVC_AccountHttpTest`.
- **Booking:** bản tích hợp **local** từ `main` tại commit `aef0a8f` và mã Booking Management của nhánh PR #22 (`f5e4323`); kiểm thử tại `http://127.0.0.1:5088`, database `ServiceHubMVC_BookingHttpTest`.
- **xUnit Authorization:** project `tests/ServiceHubMVC.Authorization.Tests`, database riêng `ServiceHubMVC_AuthTest`.
- **xUnit Booking Management:** project `tests/ServiceHubMVC.BookingManagement.Tests`, database riêng `ServiceHubMVC_BookingTest`.
- **Tình trạng tích hợp GitHub:** PR #24 đã đưa mã Booking Management vào `main`; PR #22 đóng mà không merge trực tiếp vì nội dung đã được tích hợp bằng PR #24. Các kết quả HTTP trong tài liệu này được thực hiện trên **bản local**, **chưa phải kết quả chạy lại trên `main` sau PR #24**.

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

Ban đầu có 4 tài khoản, 2 cơ sở, 2 dịch vụ và 2 lịch trong `ServiceHubMVC_BookingHttpTest`. Sau khi tạo Admin để kiểm thử, số tài khoản dự kiến là 5. **Không đưa mật khẩu, cookie, mã CSRF hoặc thông tin cá nhân thật vào tài liệu.**

## 4. Ma trận kiểm thử xác thực

| Mã | Vai trò | Tình huống | Kết quả mong đợi | Kết quả thực tế / bằng chứng | Trạng thái |
|---|---|---|---|---|---|
| AUTH-01 | Guest | Vào Profile sau đăng xuất | Chuyển đến Login | Không truy cập được Profile khi chưa đăng nhập | **PASS** |
| AUTH-02 | Guest | Đăng ký tài khoản hợp lệ | Tạo Customer | Đăng ký thành công; SQL xác nhận vai trò Customer | **PASS** |
| AUTH-03 | Guest | Đăng ký bằng email đã tồn tại | Từ chối trùng email | Bài xUnit kiểm tra `DuplicateEmail` thành công; chưa ghi nhận kiểm thử HTTP riêng | **PASS (xUnit)** |
| AUTH-04 | Guest | Đăng nhập bằng mật khẩu sai | Không tạo phiên | Giao diện báo email/mật khẩu không chính xác; không đăng nhập | **PASS** |
| AUTH-05 | Customer | Đăng nhập hợp lệ | Đăng nhập thành công | Mở được Profile sau đăng nhập | **PASS** |
| AUTH-06 | Customer | Đăng xuất | Phiên bị kết thúc | Đăng xuất thành công, truy cập lại Profile bị yêu cầu đăng nhập; đăng nhập lại thành công | **PASS** |
| AUTH-07 | Guest | Gửi thêm `Role=Admin`, `IsAdmin=true` khi đăng ký | Chỉ có quyền Customer | `customer-d-test@example.test` vẫn có vai trò Customer trong SQL Server | **PASS** |

## 5. Ma trận kiểm thử phân quyền và quyền sở hữu Booking

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

**Lưu ý về kết quả `404`:** Các bài POST trái quyền có chuyển hướng đến trang chi tiết bị chặn; mã `404` là phản hồi **cuối sau chuyển hướng**. Kết luận không cập nhật trái quyền dựa trên việc đối chiếu **SQL Server trước và sau yêu cầu**, không dựa riêng vào `404`.

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

## 6. Ma trận kiểm thử CSRF, dữ liệu thừa và API

| Mã | Tình huống | Kết quả mong đợi | Kết quả thực tế / phạm vi | Trạng thái |
|---|---|---|---|---|
| SEC-01 | POST chuyển trạng thái Booking **thiếu** antiforgery token | Từ chối yêu cầu | Đã kiểm tra thiếu token tại POST Login và POST Profile (`400`); **chưa thử HTTP riêng trên POST Booking** | **CHƯA CHẠY (Booking)** |
| SEC-02 | POST chuyển trạng thái Booking với token **sai** | Từ chối yêu cầu | POST Login với token giả trả `400`; **chưa thử HTTP riêng trên POST Booking** | **CHƯA CHẠY (Booking)** |
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
| Chạy lại đầy đủ trên `main` mới nhất sau PR #24 | Chưa ghi nhận kết quả trên phiên bản đó | **CHƯA CHẠY** |

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
| CSRF HTTP trên các POST Booking | Chưa kiểm thử riêng cả thiếu và sai token | **CHƯA CHẠY** |
| API JSON / mã lỗi chuẩn | Chờ API thực tế của Issue #10 | **BLOCKED** |
| Kiểm thử cạnh tranh HTTP/SQL thực tế | Chưa hoàn tất | **CHƯA CHẠY** |
| Chạy lại trên `main` đã merge PR #24 | Chưa có bằng chứng | **CHƯA CHẠY** |

### 8.2. Ghi chú về kết quả bất thường

Lần đầu Owner B xem Booking B từng ghi nhận FAIL. Sau khi mở phiên Chrome ẩn danh riêng và đăng nhập Owner B, danh sách chỉ hiển thị Booking `#2` và trang `/Bookings/OwnerDetails/2` mở thành công. **Kết quả đối chứng đã PASS; chưa xác định nguyên nhân chính xác của lần FAIL ban đầu**, vì vậy không khẳng định lỗi đã được sửa trong mã nguồn.

### 8.3. Các dấu vết đã đối chiếu

- Log Terminal xUnit xác nhận Authorization `4/4` và Booking Management `13/13`.
- Kết quả `curl.exe` xác nhận Guest nhận `302` với `Location` về Login.
- Ảnh Chrome xác nhận danh sách/chi tiết lịch của Customer và Owner.
- Console Chrome xác nhận các yêu cầu POST trái quyền bị chuyển hướng; SQL xác nhận trạng thái Booking không đổi.
- Truy vấn SQL Server xác nhận `#1` và `#2` vẫn `Pending (0)` sau các lần thử cập nhật trái quyền.
- Kết quả Admin gồm năm trường hợp do người kiểm thử xác nhận.

**Lưu ý bằng chứng:** Tài liệu này mô tả các log và ảnh đã xem trong quá trình kiểm thử. Nếu nộp ảnh chụp màn hình trong repository/PR, chỉ đính kèm tệp thực sự có và đã xóa/che mọi thông tin nhạy cảm; không tự ghi tên tệp bằng chứng chưa được lưu.

**Quy ước:** `PASS` – đáp ứng kỳ vọng trên phạm vi đã thử; `FAIL` – hành vi sai kỳ vọng; `BLOCKED` – thiếu module/phụ thuộc; `CHƯA CHẠY` – chưa có bằng chứng thực thi.

## 9. Điều kiện hoàn thành Issue #13

- [x] Có ma trận Guest/Customer/Owner/Admin và dữ liệu kiểm thử riêng.
- [x] Có kết quả thực tế cho truy cập chéo, thay ID, nâng quyền bằng trường thừa và cập nhật trái quyền ở các endpoint đã triển khai.
- [x] Đã kiểm tra đăng xuất, đăng nhập và Open Redirect.
- [x] Có bằng chứng CSRF tại Account/Profile.
- [x] Có kiểm thử xUnit trên SQL Server và HTTP quyền sở hữu lịch.
- [ ] Hoàn thiện kiểm thử HTTP CSRF thiếu/sai token trên POST Booking.
- [ ] Kiểm thử API JSON đúng mã lỗi khi Issue #10 sẵn sàng.
- [ ] Bổ sung kiểm thử đồng thời có cạnh tranh thực tế.
- [ ] Chạy lại build, xUnit và các luồng quan trọng trên `main` mới nhất sau khi tích hợp.
- [ ] Tạo PR của `feature/sprint3-authorization-tests` liên kết Issue #13 và được review.

**Không đóng Issue khi còn các hạng mục bắt buộc BLOCKED hoặc CHƯA CHẠY.**

## 10. Phụ thuộc, hạn chế và bảo mật

- Phụ thuộc vào các module Account/Identity, Business/Services, Booking Management và API (Issue #10).
- Chức năng Admin riêng chưa được nghiệm thu; không suy diễn năm bài ADM thành bằng chứng về toàn bộ trang Admin.
- `ServiceHubMVC_BookingHttpTest`, `ServiceHubMVC_AccountHttpTest`, `ServiceHubMVC_AuthTest`, `ServiceHubMVC_BookingTest` là database phục vụ thử nghiệm; không sử dụng database sản xuất.
- Không đẩy mật khẩu, chuỗi kết nối có bí mật, cookie, antiforgery token, database local, `bin/` hoặc `obj/` lên GitHub.
- Các thao tác kiểm thử trực tiếp đã chạy trên local; không tự nhận đã được xác minh trên `main` sau khi merge PR #24.

## 11. Kết luận và bàn giao

Trong phạm vi các ca đã chạy, ServiceHub MVC **không bộc lộ quyền đọc hoặc cập nhật lịch của người khác**; việc phân quyền Guest, Customer, Owner và Admin tại các endpoint được kiểm thử đáp ứng kỳ vọng.

**Bộ kiểm thử tự động đạt 4/4 Authorization và 13/13 Booking Management.** Các tình huống POST trái quyền được đối chiếu thêm trong SQL Server, không chỉ dựa vào mã phản hồi cuối cùng trên trình duyệt.

**Trạng thái cuối: PARTIAL – Chưa đóng Issue #13.** Những hạng mục còn thiếu được ghi ở mục 9 để trưởng nhóm review, phân công bổ sung và nghiệm thu trên phiên bản tích hợp cuối cùng.

**Liên kết:** [Issue #13](https://github.com/huyyl728/ServiceHub_MVC/issues/13) · [PR #24 đã merge](https://github.com/huyyl728/ServiceHub_MVC/pull/24) · [PR #22 đã đóng, không merge trực tiếp](https://github.com/huyyl728/ServiceHub_MVC/pull/22)
