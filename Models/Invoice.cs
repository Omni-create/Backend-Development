using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
namespace HotelApi.Models;

public enum PaymentStatus
{
    Pending,
    Confirmed,
    Paid,
    Cancelled
}

public partial class Invoice
{
    public int InvoiceId { get; set; }

    [Required]
    public int ReservationId { get; set; }

    public int? PaymentInfoId { get; set; }

    public string? Description { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalCost { get; set; }

    [Required]
    public PaymentStatus PaymentStatus { get; set; }

    [Required]
    public DateOnly IssueDate { get; set; }
    [JsonIgnore]
    public virtual PaymentInfo? PaymentInfo { get; set; }

    [JsonIgnore]
    public virtual Reservation Reservation { get; set; } = null!;

    public Invoice()
    {
        PaymentStatus = PaymentStatus.Pending;
        IssueDate = DateOnly.FromDateTime(DateTime.UtcNow);
    }
}
