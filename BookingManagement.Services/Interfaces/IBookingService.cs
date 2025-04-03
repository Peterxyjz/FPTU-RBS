using BookingManagement.Repositories.Models;

namespace BookingManagement.Services.Interfaces
{
    public interface IBookingService
    {
        Task<Booking?> AddAsync(Booking booking);
        Task<bool> CheckUserBookingLimitAsync(int userId);
        Task DeleteAsync(int id);
        Task<IEnumerable<Booking>> GetAllAsync();
        Task<List<int>> GetBookedTimeSlotIdsAsync(int roomId, DateOnly bookingDate);
        Task<IEnumerable<Booking>> GetBookingsByUserIdAsync(int userId);
        Task<Booking?> GetByIdAsync(int id);
        Task UpdateAsync(Booking booking);
    }
}