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

namespace BookingManagement.User.Razor.Pages.BookingRoom
{
    [Authorize]
    public class DetailsModel : PageModel
    {
        private readonly IBookingService _bookingService;
        private readonly ITimeSlotService _timeSlotService;

        public DetailsModel(IBookingService bookingService, ITimeSlotService timeSlotService)
        {
            _bookingService = bookingService;
            _timeSlotService = timeSlotService;
        }

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
            return Page();
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
