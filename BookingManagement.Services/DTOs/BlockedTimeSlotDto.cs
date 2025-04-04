using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingManagement.Services.DTOs
{
    public class BlockedTimeSlotDto
    {
        public int BlockedTimeSlotId { get; set; }
        public string Reason { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string StartDateDisplay { get; set; }
        public string EndDateDisplay { get; set; }
        public int TimeSlotId { get; set; }
        public string TimeSlotDisplay { get; set; }
        public int? RoomId { get; set; }
        public string RoomName { get; set; }
        public int BlockType { get; set; }
        public string BlockTypeDisplay { get; set; }
        public bool IsActive { get; set; }
        public int CreatedById { get; set; }
        public string CreatedByName { get; set; }
    }
}
