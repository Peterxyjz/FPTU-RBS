using BookingManagement.Services.DTOs;

namespace BookingManagement.Services.Interfaces
{
    public interface ITimeSlotService
    {
        Task<TimeSlotDto> GetActiveTimeSlotByIdAsync(int id);
        Task<IEnumerable<TimeSlotDto>> GetActiveTimeSlotsAsync();
    }
}