using BookingManagement.Services.DTOs;
using BookingManagement.Services.Interfaces;
using BookingManagement.Services.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookingManagement.User.Razor.Pages.Register
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
        public RegisterDto Input { get; set; } = new RegisterDto();

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

            // Kiểm tra xác nhận mật khẩu
            if (Input.Password != Input.ConfirmPassword)
            {
                ModelState.AddModelError("Input.ConfirmPassword", "Mật khẩu xác nhận không khớp.");
                return Page();
            }

            var result = await _authService.RegisterUserAsync(Input);

            if (result.Success)
            {
                _logger.LogInformation($"Người dùng {Input.Email} đăng ký thành công");
                
                // Đăng nhập người dùng sau khi đăng ký thành công
                await _authService.AuthenticateAsync(Input.Email, Input.Password, HttpContext);
                return LocalRedirect(returnUrl);
            }

            _logger.LogWarning($"Đăng ký thất bại cho người dùng {Input.Email}: {result.Message}");
            ModelState.AddModelError(string.Empty, result.Message);
            return Page();
        }
    }
}
