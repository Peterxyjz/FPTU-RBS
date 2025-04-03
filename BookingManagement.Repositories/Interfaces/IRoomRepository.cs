using BookingManagement.Repositories.Base;
using BookingManagement.Repositories.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookingManagement.Repositories.Interfaces
{
    public interface IRoomRepository : IGenericRepository<Room>
    {
        Task<IEnumerable<Room>> GetRoomsByBuildingAsync(string building);
        Task<IEnumerable<Room>> GetRoomsByTypeAsync(string roomType);
        Task<IEnumerable<Room>> GetRoomsByStatusAsync(int status);
        Task<IEnumerable<Room>> GetAvailableRoomsAsync(DateTime date, int timeSlotId);
        Task<bool> IsRoomAvailableAsync(int roomId, DateTime date, int timeSlotId);
        Task<(IEnumerable<Room> Rooms, int TotalItems)> GetFilteredRoomsAsync(
            string roomName, int? capacity, string roomType, int pageNumber, int pageSize);
        IQueryable<Room> GetQuery();
    }
}
