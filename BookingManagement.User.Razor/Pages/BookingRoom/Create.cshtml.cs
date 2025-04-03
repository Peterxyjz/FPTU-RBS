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
                _logger.LogWarning("không tìm thấy id yah");
                TempData["message"] = "không tìm thấy id yah!";
                return RedirectToPage("/RoomList/Index");
            }

            if (Booking == null)
            {
                Booking = new Booking();
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int loggedInUserId))
            {
                if (!await _bookingService.CheckUserBookingLimitAsync(loggedInUserId))
                {
                    _logger.LogWarning("chỉ giới hạn được đặt 3 phòng hoi nghe chưa!");
                    TempData["message"] = "chỉ giới hạn được đặt 3 phòng hoi nghe chưa!";
                    return RedirectToPage("/RoomList/Index");
                }

                Booking.UserId = loggedInUserId;
                UserName = User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown User";
            }
            else
            {
                _logger.LogWarning("User chưa loggin");
                TempData["message"] = "User chưa loggin!";
                return RedirectToPage("/Login/Index");
            }

            Booking.RoomId = id.Value;
            Booking.BookingDate = DateOnly.FromDateTime(DateTime.Now);

            // Lấy danh sách tất cả time slot
            var allTimeSlots = await _timeSlotService.GetActiveTimeSlotsAsync();

            // Lấy danh sách TimeSlotId đã được đặt cho phòng và ngày
            var bookedTimeSlotIds = await _bookingService.GetBookedTimeSlotIdsAsync(Booking.RoomId, Booking.BookingDate);

            // Lọc các time slot chưa được đặt
            var availableTimeSlots = allTimeSlots
                .Where(ts => !bookedTimeSlotIds.Contains(ts.TimeSlotId))
                .ToList();

            // Populate dropdown với các time slot chưa được đặt
            ViewData["TimeSlotId"] = new SelectList(
                availableTimeSlots,
                "TimeSlotId",
                "DisplayText"
            );

            return Page();
        }

        public async Task<IActionResult> OnGetAvailableTimeSlotsAsync(int roomId, string bookingDate)
        {
            // Chuyển đổi bookingDate từ string sang DateOnly
            if (!DateOnly.TryParse(bookingDate, out var parsedBookingDate))
            {
                return BadRequest("Invalid booking date format.");
            }

            // Lấy danh sách tất cả time slot
            var allTimeSlots = await _timeSlotService.GetActiveTimeSlotsAsync();

            // Lấy danh sách TimeSlotId đã được đặt
            var bookedTimeSlotIds = await _bookingService.GetBookedTimeSlotIdsAsync(roomId, parsedBookingDate);

            // Lọc các time slot chưa được đặt
            var availableTimeSlots = allTimeSlots
                .Where(ts => !bookedTimeSlotIds.Contains(ts.TimeSlotId))
                .Select(ts => new { ts.TimeSlotId, ts.DisplayText })
                .ToList();

            return new JsonResult(availableTimeSlots);
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
                TempData["SuccessMessage"] = "Đặt phòng thành công! Bạn có một thông báo mới.";
                return RedirectToPage("/RoomList/Index");

            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);

                var allTimeSlots = await _timeSlotService.GetActiveTimeSlotsAsync();

                var bookedTimeSlotIds = await _bookingService.GetBookedTimeSlotIdsAsync(Booking.RoomId, Booking.BookingDate);

                var availableTimeSlots = allTimeSlots
                    .Where(ts => !bookedTimeSlotIds.Contains(ts.TimeSlotId))
                    .ToList();

                ViewData["TimeSlotId"] = new SelectList(
                    availableTimeSlots,
                    "TimeSlotId",
                    "DisplayText"
                );

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int loggedInUserId))
                {
                    Booking.UserId = loggedInUserId;
                    UserName = User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown User";
                }

                return Page();
            }
        }
    }
}
