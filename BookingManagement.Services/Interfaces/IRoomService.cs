using BookingManagement.Repositories.Models;
using BookingManagement.Services.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingManagement.Services.Interfaces
{
    public interface IRoomService
    {
        Task<IEnumerable<Room>> GetAllRoomsAsync();
        Task<Room?> GetRoomByIdAsync(int id);
        Task<Room?> AddRoomAsync(Room room);
        Task UpdateRoomAsync(Room room);
        Task DeleteRoomAsync(int id);
        Task<IEnumerable<Room>> GetRoomsByBuildingAsync(string building);
        Task<IEnumerable<Room>> GetRoomsByTypeAsync(string roomType);
        Task<IEnumerable<Room>> GetRoomsByStatusAsync(int status);
        Task<IEnumerable<Room>> GetAvailableRoomsAsync(DateTime date, int timeSlotId);
        Task<bool> IsRoomAvailableAsync(int roomId, DateTime date, int timeSlotId);
        Task<IEnumerable<string>> GetRoomTypesAsync();
        Task<IEnumerable<string>> GetBuildingsAsync();
        Task<IEnumerable<Room>> GetArchivedRoomsAsync();
    
        Task<IEnumerable<Room>> GetAllAsync();
        Task<Room?> GetByIdAsync(int id);
        Task<(IEnumerable<Room> Rooms, int TotalPages, int CurrentPage)> GetRoomsWithFilterAsync(
            string roomName, int? capacity, string roomType, int pageNumber, int pageSize);
        List<string> GetRoomTypeList();
        string GetStatusText(int status);
        string GetStatusTextForRoomList(int status);
        string GetDirectImageUrl(string url);
    }
}
