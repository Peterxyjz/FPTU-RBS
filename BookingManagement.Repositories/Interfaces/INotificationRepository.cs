using BookingManagement.Repositories.Base;
using BookingManagement.Repositories.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookingManagement.Repositories.Interfaces
{
    public interface INotificationRepository : IGenericRepository<Notification>
    {
        Task<IEnumerable<Notification>> GetNotificationsByUserIdAsync(int userId);
        Task<IEnumerable<Notification>> GetUnreadNotificationsByUserIdAsync(int userId);
        Task<int> MarkAllAsReadAsync(int userId);
    }
}
