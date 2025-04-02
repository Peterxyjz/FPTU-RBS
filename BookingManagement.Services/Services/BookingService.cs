using BookingManagement.Repositories.Implementations;
using BookingManagement.Repositories.Interfaces;
using BookingManagement.Repositories.Models;
using BookingManagement.Repositories.UnitOfWork;
using BookingManagement.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingManagement.Services.Services
{
    public class BookingService : IBookingService
    {
        private readonly IUnitOfWork _unitOfWork;

        public BookingService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Booking>> GetAllAsync()
        {
            return await _unitOfWork.Bookings.GetAllAsync();
        }

        public async Task<Booking?> GetByIdAsync(int id)
        {
            return await _unitOfWork.Bookings.GetByIdAsync(id);
        }

        public async Task<Booking?> AddAsync(Booking booking)
        {
            try
            {
                var today = DateOnly.FromDateTime(DateTime.Now);
                var maxBookingDate = today.AddDays(14); // 2 weeks = 14 days

                if (booking.BookingDate > maxBookingDate)
                {
                    throw new InvalidOperationException("Bookings can only be made up to 2 weeks in advance.");
                }

                if (booking.BookingDate < today)
                {
                    throw new InvalidOperationException("Bookings cannot be made for past dates.");
                }

                await _unitOfWork.Bookings.AddAsync(booking);
                await _unitOfWork.CompleteAsync();
                return booking;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task UpdateAsync(Booking booking)
        {
            try
            {
                var today = DateOnly.FromDateTime(DateTime.Now);
                var maxBookingDate = today.AddDays(14); // 2 weeks = 14 days

                if (booking.BookingDate > maxBookingDate)
                {
                    throw new InvalidOperationException("Bookings can only be made up to 2 weeks in advance.");
                }

                if (booking.BookingDate < today)
                {
                    throw new InvalidOperationException("Bookings cannot be made for past dates.");
                }

                await _unitOfWork.Bookings.UpdateAsync(booking);
                await _unitOfWork.CompleteAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task DeleteAsync(int id)
        {
            try
            {
                await _unitOfWork.Bookings.DeleteAsync(id);
                await _unitOfWork.CompleteAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> CheckUserBookingLimitAsync(int userId)
        {
            const int maxActiveBookings = 3;

            // Count the number of active bookings for the user
            var activeBookingCount = await _unitOfWork.Bookings.GetBookingsByUserIdAsync(userId)
                .ContinueWith(task => task.Result.Count(b => (b.Status == 1 || b.Status == 2)));

            // Return true if the user has fewer than the maximum allowed active bookings
            return activeBookingCount <= maxActiveBookings;
        }


    }
}
