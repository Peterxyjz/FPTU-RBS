using BookingManagement.Services.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookingManagement.Services.Interfaces
{
    public interface ISpecialScheduleService
    {
        Task<IEnumerable<SpecialScheduleDto>> GetAllSpecialSchedulesAsync();
        Task<IEnumerable<SpecialScheduleDto>> GetActiveSpecialSchedulesAsync();
        Task<SpecialScheduleDto> GetSpecialScheduleByIdAsync(int id);
        Task<IEnumerable<SpecialScheduleDto>> GetSpecialSchedulesByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<SpecialScheduleDto>> GetSpecialSchedulesByRoomIdAsync(int roomId);
        Task<SpecialScheduleDto> CreateSpecialScheduleAsync(SpecialScheduleDto specialScheduleDto);
        Task<SpecialScheduleDto> UpdateSpecialScheduleAsync(SpecialScheduleDto specialScheduleDto);
        Task<bool> DeleteSpecialScheduleAsync(int id);
        Task<SpecialScheduleDto> GetSpecialScheduleForDateAsync(int? roomId, DateTime date);
    }
}
