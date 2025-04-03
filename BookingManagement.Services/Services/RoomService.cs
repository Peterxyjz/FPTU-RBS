using BookingManagement.Repositories.Interfaces;
using BookingManagement.Repositories.Models;
using BookingManagement.Repositories.UnitOfWork;
using BookingManagement.Services.DTOs;
using BookingManagement.Services.Extensions;
using BookingManagement.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingManagement.Services.Services
{
    public class RoomService : IRoomService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRoomRepository _roomRepository;

        public RoomService(IUnitOfWork unitOfWork, IRoomRepository roomRepository)
        {
            _unitOfWork = unitOfWork;
            _roomRepository = roomRepository;
        }

        public async Task<Room?> AddRoomAsync(Room room)
        {
            room.CreatedAt = DateTime.Now;
            room.UpdatedAt = DateTime.Now;

            await _roomRepository.AddAsync(room);
            await _unitOfWork.CompleteAsync();
            return room;
        }

        public async Task DeleteRoomAsync(int id)
        {
            var room = await _roomRepository.GetByIdAsync(id);
            if (room != null)
            {
                room.IsActive = false;
                room.UpdatedAt = DateTime.Now;
                await _roomRepository.UpdateAsync(room);
                await _unitOfWork.CompleteAsync();
            }
        }

        public async Task<IEnumerable<Room>> GetAllRoomsAsync()
        {
            return await _roomRepository.GetAllAsync();
        }

        public async Task<IEnumerable<Room>> GetAvailableRoomsAsync(DateTime date, int timeSlotId)
        {
            return await _roomRepository.GetAvailableRoomsAsync(date, timeSlotId);
        }

        public async Task<IEnumerable<string>> GetBuildingsAsync()
        {
            var rooms = await _roomRepository.GetAllAsync();
            return rooms
                .Where(r => !string.IsNullOrEmpty(r.Building))
                .Select(r => r.Building)
                .Distinct()
                .OrderBy(b => b);
        }

        public async Task<Room?> GetRoomByIdAsync(int id)
        {
            return await _roomRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Room>> GetRoomsByBuildingAsync(string building)
        {
            return await _roomRepository.GetRoomsByBuildingAsync(building);
        }

        public async Task<IEnumerable<Room>> GetRoomsByStatusAsync(int status)
        {
            return await _roomRepository.GetRoomsByStatusAsync(status);
        }

        public async Task<IEnumerable<Room>> GetRoomsByTypeAsync(string roomType)
        {
            return await _roomRepository.GetRoomsByTypeAsync(roomType);
        }

        public async Task<IEnumerable<string>> GetRoomTypesAsync()
        {
            var rooms = await _roomRepository.GetAllAsync();
            return rooms
                .Select(r => r.RoomType)
                .Distinct()
                .OrderBy(t => t);
        }

        public async Task<bool> IsRoomAvailableAsync(int roomId, DateTime date, int timeSlotId)
        {
            return await _roomRepository.IsRoomAvailableAsync(roomId, date, timeSlotId);
        }

        public async Task UpdateRoomAsync(Room room)
        {
            room.UpdatedAt = DateTime.Now;
            await _roomRepository.UpdateAsync(room);
            await _unitOfWork.CompleteAsync();
        }
    }
}
