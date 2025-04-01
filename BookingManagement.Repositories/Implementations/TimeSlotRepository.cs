using BookingManagement.Repositories.Base;
using BookingManagement.Repositories.Data;
using BookingManagement.Repositories.Interfaces;
using BookingManagement.Repositories.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookingManagement.Repositories.Implementations
{
    public class TimeSlotRepository : GenericRepository<TimeSlot>, ITimeSlotRepository
    {
        public TimeSlotRepository(FptuRoomBookingContext context) : base(context)
        {
        }

        public async Task<IEnumerable<TimeSlot>> GetActiveTimeSlotsAsync()
        {
            return await _context.TimeSlots
                .Where(ts => ts.IsActive)
                .OrderBy(ts => ts.StartTime)
                .ToListAsync();
        }

        public override async Task<TimeSlot?> GetByIdAsync(int id)
        {
            return await _context.TimeSlots
                .FirstOrDefaultAsync(ts => ts.TimeSlotId == id);
        }

        public override async Task<IEnumerable<TimeSlot>> GetAllAsync()
        {
            return await _context.TimeSlots
                .OrderBy(ts => ts.StartTime)
                .ToListAsync();
        }
    }
}
