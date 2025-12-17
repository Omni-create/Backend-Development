using System;
using System.Collections.Generic;

namespace Backend_Dev.Models;

public partial class Reservation
{
    public int ReservationId { get; set; }

    public int UserId { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public string ReservationStatus { get; set; } = null!;

    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();

    public virtual User User { get; set; } = null!;
}
