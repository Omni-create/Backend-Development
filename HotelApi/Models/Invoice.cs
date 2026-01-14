using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelApi.Models
{
    public enum PaymentStatus
    {
        Pending,
        Confirmed,
        Paid,
        Cancelled
    }

    [Table("Invoice")]
    public class Invoice
    {
        [Key]
        [Column("invoiceID")]
        public int InvoiceId { get; set; }

        [Required]
        [Column("reservationID")]
        public int ReservationId { get; set; }

        [Column("paymentInfoID")]
        public int? PaymentInfoId { get; set; }

        public string? Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalCost { get; set; }

        [Required]
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

        [Required]
        [Column(TypeName = "date")]
        public DateTime IssueDate { get; set; } = DateTime.UtcNow.Date;

        // Navigatie-eigenschappen
        [ForeignKey("PaymentInfoId")]
        public virtual PaymentInfo? PaymentInfo { get; set; }

        [ForeignKey("ReservationId")]
        public virtual Reservation Reservation { get; set; } = null!;
    }
}
