using BookingManagement.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BookingManagement.Admin.MVC.ViewComponents
{
    public class PendingBookingsCountViewComponent : ViewComponent
    {
        private readonly IBookingService _bookingService;

        public PendingBookingsCountViewComponent(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var bookings = await _bookingService.GetAllAsync();
            int pendingCount = bookings.Count(b => b.Status == 1); // Status 1 = Chờ duyệt
            
            return View(pendingCount);
        }
    }
}
