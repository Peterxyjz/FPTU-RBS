using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingManagement.Services.DTOs
{
    public class SpecialScheduleDto
    {
        public int SpecialScheduleId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string StartDateDisplay { get; set; }
        public string EndDateDisplay { get; set; }
        public string OpenTime { get; set; }
        public string CloseTime { get; set; }
        public bool IsClosed { get; set; }
        public int? BuildingId { get; set; }
        public int? RoomId { get; set; }
        public string RoomName { get; set; }
        public bool IsActive { get; set; }
    }
}
