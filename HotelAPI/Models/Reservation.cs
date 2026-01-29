using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HotelApi.Models;

public enum ReservationStatus
{
    Pending,
    Confirmed,
    CheckedIn,
    CheckedOut,
    Cancelled
}

public partial class Reservation
{
    public int ReservationId { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    public DateOnly StartDate { get; set; }

    [Required]
    public DateOnly EndDate { get; set; }

    public ReservationStatus Status { get; set; }

    [Required]
    public int RoomId { get; set; }


    [JsonIgnore]
    public virtual User? User { get; set; }
    public virtual ICollection<Invoice> Invoices { get; set; }

    [JsonIgnore]
    public virtual Room? Room { get; set; }


    public Reservation()
    {
        Invoices = new List<Invoice>();
        Status = ReservationStatus.Pending;
    }

    public bool ValidateDates()
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        return StartDate >= today &&
            EndDate > StartDate &&
               (EndDate.ToDateTime(TimeOnly.MinValue) - StartDate.ToDateTime(TimeOnly.MinValue)).Days <= 30; // e.g., max 30-day stay
    }

    public int NumberOfNights =>
        (EndDate.ToDateTime(TimeOnly.MinValue) - StartDate.ToDateTime(TimeOnly.MinValue)).Days;
}