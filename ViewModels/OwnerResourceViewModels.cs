using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ServiceHubMVC.ViewModels;

public sealed class BusinessFormViewModel
{
    [Display(Name = "Tên cơ sở"), Required(ErrorMessage = "Vui lòng nhập tên cơ sở."), StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Địa chỉ"), Required(ErrorMessage = "Vui lòng nhập địa chỉ."), StringLength(255)]
    public string Address { get; set; } = string.Empty;

    [Display(Name = "Số điện thoại liên hệ")]
    [Required(ErrorMessage = "Vui lòng nhập số điện thoại cơ sở.")]
    [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
    [StringLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;

    [Display(Name = "Mô tả"), StringLength(1000)]
    public string? Description { get; set; }
}

public sealed class OwnerServiceFormViewModel
{
    [Display(Name = "Tên dịch vụ"), Required(ErrorMessage = "Vui lòng nhập tên dịch vụ."), StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Danh mục"), Required(ErrorMessage = "Vui lòng nhập danh mục."), StringLength(60)]
    public string Category { get; set; } = string.Empty;

    [Display(Name = "Mô tả"), StringLength(1000)]
    public string? Description { get; set; }

    [BindRequired, Display(Name = "Giá (VNĐ)"), Range(typeof(decimal), "0", "100000000", ErrorMessage = "Giá phải từ 0 đến 100.000.000 đồng.")]
    public decimal Price { get; set; }

    [BindRequired, Display(Name = "Thời lượng (phút)"), Range(1, 1440, ErrorMessage = "Thời lượng phải từ 1 đến 1440 phút.")]
    public int DurationMinutes { get; set; }
}
