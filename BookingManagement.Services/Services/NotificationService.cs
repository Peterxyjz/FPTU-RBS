using BookingManagement.Repositories.Models;
using BookingManagement.Repositories.UnitOfWork;
using BookingManagement.Services.Hubs;
using BookingManagement.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookingManagement.Services.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(IUnitOfWork unitOfWork, IHubContext<NotificationHub> hubContext)
        {
            _unitOfWork = unitOfWork;
            _hubContext = hubContext;
        }

        public async Task<Notification> AddAsync(Notification notification)
        {
            notification.CreatedAt = DateTime.Now;
            notification.UpdatedAt = DateTime.Now;
            notification.IsRead ??= false;

            var result = await _unitOfWork.Notifications.AddAsync(notification);
            await _unitOfWork.CompleteAsync();
            
            // Send real-time notification to user
            await _hubContext.Clients.Group($"User_{notification.UserId}")
                .SendAsync("ReceiveNotification", notification.Title, notification.Message, notification.NotificationId);
            
            return result;
        }

        public async Task<Notification> CreateAsync(Notification notification)
        {
            return await AddAsync(notification);
        }

        public async Task SendNotificationToUserAsync(int userId, string title, string message, int? bookingId = null)
        {
            // Tạo thông báo trong cơ sở dữ liệu
            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                BookingId = bookingId,
                IsRead = false,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            await _unitOfWork.Notifications.AddAsync(notification);
            await _unitOfWork.CompleteAsync();
            
            // Gửi cả thông báo real-time
            await _hubContext.Clients.Group($"User_{userId}")
                .SendAsync("ReceiveNotification", title, message, notification.NotificationId);
        }

        public async Task SendNotificationToAdminsAsync(string title, string message, int? bookingId = null)
        {
            // Get all admin users
            var adminRole = await _unitOfWork.Roles.GetRoleByNameAsync("Admin");
            if (adminRole == null)
                return;

            var adminUsers = await _unitOfWork.Users.GetUsersByRoleAsync(adminRole.RoleId);
            
            // Send notification to each admin
            foreach (var admin in adminUsers)
            {
                var notification = new Notification
                {
                    UserId = admin.UserId,
                    Title = title,
                    Message = message,
                    BookingId = bookingId,
                    IsRead = false,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                await _unitOfWork.Notifications.AddAsync(notification);
            }
            
            await _unitOfWork.CompleteAsync();
            
            // Send real-time notification to all admins
            await _hubContext.Clients.Group("Admins")
                .SendAsync("ReceiveNotification", title, message);
        }

        public async Task NotifyBookingStatusChangeAsync(Booking booking, string statusText)
        {
            try
            {
                // Get room and timeslot information for message
                var room = await _unitOfWork.Rooms.GetByIdAsync(booking.RoomId);
                var timeSlot = await _unitOfWork.TimeSlots.GetByIdAsync(booking.TimeSlotId);
                
                var title = $"Booking Status Changed: {statusText}";
                var message = $"Your booking for {room.RoomName} on {booking.BookingDate.ToString("dd/MM/yyyy")} " +
                              $"({timeSlot.StartTime}-{timeSlot.EndTime}) status has changed to {statusText}";
                
                // Create notification record
                var notification = new Notification
                {
                    UserId = booking.UserId,
                    Title = title,
                    Message = message,
                    BookingId = booking.BookingId,
                    IsRead = false,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                await _unitOfWork.Notifications.AddAsync(notification);
                await _unitOfWork.CompleteAsync();
                
                // Send real-time notification
                await _hubContext.Clients.Group($"User_{booking.UserId}")
                    .SendAsync("BookingStatusChanged", booking.BookingId, statusText, title, message);
                
                System.Diagnostics.Debug.WriteLine($"Real-time notification sent to User_{booking.UserId} about booking {booking.BookingId} status change to {statusText}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error sending real-time notification: {ex.Message}");
                // Still continue without throwing the exception
            }
        }

        public async Task NotifyNewBookingAsync(Booking booking)
        {
            try
            {
                // Get user, room and timeslot information
                var user = await _unitOfWork.Users.GetByIdAsync(booking.UserId);
                var room = await _unitOfWork.Rooms.GetByIdAsync(booking.RoomId);
                var timeSlot = await _unitOfWork.TimeSlots.GetByIdAsync(booking.TimeSlotId);
                
                // Create notification for the user who made the booking
                var userTitle = "Đặt phòng thành công";
                var userMessage = $"Bạn đã đặt phòng {room.RoomName} vào ngày {booking.BookingDate.ToString("dd/MM/yyyy")}, khung giờ {timeSlot.StartTime}-{timeSlot.EndTime}. Hãy chờ xác nhận từ quản trị viên.";
                
                var userNotification = new Notification
                {
                    UserId = booking.UserId,
                    Title = userTitle,
                    Message = userMessage,
                    BookingId = booking.BookingId,
                    IsRead = false,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                
                await _unitOfWork.Notifications.AddAsync(userNotification);
                
                // Create notifications for admins
                var adminRole = await _unitOfWork.Roles.GetRoleByNameAsync("Admin");
                if (adminRole != null)
                {
                    var adminUsers = await _unitOfWork.Users.GetUsersByRoleAsync(adminRole.RoleId);
                    
                    var adminTitle = "New Booking Request";
                    var adminMessage = $"New booking request from {user.FullName} for {room.RoomName} on " +
                                      $"{booking.BookingDate.ToString("dd/MM/yyyy")} ({timeSlot.StartTime}-{timeSlot.EndTime})";
                    
                    foreach (var admin in adminUsers)
                    {
                        var adminNotification = new Notification
                        {
                            UserId = admin.UserId,
                            Title = adminTitle,
                            Message = adminMessage,
                            BookingId = booking.BookingId,
                            IsRead = false,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        };
                        
                        await _unitOfWork.Notifications.AddAsync(adminNotification);
                    }
                }
                
                await _unitOfWork.CompleteAsync();
                
                // Send real-time notifications
                // To the user
                await _hubContext.Clients.Group($"User_{booking.UserId}")
                    .SendAsync("ReceiveNotification", userTitle, userMessage, userNotification.NotificationId);
                
                System.Diagnostics.Debug.WriteLine($"Real-time booking confirmation sent to User_{booking.UserId}");
                
                // To admins
                await _hubContext.Clients.Group("Admins")
                    .SendAsync("NewBookingReceived", booking.BookingId, user.FullName);
                
                System.Diagnostics.Debug.WriteLine($"Real-time new booking notification sent to Admins group");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error sending new booking notification: {ex.Message}");
                // Still continue without throwing the exception 
            }
        }

        public async Task<IEnumerable<Notification>> GetNotificationsByUserIdAsync(int userId)
        {
            return await _unitOfWork.Notifications.GetNotificationsByUserIdAsync(userId);
        }

        public async Task<IEnumerable<Notification>> GetUnreadNotificationsByUserIdAsync(int userId)
        {
            return await _unitOfWork.Notifications.GetUnreadNotificationsByUserIdAsync(userId);
        }

        public async Task<int> GetUnreadCountAsync(int userId)
        {
            var unreadNotifications = await _unitOfWork.Notifications.GetUnreadNotificationsByUserIdAsync(userId);
            return unreadNotifications.Count();
        }

        public async Task<int> MarkAllAsReadAsync(int userId)
        {
            var count = await _unitOfWork.Notifications.MarkAllAsReadAsync(userId);
            await _unitOfWork.CompleteAsync();
            return count;
        }

        public async Task<bool> MarkAsReadAsync(int notificationId)
        {
            var notification = await _unitOfWork.Notifications.GetByIdAsync(notificationId);
            if (notification == null)
                return false;

            notification.IsRead = true;
            notification.UpdatedAt = DateTime.Now;
            await _unitOfWork.CompleteAsync();
            return true;
        }
    }
}
