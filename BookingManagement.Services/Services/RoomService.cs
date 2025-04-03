// File: BookingManagement.Services/Services/RoomService.cs
using BookingManagement.Repositories.Models;
using BookingManagement.Repositories.UnitOfWork;
using BookingManagement.Services.Extensions;
using BookingManagement.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookingManagement.Services.Services
{
    public class RoomService : IRoomService
    {
        private readonly IUnitOfWork _unitOfWork;

        public RoomService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Room>> GetAllAsync()
        {
            return await _unitOfWork.Rooms.GetAllAsync();
        }

        public async Task<Room?> GetByIdAsync(int id)
        {
            return await _unitOfWork.Rooms.GetByIdAsync(id);
        }

        public async Task<(IEnumerable<Room> Rooms, int TotalPages, int CurrentPage)> GetRoomsWithFilterAsync(
            string roomName, int? capacity, string roomType, int pageNumber, int pageSize)
        {
            try
            {
                // Lấy dữ liệu phòng với bộ lọc từ repository
                var result = await _unitOfWork.Rooms.GetFilteredRoomsAsync(
                    roomName, capacity, roomType, pageNumber, pageSize);

                // Tính tổng số trang
                int totalPages = (int)Math.Ceiling(result.TotalItems / (double)pageSize);

                // Xác định số trang hiện tại
                int currentPage = pageNumber > 0 && pageNumber <= totalPages ? pageNumber : 1;

                return (result.Rooms, totalPages, currentPage);
            }
            catch (Exception)
            {
                // Optionally log the exception here (e.g., using ILogger)
                throw;
            }
        }

        public List<string> GetRoomTypeList()
        {
            return RoomTypeExtension.RoomTypes.ToList();
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

        public string GetStatusTextForRoomList(int status)
        {
            return status switch
            {
                1 => "Hoạt động",
                2 => "Bảo trì",
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
                    // Just ignore errors and return the original URL
                }
            }
            return url;
        }
    }
}