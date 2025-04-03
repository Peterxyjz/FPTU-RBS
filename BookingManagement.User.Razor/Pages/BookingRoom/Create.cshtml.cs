using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using BookingManagement.Repositories.Models;
using System.Security.Claims;
using BookingManagement.Services.Interfaces;
using System.Data;
using Microsoft.AspNetCore.Authorization;

namespace BookingManagement.User.Razor.Pages.BookingRoom
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly IBookingService _bookingService;
        private readonly ITimeSlotService _timeSlotService;
        private readonly ILogger<CreateModel> _logger;

        public CreateModel(IBookingService bookingService, ITimeSlotService timeSlotService, ILogger<CreateModel> logger)
        {
            _bookingService = bookingService;
            _timeSlotService = timeSlotService;
            _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync(int? id)
        {

            if (id == null)
            {
                // Handle the case where RoomId is not
                _logger.LogWarning("không tìm thất id yah");
                return RedirectToPage("/RoomList/Index");
            }

            // Initialize the Booking model if not already initialized
            if (Booking == null)
            {
                Booking = new Booking();
            }

            // Get the logged-in user's UserId from the claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int loggedInUserId))
            {
                if (!await _bookingService.CheckUserBookingLimitAsync(loggedInUserId))
                {
                    _logger.LogWarning("chỉ giới hạn được đặt 3 phòng hoi nghe chưa!");
                    return RedirectToPage("/RoomList/Index");
                }

                Booking.UserId = loggedInUserId;
                UserName = User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown User";
            }
            else
            {
                // Handle the case where the user is not logged in
                _logger.LogWarning("User chưa loggin");
                return RedirectToPage("/Login/Index");
            }


            // Set the RoomId and BookingDate
            Booking.RoomId = id.Value;
            Booking.BookingDate = DateOnly.FromDateTime(DateTime.Now);


            // Populate the TimeSlotId dropdown
            var list = await _timeSlotService.GetActiveTimeSlotsAsync();
            ViewData["TimeSlotId"] = new SelectList(
                list,
                "TimeSlotId",
                "DisplayText"
            );

            return Page();
        }

        public string UserName { get; set; } = "";

        [BindProperty]
        public Booking Booking { get; set; } = default!;

        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                await _bookingService.AddAsync(Booking);

                _logger.LogInformation($"booking thành công!!!");
                TempData["SuccessMessage"] = "Đặt phòng thành công! Bạn có một thông báo mới.";
                return RedirectToPage("/RoomList/Index");
            }
            catch (Exception ex)
            {
                // Log the exception (you can use a logging framework like Serilog or ILogger)
                ModelState.AddModelError(string.Empty, ex.Message);

                // Repopulate the dropdown and UserName for the view
                var list = await _timeSlotService.GetActiveTimeSlotsAsync();
                ViewData["TimeSlotId"] = new SelectList(
                    list,
                    "TimeSlotId",
                    "DisplayText"
                );

                // Repopulate UserName for the view
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int loggedInUserId))
                {
                    Booking.UserId = loggedInUserId; // Ensure UserId is set
                    UserName = User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown User";
                }

                return Page();
            }
        }
    }
}
