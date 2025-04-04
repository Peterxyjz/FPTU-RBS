using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using BookingManagement.Repositories.Models;
using BookingManagement.Services.Interfaces;

namespace BookingManagement.User.Razor.Pages.RoomList
{
    public class IndexModel : PageModel
    {
        private readonly IRoomService _roomService;

        public IndexModel(IRoomService roomService)
        {
            _roomService = roomService;
        }

        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int pageSize { get; set; } = 3;
        public List<Room> Room { get; set; } = new List<Room>();

        [BindProperty(SupportsGet = true)]
        public string SearchRoomName { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? SearchCapacity { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SearchRoomType { get; set; }

        public SelectList RoomTypeList { get; set; }

        public async Task<IActionResult> OnGetAsync(int pageNumber = 1)
        {
            try
            {
                // Lấy danh sách các loại phòng từ Service theo đúng cách admin làm
                var existingRoomTypes = await _roomService.GetRoomTypesAsync();
                
                // Kết hợp với danh sách loại phòng mặc định
                var roomTypes = existingRoomTypes
                    .Union(BookingManagement.Services.Extensions.RoomExtensions.GetDefaultRoomTypes())
                    .Distinct()
                    .OrderBy(t => t)
                    .Select(rt => new SelectListItem { Text = rt, Value = rt });
                    
                RoomTypeList = new SelectList(roomTypes, "Value", "Text");

                var result = await _roomService.GetRoomsWithFilterAsync(
                    SearchRoomName,
                    SearchCapacity,
                    SearchRoomType,
                    pageNumber,
                    pageSize
                );

                Room = result.Rooms.ToList();
                TotalPages = result.TotalPages;
                CurrentPage = result.CurrentPage;
                return Page();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return Page();
            }
        }

        public string GetStatusTextForRoomList(int status)
        {
            return _roomService.GetStatusTextForRoomList(status);
        }

        public string GetDirectImageUrl(string url)
        {
            return _roomService.GetDirectImageUrl(url);
        }
    }
}