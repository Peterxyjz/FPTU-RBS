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

namespace BookingManagement.User.Razor.Pages.BookingRoom
{
    [Authorize]
    public class EditModel : PageModel
    {
        private readonly IBookingService _bookingService;
        private readonly ITimeSlotService _timeSlotService;
        private readonly ILogger<EditModel> _logger;

        public EditModel(IBookingService bookingService, ITimeSlotService timeSlotService, ILogger<EditModel> logger)
        {
            _bookingService = bookingService;
            _timeSlotService = timeSlotService;
            _logger = logger;
        }

        public string UserName { get; set; }

        [BindProperty]
        public Booking Booking { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                // Handle the case where RoomId is not
                _logger.LogWarning("không tìm thất id yah");
                return RedirectToPage("/BookingRoom/Index");
            }

            var booking = await _bookingService.GetByIdAsync(id ?? default);

            if (booking == null)
            {
                _logger.LogWarning("không tìm thất booking yah");
                return RedirectToPage("/BookingRoom/Index");
            }
            if(booking.Status != 1)
            {
                _logger.LogWarning("chỉ được cập nhậy khi trong trạng thái chờ xử lý");
                return RedirectToPage("/BookingRoom/Index");
            }

            Booking = booking;

            var userNameClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrEmpty(userNameClaim))
            {
                UserName = User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown User";
            }
            else
            {
                _logger.LogWarning("User chưa loggin");
                return RedirectToPage("/Login/Index");
            }
            var list = await _timeSlotService.GetActiveTimeSlotsAsync();
            ViewData["TimeSlotId"] = new SelectList(
                list,
                "TimeSlotId",
                "DisplayText"
            );

            return Page();
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
                existingBooking.RejectReason = Booking.RejectReason;
                existingBooking.IsRecurring = Booking.IsRecurring;
                existingBooking.EndRecurringDate = Booking.EndRecurringDate;

                await _bookingService.UpdateAsync(existingBooking);

                _logger.LogInformation($"edit booking thành công!!!");
                return RedirectToPage("/BookingRoom/Index");
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
            return status switch
            {
                1 => "Chờ duyệt",
                2 => "Đã duyệt",
                3 => "Từ chối",
                4 => "Đã hủy",
                _ => status.ToString()
            };
        }

    }
}
