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

namespace BookingManagement.User.Razor.Pages.RoomList
{
    public class DetailsModel : PageModel
    {
        private readonly IRoomService _roomService;

        public DetailsModel(IRoomService roomService)
        {
            _roomService = roomService;
        }


        public Room Room { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var room = await _roomService.GetByIdAsync(id ?? default);
            if (room == null)
            {
                return NotFound();
            }
            else
            {
                Room = room;
            }
            return Page();
        }

        public string GetStatusText(int status)
        {
            return _roomService.GetStatusText(status);
        }

        public string GetDirectImageUrl(string url)
        {
            return _roomService.GetDirectImageUrl(url);
        }
    }
}
