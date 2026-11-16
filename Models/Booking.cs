using System.ComponentModel.DataAnnotations;

namespace ServiceHubMVC.Models;

public class Booking
{
    public int Id { get; set; }

    [Required]
    public string CustomerId { get; set; } = string.Empty;

    public ApplicationUser Customer { get; set; } = null!;

    public int ServiceId { get; set; }

    public Service Service { get; set; } = null!;

    public DateTimeOffset StartTime { get; set; }

    public DateTimeOffset EndTime { get; set; }

    public decimal PriceAtBooking { get; set; }

    public BookingStatus Status { get; set; }
        = BookingStatus.Pending;

    public DateTimeOffset CreatedAt { get; set; }
        = DateTimeOffset.UtcNow;
}