using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace BookingManagement.User.Razor.Hubs
{
    public class NotificationHub : Hub
    {
        public async Task SendNotification(string userId, string message)
        {
            await Clients.Group(userId).SendAsync("ReceiveNotification", message);
        }

        public async Task JoinGroup(string userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);
        }
    }
}
