using BookingManagement.Repositories.Models;

namespace BookingManagement.Services.Interfaces
{
    public interface INotificationService
    {
        Task<IEnumerable<Notification>> GetNotificationsByUserIdAsync(int userId);
        Task<IEnumerable<Notification>> GetUnreadNotificationsByUserIdAsync(int userId);
        Task<int> GetUnreadCountAsync(int userId);
        Task<int> MarkAllAsReadAsync(int userId);
        Task<bool> MarkAsReadAsync(int notificationId);
        Task<Notification> AddAsync(Notification notification);
    }
}