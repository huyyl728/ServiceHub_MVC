namespace ServiceHubMVC.Services;

// Các loại kết quả khi xử lý yêu cầu đặt lịch
public enum BookingResultCode
{
    Success,
    Forbidden,
    ServiceNotFound,
    ServiceUnavailable,
    InvalidTime,
    TimeConflict,
    Busy
}

// Kết quả trả về từ nghiệp vụ đặt lịch
public record BookingResult(
    BookingResultCode Code,
    string Message,
    int? BookingId = null)
{
    public bool Success =>
        Code == BookingResultCode.Success;
}