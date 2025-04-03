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
                // Validate the booking date (must not be more than 2 weeks in the future)
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

                // Check for overlapping bookings (same room, date, and time slot)
                var hasOverlap = await _unitOfWork.Bookings.HasOverlappingBookingsAsync(
                    booking.RoomId,
                    DateTime.Now, // The date parameter is not used directly in the method, so we can pass any DateTime
                    booking.TimeSlotId);

                if (hasOverlap)
                {
                    throw new InvalidOperationException("This room is already booked for the selected date and time slot.");
                }

                // If the booking is for today, check if the time slot has already passed
                if (booking.BookingDate == today)
                {
                    // Fetch the TimeSlot to get its EndTime
                    var timeSlot = await _unitOfWork.TimeSlots.GetByIdAsync(booking.TimeSlotId);
                    if (timeSlot == null)
                    {
                        throw new InvalidOperationException("Invalid time slot selected.");
                    }

                    // Get the current time as TimeOnly
                    var currentTime = TimeOnly.FromDateTime(DateTime.Now);

                    // If the time slot's EndTime is earlier than the current time, it's in the past
                    if (timeSlot.EndTime <= currentTime)
                    {
                        throw new InvalidOperationException("Cannot book a time slot that has already passed.");
                    }
                }

                // Add the booking to the database
                await _unitOfWork.Bookings.AddAsync(booking);
                await _unitOfWork.CompleteAsync();
                return booking;
            }
            catch (Exception)
            {
                // Optionally log the exception here (e.g., using ILogger)
                throw;
            }
        }

        public async Task UpdateAsync(Booking booking)
        {
            try
            {
                if(booking.Status != 1)
                {
                    throw new InvalidOperationException("chỉ được cập nhật khi chờ phê duyệt");
                }

                // Validate the booking date (must not be more than 2 weeks in the future)
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

                // Check for overlapping bookings (same room, date, and time slot)
                var hasOverlap = await _unitOfWork.Bookings.HasOverlappingBookingsAsync(
                    booking.RoomId,
                    DateTime.Now, // The date parameter is not used directly in the method, so we can pass any DateTime
                    booking.TimeSlotId);

                if (hasOverlap)
                {
                    throw new InvalidOperationException("This room is already booked for the selected date and time slot.");
                }

                // If the booking is for today, check if the time slot has already passed
                if (booking.BookingDate == today)
                {
                    // Fetch the TimeSlot to get its EndTime
                    var timeSlot = await _unitOfWork.TimeSlots.GetByIdAsync(booking.TimeSlotId);
                    if (timeSlot == null)
                    {
                        throw new InvalidOperationException("Invalid time slot selected.");
                    }

                    // Get the current time as TimeOnly
                    var currentTime = TimeOnly.FromDateTime(DateTime.Now);

                    // If the time slot's EndTime is earlier than the current time, it's in the past
                    if (timeSlot.EndTime <= currentTime)
                    {
                        throw new InvalidOperationException("Cannot book a time slot that has already passed.");
                    }
                }

                // Add the booking to the database
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
                // Get the booking by ID
                var booking = await _unitOfWork.Bookings.GetByIdAsync(id);

                if (booking == null)
                {
                    throw new InvalidOperationException("Booking not found.");
                }

                // Only allow cancellation of pending bookings (status = 1)
                if (booking.Status != 1)
                {
                    throw new InvalidOperationException("Only pending bookings can be cancelled.");
                }

                // Update status to 4 (cancelled) instead of deleting
                booking.Status = 4;
                await _unitOfWork.Bookings.UpdateAsync(booking);
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
