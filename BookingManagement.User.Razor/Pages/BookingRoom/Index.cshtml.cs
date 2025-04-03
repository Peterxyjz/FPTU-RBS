using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using BookingManagement.Repositories.Data;
using BookingManagement.Repositories.Models;
using Microsoft.AspNetCore.Authorization;
using BookingManagement.Services.Interfaces;
using System.Security.Claims;

namespace BookingManagement.User.Razor.Pages.BookingRoom
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IRoomService _roomService;
        private readonly IBookingService _bookingService;

        public IndexModel(IBookingService bookingService, IRoomService roomService)
        {
            _bookingService = bookingService;
            _roomService = roomService;
        }

        public IList<Booking> Booking { get;set; } = default!;

        public async Task<IActionResult> OnGetAsync()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int loggedInUserId))
            {
                Booking = (IList<Booking>)await _bookingService.GetBookingsByUserIdAsync(loggedInUserId);
            }
            else
            {
                TempData["message"] = "User chưa loggin!";
                return RedirectToPage("/Login/Index");
            }
            return Page();    
        }

        public string GetStatusText(int status)
        {
            return _roomService.GetStatusText(status);
        }
    }
}
