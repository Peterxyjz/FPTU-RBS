using BookingManagement.Repositories.Base;
using BookingManagement.Repositories.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookingManagement.Repositories.Interfaces
{
    public interface IBookingRepository : IGenericRepository<Booking>
    {
        Task<IEnumerable<Booking>> GetBookingsByUserIdAsync(int userId);
        Task<IEnumerable<Booking>> GetBookingsByRoomIdAsync(int roomId);
        Task<IEnumerable<Booking>> GetBookingsByStatusAsync(int status);
        Task<IEnumerable<Booking>> GetBookingsByDateAsync(DateTime date);
        Task<IEnumerable<Booking>> GetBookingsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<Booking>> GetPendingBookingsAsync();
        Task<bool> HasOverlappingBookingsAsync(int roomId, DateTime date, int timeSlotId);
    }
}
