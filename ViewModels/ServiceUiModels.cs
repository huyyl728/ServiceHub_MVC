namespace ServiceHubMVC.ViewModels;

// Hợp đồng hiển thị đề xuất cho module truy vấn dịch vụ (#6).
public sealed class ServiceCardViewModel
{
    public int Id { get; init; }
    public string Name { get; init; } = "";
    public string Category { get; init; } = "";
    public string BusinessName { get; init; } = "";
    public string Address { get; init; } = "";
    public decimal Price { get; init; }
    public int DurationMinutes { get; init; }
}

public sealed class ServiceListViewModel
{
    public IReadOnlyList<ServiceCardViewModel> Items { get; init; } = [];
    public IReadOnlyList<string> Categories { get; init; } = [];
    public string? Keyword { get; init; }
    public string? Category { get; init; }
    public int Page { get; init; } = 1;
    public int TotalPages { get; init; }
}

public sealed class ServiceDetailViewModel
{
    public required ServiceCardViewModel Service { get; init; }
    public string? Description { get; init; }
    // Chuỗi giờ Việt Nam do khách nhập; máy chủ phải kiểm tra và chuyển đổi.
    public string? StartTimeLocal { get; init; }
}
