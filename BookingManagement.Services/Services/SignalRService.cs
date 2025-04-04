using BookingManagement.Repositories.Models;
using BookingManagement.Repositories.UnitOfWork;
using BookingManagement.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingManagement.Services.Services
{
    // Định nghĩa một hub trống cho đăng ký trong service
    public class DummyHub : Hub { }

    public class SignalRService : ISignalRService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHubContext<DummyHub> _hubContext;

        public SignalRService(IUnitOfWork unitOfWork, IHubContext<DummyHub> hubContext = null)
        {
            _unitOfWork = unitOfWork;
            _hubContext = hubContext;
        }

        public async Task SendNewBookingNotificationAsync(Booking booking)
        {
            if (booking == null)
            {
                throw new ArgumentNullException(nameof(booking));
            }

            var room = await _unitOfWork.Rooms.GetByIdAsync(booking.RoomId);
            var timeSlot = await _unitOfWork.TimeSlots.GetByIdAsync(booking.TimeSlotId);
            var user = await _unitOfWork.Users.GetByIdAsync(booking.UserId);

            if (room == null || timeSlot == null || user == null)
            {
                throw new InvalidOperationException("Cannot find room, timeslot, or user information for booking.");
            }

            var message = $"Yêu cầu đặt phòng mới: {user.FullName} đã đặt phòng {room.RoomName} cho ngày {booking.BookingDate.ToString("dd/MM/yyyy")}, khung giờ {timeSlot.StartTime}-{timeSlot.EndTime}.";

            if (_hubContext != null)
            {
                await _hubContext.Clients.All.SendAsync("ReceiveNewBooking", message, booking.BookingId);
            }
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

            if (_hubContext != null)
            {
                await _hubContext.Clients.Group(booking.UserId.ToString())
                    .SendAsync("ReceiveBookingStatusUpdate", message, booking.BookingId);
            }
        }
    }
}
