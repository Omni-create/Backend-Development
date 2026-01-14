using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

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

    public virtual User User { get; set; } = null!;
    public virtual ICollection<Invoice> Invoices { get; set; }
    
    public virtual ICollection<Room> Rooms { get; set; }
    
    public virtual ICollection<ReservedExtraOption> ReservedExtraOptions { get; private set; }
    public virtual ICollection<ReservedFacility> ReservedFacilities { get; private set; }

    public Reservation()
    {
        Invoices = new List<Invoice>();
        Rooms = new List<Room>();
        ReservedExtraOptions = new HashSet<ReservedExtraOption>();
        ReservedFacilities = new HashSet<ReservedFacility>();
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