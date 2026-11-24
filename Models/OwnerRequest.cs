using System.ComponentModel.DataAnnotations;

namespace ServiceHubMVC.Models;

public enum OwnerRequestStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2
}

public class OwnerRequest
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    public ApplicationUser User { get; set; } = null!;

    [Required]
    [StringLength(150)]
    public string BusinessName { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string BusinessAddress { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Reason { get; set; }

    public OwnerRequestStatus Status { get; set; }
        = OwnerRequestStatus.Pending;

    public DateTime CreatedAtUtc { get; set; }
        = DateTime.UtcNow;

    public DateTime? ReviewedAtUtc { get; set; }

    public string? ReviewedByUserId { get; set; }
}
