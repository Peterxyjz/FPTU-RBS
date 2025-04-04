using BookingManagement.Services.DTOs;
using BookingManagement.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BookingManagement.Admin.MVC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class TimeSlotController : Controller
    {
        private readonly ITimeSlotService _timeSlotService;

        public TimeSlotController(ITimeSlotService timeSlotService)
        {
            _timeSlotService = timeSlotService;
        }

        public async Task<IActionResult> Index()
        {
            var timeSlots = await _timeSlotService.GetAllTimeSlotsAsync();
            return View(timeSlots);
        }

        public IActionResult Create()
        {
            return View(new TimeSlotDto
            {
                StartTime = "08:00",
                EndTime = "09:30",
                IsActive = true
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TimeSlotDto timeSlotDto)
        {
            if (ModelState.IsValid)
            {
                await _timeSlotService.CreateTimeSlotAsync(timeSlotDto);
                return RedirectToAction(nameof(Index));
            }
            return View(timeSlotDto);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var timeSlot = await _timeSlotService.GetTimeSlotByIdAsync(id);
            if (timeSlot == null)
            {
                return NotFound();
            }
            return View(timeSlot);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TimeSlotDto timeSlotDto)
        {
            if (ModelState.IsValid)
            {
                await _timeSlotService.UpdateTimeSlotAsync(timeSlotDto);
                return RedirectToAction(nameof(Index));
            }
            return View(timeSlotDto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _timeSlotService.DeleteTimeSlotAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
