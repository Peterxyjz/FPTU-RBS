using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace BookingManagement.Services.Hubs
{
    public class NotificationHub : Hub
    {
        // Connection method that users/admins will call when they connect
        public async Task JoinUserGroup(string userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
        }

        // Connection method for admins
        public async Task JoinAdminGroup()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
        }

        // Method to send notification to a specific user
        public async Task SendNotificationToUser(string userId, string title, string message)
        {
            await Clients.Group($"User_{userId}").SendAsync("ReceiveNotification", title, message);
        }

        // Method to send notification to all admins
        public async Task SendNotificationToAdmins(string title, string message)
        {
            await Clients.Group("Admins").SendAsync("ReceiveNotification", title, message);
        }

        // Method to send notification to everyone
        public async Task SendNotificationToAll(string title, string message)
        {
            await Clients.All.SendAsync("ReceiveNotification", title, message);
        }

        // Method called when a booking status changes
        public async Task BookingStatusChanged(int bookingId, int userId, string status)
        {
            await Clients.Group($"User_{userId}").SendAsync("BookingStatusChanged", bookingId, status);
        }

        // Method called when a new booking is created (notify admins)
        public async Task NewBookingCreated(int bookingId, string userName)
        {
            await Clients.Group("Admins").SendAsync("NewBookingReceived", bookingId, userName);
        }

        // Disconnect method
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}
