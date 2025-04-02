using BookingManagement.Services.DTOs;

namespace BookingManagement.Services.Interfaces
{
    public interface ITimeSlotService
    {
        Task<IEnumerable<TimeSlotDto>> GetActiveTimeSlotsAsync();
    }
}