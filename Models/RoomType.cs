using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace HotelApi.Models;

public partial class RoomType
{
    public int RoomTypeId { get; set; }
    [Required]
    public int Capacity { get; set; }

    public string? Description { get; set; }
    [Required]
    public string Type { get; set; } = null!;
    [Required]
    [Column(TypeName = "decimal(10,2)")]
    public decimal PricePerNight { get; set; }

    public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();

    public RoomType()
    {
        Rooms = new List<Room>();
    }
}
