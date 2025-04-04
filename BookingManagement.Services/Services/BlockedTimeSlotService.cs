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
    public class BlockedTimeSlotService : IBlockedTimeSlotService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly string[] _blockTypes = { "Maintenance", "Special Event", "Admin Reserved", "Other" };

        public BlockedTimeSlotService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<BlockedTimeSlotDto> CreateBlockedTimeSlotAsync(BlockedTimeSlotDto blockedTimeSlotDto)
        {
            var blockedTimeSlot = new BlockedTimeSlot
            {
                Reason = blockedTimeSlotDto.Reason,
                StartDate = blockedTimeSlotDto.StartDate,
                EndDate = blockedTimeSlotDto.EndDate,
                TimeSlotId = blockedTimeSlotDto.TimeSlotId,
                RoomId = blockedTimeSlotDto.RoomId,
                BlockType = blockedTimeSlotDto.BlockType,
                IsActive = blockedTimeSlotDto.IsActive,
                CreatedById = blockedTimeSlotDto.CreatedById,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            await _unitOfWork.BlockedTimeSlots.AddAsync(blockedTimeSlot);
            await _unitOfWork.CompleteAsync();

            blockedTimeSlotDto.BlockedTimeSlotId = blockedTimeSlot.BlockedTimeSlotId;
            return blockedTimeSlotDto;
        }

        public async Task<bool> DeleteBlockedTimeSlotAsync(int id)
        {
            var blockedTimeSlot = await _unitOfWork.BlockedTimeSlots.GetByIdAsync(id);
            if (blockedTimeSlot == null)
                return false;

            blockedTimeSlot.IsActive = false;
            blockedTimeSlot.UpdatedAt = DateTime.Now;
            
            await _unitOfWork.BlockedTimeSlots.UpdateAsync(blockedTimeSlot);
            await _unitOfWork.CompleteAsync();
            
            return true;
        }

        public async Task<IEnumerable<BlockedTimeSlotDto>> GetActiveBlockedTimeSlotsAsync()
        {
            var blockedTimeSlots = await _unitOfWork.BlockedTimeSlots.GetActiveBlockedTimeSlotsAsync();
            return blockedTimeSlots.Select(MapToDto);
        }

        public async Task<IEnumerable<BlockedTimeSlotDto>> GetAllBlockedTimeSlotsAsync()
        {
            var blockedTimeSlots = await _unitOfWork.BlockedTimeSlots.GetAllAsync();
            return blockedTimeSlots.Select(MapToDto);
        }

        public async Task<BlockedTimeSlotDto> GetBlockedTimeSlotByIdAsync(int id)
        {
            var blockedTimeSlot = await _unitOfWork.BlockedTimeSlots.GetByIdAsync(id);
            return blockedTimeSlot == null ? null : MapToDto(blockedTimeSlot);
        }

        public async Task<IEnumerable<BlockedTimeSlotDto>> GetBlockedTimeSlotsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var blockedTimeSlots = await _unitOfWork.BlockedTimeSlots.GetBlockedTimeSlotsByDateRangeAsync(startDate, endDate);
            return blockedTimeSlots.Select(MapToDto);
        }

        public async Task<IEnumerable<BlockedTimeSlotDto>> GetBlockedTimeSlotsByRoomIdAsync(int roomId)
        {
            var blockedTimeSlots = await _unitOfWork.BlockedTimeSlots.GetBlockedTimeSlotsByRoomIdAsync(roomId);
            return blockedTimeSlots.Select(MapToDto);
        }

        public async Task<IEnumerable<BlockedTimeSlotDto>> GetBlockedTimeSlotsByTimeSlotIdAsync(int timeSlotId)
        {
            var blockedTimeSlots = await _unitOfWork.BlockedTimeSlots.GetBlockedTimeSlotsByTimeSlotIdAsync(timeSlotId);
            return blockedTimeSlots.Select(MapToDto);
        }

        public async Task<bool> IsTimeSlotBlockedAsync(int timeSlotId, int? roomId, DateTime date)
        {
            return await _unitOfWork.BlockedTimeSlots.IsTimeSlotBlockedAsync(timeSlotId, roomId, date);
        }

        public async Task<BlockedTimeSlotDto> UpdateBlockedTimeSlotAsync(BlockedTimeSlotDto blockedTimeSlotDto)
        {
            var blockedTimeSlot = await _unitOfWork.BlockedTimeSlots.GetByIdAsync(blockedTimeSlotDto.BlockedTimeSlotId);
            if (blockedTimeSlot == null)
                return null;

            blockedTimeSlot.Reason = blockedTimeSlotDto.Reason;
            blockedTimeSlot.StartDate = blockedTimeSlotDto.StartDate;
            blockedTimeSlot.EndDate = blockedTimeSlotDto.EndDate;
            blockedTimeSlot.TimeSlotId = blockedTimeSlotDto.TimeSlotId;
            blockedTimeSlot.RoomId = blockedTimeSlotDto.RoomId;
            blockedTimeSlot.BlockType = blockedTimeSlotDto.BlockType;
            blockedTimeSlot.IsActive = blockedTimeSlotDto.IsActive;
            blockedTimeSlot.UpdatedAt = DateTime.Now;

            await _unitOfWork.BlockedTimeSlots.UpdateAsync(blockedTimeSlot);
            await _unitOfWork.CompleteAsync();

            return blockedTimeSlotDto;
        }

        private BlockedTimeSlotDto MapToDto(BlockedTimeSlot blockedTimeSlot)
        {
            string blockTypeDisplay = blockedTimeSlot.BlockType >= 0 && blockedTimeSlot.BlockType < _blockTypes.Length
                ? _blockTypes[blockedTimeSlot.BlockType]
                : "Unknown";

            return new BlockedTimeSlotDto
            {
                BlockedTimeSlotId = blockedTimeSlot.BlockedTimeSlotId,
                Reason = blockedTimeSlot.Reason,
                StartDate = blockedTimeSlot.StartDate,
                EndDate = blockedTimeSlot.EndDate,
                StartDateDisplay = blockedTimeSlot.StartDate.ToString("dd/MM/yyyy"),
                EndDateDisplay = blockedTimeSlot.EndDate.ToString("dd/MM/yyyy"),
                TimeSlotId = blockedTimeSlot.TimeSlotId,
                TimeSlotDisplay = $"{blockedTimeSlot.TimeSlot?.StartTime.ToString("HH:mm")} - {blockedTimeSlot.TimeSlot?.EndTime.ToString("HH:mm")}",
                RoomId = blockedTimeSlot.RoomId,
                RoomName = blockedTimeSlot.Room?.RoomName,
                BlockType = blockedTimeSlot.BlockType,
                BlockTypeDisplay = blockTypeDisplay,
                IsActive = blockedTimeSlot.IsActive,
                CreatedById = blockedTimeSlot.CreatedById,
                CreatedByName = blockedTimeSlot.CreatedBy?.FullName
            };
        }
    }
}
