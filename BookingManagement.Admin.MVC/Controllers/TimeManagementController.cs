using BookingManagement.Admin.MVC.Models;
using BookingManagement.Repositories.Models;
using BookingManagement.Services.DTOs;
using BookingManagement.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookingManagement.Admin.MVC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class TimeManagementController : Controller
    {
        private readonly IOperationalHoursService _operationalHoursService;
        private readonly ISpecialScheduleService _specialScheduleService;
        private readonly IBlockedTimeSlotService _blockedTimeSlotService;
        private readonly IRoomService _roomService;
        private readonly ITimeSlotService _timeSlotService;

        public TimeManagementController(
            IOperationalHoursService operationalHoursService,
            ISpecialScheduleService specialScheduleService,
            IBlockedTimeSlotService blockedTimeSlotService,
            IRoomService roomService,
            ITimeSlotService timeSlotService)
        {
            _operationalHoursService = operationalHoursService;
            _specialScheduleService = specialScheduleService;
            _blockedTimeSlotService = blockedTimeSlotService;
            _roomService = roomService;
            _timeSlotService = timeSlotService;
        }

        // Dashboard
        public IActionResult Index()
        {
            return View();
        }

        #region Operational Hours
        public async Task<IActionResult> OperationalHours()
        {
            var operationalHours = await _operationalHoursService.GetAllOperationalHoursAsync();
            return View(operationalHours);
        }

        public async Task<IActionResult> CreateOperationalHours()
        {
            ViewBag.Rooms = await GetRoomsSelectList();
            ViewBag.DaysOfWeek = GetDaysOfWeekMultiSelectList();
            return View(new OperationalHoursDto
            {
                IsActive = true,
                OpenTime = "08:00",
                CloseTime = "17:00",
                DaysOfWeekList = new List<int> { 1, 2, 3, 4, 5 } // Default Mon-Fri
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOperationalHours(OperationalHoursDto operationalHoursDto, int[] selectedDays)
        {
            if (ModelState.IsValid)
            {
                operationalHoursDto.DaysOfWeekList = selectedDays.ToList();
                await _operationalHoursService.CreateOperationalHoursAsync(operationalHoursDto);
                return RedirectToAction(nameof(OperationalHours));
            }

            ViewBag.Rooms = await GetRoomsSelectList();
            ViewBag.DaysOfWeek = GetDaysOfWeekMultiSelectList();
            return View(operationalHoursDto);
        }

        public async Task<IActionResult> EditOperationalHours(int id)
        {
            var operationalHours = await _operationalHoursService.GetOperationalHoursByIdAsync(id);
            if (operationalHours == null)
            {
                return NotFound();
            }

            ViewBag.Rooms = await GetRoomsSelectList();
            ViewBag.DaysOfWeek = GetDaysOfWeekMultiSelectList();
            return View(operationalHours);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditOperationalHours(OperationalHoursDto operationalHoursDto, int[] selectedDays)
        {
            if (ModelState.IsValid)
            {
                operationalHoursDto.DaysOfWeekList = selectedDays.ToList();
                await _operationalHoursService.UpdateOperationalHoursAsync(operationalHoursDto);
                return RedirectToAction(nameof(OperationalHours));
            }

            ViewBag.Rooms = await GetRoomsSelectList();
            ViewBag.DaysOfWeek = GetDaysOfWeekMultiSelectList();
            return View(operationalHoursDto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteOperationalHours(int id)
        {
            await _operationalHoursService.DeleteOperationalHoursAsync(id);
            return RedirectToAction(nameof(OperationalHours));
        }
        #endregion

        #region Special Schedules
        public async Task<IActionResult> SpecialSchedules()
        {
            var specialSchedules = await _specialScheduleService.GetAllSpecialSchedulesAsync();
            return View(specialSchedules);
        }

        public async Task<IActionResult> CreateSpecialSchedule()
        {
            ViewBag.Rooms = await GetRoomsSelectList();
            return View(new SpecialScheduleDto
            {
                IsActive = true,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today,
                OpenTime = "08:00",
                CloseTime = "17:00"
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSpecialSchedule(SpecialScheduleDto specialScheduleDto)
        {
            if (ModelState.IsValid)
            {
                await _specialScheduleService.CreateSpecialScheduleAsync(specialScheduleDto);
                return RedirectToAction(nameof(SpecialSchedules));
            }

            ViewBag.Rooms = await GetRoomsSelectList();
            return View(specialScheduleDto);
        }

        public async Task<IActionResult> EditSpecialSchedule(int id)
        {
            var specialSchedule = await _specialScheduleService.GetSpecialScheduleByIdAsync(id);
            if (specialSchedule == null)
            {
                return NotFound();
            }

            ViewBag.Rooms = await GetRoomsSelectList();
            return View(specialSchedule);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSpecialSchedule(SpecialScheduleDto specialScheduleDto)
        {
            if (ModelState.IsValid)
            {
                await _specialScheduleService.UpdateSpecialScheduleAsync(specialScheduleDto);
                return RedirectToAction(nameof(SpecialSchedules));
            }

            ViewBag.Rooms = await GetRoomsSelectList();
            return View(specialScheduleDto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSpecialSchedule(int id)
        {
            await _specialScheduleService.DeleteSpecialScheduleAsync(id);
            return RedirectToAction(nameof(SpecialSchedules));
        }
        #endregion

        #region Blocked Time Slots
        public async Task<IActionResult> BlockedTimeSlots()
        {
            var blockedTimeSlots = await _blockedTimeSlotService.GetAllBlockedTimeSlotsAsync();
            return View(blockedTimeSlots);
        }

        public async Task<IActionResult> CreateBlockedTimeSlot()
        {
            ViewBag.Rooms = await GetRoomsSelectList();
            ViewBag.TimeSlots = await GetTimeSlotsSelectList();
            ViewBag.BlockTypes = GetBlockTypesSelectList();
            
            // Get current user ID from claims
            int currentUserId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value ?? "1");
            
            return View(new BlockedTimeSlotDto
            {
                IsActive = true,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(1),
                CreatedById = currentUserId
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBlockedTimeSlot(BlockedTimeSlotDto blockedTimeSlotDto)
        {
            if (ModelState.IsValid)
            {
                await _blockedTimeSlotService.CreateBlockedTimeSlotAsync(blockedTimeSlotDto);
                return RedirectToAction(nameof(BlockedTimeSlots));
            }

            ViewBag.Rooms = await GetRoomsSelectList();
            ViewBag.TimeSlots = await GetTimeSlotsSelectList();
            ViewBag.BlockTypes = GetBlockTypesSelectList();
            return View(blockedTimeSlotDto);
        }

        public async Task<IActionResult> EditBlockedTimeSlot(int id)
        {
            var blockedTimeSlot = await _blockedTimeSlotService.GetBlockedTimeSlotByIdAsync(id);
            if (blockedTimeSlot == null)
            {
                return NotFound();
            }

            ViewBag.Rooms = await GetRoomsSelectList();
            ViewBag.TimeSlots = await GetTimeSlotsSelectList();
            ViewBag.BlockTypes = GetBlockTypesSelectList();
            return View(blockedTimeSlot);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBlockedTimeSlot(BlockedTimeSlotDto blockedTimeSlotDto)
        {
            if (ModelState.IsValid)
            {
                await _blockedTimeSlotService.UpdateBlockedTimeSlotAsync(blockedTimeSlotDto);
                return RedirectToAction(nameof(BlockedTimeSlots));
            }

            ViewBag.Rooms = await GetRoomsSelectList();
            ViewBag.TimeSlots = await GetTimeSlotsSelectList();
            ViewBag.BlockTypes = GetBlockTypesSelectList();
            return View(blockedTimeSlotDto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteBlockedTimeSlot(int id)
        {
            await _blockedTimeSlotService.DeleteBlockedTimeSlotAsync(id);
            return RedirectToAction(nameof(BlockedTimeSlots));
        }
        #endregion

        #region Helper Methods
        private async Task<SelectList> GetRoomsSelectList()
        {
            var rooms = await _roomService.GetActiveRoomsAsync();
            return new SelectList(rooms, "RoomId", "RoomName");
        }

        private async Task<SelectList> GetTimeSlotsSelectList()
        {
            var timeSlots = await _timeSlotService.GetActiveTimeSlotsAsync();
            return new SelectList(timeSlots, "TimeSlotId", "DisplayText");
        }

        private MultiSelectList GetDaysOfWeekMultiSelectList()
        {
            var daysOfWeek = new List<SelectListItem>
            {
                new SelectListItem { Value = "0", Text = "Sunday" },
                new SelectListItem { Value = "1", Text = "Monday" },
                new SelectListItem { Value = "2", Text = "Tuesday" },
                new SelectListItem { Value = "3", Text = "Wednesday" },
                new SelectListItem { Value = "4", Text = "Thursday" },
                new SelectListItem { Value = "5", Text = "Friday" },
                new SelectListItem { Value = "6", Text = "Saturday" }
            };

            return new MultiSelectList(daysOfWeek, "Value", "Text");
        }

        private SelectList GetBlockTypesSelectList()
        {
            var blockTypes = new List<SelectListItem>
            {
                new SelectListItem { Value = "0", Text = "Maintenance" },
                new SelectListItem { Value = "1", Text = "Special Event" },
                new SelectListItem { Value = "2", Text = "Admin Reserved" },
                new SelectListItem { Value = "3", Text = "Other" }
            };

            return new SelectList(blockTypes, "Value", "Text");
        }
        #endregion
    }
}
