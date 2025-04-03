using BookingManagement.Repositories.UnitOfWork;
using BookingManagement.Services.DTOs;
using BookingManagement.Services.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingManagement.Services.Services
{
    public class TimeSlotService : ITimeSlotService
    {
        private readonly IUnitOfWork _unitOfWork;

        public TimeSlotService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<TimeSlotDto>> GetActiveTimeSlotsAsync()
        {
            var timeslots = await _unitOfWork.TimeSlots.GetActiveTimeSlotsAsync();

            var timeSlotDtos = timeslots.Select(ts => new TimeSlotDto
            {
                TimeSlotId = ts.TimeSlotId,
                DisplayText = $"Slot {ts.TimeSlotId}: {ts.StartTime:hh\\:mm} - {ts.EndTime:hh\\:mm}"
            });

            return timeSlotDtos;
        }

        public async Task<TimeSlotDto> GetActiveTimeSlotByIdAsync(int id)
        {
            var timeslot = await _unitOfWork.TimeSlots.GetByIdAsync(id);

            var timeSlotDto = new TimeSlotDto
            {
                TimeSlotId = timeslot.TimeSlotId,
                DisplayText = $"Slot {timeslot.TimeSlotId}: {timeslot.StartTime:hh\\:mm} - {timeslot.EndTime:hh\\:mm}"
            };

            return timeSlotDto;
        }
    }
}
