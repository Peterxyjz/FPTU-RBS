using BookingManagement.Services.DTOs;
using BookingManagement.Services.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookingManagement.Admin.MVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IAuthService authService, ILogger<AccountController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            // Nếu đã đăng nhập thì chuyển hướng
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginDto model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
                return View(model);

            var (success, role) = await _authService.AuthenticateAsync(model.Email, model.Password, HttpContext, model.RememberMe);

            if (success)
            {
                _logger.LogInformation($"Người dùng {model.Email} đăng nhập thành công với vai trò {role}");
                
                // Kiểm tra xem user có phải là Admin không
                if (role == "Admin")
                {
                    return RedirectToLocal(returnUrl ?? "/Home/Index");
                }
                else
                {
                    // Nếu không phải Admin, chuyển hướng đến trang AccessDenied
                    await _authService.SignOutAsync(HttpContext);
                    ModelState.AddModelError(string.Empty, "Bạn không có quyền truy cập vào trang quản trị.");
                    return View(model);
                }
            }

            _logger.LogWarning($"Đăng nhập thất bại cho người dùng {model.Email}");
            ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không đúng.");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var userName = User.FindFirstValue(ClaimTypes.Name);
            await _authService.SignOutAsync(HttpContext);
            _logger.LogInformation($"Người dùng {userName} đã đăng xuất");
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            else
                return RedirectToAction("Index", "Home");
        }
    }
}
