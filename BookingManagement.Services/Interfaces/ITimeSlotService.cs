using BookingManagement.Services.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookingManagement.Services.Interfaces
{
    public interface ITimeSlotService
    {
        Task<TimeSlotDto> GetTimeSlotByIdAsync(int id);
        Task<TimeSlotDto> GetActiveTimeSlotByIdAsync(int id);
        Task<IEnumerable<TimeSlotDto>> GetAllTimeSlotsAsync();
        Task<IEnumerable<TimeSlotDto>> GetActiveTimeSlotsAsync();
        Task<TimeSlotDto> CreateTimeSlotAsync(TimeSlotDto timeSlotDto);
        Task<TimeSlotDto> UpdateTimeSlotAsync(TimeSlotDto timeSlotDto);
        Task<bool> DeleteTimeSlotAsync(int id);
    }
}