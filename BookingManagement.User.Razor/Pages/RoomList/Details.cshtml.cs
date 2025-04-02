using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using BookingManagement.Repositories.Data;
using BookingManagement.Repositories.Models;

namespace BookingManagement.User.Razor.Pages.RoomList
{
    public class DetailsModel : PageModel
    {
        private readonly BookingManagement.Repositories.Data.FptuRoomBookingContext _context;

        public DetailsModel(BookingManagement.Repositories.Data.FptuRoomBookingContext context)
        {
            _context = context;
        }

        public Room Room { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var room = await _context.Rooms.FirstOrDefaultAsync(m => m.RoomId == id);
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
            return status switch
            {
                1 => "Chờ duyệt",
                2 => "Đã duyệt",
                3 => "Từ chối",
                4 => "Đã hủy",
                _ => status.ToString()
            };
        }

        public string GetDirectImageUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
                return string.Empty;
            // Kiểm tra nếu là URL Google Search
            if (url.Contains("google.com/url"))
            {
                try
                {
                    Uri uri = new Uri(url);
                    var queryParams = System.Web.HttpUtility.ParseQueryString(uri.Query);
                    string directUrl = queryParams["url"];
                    if (!string.IsNullOrEmpty(directUrl))
                        return directUrl;
                }
                catch
                {
                }
            }
            return url;
        }
    }
}
