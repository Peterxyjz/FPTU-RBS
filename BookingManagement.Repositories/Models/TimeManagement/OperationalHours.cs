using System;
using System.Collections.Generic;

namespace BookingManagement.Repositories.Models.TimeManagement
{
    public class OperationalHours
    {
        public int OperationalHoursId { get; set; }
        
        public string Name { get; set; }
        
        public string Description { get; set; }
        
        public TimeOnly OpenTime { get; set; }
        
        public TimeOnly CloseTime { get; set; }
        
        public bool IsActive { get; set; }
        
        // Building or Room reference - null for general hours
        public int? BuildingId { get; set; }
        
        public int? RoomId { get; set; }
        
        // Navigation property
        public virtual Room Room { get; set; }
        
        // Days of week this schedule applies to (comma-separated string of day numbers, 0=Sunday, 1=Monday, etc.)
        public string DaysOfWeek { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime UpdatedAt { get; set; }
    }
}
