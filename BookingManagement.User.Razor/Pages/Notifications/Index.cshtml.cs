using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BookingManagement.Repositories.Models;
using BookingManagement.Services.Interfaces;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace BookingManagement.User.Razor.Pages.Notifications
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

        public IEnumerable<BookingManagement.Repositories.Models.Notification> Notifications { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                _logger.LogWarning("User chưa đăng nhập");
                return RedirectToPage("/Login/Index");
            }

            Notifications = await _notificationService.GetNotificationsByUserIdAsync(userId);
            return Page();
        }

        public async Task<JsonResult> OnGetUnreadCountAsync()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return new JsonResult(0);
            }

            var count = await _notificationService.GetUnreadCountAsync(userId);
            return new JsonResult(count);
        }

        public async Task<IActionResult> OnGetMarkAllAsReadAsync()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                _logger.LogWarning("User chưa đăng nhập");
                return RedirectToPage("/Login/Index");
            }

            var count = await _notificationService.MarkAllAsReadAsync(userId);
            TempData["SuccessMessage"] = $"Đã đánh dấu {count} thông báo là đã đọc";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnGetMarkAsReadAsync(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                _logger.LogWarning("User chưa đăng nhập");
                return RedirectToPage("/Login/Index");
            }

            await _notificationService.MarkAsReadAsync(id);
            TempData["SuccessMessage"] = "Đã đánh dấu thông báo là đã đọc";
            return RedirectToPage();
        }
    }
}