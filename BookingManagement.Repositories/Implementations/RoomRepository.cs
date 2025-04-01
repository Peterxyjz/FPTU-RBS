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
    public class RoomRepository : GenericRepository<Room>, IRoomRepository
    {
        public RoomRepository(FptuRoomBookingContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Room>> GetRoomsByBuildingAsync(string building)
        {
            return await _context.Rooms
                .Where(r => r.Building == building && r.IsActive == true)
                .ToListAsync();
        }

        public async Task<IEnumerable<Room>> GetRoomsByTypeAsync(string roomType)
        {
            return await _context.Rooms
                .Where(r => r.RoomType == roomType && r.IsActive == true)
                .ToListAsync();
        }

        public async Task<IEnumerable<Room>> GetRoomsByStatusAsync(int status)
        {
            return await _context.Rooms
                .Where(r => r.Status == status && r.IsActive == true)
                .ToListAsync();
        }

        public async Task<IEnumerable<Room>> GetAvailableRoomsAsync(DateTime date, int timeSlotId)
        {
            // Convert DateTime to DateOnly for comparison
            var dateOnly = DateOnly.FromDateTime(date);
            
            // Lấy danh sách các phòng đã được đặt cho ngày và khung giờ cụ thể
            var bookedRoomIds = await _context.Bookings
                .Where(b => b.BookingDate == dateOnly && 
                           b.TimeSlotId == timeSlotId && 
                           (b.Status == 1 || b.Status == 2)) // Pending hoặc Approved
                .Select(b => b.RoomId)
                .ToListAsync();

            // Lấy các phòng không nằm trong danh sách đã đặt và đang hoạt động
            return await _context.Rooms
                .Where(r => !bookedRoomIds.Contains(r.RoomId) && 
                           r.IsActive == true && 
                           r.Status == 1) // Room đang hoạt động
                .ToListAsync();
        }

        public async Task<bool> IsRoomAvailableAsync(int roomId, DateTime date, int timeSlotId)
        {
            // Convert DateTime to DateOnly for comparison
            var dateOnly = DateOnly.FromDateTime(date);
            
            // Kiểm tra xem phòng có lịch đặt trùng khớp không
            var hasBooking = await _context.Bookings
                .AnyAsync(b => b.RoomId == roomId && 
                              b.BookingDate == dateOnly && 
                              b.TimeSlotId == timeSlotId && 
                              (b.Status == 1 || b.Status == 2)); // Pending hoặc Approved

            // Kiểm tra xem phòng có đang hoạt động không
            var room = await _context.Rooms
                .FirstOrDefaultAsync(r => r.RoomId == roomId);

            if (room == null || room.IsActive != true || room.Status != 1)
                return false;

            return !hasBooking;
        }

        public override async Task<Room?> GetByIdAsync(int id)
        {
            return await _context.Rooms
                .FirstOrDefaultAsync(r => r.RoomId == id);
        }
    }
}
