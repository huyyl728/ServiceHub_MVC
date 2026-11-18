using System.ComponentModel.DataAnnotations;

namespace ServiceHubMVC.Models;

public class Business
{
    public int Id { get; set; }

    [Required]
    [StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string Address { get; set; } = string.Empty;

    [StringLength(20)]
    public string? PhoneNumber { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    [Required]
    public string OwnerId { get; set; } = string.Empty;

    public ApplicationUser Owner { get; set; } = null!;

    public ICollection<Service> Services { get; set; }
        = new List<Service>();
}