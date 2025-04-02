﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using BookingManagement.Repositories.Data;
using BookingManagement.Repositories.Models;

namespace BookingManagement.User.Razor.Pages.RoomPage
{
    public class IndexModel : PageModel
    {
        private readonly BookingManagement.Repositories.Data.FptuRoomBookingContext _context;

        public IndexModel(BookingManagement.Repositories.Data.FptuRoomBookingContext context)
        {
            _context = context;
        }

        public IList<Room> Room { get;set; } = default!;

        public async Task OnGetAsync()
        {
            Room = await _context.Rooms.ToListAsync();
        }
    }
}
