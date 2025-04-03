using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using BookingManagement.Repositories.Data;
using BookingManagement.Repositories.Models;
using BookingManagement.Services.Interfaces;
using BookingManagement.Services.DTOs;
using Microsoft.AspNetCore.Authorization;
using BookingManagement.Services.Services;

namespace BookingManagement.User.Razor.Pages.BookingRoom
{
    [Authorize]
    public class DeleteModel : PageModel
    {
        private readonly IBookingService _bookingService;
        private readonly ITimeSlotService _timeSlotService;
        private readonly IRoomService _roomService;
        private readonly ILogger<DeleteModel> _logger;

        public DeleteModel(IBookingService bookingService, ITimeSlotService timeSlotService, ILogger<DeleteModel> logger, IRoomService roomService)
        {
            _bookingService = bookingService;
            _timeSlotService = timeSlotService;
            _logger = logger;
            _roomService = roomService;
        }

        [BindProperty]
        public Booking Booking { get; set; } = default!;
        public TimeSlotDto TimeSlotDto { get; set; } = default!;


        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _bookingService.GetByIdAsync(id ?? default);
            var timeSlot = await _timeSlotService.GetActiveTimeSlotByIdAsync(booking.TimeSlotId);
            if (booking == null && timeSlot == null)
            {
                return NotFound();
            }
            else
            {
                Booking = booking;
                TimeSlotDto = timeSlot;
            }

            if (booking.Status != 1)
            {
                _logger.LogWarning("chỉ được cập nhậy khi trong trạng thái chờ xử lý");
                return RedirectToPage("/BookingRoom/Index");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }


            try
            {
                await _bookingService.DeleteAsync(id ?? default);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                throw;
            }

            return RedirectToPage("./Index");
        }

        public string GetStatusText(int status)
        {
            return _roomService.GetStatusText(status);
        }
    }
}