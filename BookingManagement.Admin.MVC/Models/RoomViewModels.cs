using BookingManagement.Services.DTOs;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BookingManagement.Admin.MVC.Models
{
    // Class này dùng để đảm bảo rằng các tham chiếu đến RoomDto được nhận diện đúng
    public class RoomViewModels
    {
        // Tham chiếu đến RoomDto từ Services project
        public RoomDto RoomDto { get; set; }

        // Danh sách phòng
        public List<RoomDto> Rooms { get; set; }
    }
}
