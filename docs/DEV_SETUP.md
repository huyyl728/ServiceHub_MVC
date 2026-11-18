# Khởi động ServiceHub_MVC trên Mac và Windows

> Dành cho môi trường **Development nội bộ**. Không chạy Seeder mẫu trên production.

## Thành phần bắt buộc

- .NET SDK 10 và dotnet-ef tương ứng.
- SQL Server: **Windows** có thể dùng LocalDB; **Mac** dùng SQL Server qua Docker hoặc máy chủ SQL Server mà bạn có quyền truy cập.
- Git. Không cần sao chép file .mdf hoặc .bak lên GitHub.

## 1. Clone mã nguồn

```bash
git clone https://github.com/huyyl728/ServiceHub_MVC.git
cd ServiceHub_MVC
```

Trên Windows có LocalDB, khi không cấu hình chuỗi kết nối, Development mặc định dùng `(localdb)\\MSSQLLocalDB` với database `ServiceHubDB`.

Trên Mac, phải lưu chuỗi kết nối riêng bằng User Secrets (đổi máy chủ, cổng và mật khẩu đúng với SQL Server của máy):

```bash
read -rs 'sqlPassword?Mật khẩu SQL Server: '
echo
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=ServiceHubDB;User Id=sa;Password=${sqlPassword};TrustServerCertificate=True"
unset sqlPassword
```

Nếu Windows không có LocalDB hoặc dùng SQL Server khác, thiết lập `ConnectionStrings:DefaultConnection` qua `dotnet user-secrets set`.

## 2. Nhập tài khoản demo nội bộ một lần

Nhận file **TAI_KHOAN_SERVICEHUB_NOI_BO.txt** qua kênh riêng từ trưởng nhóm. **Không tải file chứa mật khẩu lên Git hoặc PR.** Các email cố định trong Seeder:

- Admin: `admin1@servicehub.test` đến `admin4@servicehub.test`
- Owner: `owner@servicehub.test`
- Customer: `customer1@servicehub.test`, `customer2@servicehub.test`

Sau khi có mật khẩu trong file nội bộ, chạy từ thư mục dự án:

```bash
dotnet user-secrets set "SeedAccounts:SharedDemo:Enabled" "true"
dotnet user-secrets set "SeedAccounts:SharedDemo:AdminPassword" "<mật khẩu Admin từ file nội bộ>"
dotnet user-secrets set "SeedAccounts:SharedDemo:OwnerPassword" "<mật khẩu Owner từ file nội bộ>"
dotnet user-secrets set "SeedAccounts:SharedDemo:CustomerPassword" "<mật khẩu Customer từ file nội bộ>"
```

Lệnh trên chạy tương tự trong PowerShell trên Windows. Thay phần trong ngoặc nhọn bằng mật khẩu được gửi riêng. Không đăng dòng lệnh có mật khẩu lên GitHub.

Seeder không thay mật khẩu hoặc vai trò của tài khoản đã tồn tại; nếu email demo bị trùng với tài khoản khác vai trò, ứng dụng báo lỗi rõ ràng để tránh nâng quyền trái phép.

## 3. Chạy website

```bash
dotnet restore
dotnet run
```

Trong **Development**, `Program.cs` tự gọi `Database.MigrateAsync()` trước khi seed role, tài khoản và 16 dịch vụ demo thuộc 4 cơ sở. Lần chạy sau không tạo trùng dữ liệu; lịch đặt thật không bị xóa.

Migration `AddBusinessContactPhone` cũng đã được đưa lên Git: cột `Businesses.PhoneNumber` sẽ được bổ sung tự động. Owner cần vào sửa cơ sở cũ để nhập số liên hệ.

Nếu đã đổi Model, **phải đưa migration mới lên Git** để đồng đội áp dụng tự động. Không sửa schema bằng tay trên từng máy.

## 4. Lưu ý dữ liệu và chia sẻ

- Các máy có **cùng bộ dữ liệu ban đầu**, nhưng các lịch đặt/sửa đổi tiếp theo **không tự đồng bộ**: mỗi máy là database riêng.
- Muốn cùng một dữ liệu realtime, cả nhóm phải kết nối vào một SQL Server tập trung; không dùng Git để đồng bộ database.
- User Secrets lưu trên máy cục bộ, không nằm trong repo. File mật khẩu chỉ lưu riêng, không push lên Git.
- Website chỉ nên chạy localhost trong quá trình dùng bộ tài khoản demo.
- Khi phát hành ra Internet, tắt `SeedAccounts:SharedDemo:Enabled`, thay tất cả mật khẩu demo, áp dụng migration qua quy trình triển khai có kiểm soát.

## 5. Sự cố thường gặp

- `Invalid column name 'PhoneNumber'`: ứng dụng đang trỏ đến DB chưa có migration thay đổi này. Kiểm tra đúng tên DB và migration; không xóa DB.
- `ConnectionStrings:DefaultConnection`: trên Mac chưa thiết lập User Secrets.
- `Cannot connect to SQL Server`: kiểm tra container SQL Server / LocalDB đang chạy và cổng đúng.
- Thiếu `SeedAccounts:SharedDemo:...Password`: nhập 3 mật khẩu từ file nội bộ.
