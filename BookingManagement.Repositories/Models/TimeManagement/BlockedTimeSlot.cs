using System;
using System.Collections.Generic;

namespace BookingManagement.Repositories.Models.TimeManagement
{
    public class BlockedTimeSlot
    {
        public int BlockedTimeSlotId { get; set; }
        
        public string Reason { get; set; }
        
        // Date range for which slots are blocked
        public DateTime StartDate { get; set; }
        
        public DateTime EndDate { get; set; }
        
        // Specific time slot IDs that are blocked (can be multiple)
        public int TimeSlotId { get; set; }
        
        // Navigation property
        public virtual TimeSlot TimeSlot { get; set; }
        
        // Can be specific to a room
        public int? RoomId { get; set; }
        
        // Navigation property
        public virtual Room Room { get; set; }
        
        // BlockType: 1 = Maintenance, 2 = Special Event, 3 = Admin Reserved, etc.
        public int BlockType { get; set; }
        
        public bool IsActive { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime UpdatedAt { get; set; }
        
        // Who created this block
        public int CreatedById { get; set; }
        
        // Navigation property
        public virtual User CreatedBy { get; set; }
    }
}
