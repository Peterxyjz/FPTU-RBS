using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace BookingManagement.Admin.MVC.Hubs
{
    public class BookingHub : Hub
    {
        private readonly ILogger<BookingHub> _logger;

        public BookingHub(ILogger<BookingHub> logger)
        {
            _logger = logger;
        }
        public async Task SendNewBookingNotification(string message, int bookingId)
        {
            _logger.LogInformation($"Sending new booking notification: {message}, bookingId: {bookingId}");
            await Clients.All.SendAsync("ReceiveNewBooking", message, bookingId);
        }

        public async Task SendBookingApprovalNotification(string userId, string message, int bookingId)
        {
            _logger.LogInformation($"Sending booking approval notification to user {userId}: {message}, bookingId: {bookingId}");
            await Clients.Group(userId).SendAsync("ReceiveBookingApproval", message, bookingId);
        }

        public async Task SendBookingRejectionNotification(string userId, string message, int bookingId)
        {
            _logger.LogInformation($"Sending booking rejection notification to user {userId}: {message}, bookingId: {bookingId}");
            await Clients.Group(userId).SendAsync("ReceiveBookingRejection", message, bookingId);
        }

        public async Task JoinUserGroup(string userId)
        {
            _logger.LogInformation($"User {userId} joining group with connection ID {Context.ConnectionId}");
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);
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
