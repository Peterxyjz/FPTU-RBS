using BookingManagement.Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingManagement.Services.Interfaces
{
    public interface ISignalRService
    {
        Task SendNewBookingNotificationAsync(Booking booking);
        Task SendBookingStatusUpdateAsync(Booking booking, string message);
    }
}
