using BookingManagement.Repositories.Models;
using BookingManagement.Repositories.UnitOfWork;
using BookingManagement.Services.DTOs;
using BookingManagement.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public async Task<TimeSlotDto> GetTimeSlotByIdAsync(int id)
        {
            var timeslot = await _unitOfWork.TimeSlots.GetByIdAsync(id);
            return timeslot == null ? null : MapToDto(timeslot);
        }

        public async Task<TimeSlotDto> GetActiveTimeSlotByIdAsync(int id)
        {
            var timeslot = await _unitOfWork.TimeSlots.GetByIdAsync(id);
            if (timeslot == null || timeslot.IsActive != true)
                return null;

            return MapToDto(timeslot);
        }

        public async Task<IEnumerable<TimeSlotDto>> GetAllTimeSlotsAsync()
        {
            var timeslots = await _unitOfWork.TimeSlots.GetAllAsync();
            return timeslots.Select(MapToDto);
        }

        public async Task<IEnumerable<TimeSlotDto>> GetActiveTimeSlotsAsync()
        {
            var timeslots = await _unitOfWork.TimeSlots.GetActiveTimeSlotsAsync();
            return timeslots.Select(MapToDto);
        }

        public async Task<TimeSlotDto> CreateTimeSlotAsync(TimeSlotDto timeSlotDto)
        {
            var timeSlot = new TimeSlot
            {
                StartTime = TimeOnly.Parse(timeSlotDto.StartTime),
                EndTime = TimeOnly.Parse(timeSlotDto.EndTime),
                IsActive = true,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            await _unitOfWork.TimeSlots.AddAsync(timeSlot);
            await _unitOfWork.CompleteAsync();

            timeSlotDto.TimeSlotId = timeSlot.TimeSlotId;
            return timeSlotDto;
        }

        public async Task<TimeSlotDto> UpdateTimeSlotAsync(TimeSlotDto timeSlotDto)
        {
            var timeSlot = await _unitOfWork.TimeSlots.GetByIdAsync(timeSlotDto.TimeSlotId);
            if (timeSlot == null)
                return null;

            timeSlot.StartTime = TimeOnly.Parse(timeSlotDto.StartTime);
            timeSlot.EndTime = TimeOnly.Parse(timeSlotDto.EndTime);
            timeSlot.IsActive = true;
            timeSlot.UpdatedAt = DateTime.Now;

            await _unitOfWork.TimeSlots.UpdateAsync(timeSlot);
            await _unitOfWork.CompleteAsync();

            return timeSlotDto;
        }

        public async Task<bool> DeleteTimeSlotAsync(int id)
        {
            var timeSlot = await _unitOfWork.TimeSlots.GetByIdAsync(id);
            if (timeSlot == null)
                return false;

            timeSlot.IsActive = false;
            timeSlot.UpdatedAt = DateTime.Now;
            
            await _unitOfWork.TimeSlots.UpdateAsync(timeSlot);
            await _unitOfWork.CompleteAsync();
            
            return true;
        }

        private TimeSlotDto MapToDto(TimeSlot timeSlot)
        {
            return new TimeSlotDto
            {
                TimeSlotId = timeSlot.TimeSlotId,
                StartTime = timeSlot.StartTime.ToString("HH:mm"),
                EndTime = timeSlot.EndTime.ToString("HH:mm"),
                IsActive = timeSlot.IsActive == true,
                DisplayText = $"Slot {timeSlot.TimeSlotId}: {timeSlot.StartTime:hh\\:mm} - {timeSlot.EndTime:hh\\:mm}"
            };
        }
    }
}
