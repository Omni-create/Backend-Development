using System;
using System.Collections.Generic;

namespace Backend_Dev.Models;

public partial class Room
{
    public int RoomId { get; set; }

    public int RoomTypeId { get; set; }

    public string Status { get; set; } = null!;

    public int? ReservationId { get; set; }

    public virtual Reservation? Reservation { get; set; }

    public virtual RoomType RoomType { get; set; } = null!;
}
