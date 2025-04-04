using BookingManagement.Repositories.Models;
using BookingManagement.Services.DTOs;
using BookingManagement.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;

namespace BookingManagement.Admin.MVC.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class BookingController : Controller
    {
        private readonly IBookingService _bookingService;
        private readonly IRoomService _roomService;
        private readonly IUserService _userService;
        private readonly ITimeSlotService _timeSlotService;
        private readonly INotificationService _notificationService;

        public BookingController(
            IBookingService bookingService,
            IRoomService roomService,
            IUserService userService,
            ITimeSlotService timeSlotService,
            INotificationService notificationService)
        {
            _bookingService = bookingService;
            _roomService = roomService;
            _userService = userService;
            _timeSlotService = timeSlotService;
            _notificationService = notificationService;
        }

        // GET: Booking
        public async Task<IActionResult> Index(string status = "all", string searchString = "", string dateFilter = "")
        {
            var bookings = await _bookingService.GetAllAsync();

            // Lọc theo trạng thái nếu có yêu cầu
            if (status != "all")
            {
                if (int.TryParse(status, out int statusInt))
                {
                    bookings = bookings.Where(b => b.Status == statusInt);
                }
            }

            // Lọc theo từ khóa tìm kiếm (phòng hoặc người dùng)
            if (!string.IsNullOrEmpty(searchString))
            {
                bookings = bookings.Where(b => 
                    b.Room.RoomName.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                    b.User.FullName.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                    b.User.Email.Contains(searchString, StringComparison.OrdinalIgnoreCase));
            }

            // Lọc theo ngày
            if (!string.IsNullOrEmpty(dateFilter) && DateOnly.TryParse(dateFilter, out DateOnly filterDate))
            {
                bookings = bookings.Where(b => b.BookingDate == filterDate);
            }

            // Sắp xếp: mới nhất lên đầu, sau đó là status (chờ duyệt lên đầu)
            bookings = bookings.OrderByDescending(b => b.CreatedAt)
                               .ThenBy(b => b.Status);

            // Lưu các giá trị filter cho view
            ViewBag.CurrentStatus = status;
            ViewBag.CurrentSearch = searchString;
            ViewBag.CurrentDate = dateFilter;
            ViewBag.TodayDate = DateOnly.FromDateTime(DateTime.Today).ToString("yyyy-MM-dd");

            return View(bookings);
        }

        // GET: Booking/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var booking = await _bookingService.GetByIdAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        // GET: Booking/Approve/5
        public async Task<IActionResult> Approve(int id)
        {
            var booking = await _bookingService.GetByIdAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            // Chỉ cho phép duyệt booking đang ở trạng thái chờ duyệt (1)
            if (booking.Status != 1)
            {
                TempData["ErrorMessage"] = "Chỉ có thể phê duyệt yêu cầu đang ở trạng thái chờ duyệt.";
                return RedirectToAction(nameof(Index));
            }

            return View(booking);
        }

        // POST: Booking/Approve/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveConfirmed(int id)
        {
            var booking = await _bookingService.GetByIdAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            if (booking.Status != 1)
            {
                TempData["ErrorMessage"] = "Chỉ có thể phê duyệt yêu cầu đang ở trạng thái chờ duyệt.";
                return RedirectToAction(nameof(Index));
            }

            // Cập nhật trạng thái thành "Đã duyệt" (2)
            booking.Status = 2;
            booking.UpdatedAt = DateTime.Now;
            await _bookingService.UpdateAsync(booking);

            // Lấy thông tin phòng và khung giờ cho thông báo
            var room = await _roomService.GetRoomByIdAsync(booking.RoomId);
            var timeSlotDto = await _timeSlotService.GetTimeSlotByIdAsync(booking.TimeSlotId);

            // Tạo thông báo cho người dùng
            await _notificationService.CreateAsync(new Notification
            {
                UserId = booking.UserId,
                Title = "Đặt phòng đã được phê duyệt",
                Message = $"Yêu cầu đặt phòng {room.RoomName} vào ngày {booking.BookingDate.ToString("dd/MM/yyyy")}, khung giờ {timeSlotDto.StartTime}-{timeSlotDto.EndTime} đã được phê duyệt.",
                IsRead = false,
                BookingId = booking.BookingId,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            });

            TempData["SuccessMessage"] = "Yêu cầu đặt phòng đã được phê duyệt thành công.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Booking/Reject/5
        public async Task<IActionResult> Reject(int id)
        {
            var booking = await _bookingService.GetByIdAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            // Chỉ cho phép từ chối booking đang ở trạng thái chờ duyệt (1)
            if (booking.Status != 1)
            {
                TempData["ErrorMessage"] = "Chỉ có thể từ chối yêu cầu đang ở trạng thái chờ duyệt.";
                return RedirectToAction(nameof(Index));
            }

            return View(booking);
        }

        // POST: Booking/Reject/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectConfirmed(int id, string rejectReason)
        {
            var booking = await _bookingService.GetByIdAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            if (booking.Status != 1)
            {
                TempData["ErrorMessage"] = "Chỉ có thể từ chối yêu cầu đang ở trạng thái chờ duyệt.";
                return RedirectToAction(nameof(Index));
            }

            // Cập nhật trạng thái thành "Từ chối" (3) và lý do từ chối
            booking.Status = 3;
            booking.RejectReason = rejectReason;
            booking.UpdatedAt = DateTime.Now;
            await _bookingService.UpdateAsync(booking);

            // Lấy thông tin phòng và khung giờ cho thông báo
            var room = await _roomService.GetRoomByIdAsync(booking.RoomId);
            var timeSlotDto = await _timeSlotService.GetTimeSlotByIdAsync(booking.TimeSlotId);

            // Tạo thông báo cho người dùng
            await _notificationService.CreateAsync(new Notification
            {
                UserId = booking.UserId,
                Title = "Đặt phòng đã bị từ chối",
                Message = $"Yêu cầu đặt phòng {room.RoomName} vào ngày {booking.BookingDate.ToString("dd/MM/yyyy")}, khung giờ {timeSlotDto.StartTime}-{timeSlotDto.EndTime} đã bị từ chối. Lý do: {rejectReason}",
                IsRead = false,
                BookingId = booking.BookingId,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            });

            TempData["SuccessMessage"] = "Yêu cầu đặt phòng đã bị từ chối.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Booking/Complete/5
        public async Task<IActionResult> Complete(int id)
        {
            var booking = await _bookingService.GetByIdAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            // Chỉ cho phép đánh dấu hoàn thành các booking đã được duyệt (2)
            if (booking.Status != 2)
            {
                TempData["ErrorMessage"] = "Chỉ có thể đánh dấu hoàn thành các yêu cầu đã được duyệt.";
                return RedirectToAction(nameof(Index));
            }

            return View(booking);
        }

        // POST: Booking/Complete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteConfirmed(int id)
        {
            var booking = await _bookingService.GetByIdAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            if (booking.Status != 2)
            {
                TempData["ErrorMessage"] = "Chỉ có thể đánh dấu hoàn thành các yêu cầu đã được duyệt.";
                return RedirectToAction(nameof(Index));
            }

            // Cập nhật trạng thái thành "Đã hoàn thành" (4)
            booking.Status = 4;
            booking.UpdatedAt = DateTime.Now;
            await _bookingService.UpdateAsync(booking);

            // Lấy thông tin phòng và khung giờ cho thông báo
            var room = await _roomService.GetRoomByIdAsync(booking.RoomId);
            var timeSlotDto = await _timeSlotService.GetTimeSlotByIdAsync(booking.TimeSlotId);

            // Tạo thông báo cho người dùng
            await _notificationService.CreateAsync(new Notification
            {
                UserId = booking.UserId,
                Title = "Đặt phòng đã hoàn thành",
                Message = $"Yêu cầu đặt phòng {room.RoomName} vào ngày {booking.BookingDate.ToString("dd/MM/yyyy")}, khung giờ {timeSlotDto.StartTime}-{timeSlotDto.EndTime} đã được đánh dấu hoàn thành.",
                IsRead = false,
                BookingId = booking.BookingId,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            });

            TempData["SuccessMessage"] = "Yêu cầu đặt phòng đã được đánh dấu hoàn thành.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Booking/Export
        public async Task<IActionResult> Export(string status = "all", string searchString = "", string dateFilter = "")
        {
            var bookings = await _bookingService.GetAllAsync();

            // Lọc theo trạng thái nếu có yêu cầu
            if (status != "all")
            {
                if (int.TryParse(status, out int statusInt))
                {
                    bookings = bookings.Where(b => b.Status == statusInt);
                }
            }

            // Lọc theo từ khóa tìm kiếm (phòng hoặc người dùng)
            if (!string.IsNullOrEmpty(searchString))
            {
                bookings = bookings.Where(b => 
                    b.Room.RoomName.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                    b.User.FullName.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                    b.User.Email.Contains(searchString, StringComparison.OrdinalIgnoreCase));
            }

            // Lọc theo ngày
            if (!string.IsNullOrEmpty(dateFilter) && DateOnly.TryParse(dateFilter, out DateOnly filterDate))
            {
                bookings = bookings.Where(b => b.BookingDate == filterDate);
            }

            // Sắp xếp theo ngày đặt, khung giờ
            bookings = bookings.OrderBy(b => b.BookingDate)
                                .ThenBy(b => b.TimeSlot.StartTime);

            // Tạo chuỗi CSV
            var csv = new System.Text.StringBuilder();
            
            // Tiêu đề cột
            csv.AppendLine("Mã đặt phòng,Người đặt,Email,Phòng,Tòa nhà,Ngày đặt,Khung giờ,Trạng thái,Lý do từ chối,Ngày tạo");
            
            // Dữ liệu
            foreach (var booking in bookings)
            {
                string status_text = GetStatusText(booking.Status);
                string reject_reason = booking.RejectReason ?? "";
                
                // Thay thế dấu phẩy trong các trường văn bản để tránh xảy ra lỗi CSV
                string roomName = booking.Room.RoomName.Replace(",", " ");
                string userName = booking.User.FullName.Replace(",", " ");
                string userEmail = booking.User.Email.Replace(",", " ");
                string building = (booking.Room.Building ?? "").Replace(",", " ");
                reject_reason = reject_reason.Replace(",", " ");
                
                // Lấy thông tin time slot
                var timeSlotDto = await _timeSlotService.GetTimeSlotByIdAsync(booking.TimeSlotId);
                var timeSlotInfo = $"{timeSlotDto.StartTime}-{timeSlotDto.EndTime}";
                
                csv.AppendLine($"{booking.BookingId},{userName},{userEmail},{roomName},{building},{booking.BookingDate:dd/MM/yyyy},{timeSlotInfo},{status_text},{reject_reason},{booking.CreatedAt:dd/MM/yyyy HH:mm}");
            }

            // Tạo tên file với thời gian hiện tại
            string fileName = $"danh-sach-dat-phong_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            
            // Trả về file CSV
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", fileName);
        }

        // Phương thức trợ giúp để lấy tên trạng thái theo mã số
        private string GetStatusText(int status)
        {
            return status switch
            {
                1 => "Chờ duyệt",
                2 => "Đã duyệt",
                3 => "Từ chối",
                4 => "Đã hoàn thành",
                5 => "Đã hủy",
                _ => status.ToString()
            };
        }
    }
}