using BookingManagement.Repositories.Models;
using BookingManagement.Services.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingManagement.Services.Extensions
{
    public static class RoomExtensions
    {
        public static RoomDto ToDto(this Room room)
        {
            return new RoomDto
            {
                RoomNumber = room.RoomId,
                RoomName = room.RoomName,
                Capacity = room.Capacity,
                RoomType = room.RoomType,
                Building = room.Building,
                Description = room.Description,
                ImageUrl = room.ImageUrl,
                Status = room.Status,
                IsActive = room.IsActive ?? true,
                CreatedAt = room.CreatedAt,
                UpdatedAt = room.UpdatedAt
            };
        }

        public static Room ToEntity(this RoomDto dto)
        {
            return new Room
            {
                RoomId = dto.RoomNumber,
                RoomName = dto.RoomName,
                Capacity = dto.Capacity,
                RoomType = dto.RoomType,
                Building = dto.Building,
                Description = dto.Description,
                ImageUrl = dto.ImageUrl,
                Status = dto.Status,
                IsActive = dto.IsActive,
                CreatedAt = dto.CreatedAt,
                UpdatedAt = dto.UpdatedAt
            };
        }

        public static string GetRoomStatusName(int status)
        {
            return status switch
            {
                1 => "Hoạt động",
                2 => "Bảo trì",
                _ => "Không xác định"
            };
        }

        public static Dictionary<int, string> GetRoomStatusList()
        {
            return new Dictionary<int, string>
            {
                { 1, "Hoạt động" },
                { 2, "Bảo trì" }
            };
        }

        public static List<string> GetDefaultRoomTypes()
        {
            return new List<string>
            {
                "Phòng học",
                "Phòng máy tính",
                "Phòng hội thảo",
                "Phòng họp",
                "Phòng thực hành",
                "Phòng đa năng"
            };
        }

        public static List<string> GetDefaultBuildings()
        {
            return new List<string>
            {
                "Alpha",
                "Beta",
                "Gamma",
                "Delta"
            };
        }
    }
}
