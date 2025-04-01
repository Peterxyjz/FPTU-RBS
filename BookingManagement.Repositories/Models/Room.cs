using System;
using System.Collections.Generic;

namespace BookingManagement.Repositories.Models;

public partial class Room
{
    public int RoomId { get; set; }

    public string RoomName { get; set; } = null!;

    public int Capacity { get; set; }

    public string RoomType { get; set; } = null!;

    public string? Building { get; set; }

    public string? Description { get; set; }

    public string? ImageUrl { get; set; }

    public int Status { get; set; }

    public bool? IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
