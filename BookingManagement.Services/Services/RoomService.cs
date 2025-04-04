       
// File: BookingManagement.Services/Services/RoomService.cs
using BookingManagement.Repositories.Interfaces;
using BookingManagement.Repositories.Models;
using BookingManagement.Repositories.UnitOfWork;
using BookingManagement.Services.DTOs;
using BookingManagement.Services.Extensions;
using BookingManagement.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingManagement.Services.Services
{
    public class RoomService : IRoomService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRoomRepository _roomRepository;

        public RoomService(IUnitOfWork unitOfWork, IRoomRepository roomRepository)
        {
            _unitOfWork = unitOfWork;
            _roomRepository = roomRepository;
        }

        public async Task<IEnumerable<RoomDto>> GetActiveRoomsAsync()
        {
            var rooms = await _roomRepository.GetRoomsByStatusAsync(1); // Status 1 is Active
            return rooms.Select(r => new RoomDto
            {
                RoomNumber = r.RoomId,
                RoomName = r.RoomName,
                Building = r.Building,
                Capacity = r.Capacity,
                RoomType = r.RoomType,
                Description = r.Description,
                Status = r.Status,
                ImageUrl = r.ImageUrl,
                IsActive = r.IsActive == true
            });
        }

        public async Task<Room?> AddRoomAsync(Room room)
        {
            room.CreatedAt = DateTime.Now;
            room.UpdatedAt = DateTime.Now;

            await _roomRepository.AddAsync(room);
            await _unitOfWork.CompleteAsync();
            return room;
        }

        public async Task DeleteRoomAsync(int id)
        {
            var room = await _roomRepository.GetByIdAsync(id);
            if (room != null)
            {
                room.IsActive = false;
                room.UpdatedAt = DateTime.Now;
                await _roomRepository.UpdateAsync(room);
                await _unitOfWork.CompleteAsync();
            }
        }

        public async Task<IEnumerable<Room>> GetAllRoomsAsync()
        {
            return await _roomRepository.GetAllAsync();
        }

        public async Task<IEnumerable<Room>> GetAvailableRoomsAsync(DateTime date, int timeSlotId)
        {
            return await _roomRepository.GetAvailableRoomsAsync(date, timeSlotId);
        }

        public async Task<IEnumerable<string>> GetBuildingsAsync()
        {
            var rooms = await _roomRepository.GetAllAsync();
            return rooms
                .Where(r => !string.IsNullOrEmpty(r.Building))
                .Select(r => r.Building)
                .Distinct()
                .OrderBy(b => b);
        }

        public async Task<Room?> GetRoomByIdAsync(int id)
        {
            return await _roomRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Room>> GetRoomsByBuildingAsync(string building)
        {
            return await _roomRepository.GetRoomsByBuildingAsync(building);
        }

        public async Task<IEnumerable<Room>> GetRoomsByStatusAsync(int status)
        {
            return await _roomRepository.GetRoomsByStatusAsync(status);
        }

        public async Task<IEnumerable<Room>> GetRoomsByTypeAsync(string roomType)
        {
            return await _roomRepository.GetRoomsByTypeAsync(roomType);
        }

        public async Task<IEnumerable<string>> GetRoomTypesAsync()
        {
            var rooms = await _roomRepository.GetAllAsync();
            return rooms
                .Select(r => r.RoomType)
                .Distinct()
                .OrderBy(t => t);
        }

        public async Task<bool> IsRoomAvailableAsync(int roomId, DateTime date, int timeSlotId)
        {
            return await _roomRepository.IsRoomAvailableAsync(roomId, date, timeSlotId);
        }

        public async Task UpdateRoomAsync(Room room)
        {
            room.UpdatedAt = DateTime.Now;
            await _roomRepository.UpdateAsync(room);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<IEnumerable<Room>> GetArchivedRoomsAsync()
        {
            return await _roomRepository.GetArchivedRoomsAsync();
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