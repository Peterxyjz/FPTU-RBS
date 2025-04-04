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
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;

        public NotificationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Notification> AddAsync(Notification notification)
        {
            notification.CreatedAt = DateTime.Now;
            notification.UpdatedAt = DateTime.Now;
            notification.IsRead ??= false;

            var result = await _unitOfWork.Notifications.AddAsync(notification);
            await _unitOfWork.CompleteAsync();
            return result;
        }

        public async Task<Notification> CreateAsync(Notification notification)
        {
            return await AddAsync(notification);
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
