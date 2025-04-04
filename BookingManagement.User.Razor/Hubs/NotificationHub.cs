using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace BookingManagement.User.Razor.Hubs
{
    public class NotificationHub : Hub
    {
        private readonly ILogger<NotificationHub> _logger;

        public NotificationHub(ILogger<NotificationHub> logger)
        {
            _logger = logger;
        }
        public async Task SendNotification(string userId, string message)
        {
            _logger.LogInformation($"Sending notification to user {userId}: {message}");
            await Clients.Group(userId).SendAsync("ReceiveNotification", message);
        }

        public async Task SendBookingApproval(string userId, string message, int bookingId)
        {
            _logger.LogInformation($"Sending booking approval to user {userId}: {message}, bookingId: {bookingId}");
            await Clients.Group(userId).SendAsync("ReceiveBookingApproval", message, bookingId);
        }

        public async Task SendBookingRejection(string userId, string message, int bookingId)
        {
            _logger.LogInformation($"Sending booking rejection to user {userId}: {message}, bookingId: {bookingId}");
            await Clients.Group(userId).SendAsync("ReceiveBookingRejection", message, bookingId);
        }

        public async Task SendBookingCompletion(string userId, string message, int bookingId)
        {
            _logger.LogInformation($"Sending booking completion to user {userId}: {message}, bookingId: {bookingId}");
            await Clients.Group(userId).SendAsync("ReceiveBookingCompletion", message, bookingId);
        }

        public async Task JoinGroup(string userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);
            _logger.LogInformation($"User {userId} joined group with connection ID {Context.ConnectionId}");
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation($"Client connected: {Context.ConnectionId}");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            _logger.LogInformation($"Client disconnected: {Context.ConnectionId}, Exception: {exception?.Message}");
            await base.OnDisconnectedAsync(exception);
        }
    }
}
