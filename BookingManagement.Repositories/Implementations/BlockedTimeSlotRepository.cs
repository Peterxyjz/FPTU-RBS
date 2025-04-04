using BookingManagement.Repositories.Base;
using BookingManagement.Repositories.Data;
using BookingManagement.Repositories.Interfaces;
using BookingManagement.Repositories.Models.TimeManagement;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookingManagement.Repositories.Implementations
{
    public class BlockedTimeSlotRepository : GenericRepository<BlockedTimeSlot>, IBlockedTimeSlotRepository
    {
        public BlockedTimeSlotRepository(FptuRoomBookingContext context) : base(context)
        {
        }

        public async Task<IEnumerable<BlockedTimeSlot>> GetActiveBlockedTimeSlotsAsync()
        {
            return await _context.BlockedTimeSlots
                .Where(b => b.IsActive)
                .OrderByDescending(b => b.StartDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<BlockedTimeSlot>> GetBlockedTimeSlotsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.BlockedTimeSlots
                .Where(b => b.IsActive && 
                    ((b.StartDate <= endDate && b.EndDate >= startDate) || 
                     (b.StartDate >= startDate && b.StartDate <= endDate) ||
                     (b.EndDate >= startDate && b.EndDate <= endDate)))
                .OrderBy(b => b.StartDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<BlockedTimeSlot>> GetBlockedTimeSlotsByRoomIdAsync(int roomId)
        {
            return await _context.BlockedTimeSlots
                .Where(b => b.IsActive && b.RoomId == roomId)
                .OrderByDescending(b => b.StartDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<BlockedTimeSlot>> GetBlockedTimeSlotsByTimeSlotIdAsync(int timeSlotId)
        {
            return await _context.BlockedTimeSlots
                .Where(b => b.IsActive && b.TimeSlotId == timeSlotId)
                .OrderByDescending(b => b.StartDate)
                .ToListAsync();
        }

        public async Task<bool> IsTimeSlotBlockedAsync(int timeSlotId, int? roomId, DateTime date)
        {
            // Check for global time slot blocks (no specific room)
            var globalBlock = await _context.BlockedTimeSlots
                .AnyAsync(b => b.IsActive && 
                          b.TimeSlotId == timeSlotId && 
                          b.RoomId == null && 
                          b.StartDate <= date && 
                          b.EndDate >= date);
            
            if (globalBlock)
                return true;
            
            // Check for room-specific blocks if roomId is provided
            if (roomId.HasValue)
            {
                var roomBlock = await _context.BlockedTimeSlots
                    .AnyAsync(b => b.IsActive && 
                              b.TimeSlotId == timeSlotId && 
                              b.RoomId == roomId && 
                              b.StartDate <= date && 
                              b.EndDate >= date);
                
                if (roomBlock)
                    return true;
            }
            
            return false;
        }

        public override async Task<BlockedTimeSlot?> GetByIdAsync(int id)
        {
            return await _context.BlockedTimeSlots
                .Include(b => b.Room)
                .Include(b => b.TimeSlot)
                .Include(b => b.CreatedBy)
                .FirstOrDefaultAsync(b => b.BlockedTimeSlotId == id);
        }

        public override async Task<IEnumerable<BlockedTimeSlot>> GetAllAsync()
        {
            return await _context.BlockedTimeSlots
                .Include(b => b.Room)
                .Include(b => b.TimeSlot)
                .Include(b => b.CreatedBy)
                .OrderByDescending(b => b.StartDate)
                .ToListAsync();
        }
    }
}
