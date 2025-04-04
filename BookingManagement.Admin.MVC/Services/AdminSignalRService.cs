using BookingManagement.Admin.MVC.Hubs;
using BookingManagement.Repositories.Models;
using BookingManagement.Repositories.UnitOfWork;
using BookingManagement.Services.Interfaces;
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

            await _hubContext.Clients.Group(booking.UserId.ToString())
                .SendAsync("ReceiveBookingStatusUpdate", message, booking.BookingId);
        }
    }
}
