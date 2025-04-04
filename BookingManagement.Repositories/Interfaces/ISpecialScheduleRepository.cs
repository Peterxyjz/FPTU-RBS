using BookingManagement.Repositories.Base;
using BookingManagement.Repositories.Models.TimeManagement;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookingManagement.Repositories.Interfaces
{
    public interface ISpecialScheduleRepository : IGenericRepository<SpecialSchedule>
    {
        Task<IEnumerable<SpecialSchedule>> GetActiveSchedulesAsync();
        Task<IEnumerable<SpecialSchedule>> GetSchedulesByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<SpecialSchedule>> GetSchedulesByRoomIdAsync(int roomId);
    }
}
