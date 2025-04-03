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

namespace BookingManagement.User.Razor.Pages.BookingRoom
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IRoomService _roomService;

        private readonly BookingManagement.Repositories.Data.FptuRoomBookingContext _context;

        public IndexModel(BookingManagement.Repositories.Data.FptuRoomBookingContext context, IRoomService roomService)
        {
            _context = context;
            _roomService = roomService;
        }

        public IList<Booking> Booking { get;set; } = default!;

        public async Task OnGetAsync()
        {
            Booking = await _context.Bookings
                .Include(b => b.Room)
                .Include(b => b.TimeSlot)
                .Include(b => b.User).ToListAsync();
        }

        public string GetStatusText(int status)
        {
            return _roomService.GetStatusText(status);
        }
    }
}
