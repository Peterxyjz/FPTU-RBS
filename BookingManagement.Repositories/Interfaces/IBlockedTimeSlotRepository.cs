using BookingManagement.Repositories.Base;
using BookingManagement.Repositories.Models.TimeManagement;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookingManagement.Repositories.Interfaces
{
    public interface IBlockedTimeSlotRepository : IGenericRepository<BlockedTimeSlot>
    {
        Task<IEnumerable<BlockedTimeSlot>> GetActiveBlockedTimeSlotsAsync();
        Task<IEnumerable<BlockedTimeSlot>> GetBlockedTimeSlotsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<BlockedTimeSlot>> GetBlockedTimeSlotsByRoomIdAsync(int roomId);
        Task<IEnumerable<BlockedTimeSlot>> GetBlockedTimeSlotsByTimeSlotIdAsync(int timeSlotId);
        Task<bool> IsTimeSlotBlockedAsync(int timeSlotId, int? roomId, DateTime date);
    }
}
