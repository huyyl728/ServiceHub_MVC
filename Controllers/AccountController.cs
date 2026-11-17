using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ServiceHubMVC.Models;
using ServiceHubMVC.ViewModels;

namespace ServiceHubMVC.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction(nameof(Profile));
        }

        return View(new RegisterViewModel());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction(nameof(Profile));
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var email = model.Email.Trim();
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = model.FullName.Trim()
        };

        // Quyền đăng ký công khai luôn là Customer, không nhận vai trò từ biểu mẫu.
        var createResult = await _userManager.CreateAsync(user, model.Password);
        if (!createResult.Succeeded)
        {
            foreach (var error in createResult.Errors)
            {
                ModelState.AddModelError(string.Empty, TranslateIdentityError(error.Code));
            }

            return View(model);
        }

        var roleResult = await _userManager.AddToRoleAsync(user, "Customer");
        if (!roleResult.Succeeded)
        {
            // Tránh lưu tài khoản chưa được gán vai trò nếu cấu hình phân quyền gặp lỗi.
            await _userManager.DeleteAsync(user);
            ModelState.AddModelError(string.Empty,
                "Chưa thể hoàn tất đăng ký. Vui lòng thử lại sau.");
            return View(model);
        }

        await _signInManager.SignInAsync(user, isPersistent: false);
        return RedirectToAction(nameof(Profile));
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction(nameof(Profile));
        }

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction(nameof(Profile));
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.FindByEmailAsync(model.Email.Trim());
        if (user is null)
        {
            ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không chính xác.");
            return View(model);
        }

        // Identity xác minh mật khẩu băm; khóa tạm thời khi thử sai nhiều lần.
        var result = await _signInManager.PasswordSignInAsync(
            user, model.Password, model.RememberMe, lockoutOnFailure: true);

        if (result.Succeeded)
        {
            // Chỉ chấp nhận đường dẫn nội bộ, ngăn chuyển hướng sang website khác.
            if (!string.IsNullOrWhiteSpace(model.ReturnUrl) &&
                Url.IsLocalUrl(model.ReturnUrl))
            {
                return LocalRedirect(model.ReturnUrl);
            }

            return RedirectToAction(nameof(Profile));
        }

        ModelState.AddModelError(string.Empty,
            result.IsLockedOut
                ? "Tài khoản tạm thời bị khóa do đăng nhập sai nhiều lần."
                : "Email hoặc mật khẩu không chính xác.");
        return View(model);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        // Đăng xuất bằng POST kèm mã chống giả mạo, không dùng liên kết GET.
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Profile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return Challenge();
        }

        ViewData["Email"] = user.Email;
        return View(new UpdateProfileViewModel
        {
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber
        });
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(UpdateProfileViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return Challenge();
        }

        ViewData["Email"] = user.Email;
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Chỉ lấy ID từ phiên đăng nhập; không nhận UserId/Role/Email từ biểu mẫu.
        user.FullName = model.FullName.Trim();
        user.PhoneNumber = string.IsNullOrWhiteSpace(model.PhoneNumber)
            ? null
            : model.PhoneNumber.Trim();

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty,
                "Không thể cập nhật hồ sơ. Vui lòng thử lại.");
            return View(model);
        }

        await _signInManager.RefreshSignInAsync(user);
        TempData["SuccessMessage"] = "Cập nhật hồ sơ thành công.";
        return RedirectToAction(nameof(Profile));
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult AccessDenied() => View();

    private static string TranslateIdentityError(string code) => code switch
    {
        "DuplicateEmail" or "DuplicateUserName" =>
            "Email này đã được sử dụng.",
        "PasswordTooShort" => "Mật khẩu quá ngắn (tối thiểu 8 ký tự).",
        "PasswordRequiresDigit" => "Mật khẩu cần có ít nhất một chữ số.",
        "PasswordRequiresLower" => "Mật khẩu cần có chữ cái thường.",
        "PasswordRequiresUpper" => "Mật khẩu cần có chữ cái in hoa.",
        "PasswordRequiresNonAlphanumeric" =>
            "Mật khẩu cần có ít nhất một ký tự đặc biệt.",
        _ => "Dữ liệu đăng ký chưa hợp lệ. Vui lòng kiểm tra và thử lại."
    };
}
