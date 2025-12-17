using System;
using System.Collections.Generic;

namespace Backend_Dev.Models;

public partial class RoomType
{
    public int RoomTypeId { get; set; }

    public int Capacity { get; set; }

    public string? Description { get; set; }

    public string Type { get; set; } = null!;

    public decimal PricePerNight { get; set; }

    public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();
}
