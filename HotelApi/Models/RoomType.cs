using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelApi.Models
{
    [Table("RoomType")]
    public class RoomType
    {
        [Key]
        [Column("roomTypeID")]
        public int RoomTypeId { get; set; }

        [Required]
        public int Capacity { get; set; }

        public string? Description { get; set; }

        [Required]
        [StringLength(30)]
        public string Type { get; set; } = null!;

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal PricePerNight { get; set; }

        // Navigatie-eigenschap naar Rooms
        public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();
    }
}
