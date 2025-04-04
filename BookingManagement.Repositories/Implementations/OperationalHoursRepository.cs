using BookingManagement.Repositories.Base;
using BookingManagement.Repositories.Data;
using BookingManagement.Repositories.Interfaces;
using BookingManagement.Repositories.Models.TimeManagement;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookingManagement.Repositories.Implementations
{
    public class OperationalHoursRepository : GenericRepository<OperationalHours>, IOperationalHoursRepository
    {
        public OperationalHoursRepository(FptuRoomBookingContext context) : base(context)
        {
        }

        public async Task<IEnumerable<OperationalHours>> GetActiveOperationalHoursAsync()
        {
            return await _context.OperationalHours
                .Where(o => o.IsActive)
                .OrderBy(o => o.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<OperationalHours>> GetOperationalHoursByBuildingIdAsync(int buildingId)
        {
            return await _context.OperationalHours
                .Where(o => o.BuildingId == buildingId && o.IsActive)
                .OrderBy(o => o.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<OperationalHours>> GetOperationalHoursByRoomIdAsync(int roomId)
        {
            return await _context.OperationalHours
                .Where(o => o.RoomId == roomId && o.IsActive)
                .OrderBy(o => o.Name)
                .ToListAsync();
        }

        public override async Task<OperationalHours?> GetByIdAsync(int id)
        {
            return await _context.OperationalHours
                .Include(o => o.Room)
                .FirstOrDefaultAsync(o => o.OperationalHoursId == id);
        }

        public override async Task<IEnumerable<OperationalHours>> GetAllAsync()
        {
            return await _context.OperationalHours
                .Include(o => o.Room)
                .OrderBy(o => o.Name)
                .ToListAsync();
        }
    }
}
