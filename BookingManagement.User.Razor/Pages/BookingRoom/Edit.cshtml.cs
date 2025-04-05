using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BookingManagement.Repositories.Data;
using BookingManagement.Repositories.Models;
using BookingManagement.Services.Interfaces;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using BookingManagement.Services.Services;

namespace BookingManagement.User.Razor.Pages.BookingRoom
{
    [Authorize]
    public class EditModel : PageModel
    {
        private readonly IBookingService _bookingService;
        private readonly ITimeSlotService _timeSlotService;
        private readonly IRoomService _roomService;
        private readonly ILogger<EditModel> _logger;

        public EditModel(IBookingService bookingService, ITimeSlotService timeSlotService, ILogger<EditModel> logger, IRoomService roomService)
        {
            _bookingService = bookingService;
            _timeSlotService = timeSlotService;
            _logger = logger;
            _roomService = roomService;
        }

        public string UserName { get; set; }

        [BindProperty]
        public Booking Booking { get; set; } = default!;
        
        public Room RoomInfo { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("không tìm thấy id yah");
                TempData["message"] = "không tìm thấy id yah!";
                return RedirectToPage("/BookingRoom/Index");
            }

            var booking = await _bookingService.GetByIdAsync(id ?? default);

            if (booking == null)
            {
                _logger.LogWarning("không tìm thấy booking yah");
                TempData["message"] = "không tìm thấy booking yah!";
                return RedirectToPage("/BookingRoom/Index");
            }
            if (booking.Status != 1)
            {
                _logger.LogWarning("chỉ được cập nhật khi trong trạng thái chờ xử lý");
                TempData["message"] = "chỉ được cập nhật khi trong trạng thái chờ xử lý!";
                return RedirectToPage("/BookingRoom/Index");
            }

            Booking = booking;
            // Get room information for the sidebar
            RoomInfo = await _roomService.GetByIdAsync(booking.RoomId);

            var userNameClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrEmpty(userNameClaim))
            {
                UserName = User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown User";
            }
            else
            {
                _logger.LogWarning("User chưa loggin");
                TempData["message"] = "User chưa loggin!";
                return RedirectToPage("/Login/Index");
            }

            // Lấy danh sách tất cả time slot
            var allTimeSlots = await _timeSlotService.GetActiveTimeSlotsAsync();

            // Lấy danh sách TimeSlotId đã được đặt cho phòng và ngày (trừ booking hiện tại)
            var bookedTimeSlotIds = await _bookingService.GetBookedTimeSlotIdsAsync(Booking.RoomId, Booking.BookingDate);

            // Lọc các time slot chưa được đặt, nhưng giữ lại time slot của booking hiện tại
            var availableTimeSlots = allTimeSlots
                .Where(ts => !bookedTimeSlotIds.Contains(ts.TimeSlotId) || ts.TimeSlotId == Booking.TimeSlotId)
                .ToList();

            // Populate dropdown với các time slot chưa được đặt
            ViewData["TimeSlotId"] = new SelectList(
                availableTimeSlots,
                "TimeSlotId",
                "DisplayText",
                Booking.TimeSlotId // Đặt giá trị mặc định là TimeSlotId của booking hiện tại
            );

            return Page();
        }

        public async Task<IActionResult> OnGetAvailableTimeSlotsAsync(int roomId, string bookingDate, int bookingId)
        {
            _logger.LogInformation($"OnGetAvailableTimeSlotsAsync called with roomId: {roomId}, bookingDate: {bookingDate}, bookingId: {bookingId}");

            // Chuyển đổi bookingDate từ string sang DateOnly
            if (string.IsNullOrEmpty(bookingDate) || !DateOnly.TryParse(bookingDate, out var parsedBookingDate))
            {
                _logger.LogWarning($"Invalid booking date format: {bookingDate}");
                return BadRequest(new { error = "Invalid booking date format. Please select a valid date." });
            }

            // Lấy danh sách tất cả time slot
            var allTimeSlots = await _timeSlotService.GetActiveTimeSlotsAsync();

            // Lấy danh sách TimeSlotId đã được đặt
            var bookedTimeSlotIds = await _bookingService.GetBookedTimeSlotIdsAsync(roomId, parsedBookingDate);

            // Lấy thông tin booking hiện tại để giữ time slot của nó
            var currentBooking = await _bookingService.GetByIdAsync(bookingId);
            int? currentTimeSlotId = currentBooking?.TimeSlotId;

            // Lọc các time slot chưa được đặt, nhưng giữ lại time slot của booking hiện tại
            var availableTimeSlots = allTimeSlots
                .Where(ts => !bookedTimeSlotIds.Contains(ts.TimeSlotId) || ts.TimeSlotId == currentTimeSlotId)
                .Select(ts => new { ts.TimeSlotId, ts.DisplayText })
                .ToList();

            return new JsonResult(availableTimeSlots);
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                var existingBooking = await _bookingService.GetByIdAsync(Booking.BookingId);

                existingBooking.UserId = Booking.UserId;
                existingBooking.RoomId = Booking.RoomId;
                existingBooking.BookingDate = Booking.BookingDate;
                existingBooking.TimeSlotId = Booking.TimeSlotId;

                await _bookingService.UpdateAsync(existingBooking);

                _logger.LogInformation($"edit booking thành công!!!");
                TempData["messageSC"] = "edit booking thành công!!!";
                return RedirectToPage("/BookingRoom/Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);

                // Lấy danh sách tất cả time slot
                var allTimeSlots = await _timeSlotService.GetActiveTimeSlotsAsync();

                // Lấy danh sách TimeSlotId đã được đặt cho phòng và ngày
                var bookedTimeSlotIds = await _bookingService.GetBookedTimeSlotIdsAsync(Booking.RoomId, Booking.BookingDate);

                // Lọc các time slot chưa được đặt, nhưng giữ lại time slot của booking hiện tại
                var availableTimeSlots = allTimeSlots
                    .Where(ts => !bookedTimeSlotIds.Contains(ts.TimeSlotId) || ts.TimeSlotId == Booking.TimeSlotId)
                    .ToList();

                // Populate dropdown với các time slot chưa được đặt
                ViewData["TimeSlotId"] = new SelectList(
                    availableTimeSlots,
                    "TimeSlotId",
                    "DisplayText",
                    Booking.TimeSlotId
                );

                // Repopulate UserName for the view
                var userNameClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userNameClaim))
                {
                    UserName = User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown User";
                }

                return Page();
            }
        }
        public string GetStatusText(int status)
        {
            return _roomService.GetStatusText(status);
        }
        
        public string GetStatusClass(int status)
        {
            return status switch
            {
                1 => "pending", // Pending
                2 => "approved", // Approved
                3 => "rejected", // Rejected
                4 => "completed", // Completed
                5 => "cancelled", // Cancelled
                _ => "pending"
            };
        }

    }
}
