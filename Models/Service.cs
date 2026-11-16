using System.ComponentModel.DataAnnotations;

namespace ServiceHubMVC.Models;

public class Service
{
    public int Id { get; set; }

    [Required]
    [StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(60)]
    public string Category { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Range(0, 100000000)]
    public decimal Price { get; set; }

    [Range(1, 1440)]
    public int DurationMinutes { get; set; }

    public bool IsAvailable { get; set; } = true;

    public int BusinessId { get; set; }

    public Business Business { get; set; } = null!;

    public ICollection<Booking> Bookings { get; set; }
        = new List<Booking>();
}