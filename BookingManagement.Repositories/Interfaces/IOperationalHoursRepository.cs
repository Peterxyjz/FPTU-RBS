using BookingManagement.Repositories.Base;
using BookingManagement.Repositories.Models.TimeManagement;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookingManagement.Repositories.Interfaces
{
    public interface IOperationalHoursRepository : IGenericRepository<OperationalHours>
    {
        Task<IEnumerable<OperationalHours>> GetOperationalHoursByRoomIdAsync(int roomId);
        Task<IEnumerable<OperationalHours>> GetOperationalHoursByBuildingIdAsync(int buildingId);
        Task<IEnumerable<OperationalHours>> GetActiveOperationalHoursAsync();
    }
}
