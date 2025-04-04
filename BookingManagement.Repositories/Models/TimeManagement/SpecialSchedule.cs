using System;
using System.Collections.Generic;

namespace BookingManagement.Repositories.Models.TimeManagement
{
    public class SpecialSchedule
    {
        public int SpecialScheduleId { get; set; }
        
        public string Title { get; set; }
        
        public string Description { get; set; }
        
        // Date range this schedule applies to
        public DateTime StartDate { get; set; }
        
        public DateTime EndDate { get; set; }
        
        // Special hours for this schedule
        public TimeOnly? OpenTime { get; set; }
        
        public TimeOnly? CloseTime { get; set; }
        
        // If true, facility is closed for this period (holidays, etc.)
        public bool IsClosed { get; set; }
        
        // Can apply to specific building or room - null for all campus
        public int? BuildingId { get; set; }
        
        public int? RoomId { get; set; }
        
        // Navigation property
        public virtual Room Room { get; set; }
        
        public bool IsActive { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime UpdatedAt { get; set; }
    }
}
