using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingManagement.Services.DTOs
{
    public class TimeSlotDto
    {
        public int TimeSlotId { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public bool IsActive { get; set; }
        public string DisplayText { get; set; }
    }
}
