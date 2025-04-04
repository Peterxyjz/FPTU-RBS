using BookingManagement.Repositories.Implementations;
using BookingManagement.Repositories.Interfaces;
using BookingManagement.Repositories.Models;
using BookingManagement.Repositories.UnitOfWork;
using BookingManagement.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookingManagement.Services.Hubs;

namespace BookingManagement.Services.Services
{
    public class BookingService : IBookingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;

        public BookingService(IUnitOfWork unitOfWork, INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
        }

        public async Task<IEnumerable<Booking>> GetAllAsync()
        {
            return await _unitOfWork.Bookings.GetAllAsync();
        }

        public async Task<IEnumerable<Booking>> GetBookingsByUserIdAsync(int userId)
        {
            return await _unitOfWork.Bookings.GetBookingsByUserIdAsync(userId);
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
                    var timeslot = await _unitOfWork.TimeSlots.GetByIdAsync(booking.TimeSlotId);
                    if (timeslot == null)
                    {
                        throw new InvalidOperationException("Invalid time slot selected.");
                    }

                    // Get the current time as TimeOnly
                    var currentTime = TimeOnly.FromDateTime(DateTime.Now);
                    // If the time slot's EndTime is earlier than the current time, it's in the past
                    if (timeslot.EndTime <= currentTime)
                    {
                        throw new InvalidOperationException("Cannot book a time slot that has already passed.");
                    }
                }

                // Set default values for booking
                booking.CreatedAt = DateTime.Now;
                booking.UpdatedAt = DateTime.Now;
                booking.Status = 1; // Pending status according to DB
                booking.IsRecurring ??= false;

                // Add the booking to the database
                await _unitOfWork.Bookings.AddAsync(booking);
                await _unitOfWork.CompleteAsync();

                // Gửi thông báo real-time cho người dùng và admin
                await _notificationService.NotifyNewBookingAsync(booking);

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
                // Lấy booking hiện tại từ database để so sánh các thay đổi
                var existingBooking = await _unitOfWork.Bookings.GetByIdAsync(booking.BookingId);
                if (existingBooking == null)
                {
                    throw new InvalidOperationException("Booking không tồn tại.");
                }

                if (booking.Status != 1 && existingBooking.Status == 1)
                {
                    throw new InvalidOperationException("chỉ được cập nhật khi chờ phê duyệt");
                }

                // Lưu các giá trị cũ để kiểm tra thay đổi
                var oldStatus = existingBooking.Status;
                var oldBookingDate = existingBooking.BookingDate;
                var oldTimeSlotId = existingBooking.TimeSlotId;

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

                // Kiểm tra trùng lịch chỉ khi thay đổi ngày đặt hoặc khung giờ
                if (oldBookingDate != booking.BookingDate || oldTimeSlotId != booking.TimeSlotId)
                {
                    var hasOverlap = await _unitOfWork.Bookings.HasOverlappingBookingsAsync(
                        booking.RoomId,
                        DateTime.Now, // The date parameter is not used directly in the method, so we can pass any DateTime
                        booking.TimeSlotId);

                    if (hasOverlap)
                    {
                        throw new InvalidOperationException("This room is already booked for the selected date and time slot.");
                    }
                }

                // If the booking is for today, check if the time slot has already passed
                if (booking.BookingDate == today)
                {
                    // Fetch the TimeSlot to get its EndTime
                    var timeslot = await _unitOfWork.TimeSlots.GetByIdAsync(booking.TimeSlotId);
                    if (timeslot == null)
                    {
                        throw new InvalidOperationException("Invalid time slot selected.");
                    }

                    // Get the current time as TimeOnly
                    var currentTime = TimeOnly.FromDateTime(DateTime.Now);

                    // If the time slot's EndTime is earlier than the current time, it's in the past
                    if (timeslot.EndTime <= currentTime)
                    {
                        throw new InvalidOperationException("Cannot book a time slot that has already passed.");
                    }
                }

                // Cập nhật booking trong database
                booking.UpdatedAt = DateTime.Now;
                await _unitOfWork.Bookings.UpdateAsync(booking);

                // Get room and time slot information
                var room = await _unitOfWork.Rooms.GetByIdAsync(booking.RoomId);
                var timeSlot = await _unitOfWork.TimeSlots.GetByIdAsync(booking.TimeSlotId);
                
                string title;
                string message;
                
