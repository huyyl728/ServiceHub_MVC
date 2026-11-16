using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ServiceHubMVC.Models;

public class ApplicationUser : IdentityUser
{
    [Required]
    [StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    public ICollection<Business> Businesses { get; set; }
        = new List<Business>();

    public ICollection<Booking> Bookings { get; set; }
        = new List<Booking>();
}