-- Chạy trong SSMS hoặc sqlcmd, sau khi Development bootstrap đã tạo dữ liệu demo.
-- Chỉ chạy trên database thử riêng. Không dùng tài khoản hay dữ liệu người thật.
SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @ExpectedDatabase sysname = N'ServiceHubDB_S3_04_Test';
IF DB_NAME() <> @ExpectedDatabase
    THROW 51014, N'Sai database thử: không tạo dữ liệu S3-04.', 1;

DECLARE @OwnerId nvarchar(450);
DECLARE @BusinessId int;

SELECT @OwnerId = [Id]
FROM dbo.AspNetUsers
WHERE [NormalizedEmail] = N'OWNER@SERVICEHUB.TEST';

IF @OwnerId IS NULL
    THROW 51014, N'Chưa có Owner demo; hãy khởi động ứng dụng Development trước.', 1;

SELECT @BusinessId = [Id]
FROM dbo.Businesses
WHERE [OwnerId] = @OwnerId AND [Name] = N'[DEMO] Sân thể thao' AND [IsActive] = 1;

IF @BusinessId IS NULL
    THROW 51014, N'Không thấy cơ sở demo đang hoạt động; không sửa dữ liệu hiện có.', 1;

BEGIN TRANSACTION;

DECLARE @Index int = 1;
WHILE @Index <= 13
BEGIN
    DECLARE @Name nvarchar(150) =
        N'[S3-04] Cầu lông mẫu ' + RIGHT(N'00' + CONVERT(nvarchar(2), @Index), 2);

    IF NOT EXISTS (
        SELECT 1 FROM dbo.Services WITH (UPDLOCK, HOLDLOCK)
        WHERE [BusinessId] = @BusinessId AND [Name] = @Name
    )
    BEGIN
        INSERT INTO dbo.Services
            ([BusinessId], [Name], [Category], [Description], [Price], [DurationMinutes], [IsAvailable])
        VALUES
            (@BusinessId, @Name, N'Thể thao',
             N'Dữ liệu giả để kiểm tra tìm kiếm, lọc và phân trang.',
             90000 + @Index * 1000, 30 + (@Index % 3) * 15, 1);
    END;

    SET @Index += 1;
END;

COMMIT TRANSACTION;

SELECT [Id], [Name], [Category], [Price], [DurationMinutes], [IsAvailable]
FROM dbo.Services
WHERE [BusinessId] = @BusinessId AND [Name] LIKE N'[[]S3-04] Cầu lông mẫu %'
ORDER BY [Name], [Id];

-- 13 dịch vụ cùng keyword/category: trang 1 có 12, trang 2 có 1.
-- Chạy lại script không tạo bản ghi trùng; không xóa dịch vụ hoặc lịch đã đặt.