                // Determine notification content based on change type
                if (oldStatus != booking.Status)
                {
                    title = GetStatusChangeTitle(booking.Status);
                    message = GetStatusChangeMessage(booking.Status, room.RoomName, booking.BookingDate, timeSlot);
                    
                    // Notify of status change
                    await _notificationService.NotifyBookingStatusChangeAsync(booking, GetStatusText(booking.Status));
                }
                else if (oldBookingDate != booking.BookingDate || oldTimeSlotId != booking.TimeSlotId)
                {
                    title = "Đặt phòng đã được cập nhật";
                    message = $"Đặt phòng của bạn cho phòng {room.RoomName} đã được thay đổi thành ngày {booking.BookingDate.ToString("dd/MM/yyyy")}, khung giờ {timeSlot.StartTime}-{timeSlot.EndTime}.";
                    
                    // Send update notification
                    await _notificationService.SendNotificationToUserAsync(
                        booking.UserId,
                        title,
                        message,
                        booking.BookingId);
                }
                else
                {
                    // Always create notification for any update
                    title = "Đặt phòng đã được cập nhật";
                    message = $"Đặt phòng của bạn cho phòng {room.RoomName} đã được cập nhật thành công.";
                    
                    // Send generic update notification
                    await _notificationService.SendNotificationToUserAsync(
                        booking.UserId,
                        title,
                        message,
                        booking.BookingId);
                }

                // Commit tất cả các thay đổi trong một transaction
                await _unitOfWork.CompleteAsync();
            }
            catch (Exception ex)
            {
                // Log the exception for debugging
                Console.WriteLine($"Error updating booking: {ex.Message}");
                throw;
            }
        }

        private string GetStatusChangeTitle(int status)
        {
            switch (status)
            {
                case 1: return "Đặt phòng đang chờ xử lý";
                case 2: return "Đặt phòng đã được phê duyệt";
                case 3: return "Đặt phòng đã bị từ chối";
                case 4: return "Đặt phòng đã hoàn thành";
                case 5: return "Đặt phòng đã bị hủy";
                default: return "Cập nhật trạng thái đặt phòng";
            }
        }
        
        private string GetStatusText(int status)
        {
            switch (status)
            {
                case 1: return "Pending";
                case 2: return "Approved";
                case 3: return "Rejected";
                case 4: return "Completed";
                case 5: return "Cancelled";
                default: return "Unknown";
            }
        }

        private string GetStatusChangeMessage(int status, string roomName, DateOnly bookingDate, TimeSlot timeSlot)
        {
            switch (status)
            {
                case 1:
                    return $"Đặt phòng của bạn cho phòng {roomName} vào ngày {bookingDate.ToString("dd/MM/yyyy")}, khung giờ {timeSlot.StartTime}-{timeSlot.EndTime} đang được xử lý.";
                case 2:
                    return $"Đặt phòng của bạn cho phòng {roomName} vào ngày {bookingDate.ToString("dd/MM/yyyy")}, khung giờ {timeSlot.StartTime}-{timeSlot.EndTime} đã được phê duyệt.";
                case 3:
                    return $"Đặt phòng của bạn cho phòng {roomName} vào ngày {bookingDate.ToString("dd/MM/yyyy")}, khung giờ {timeSlot.StartTime}-{timeSlot.EndTime} đã bị từ chối.";
                case 4:
                    return $"Đặt phòng của bạn cho phòng {roomName} vào ngày {bookingDate.ToString("dd/MM/yyyy")}, khung giờ {timeSlot.StartTime}-{timeSlot.EndTime} đã hoàn thành.";
                case 5:
                    return $"Đặt phòng của bạn cho phòng {roomName} vào ngày {bookingDate.ToString("dd/MM/yyyy")}, khung giờ {timeSlot.StartTime}-{timeSlot.EndTime} đã bị hủy.";
                default:
                    return $"Trạng thái đặt phòng của bạn cho phòng {roomName} vào ngày {bookingDate.ToString("dd/MM/yyyy")}, khung giờ {timeSlot.StartTime}-{timeSlot.EndTime} đã được cập nhật.";
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

                // Update status to 5 (cancelled) instead of deleting
                booking.Status = 5;
                booking.UpdatedAt = DateTime.Now;
                await _unitOfWork.Bookings.UpdateAsync(booking);
                await _unitOfWork.CompleteAsync();

                // Notify cancellation using real-time notification
                await _notificationService.NotifyBookingStatusChangeAsync(booking, "Cancelled");
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
                .ContinueWith(task => task.Result.Count(b => (b.Status == 2 || b.Status == 4)));

            // Return true if the user has fewer than the maximum allowed active bookings
            return activeBookingCount < maxActiveBookings;
        }

        public async Task<List<int>> GetBookedTimeSlotIdsAsync(int roomId, DateOnly bookingDate)
        {
            // Lấy tất cả booking cho phòng và ngày đã chọn
            var bookings = await _unitOfWork.Bookings.GetAllAsync();

            // Lọc các booking có trạng thái 2 (Đã duyệt) hoặc 4 (Đã hoàn thành) cho phòng và ngày cụ thể
            var bookedTimeSlots = bookings
                .Where(b => b.RoomId == roomId &&
                            b.BookingDate == bookingDate &&
                            (b.Status == 2 || b.Status == 4))
                .Select(b => b.TimeSlotId)
                .Distinct()
                .ToList();

            return bookedTimeSlots;
        }
    }
}
