using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelApi.Models
{
    public enum ReservationStatus
    {
        Pending,
        Confirmed,
        CheckedIn,
        CheckedOut,
        Cancelled
    }

    [Table("Reservation")]
    public class Reservation
    {
        [Key]
        [Column("reservationID")]
        public int ReservationId { get; set; }

        [Required]
        [Column("userID")]
        public int UserId { get; set; }

        [Required]
        [Column(TypeName = "date")]
        public DateTime StartDate { get; set; }

        [Required]
        [Column(TypeName = "date")]
        public DateTime EndDate { get; set; }

        [Required]
        public ReservationStatus Status { get; set; } = ReservationStatus.Pending;

        // Navigatie-eigenschappen
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        // Eén reservering kan meerdere facturen hebben
        public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

        public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();

        public virtual ICollection<ReservedExtraOption> ReservedExtraOptions { get; set; } = new HashSet<ReservedExtraOption>();

        public virtual ICollection<ReservedFacility> ReservedFacilities { get; set; } = new HashSet<ReservedFacility>();

        // Hulpmethode om datums te valideren
        public bool ValidateDates()
        {
            var today = DateTime.Today;
            return StartDate >= today &&
                   EndDate > StartDate &&
                   (EndDate - StartDate).TotalDays <= 30; // Max 30 dagen verblijf
        }

        // Bereken aantal nachten
        [NotMapped]
        public int NumberOfNights => (EndDate - StartDate).Days;

        // Verwijderd: public virtual Invoice? Invoice { get; set; } 
        // Want een reservering heeft nu meerdere invoices
    }
}
