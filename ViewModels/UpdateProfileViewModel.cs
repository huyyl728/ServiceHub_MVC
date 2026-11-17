using System.ComponentModel.DataAnnotations;

namespace ServiceHubMVC.ViewModels;

public class UpdateProfileViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập họ và tên.")]
    [StringLength(100, ErrorMessage = "Họ và tên tối đa 100 ký tự.")]
    [Display(Name = "Họ và tên")]
    public string FullName { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Số điện thoại không đúng định dạng.")]
    [StringLength(20, ErrorMessage = "Số điện thoại tối đa 20 ký tự.")]
    [Display(Name = "Số điện thoại")]
    public string? PhoneNumber { get; set; }
}
