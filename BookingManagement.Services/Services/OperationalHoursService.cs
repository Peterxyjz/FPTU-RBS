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
    public class OperationalHoursService : IOperationalHoursService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly string[] _daysOfWeek = { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };

        public OperationalHoursService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<OperationalHoursDto> CreateOperationalHoursAsync(OperationalHoursDto operationalHoursDto)
        {
            var operationalHours = new OperationalHours
            {
                Name = operationalHoursDto.Name,
                Description = operationalHoursDto.Description,
                OpenTime = TimeOnly.Parse(operationalHoursDto.OpenTime),
                CloseTime = TimeOnly.Parse(operationalHoursDto.CloseTime),
                BuildingId = operationalHoursDto.BuildingId,
                RoomId = operationalHoursDto.RoomId,
                DaysOfWeek = string.Join(",", operationalHoursDto.DaysOfWeekList),
                IsActive = operationalHoursDto.IsActive,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            await _unitOfWork.OperationalHours.AddAsync(operationalHours);
            await _unitOfWork.CompleteAsync();

            operationalHoursDto.OperationalHoursId = operationalHours.OperationalHoursId;
            return operationalHoursDto;
        }

        public async Task<bool> DeleteOperationalHoursAsync(int id)
        {
            var operationalHours = await _unitOfWork.OperationalHours.GetByIdAsync(id);
            if (operationalHours == null)
                return false;

            operationalHours.IsActive = false;
            operationalHours.UpdatedAt = DateTime.Now;
            
            await _unitOfWork.OperationalHours.UpdateAsync(operationalHours);
            await _unitOfWork.CompleteAsync();
            
            return true;
        }

        public async Task<IEnumerable<OperationalHoursDto>> GetActiveOperationalHoursAsync()
        {
            var operationalHours = await _unitOfWork.OperationalHours.GetActiveOperationalHoursAsync();
            return operationalHours.Select(MapToDto);
        }

        public async Task<IEnumerable<OperationalHoursDto>> GetAllOperationalHoursAsync()
        {
            var operationalHours = await _unitOfWork.OperationalHours.GetAllAsync();
            return operationalHours.Select(MapToDto);
        }

        public async Task<OperationalHoursDto> GetOperationalHoursByIdAsync(int id)
        {
            var operationalHours = await _unitOfWork.OperationalHours.GetByIdAsync(id);
            return operationalHours == null ? null : MapToDto(operationalHours);
        }

        public async Task<IEnumerable<OperationalHoursDto>> GetOperationalHoursByBuildingIdAsync(int buildingId)
        {
            var operationalHours = await _unitOfWork.OperationalHours.GetOperationalHoursByBuildingIdAsync(buildingId);
            return operationalHours.Select(MapToDto);
        }

        public async Task<IEnumerable<OperationalHoursDto>> GetOperationalHoursByRoomIdAsync(int roomId)
        {
            var operationalHours = await _unitOfWork.OperationalHours.GetOperationalHoursByRoomIdAsync(roomId);
            return operationalHours.Select(MapToDto);
        }

        public async Task<bool> IsOperationalAsync(int roomId, DateTime date, TimeOnly time)
        {
            // Get room operational hours
            var roomHours = await _unitOfWork.OperationalHours.GetOperationalHoursByRoomIdAsync(roomId);
            
            // If no room-specific hours are set, get building hours
            if (!roomHours.Any())
            {
                var room = await _unitOfWork.Rooms.GetByIdAsync(roomId);
                if (room != null && !string.IsNullOrEmpty(room.Building))
                {
                    // We don't have a BuildingId in the Room model, so we'll need to use the actual Building repositories
                    // and services if they exist. For now, we'll assume we can't find building hours.
                }
            }
            
            // Check for special schedules for this date
            var specialSchedule = await GetSpecialScheduleForDateAsync(roomId, date);
            
            // If there's a special schedule and it's marked as closed, the venue is not operational
            if (specialSchedule != null && specialSchedule.IsClosed)
                return false;
                
            // If there's a special schedule with custom hours, check against those
            if (specialSchedule != null && 
                !string.IsNullOrEmpty(specialSchedule.OpenTime) && 
                !string.IsNullOrEmpty(specialSchedule.CloseTime))
            {
                var openTime = TimeOnly.Parse(specialSchedule.OpenTime);
                var closeTime = TimeOnly.Parse(specialSchedule.CloseTime);
                
                return time >= openTime && time <= closeTime;
            }
            
            // Check against regular operational hours
            int dayOfWeek = (int)date.DayOfWeek;
            bool isOperational = false;
            
            foreach (var hours in roomHours.Where(h => h.IsActive))
            {
                var daysOfWeek = hours.DaysOfWeek.Split(',').Select(int.Parse);
                
                if (daysOfWeek.Contains(dayOfWeek) && 
                    time >= hours.OpenTime && 
                    time <= hours.CloseTime)
                {
                    isOperational = true;
                    break;
                }
            }
            
            return isOperational;
        }

        public async Task<OperationalHoursDto> UpdateOperationalHoursAsync(OperationalHoursDto operationalHoursDto)
        {
            var operationalHours = await _unitOfWork.OperationalHours.GetByIdAsync(operationalHoursDto.OperationalHoursId);
            if (operationalHours == null)
                return null;

            operationalHours.Name = operationalHoursDto.Name;
            operationalHours.Description = operationalHoursDto.Description;
            operationalHours.OpenTime = TimeOnly.Parse(operationalHoursDto.OpenTime);
            operationalHours.CloseTime = TimeOnly.Parse(operationalHoursDto.CloseTime);
            operationalHours.BuildingId = operationalHoursDto.BuildingId;
            operationalHours.RoomId = operationalHoursDto.RoomId;
            operationalHours.DaysOfWeek = string.Join(",", operationalHoursDto.DaysOfWeekList);
            operationalHours.IsActive = operationalHoursDto.IsActive;
            operationalHours.UpdatedAt = DateTime.Now;

            await _unitOfWork.OperationalHours.UpdateAsync(operationalHours);
            await _unitOfWork.CompleteAsync();

            return operationalHoursDto;
        }

        private OperationalHoursDto MapToDto(OperationalHours operationalHours)
        {
            var daysOfWeekList = operationalHours.DaysOfWeek?.Split(',')
                .Select(int.Parse)
                .ToList() ?? new List<int>();

            string daysOfWeekDisplay = string.Join(", ", daysOfWeekList.Select(d => _daysOfWeek[d]));

            return new OperationalHoursDto
            {
                OperationalHoursId = operationalHours.OperationalHoursId,
                Name = operationalHours.Name,
                Description = operationalHours.Description,
                OpenTime = operationalHours.OpenTime.ToString("HH:mm"),
                CloseTime = operationalHours.CloseTime.ToString("HH:mm"),
                BuildingId = operationalHours.BuildingId,
                RoomId = operationalHours.RoomId,
                RoomName = operationalHours.Room?.RoomName,
                IsActive = operationalHours.IsActive,
                DaysOfWeekList = daysOfWeekList,
                DaysOfWeekDisplay = daysOfWeekDisplay
            };
        }

        private async Task<SpecialScheduleDto> GetSpecialScheduleForDateAsync(int roomId, DateTime date)
        {
            var specialSchedules = await _unitOfWork.SpecialSchedules.GetSchedulesByRoomIdAsync(roomId);
            var applicableSchedule = specialSchedules
                .Where(s => s.IsActive && s.StartDate.Date <= date.Date && s.EndDate.Date >= date.Date)
                .OrderByDescending(s => s.StartDate) // Prioritize the most recently created schedule
                .FirstOrDefault();

            if (applicableSchedule == null)
                return null;

            return new SpecialScheduleDto
            {
                SpecialScheduleId = applicableSchedule.SpecialScheduleId,
                Title = applicableSchedule.Title,
                Description = applicableSchedule.Description,
                StartDate = applicableSchedule.StartDate,
                EndDate = applicableSchedule.EndDate,
                OpenTime = applicableSchedule.OpenTime?.ToString("HH:mm"),
                CloseTime = applicableSchedule.CloseTime?.ToString("HH:mm"),
                IsClosed = applicableSchedule.IsClosed,
                BuildingId = applicableSchedule.BuildingId,
                RoomId = applicableSchedule.RoomId,
                RoomName = applicableSchedule.Room?.RoomName,
                IsActive = applicableSchedule.IsActive
            };
        }
    }
}
