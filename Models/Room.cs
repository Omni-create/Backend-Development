using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
namespace HotelApi.Models;

public enum RoomStatus
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
    public RoomStatus Status { get; set; } = RoomStatus.Available;

    [JsonIgnore]
    public virtual RoomType RoomType { get; set; } = null!;

    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();

    public Room()
    {
        Status = RoomStatus.Available;
    }
}
