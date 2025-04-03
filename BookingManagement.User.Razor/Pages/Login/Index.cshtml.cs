using BookingManagement.Services.DTOs;
using BookingManagement.Services.Interfaces;
using BookingManagement.Services.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookingManagement.User.Razor.Pages.Login
{
    public class IndexModel : PageModel
    {
        private readonly IAuthService _authService;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(IAuthService authService, ILogger<IndexModel> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [BindProperty]
        public LoginDto Input { get; set; } = new LoginDto();

        [BindProperty(SupportsGet = true)]
        public string ReturnUrl { get; set; } = null!;

        public void OnGet(string returnUrl = null)
        {
            // Nếu đã đăng nhập thì chuyển hướng
            if (User.Identity?.IsAuthenticated == true)
            {
                Response.Redirect("/");
                return;
            }

            ReturnUrl = returnUrl ?? Url.Content("~/");
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");

            if (!ModelState.IsValid)
                return Page();

            var (success, role) = await _authService.AuthenticateAsync(Input.Email, Input.Password, HttpContext, Input.RememberMe);

            if (success)
            {
                _logger.LogInformation($"Người dùng {Input.Email} đăng nhập thành công với vai trò {role}");
                return LocalRedirect(returnUrl);
            }

            _logger.LogWarning($"Đăng nhập thất bại cho người dùng {Input.Email}");
            ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không đúng.");
            return Page();
        }
    }
}
