using System;
using System.Collections.Generic;

namespace Backend_Dev.Models;

public partial class Invoice
{
    public int InvoiceId { get; set; }

    public int ReservationId { get; set; }

    public int? PaymentInfoId { get; set; }

    public string? Description { get; set; }

    public decimal TotalCost { get; set; }

    public string PaymentStatus { get; set; } = null!;

    public DateOnly? IssueDate { get; set; }

    public virtual PaymentInfo? PaymentInfo { get; set; }

    public virtual Reservation Reservation { get; set; } = null!;
}
