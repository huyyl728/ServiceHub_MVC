# Clone sạch và chạy ServiceHub trên Mac / Windows

Dành cho Development nội bộ. Thành viên mới dùng bản đã tích hợp trên
`main`, **không dùng ZIP cũ**. Nhánh `feature/shared-dev-bootstrap` là nhánh
phát triển của PR #27, không phải nhánh cần clone cho thiết lập mới.

## 1. Chuẩn bị một lần trên mỗi máy

- Git và **.NET SDK 10** (`dotnet --info` để kiểm tra).
- SQL Server đang chạy, tài khoản có quyền tạo database và áp dụng migrations.
- Windows: có thể cài SQL Server Express LocalDB (`sqllocaldb info MSSQLLocalDB`).
- macOS: LocalDB không chạy trên Mac. Dùng SQL Server tại máy chủ mà nhóm cấp,
  hoặc môi trường SQL Server Docker tương thích với máy. Trên Apple Silicon,
  ưu tiên SQL Server trên máy/VM x64 được hỗ trợ; container amd64 qua giả lập không
  được Microsoft hỗ trợ. Xem [yêu cầu nền tảng SQL Server](https://learn.microsoft.com/en-us/sql/linux/install-upgrade/setup?view=sql-server-ver17). Không cần đổi code để đổi máy chủ: chỉ đổi file nội bộ.
- Không cần cài `dotnet-ef` để chạy app: chương trình tự áp dụng migration đã commit.

## 2. Clone bản tích hợp trên main

Lệnh giống nhau trên Terminal macOS và PowerShell Windows; chọn thư mục mới:

```sh
git clone --branch main https://github.com/huyyl728/ServiceHub_MVC.git ServiceHub_MVC
cd ServiceHub_MVC
git branch --show-current
```

Kết quả cuối phải là `main`. Với bản clone đúng nhánh đang
có, dùng `git pull --ff-only` sau khi đã lưu/commit các thay đổi riêng.
Đừng chép source, `bin`, `obj`, `.git` hoặc database từ ZIP vào bản clone này.

## 3. Nhận đúng một file cấu hình riêng

Trưởng nhóm gửi **`appsettings.Development.local.json`** qua kênh riêng.
Đặt file **cạnh `ServiceHubMVC.csproj` và `Program.cs`**, không đặt trong `wwwroot`.
Đảm bảo Windows không đổi thành `.json.txt`.

File chứa:

- `ConnectionStrings:DefaultConnection`: kết nối SQL Server riêng của máy.
- `SeedAccounts:SharedDemo:Enabled`: `true`.
- `AdminEmails`: đúng 4 email; `OwnerEmail`: 1 email; `CustomerEmails`: đúng 2 email.
- `AdminPassword`, `OwnerPassword`, `CustomerPassword`: mật khẩu nhóm cấp riêng.
  Bốn Admin dùng chung mật khẩu Admin, hai Customer dùng chung mật khẩu Customer.

Email mặc định (giữ nguyên để tương thích với dữ liệu PR trước):

| Vai trò | Email |
| --- | --- |
| Admin | `admin1@servicehub.test` … `admin4@servicehub.test` |
| Owner | `owner@servicehub.test` |
| Customer | `customer1@servicehub.test`, `customer2@servicehub.test` |

**Windows + LocalDB:** để `DefaultConnection` là chuỗi rỗng. Development sẽ dùng
`Server=(localdb)\MSSQLLocalDB;Database=ServiceHubDB;Trusted_Connection=True;TrustServerCertificate=True`.
Nếu cần kiểm thử database mới mà vẫn giữ database cũ, điền chuỗi này với tên
`Database=ServiceHubDB_PR27_Clean`. Trong JSON phải viết `\\` thay cho `\`.

**Mac / SQL Server có tài khoản SQL:** trưởng nhóm điền chuỗi kết nối theo mẫu
sau trong file riêng (thay các chỗ `<...>`; không dùng chúng làm giá trị thật):

```text
Server=<host>,<port>;Database=ServiceHubDB_PR27_Clean;User Id=<sql-user>;Password=<sql-password>;TrustServerCertificate=True
```

Mật khẩu SQL Server và mật khẩu đăng nhập website là hai loại khác nhau. Không gửi
mật khẩu qua câu lệnh terminal, ảnh chụp màn hình, issue hoặc PR. Chuỗi kết nối có
ký tự đặc biệt cần tuân theo cú pháp SQL connection string và escape JSON.
`TrustServerCertificate=True` chỉ dành cho kết nối dev đã tin cậy.

## 4. Chạy và đăng nhập

```sh
dotnet build ServiceHubMVC.csproj
dotnet run --project ServiceHubMVC.csproj
```

Hoặc chỉ `dotnet run` trong thư mục dự án (tự restore/build). Launch profile mặc
định đặt `ASPNETCORE_ENVIRONMENT=Development`; mở **http://localhost:5247**.
Nhập email/mật khẩu trong file trưởng nhóm gửi. Không cần chạy lệnh seed hay
migration riêng, không cần nhập lại User Secrets trên từng máy.

Startup Development thực hiện:

1. Đọc file riêng, kiểm tra đủ 7 email không trùng và mật khẩu đạt chính sách.
2. Áp dụng các EF migrations, gồm cột `Businesses.PhoneNumber`.
3. Seed 3 roles, đúng 4 Admin / 1 Owner / 2 Customer trên database mới.
4. Seed 4 cơ sở / 16 dịch vụ. Toàn bộ seed chạy trong một transaction và dùng
   khóa SQL để hai lần khởi động đồng thời không tạo trùng.

Chạy lại giữ nguyên tài khoản, mật khẩu, vai trò, thông tin đã sửa và lịch đặt.
Nếu email tồn tại nhưng sai vai trò/mật khẩu, startup **báo lỗi**, không tự nâng
quyền/đặt lại mật khẩu. Dùng lại đúng file đã seed hoặc đổi sang database dev mới;
không xóa database đang có dữ liệu. Nếu seed lỗi, các ghi mới trong lần seed đó
được rollback; migrations đã áp dụng vẫn còn.

Khi SharedDemo bật, cấu hình seed Admin/Owner kiểu cũ được bỏ qua để không tạo
thêm tài khoản ngoài bộ 7. Database cũ có tài khoản khác sẽ không bị xóa chúng.
Nếu đổi email hay đổi tên cơ sở/dịch vụ demo đã seed, đây không phải cơ chế đồng
bộ/đổi tên: hãy dùng database mới để kiểm thử một bộ demo sạch.

## 5. Trưởng nhóm tạo và gửi file bí mật

Tạo bản riêng từ **template chỉ chứa placeholder, không có mật khẩu thật**:

macOS:

```sh
cp appsettings.Development.local.example.json appsettings.Development.local.json
chmod 600 appsettings.Development.local.json
```

Windows PowerShell:

```powershell
Copy-Item appsettings.Development.local.example.json appsettings.Development.local.json
```

Mở file riêng trong trình soạn thảo, thay 3 placeholder bằng **mật khẩu demo đã
thống nhất trong `TAI_KHOAN_SERVICEHUB_NOI_BO.txt`**; không tự đổi nếu muốn các tài
khoản đã tạo tiếp tục dùng được. Mỗi mật khẩu tối thiểu 8 ký tự, có chữ hoa,
chữ thường, số và ký tự đặc biệt. Điền kết nối SQL phù hợp cho máy nhận.

Gửi **chỉ file riêng đã điền** qua tin nhắn riêng/chia sẻ giới hạn thành viên
hoặc password manager của nhóm. Nếu dùng file nén mã hóa, gửi mật khẩu mở file
qua kênh khác. Người nhận đặt file cạnh `.csproj`, sửa phần kết nối nếu cần và
chạy `dotnet run`. Không đưa file chứa bí mật vào ZIP source, repository, PR,
issue, thư mục public hoặc `wwwroot`. File `.txt` cũ chỉ là tài liệu tham khảo;
app **không đọc** file `.txt`.

Kiểm tra trước khi commit (các lệnh không in nội dung bí mật):

```sh
git check-ignore appsettings.Development.local.json
git ls-files -- appsettings.Development.local.json
git status --short
```

Lệnh đầu phải in tên file; lệnh thứ hai phải **không có kết quả**. Không dùng
`git add -f` cho file này. Git ignore không bảo vệ file đã từng được track;
nếu đã lỡ push bí mật, thu hồi/đổi bí mật trước khi xử lý lịch sử.

File local và bản sao `appsettings.*.local*.json` được ignore và loại khỏi
build/publish. Chỉ template `.example.json` được commit. Production không nạp
file nội bộ, không chạy auto migration hoặc demo seeder. Quy trình triển khai
phải cấp cấu hình production riêng và tự áp dụng migrations có kiểm soát.

Thứ tự ưu tiên: appsettings / User Secrets → file local → biến môi trường →
tham số dòng lệnh. Nếu bị trỏ nhầm DB, kiểm tra biến `ConnectionStrings__DefaultConnection`.
File thiếu vẫn cho phép cách cấu hình User Secrets/biến môi trường cũ hoạt động.
File có JSON sai sẽ dừng startup mà không in nội dung bí mật.

## 6. Checklist clone sạch trên từng hệ điều hành

- Chọn database mới, ví dụ `ServiceHubDB_PR27_Clean`, không trỏ vào production.
- Clone đúng nhánh, đặt file nội bộ, build và `dotnet run`.
- Kiểm tra 7 tài khoản (4/1/2), 4 cơ sở, 16 dịch vụ; đăng nhập thử từng vai trò.
- Dừng và chạy lại; số bản ghi không tăng. Tạo lịch thử trước khi chạy lại và
  xác nhận lịch vẫn còn.
- Customer đặt lịch → chi tiết hiển thị địa chỉ/liên hệ cơ sở.
- Owner sửa cơ sở → xem lịch và thông tin khách; kiểm tra không truy cập lịch của
  Owner khác. Kiểm tra trùng lịch và giờ không hợp lệ.
- Ghi rõ hệ điều hành, commit, SQL engine và kết quả; **không kèm file bí mật**.
- Mac và Windows dùng database riêng chỉ có cùng dữ liệu ban đầu; thay đổi sau
  đó không tự đồng bộ. Muốn đồng bộ cần SQL Server dùng chung do nhóm quản lý.

## 7. Build / test dành cho người phát triển

```sh
dotnet build ServiceHubMVC.csproj
dotnet test tests/ServiceHubMVC.Tests/ServiceHubMVC.Tests.csproj
dotnet test tests/ServiceHubMVC.BookingManagement.Tests/ServiceHubMVC.BookingManagement.Tests.csproj
```

Các test tích hợp cần SQL Server **riêng cho kiểm thử**, không dùng DB thật:

- `SERVICEHUB_BOOTSTRAP_TEST_CONNECTION`: kết nối tới instance test, quyền tạo/xóa
  DB. Test tạo tên `ServiceHubBootstrapTest_<GUID>` rồi chỉ xóa DB vừa tạo.
  Kiểm tra migration từ DB trắng, 7 login/roles, 4/16 dữ liệu, chạy lại/đồng thời,
  rollback khi xung đột, giữ thông tin cơ sở và booking. Không có biến này thì
  test SQL bootstrap báo **Skipped**, không được coi là đã test database.
- `SERVICEHUB_TEST_CONNECTION`: trỏ vào `ServiceHubDB_Test`, đã chạy Development
  bootstrap với file nội bộ dành cho test. Test booking cũ vẫn hỗ trợ
  `SERVICEHUB_TEST_SQL_PASSWORD` cho `localhost,1433` như trước.
- `SERVICEHUB_BOOKING_TEST_CONNECTION`: trỏ vào `ServiceHubMVC_BookingTest`, đã
  áp dụng migrations (có thể khởi động Development với cấu hình test).

Đặt biến qua cấu hình riêng của công cụ chạy test, không commit giá trị. Test
booking hiện hữu sẽ báo lỗi nếu thiếu cấu hình, thay vì báo thành công giả.

## 8. Sự cố thường gặp

- Thiếu `DefaultConnection`: Mac chưa điền kết nối trong file hoặc file đặt sai chỗ.
- Cannot connect: SQL Server chưa chạy, sai cổng/tài khoản hoặc thiếu quyền tạo DB.
- Thiếu mật khẩu / placeholder: trưởng nhóm chưa thay các giá trị `<...>`.
- Mật khẩu không khớp: DB đã seed bằng file khác; startup không reset mật khẩu.
- Sai role: email đang thuộc tài khoản khác; dùng email demo riêng hoặc DB dev mới.
- `Invalid column name 'PhoneNumber'`: kiểm tra đúng branch, môi trường Development
  và đúng database; không sửa bảng bằng tay hay xóa dữ liệu để chữa lỗi.
- Bootstrap busy: một tiến trình khác đang seed; đợi tiến trình đó xong rồi chạy lại.

Tài liệu tham khảo: [thứ tự cấu hình ASP.NET Core](https://learn.microsoft.com/aspnet/core/fundamentals/configuration/?view=aspnetcore-10.0),
[khóa transaction SQL Server](https://learn.microsoft.com/sql/relational-databases/system-stored-procedures/sp-getapplock-transact-sql).
