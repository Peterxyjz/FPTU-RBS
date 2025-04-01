using BookingManagement.Repositories.Base;
using BookingManagement.Repositories.Data;
using BookingManagement.Repositories.Interfaces;
using BookingManagement.Repositories.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookingManagement.Repositories.Implementations
{
    public class BookingRepository : GenericRepository<Booking>, IBookingRepository
    {
        public BookingRepository(FptuRoomBookingContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Booking>> GetBookingsByUserIdAsync(int userId)
        {
            return await _context.Bookings
                .Include(b => b.Room)
                .Include(b => b.TimeSlot)
                .Include(b => b.User)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.BookingDate)
                .ThenBy(b => b.TimeSlot.StartTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Booking>> GetBookingsByRoomIdAsync(int roomId)
        {
            return await _context.Bookings
                .Include(b => b.Room)
                .Include(b => b.TimeSlot)
                .Include(b => b.User)
                .Where(b => b.RoomId == roomId)
                .OrderByDescending(b => b.BookingDate)
                .ThenBy(b => b.TimeSlot.StartTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Booking>> GetBookingsByStatusAsync(int status)
        {
            return await _context.Bookings
                .Include(b => b.Room)
                .Include(b => b.TimeSlot)
                .Include(b => b.User)
                .Where(b => b.Status == status)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Booking>> GetBookingsByDateAsync(DateTime date)
        {
            var dateOnly = DateOnly.FromDateTime(date);
            return await _context.Bookings
                .Include(b => b.Room)
                .Include(b => b.TimeSlot)
                .Include(b => b.User)
                .Where(b => b.BookingDate == dateOnly)
                .OrderBy(b => b.TimeSlot.StartTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Booking>> GetBookingsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var startDateOnly = DateOnly.FromDateTime(startDate);
            var endDateOnly = DateOnly.FromDateTime(endDate);
            return await _context.Bookings
                .Include(b => b.Room)
                .Include(b => b.TimeSlot)
                .Include(b => b.User)
                .Where(b => b.BookingDate >= startDateOnly && b.BookingDate <= endDateOnly)
                .OrderBy(b => b.BookingDate)
                .ThenBy(b => b.TimeSlot.StartTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Booking>> GetPendingBookingsAsync()
        {
            return await _context.Bookings
                .Include(b => b.Room)
                .Include(b => b.TimeSlot)
                .Include(b => b.User)
                .Where(b => b.Status == 1) // Status = 1 for Pending
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> HasOverlappingBookingsAsync(int roomId, DateTime date, int timeSlotId)
        {
            var dateOnly = DateOnly.FromDateTime(date);
            return await _context.Bookings
                .AnyAsync(b => b.RoomId == roomId &&
                               b.BookingDate == dateOnly &&
                               b.TimeSlotId == timeSlotId &&
                               (b.Status == 1 || b.Status == 2)); // Pending or Approved
        }

        public override async Task<Booking?> GetByIdAsync(int id)
        {
            return await _context.Bookings
                .Include(b => b.Room)
                .Include(b => b.TimeSlot)
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.BookingId == id);
        }

        public override async Task<IEnumerable<Booking>> GetAllAsync()
        {
            return await _context.Bookings
                .Include(b => b.Room)
                .Include(b => b.TimeSlot)
                .Include(b => b.User)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();
        }
    }
}
