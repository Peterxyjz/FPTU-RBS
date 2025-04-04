using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BookingManagement.Repositories.Models;
using BookingManagement.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookingManagement.User.Razor.Pages.Notification
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(INotificationService notificationService, ILogger<IndexModel> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        public IEnumerable<BookingManagement.Repositories.Models.Notification> Notifications { get; set; } = new List<BookingManagement.Repositories.Models.Notification>();

        public async Task<IActionResult> OnGetAsync()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return RedirectToPage("/Login/Index");
            }

            Notifications = (await _notificationService.GetNotificationsByUserIdAsync(userId))
                .OrderByDescending(n => n.CreatedAt);
            
            return Page();
        }

        public async Task<IActionResult> OnPostMarkAsReadAsync(int id)
        {
            await _notificationService.MarkAsReadAsync(id);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostMarkAllAsReadAsync()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return RedirectToPage("/Login/Index");
            }

            await _notificationService.MarkAllAsReadAsync(userId);
            return RedirectToPage();
        }
    }
}
