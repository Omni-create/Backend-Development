using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelApi.Models
{
    [Table("ReservedExtraOptions")]
    public class ReservedExtraOption
    {
        public int ExtraOptionId { get; set; }
        public int ReservationId { get; set; }

        public virtual ExtraOption ExtraOption { get; set; } = null!;
        public virtual Reservation Reservation { get; set; } = null!;
    }
}
