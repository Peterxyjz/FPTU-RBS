using BookingManagement.Services.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace BookingManagement.User.Razor.Pages.Account
{
    [Authorize]
    public class LogoutModel : PageModel
    {
        private readonly IAuthService _authService;
        private readonly ILogger<LogoutModel> _logger;

        public LogoutModel(IAuthService authService, ILogger<LogoutModel> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userName = User.FindFirstValue(ClaimTypes.Name);
            await _authService.SignOutAsync(HttpContext);
            _logger.LogInformation($"Người dùng {userName} đã đăng xuất");
            return RedirectToPage("/Account/Login");
        }
    }
}
