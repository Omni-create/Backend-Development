using System;

namespace Presentation.Transfer;

public class InvoiceDto
{
    public int InvoiceId { get; set; }
    public int ReservationId { get; set; }
    public int? PaymentInfoId { get; set; }
    public string? Description { get; set; }
    public decimal TotalCost { get; set; }
    public string PaymentStatus { get; set; } = "Pending";
    public DateOnly IssueDate { get; set; }
}

