using BookingManagement.Services.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookingManagement.Services.Interfaces
{
    public interface IBlockedTimeSlotService
    {
        Task<IEnumerable<BlockedTimeSlotDto>> GetAllBlockedTimeSlotsAsync();
        Task<IEnumerable<BlockedTimeSlotDto>> GetActiveBlockedTimeSlotsAsync();
        Task<BlockedTimeSlotDto> GetBlockedTimeSlotByIdAsync(int id);
        Task<IEnumerable<BlockedTimeSlotDto>> GetBlockedTimeSlotsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<BlockedTimeSlotDto>> GetBlockedTimeSlotsByRoomIdAsync(int roomId);
        Task<IEnumerable<BlockedTimeSlotDto>> GetBlockedTimeSlotsByTimeSlotIdAsync(int timeSlotId);
        Task<BlockedTimeSlotDto> CreateBlockedTimeSlotAsync(BlockedTimeSlotDto blockedTimeSlotDto);
        Task<BlockedTimeSlotDto> UpdateBlockedTimeSlotAsync(BlockedTimeSlotDto blockedTimeSlotDto);
        Task<bool> DeleteBlockedTimeSlotAsync(int id);
        Task<bool> IsTimeSlotBlockedAsync(int timeSlotId, int? roomId, DateTime date);
    }
}
