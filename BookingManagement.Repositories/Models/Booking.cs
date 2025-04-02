using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BookingManagement.Repositories.Models;

public partial class Booking
{
    public int BookingId { get; set; }

    public int UserId { get; set; }

    public int RoomId { get; set; }

    [Required(ErrorMessage = "Booking date is required.")]
    public DateOnly BookingDate { get; set; }

    public int TimeSlotId { get; set; }

    public int Status { get; set; }

    public string? RejectReason { get; set; }

    public bool? IsRecurring { get; set; }

    public DateOnly? EndRecurringDate { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual Room Room { get; set; } = null!;

    public virtual TimeSlot TimeSlot { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
