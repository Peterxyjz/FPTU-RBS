using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingManagement.Services.DTOs
{
    // Class này để đảm bảo compiler nhận diện namespace DTOs
    public class RoomDetailsDto
    {
        public RoomDto Room { get; set; } = null!;
        public bool HasBookings { get; set; }
        public int TotalBookings { get; set; }
    }
}
