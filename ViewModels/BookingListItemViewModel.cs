
using ServiceHubMVC.Models;

namespace ServiceHubMVC.ViewModels;

public class BookingListItemViewModel
{
    public int Id { get; set; }

    public string ServiceName { get; set; } = string.Empty;

    public string BusinessName { get; set; } = string.Empty;

    public string BusinessAddress { get; set; } = string.Empty;

    public string? BusinessPhoneNumber { get; set; }

    public string? CustomerPhoneNumber { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public DateTimeOffset StartTime { get; set; }

    public DateTimeOffset EndTime { get; set; }

    public decimal PriceAtBooking { get; set; }

    public BookingStatus Status { get; set; }
}
