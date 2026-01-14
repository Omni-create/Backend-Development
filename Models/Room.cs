using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace HotelApi.Models;

public enum Status
{
    Maintenance,
    Occupied,
    Available
}

public partial class Room
{
    public int RoomId { get; set; }
    [Required]
    public int RoomTypeId { get; set; }
    [Required]
    public Status Status { get; set; }

    public virtual RoomType RoomType { get; set; } = null!;

    public Room()
    {
        Status = Status.Available;
    }
}
