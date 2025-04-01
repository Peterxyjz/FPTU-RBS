using BookingManagement.Services.DTOs;
using BookingManagement.Services.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookingManagement.User.Razor.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly IAuthService _authService;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(IAuthService authService, ILogger<LoginModel> logger)
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

            var (success, role) = await _authService.AuthenticateAsync(Input.Email, Input.Password, HttpContext);

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
