using System.ComponentModel.DataAnnotations;

namespace ServiceHubMVC.ViewModels;

public class OwnerRequestFormViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập tên cơ sở.")]
    [StringLength(150)]
    [Display(Name = "Tên cơ sở")]
    public string BusinessName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập địa chỉ cơ sở.")]
    [StringLength(255)]
    [Display(Name = "Địa chỉ cơ sở")]
    public string BusinessAddress { get; set; } = string.Empty;

    [StringLength(1000)]
    [Display(Name = "Thông tin bổ sung")]
    public string? Reason { get; set; }
}