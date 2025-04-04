using BookingManagement.Services.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookingManagement.Services.Interfaces
{
    public interface IOperationalHoursService
    {
        Task<IEnumerable<OperationalHoursDto>> GetAllOperationalHoursAsync();
        Task<IEnumerable<OperationalHoursDto>> GetActiveOperationalHoursAsync();
        Task<OperationalHoursDto> GetOperationalHoursByIdAsync(int id);
        Task<IEnumerable<OperationalHoursDto>> GetOperationalHoursByRoomIdAsync(int roomId);
        Task<IEnumerable<OperationalHoursDto>> GetOperationalHoursByBuildingIdAsync(int buildingId);
        Task<OperationalHoursDto> CreateOperationalHoursAsync(OperationalHoursDto operationalHoursDto);
        Task<OperationalHoursDto> UpdateOperationalHoursAsync(OperationalHoursDto operationalHoursDto);
        Task<bool> DeleteOperationalHoursAsync(int id);
        Task<bool> IsOperationalAsync(int roomId, DateTime date, TimeOnly time);
    }
}
