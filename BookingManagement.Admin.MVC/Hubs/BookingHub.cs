using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace BookingManagement.Admin.MVC.Hubs
{
    public class BookingHub : Hub
    {
        public async Task SendNewBookingNotification(string message, int bookingId)
        {
            await Clients.All.SendAsync("ReceiveNewBooking", message, bookingId);
        }

        public async Task SendBookingApprovalNotification(string userId, string message, int bookingId)
        {
            await Clients.Group(userId).SendAsync("ReceiveBookingApproval", message, bookingId);
        }

        public async Task SendBookingRejectionNotification(string userId, string message, int bookingId)
        {
            await Clients.Group(userId).SendAsync("ReceiveBookingRejection", message, bookingId);
        }

        public async Task JoinUserGroup(string userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);
        }
    }
}
