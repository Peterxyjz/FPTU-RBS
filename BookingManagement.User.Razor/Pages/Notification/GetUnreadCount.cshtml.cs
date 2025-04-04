using BookingManagement.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace BookingManagement.User.Razor.Pages.Notification
{
    [Authorize]
    public class GetUnreadCountModel : PageModel
    {
        private readonly INotificationService _notificationService;

        public GetUnreadCountModel(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return new JsonResult(0);
            }

            var count = await _notificationService.GetUnreadCountAsync(userId);
            return new JsonResult(count);
        }
    }
}
