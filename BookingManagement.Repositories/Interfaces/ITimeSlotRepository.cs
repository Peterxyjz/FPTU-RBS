using BookingManagement.Repositories.Base;
using BookingManagement.Repositories.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookingManagement.Repositories.Interfaces
{
    public interface ITimeSlotRepository : IGenericRepository<TimeSlot>
    {
        Task<IEnumerable<TimeSlot>> GetActiveTimeSlotsAsync();
    }
}
