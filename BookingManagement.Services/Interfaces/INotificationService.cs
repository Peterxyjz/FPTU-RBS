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
        Task<Notification> CreateAsync(Notification notification);
        
        // New methods for SignalR real-time notifications
        Task SendNotificationToUserAsync(int userId, string title, string message, int? bookingId = null);
        Task SendNotificationToAdminsAsync(string title, string message, int? bookingId = null);
        Task NotifyBookingStatusChangeAsync(Booking booking, string statusText);
        Task NotifyNewBookingAsync(Booking booking);
    }
}