using BookingManagement.Repositories.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookingManagement.Services.Interfaces
{
    public interface IRoomService
    {
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