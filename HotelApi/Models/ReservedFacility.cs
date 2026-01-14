using System.ComponentModel.DataAnnotations.Schema;

namespace HotelApi.Models
{
    [Table("ReservedFacilities")]
    public class ReservedFacility
    {
        [Column("facilityID")]
        public int FacilityId { get; set; }

        [Column("reservationID")]
        public int ReservationId { get; set; }

        [ForeignKey("FacilityId")]
        public virtual Facility Facility { get; set; } = null!;

        [ForeignKey("ReservationId")]
        public virtual Reservation Reservation { get; set; } = null!;
    }
}
