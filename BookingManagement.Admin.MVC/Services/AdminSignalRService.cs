using BookingManagement.Admin.MVC.Hubs;
using BookingManagement.Repositories.Models;
using BookingManagement.Repositories.UnitOfWork;
using BookingManagement.Services.Interfaces;
using BookingManagement.Services.Shared;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace BookingManagement.Admin.MVC.Services
{
    public class AdminSignalRService : ISignalRService
    {
        private readonly IHubContext<BookingHub> _hubContext;
        private readonly IUnitOfWork _unitOfWork;

        public AdminSignalRService(IHubContext<BookingHub> hubContext, IUnitOfWork unitOfWork)
        {
            _hubContext = hubContext;
            _unitOfWork = unitOfWork;
        }

        public async Task SendNewBookingNotificationAsync(Booking booking)
        {
            if (booking == null)
            {
                throw new ArgumentNullException(nameof(booking));
            }

            Console.WriteLine($"SendNewBookingNotificationAsync called for booking ID: {booking.BookingId}");

            var room = await _unitOfWork.Rooms.GetByIdAsync(booking.RoomId);
            var timeSlot = await _unitOfWork.TimeSlots.GetByIdAsync(booking.TimeSlotId);
            var user = await _unitOfWork.Users.GetByIdAsync(booking.UserId);

            if (room == null || timeSlot == null || user == null)
            {
                throw new InvalidOperationException("Cannot find room, timeslot, or user information for booking.");
            }

            var message = $"Yêu cầu đặt phòng mới: {user.FullName} đã đặt phòng {room.RoomName} cho ngày {booking.BookingDate.ToString("dd/MM/yyyy")}, khung giờ {timeSlot.StartTime}-{timeSlot.EndTime}.";

            Console.WriteLine($"Sending real-time notification: {message}");

            await _hubContext.Clients.All.SendAsync("ReceiveNewBooking", message, booking.BookingId);
        }

        public async Task SendBookingStatusUpdateAsync(Booking booking, string message)
        {
            if (booking == null)
            {
                throw new ArgumentNullException(nameof(booking));
            }

            if (string.IsNullOrEmpty(message))
            {
                throw new ArgumentException("Message cannot be null or empty", nameof(message));
            }

            // Phương thức này sẽ gửi các sự kiện cụ thể dựa trên status của booking
            switch (booking.Status)
            {
                case BookingStatus.Approved: // Đã duyệt
                    await _hubContext.Clients.Group(booking.UserId.ToString())
                        .SendAsync("ReceiveBookingApproval", message, booking.BookingId);
                    break;
                case BookingStatus.Rejected: // Từ chối
                    await _hubContext.Clients.Group(booking.UserId.ToString())
                        .SendAsync("ReceiveBookingRejection", message, booking.BookingId);
                    break;
                case BookingStatus.Completed: // Đã hoàn thành
                    await _hubContext.Clients.Group(booking.UserId.ToString())
                        .SendAsync("ReceiveBookingCompletion", message, booking.BookingId);
                    break;
                default:
                    await _hubContext.Clients.Group(booking.UserId.ToString())
                        .SendAsync("ReceiveNotification", message);
                    break;
            }

            // Gửi thêm một thông báo chung để đảm bảo khả năng nhận
            await _hubContext.Clients.Group(booking.UserId.ToString())
                .SendAsync("ReceiveNotification", message);
            
            Console.WriteLine($"Sent notification to user {booking.UserId}: {message}");
        }
    }
}
