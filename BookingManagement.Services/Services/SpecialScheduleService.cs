using BookingManagement.Repositories.Models.TimeManagement;
using BookingManagement.Repositories.UnitOfWork;
using BookingManagement.Services.DTOs;
using BookingManagement.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingManagement.Services.Services
{
    public class SpecialScheduleService : ISpecialScheduleService
    {
        private readonly IUnitOfWork _unitOfWork;

        public SpecialScheduleService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<SpecialScheduleDto> CreateSpecialScheduleAsync(SpecialScheduleDto specialScheduleDto)
        {
            var specialSchedule = new SpecialSchedule
            {
                Title = specialScheduleDto.Title,
                Description = specialScheduleDto.Description,
                StartDate = specialScheduleDto.StartDate,
                EndDate = specialScheduleDto.EndDate,
                OpenTime = string.IsNullOrEmpty(specialScheduleDto.OpenTime) ? null : TimeOnly.Parse(specialScheduleDto.OpenTime),
                CloseTime = string.IsNullOrEmpty(specialScheduleDto.CloseTime) ? null : TimeOnly.Parse(specialScheduleDto.CloseTime),
                IsClosed = specialScheduleDto.IsClosed,
                BuildingId = specialScheduleDto.BuildingId,
                RoomId = specialScheduleDto.RoomId,
                IsActive = specialScheduleDto.IsActive,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            await _unitOfWork.SpecialSchedules.AddAsync(specialSchedule);
            await _unitOfWork.CompleteAsync();

            specialScheduleDto.SpecialScheduleId = specialSchedule.SpecialScheduleId;
            return specialScheduleDto;
        }

        public async Task<bool> DeleteSpecialScheduleAsync(int id)
        {
            var specialSchedule = await _unitOfWork.SpecialSchedules.GetByIdAsync(id);
            if (specialSchedule == null)
                return false;

            specialSchedule.IsActive = false;
            specialSchedule.UpdatedAt = DateTime.Now;
            
            await _unitOfWork.SpecialSchedules.UpdateAsync(specialSchedule);
            await _unitOfWork.CompleteAsync();
            
            return true;
        }

        public async Task<IEnumerable<SpecialScheduleDto>> GetActiveSpecialSchedulesAsync()
        {
            var specialSchedules = await _unitOfWork.SpecialSchedules.GetActiveSchedulesAsync();
            return specialSchedules.Select(MapToDto);
        }

        public async Task<IEnumerable<SpecialScheduleDto>> GetAllSpecialSchedulesAsync()
        {
            var specialSchedules = await _unitOfWork.SpecialSchedules.GetAllAsync();
            return specialSchedules.Select(MapToDto);
        }

        public async Task<SpecialScheduleDto> GetSpecialScheduleByIdAsync(int id)
        {
            var specialSchedule = await _unitOfWork.SpecialSchedules.GetByIdAsync(id);
            return specialSchedule == null ? null : MapToDto(specialSchedule);
        }

        public async Task<IEnumerable<SpecialScheduleDto>> GetSpecialSchedulesByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var specialSchedules = await _unitOfWork.SpecialSchedules.GetSchedulesByDateRangeAsync(startDate, endDate);
            return specialSchedules.Select(MapToDto);
        }

        public async Task<IEnumerable<SpecialScheduleDto>> GetSpecialSchedulesByRoomIdAsync(int roomId)
        {
            var specialSchedules = await _unitOfWork.SpecialSchedules.GetSchedulesByRoomIdAsync(roomId);
            return specialSchedules.Select(MapToDto);
        }

        public async Task<SpecialScheduleDto> GetSpecialScheduleForDateAsync(int? roomId, DateTime date)
        {
            IEnumerable<SpecialSchedule> specialSchedules;
            
            if (roomId.HasValue)
            {
                specialSchedules = await _unitOfWork.SpecialSchedules.GetSchedulesByRoomIdAsync(roomId.Value);
            }
            else
            {
                // If no room is specified, get all active schedules (campus-wide)
                specialSchedules = await _unitOfWork.SpecialSchedules.GetActiveSchedulesAsync();
                specialSchedules = specialSchedules.Where(s => !s.RoomId.HasValue && !s.BuildingId.HasValue);
            }
            
            // Find a schedule that applies to this date
            var applicableSchedule = specialSchedules
                .Where(s => s.IsActive && s.StartDate.Date <= date.Date && s.EndDate.Date >= date.Date)
                .OrderByDescending(s => s.StartDate)
                .FirstOrDefault();

            return applicableSchedule == null ? null : MapToDto(applicableSchedule);
        }

        public async Task<SpecialScheduleDto> UpdateSpecialScheduleAsync(SpecialScheduleDto specialScheduleDto)
        {
            var specialSchedule = await _unitOfWork.SpecialSchedules.GetByIdAsync(specialScheduleDto.SpecialScheduleId);
            if (specialSchedule == null)
                return null;

            specialSchedule.Title = specialScheduleDto.Title;
            specialSchedule.Description = specialScheduleDto.Description;
            specialSchedule.StartDate = specialScheduleDto.StartDate;
            specialSchedule.EndDate = specialScheduleDto.EndDate;
            specialSchedule.OpenTime = string.IsNullOrEmpty(specialScheduleDto.OpenTime) ? null : TimeOnly.Parse(specialScheduleDto.OpenTime);
            specialSchedule.CloseTime = string.IsNullOrEmpty(specialScheduleDto.CloseTime) ? null : TimeOnly.Parse(specialScheduleDto.CloseTime);
            specialSchedule.IsClosed = specialScheduleDto.IsClosed;
            specialSchedule.BuildingId = specialScheduleDto.BuildingId;
            specialSchedule.RoomId = specialScheduleDto.RoomId;
            specialSchedule.IsActive = specialScheduleDto.IsActive;
            specialSchedule.UpdatedAt = DateTime.Now;

            await _unitOfWork.SpecialSchedules.UpdateAsync(specialSchedule);
            await _unitOfWork.CompleteAsync();

            return specialScheduleDto;
        }

        private SpecialScheduleDto MapToDto(SpecialSchedule specialSchedule)
        {
            return new SpecialScheduleDto
            {
                SpecialScheduleId = specialSchedule.SpecialScheduleId,
                Title = specialSchedule.Title,
                Description = specialSchedule.Description,
                StartDate = specialSchedule.StartDate,
                EndDate = specialSchedule.EndDate,
                StartDateDisplay = specialSchedule.StartDate.ToString("dd/MM/yyyy"),
                EndDateDisplay = specialSchedule.EndDate.ToString("dd/MM/yyyy"),
                OpenTime = specialSchedule.OpenTime?.ToString("HH:mm"),
                CloseTime = specialSchedule.CloseTime?.ToString("HH:mm"),
                IsClosed = specialSchedule.IsClosed,
                BuildingId = specialSchedule.BuildingId,
                RoomId = specialSchedule.RoomId,
                RoomName = specialSchedule.Room?.RoomName,
                IsActive = specialSchedule.IsActive
            };
        }
    }
}
