// Cập nhật file IndexModel.cs

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
using BookingManagement.Services.Extensions;

namespace BookingManagement.User.Razor.Pages.RoomList
{
    public class IndexModel : PageModel
    {
        private readonly BookingManagement.Repositories.Data.FptuRoomBookingContext _context;
        public IndexModel(BookingManagement.Repositories.Data.FptuRoomBookingContext context)
        {
            _context = context;
        }

        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int pageSize { get; set; } = 3;
        public IList<Room> Room { get; set; } = default!;

        [BindProperty(SupportsGet = true)]
        public string SearchRoomName { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? SearchCapacity { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SearchRoomType { get; set; }

        public SelectList RoomTypeList { get; set; }

        public async Task<IActionResult> OnGetAsync(int pageNumber = 1)
        {
            // Lấy danh sách RoomTypes để tạo dropdown
            var roomTypes = RoomTypeExtension.RoomTypes;
            RoomTypeList = new SelectList(roomTypes);

            // Lấy dữ liệu phòng và áp dụng bộ lọc tìm kiếm
            IQueryable<Room> roomsQuery = _context.Rooms;

            // Tìm kiếm theo RoomName
            if (!string.IsNullOrEmpty(SearchRoomName))
            {
                roomsQuery = roomsQuery.Where(r => r.RoomName.Contains(SearchRoomName));
            }

            // Tìm kiếm theo Capacity
            if (SearchCapacity.HasValue && SearchCapacity > 0)
            {
                roomsQuery = roomsQuery.Where(r => r.Capacity >= SearchCapacity);
            }

            // Tìm kiếm theo RoomType
            if (!string.IsNullOrEmpty(SearchRoomType))
            {
                roomsQuery = roomsQuery.Where(r => r.RoomType == SearchRoomType);
            }

            // Lấy tổng số phòng sau khi lọc để tính phân trang
            var totalRooms = await roomsQuery.CountAsync();
            TotalPages = (int)Math.Ceiling(totalRooms / (double)pageSize);
            CurrentPage = pageNumber > 0 && pageNumber <= TotalPages ? pageNumber : 1;

            // Lấy dữ liệu phòng cho trang hiện tại
            Room = await roomsQuery
                .Skip((CurrentPage - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

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