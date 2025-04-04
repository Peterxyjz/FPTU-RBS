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
    public class SpecialScheduleRepository : GenericRepository<SpecialSchedule>, ISpecialScheduleRepository
    {
        public SpecialScheduleRepository(FptuRoomBookingContext context) : base(context)
        {
        }

        public async Task<IEnumerable<SpecialSchedule>> GetActiveSchedulesAsync()
        {
            return await _context.SpecialSchedules
                .Where(s => s.IsActive)
                .OrderByDescending(s => s.StartDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<SpecialSchedule>> GetSchedulesByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.SpecialSchedules
                .Where(s => s.IsActive && 
                    ((s.StartDate <= endDate && s.EndDate >= startDate) || 
                     (s.StartDate >= startDate && s.StartDate <= endDate) ||
                     (s.EndDate >= startDate && s.EndDate <= endDate)))
                .OrderBy(s => s.StartDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<SpecialSchedule>> GetSchedulesByRoomIdAsync(int roomId)
        {
            return await _context.SpecialSchedules
                .Where(s => s.IsActive && s.RoomId == roomId)
                .OrderByDescending(s => s.StartDate)
                .ToListAsync();
        }

        public override async Task<SpecialSchedule?> GetByIdAsync(int id)
        {
            return await _context.SpecialSchedules
                .Include(s => s.Room)
                .FirstOrDefaultAsync(s => s.SpecialScheduleId == id);
        }

        public override async Task<IEnumerable<SpecialSchedule>> GetAllAsync()
        {
            return await _context.SpecialSchedules
                .Include(s => s.Room)
                .OrderByDescending(s => s.StartDate)
                .ToListAsync();
        }
    }
}
