using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingManagement.Services.DTOs
{
    public class OperationalHoursDto
    {
        public int OperationalHoursId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string OpenTime { get; set; }
        public string CloseTime { get; set; }
        public bool IsActive { get; set; }
        public int? BuildingId { get; set; }
        public int? RoomId { get; set; }
        public string RoomName { get; set; }
        public List<int> DaysOfWeekList { get; set; } = new List<int>();
        public string DaysOfWeekDisplay { get; set; }
    }
}
