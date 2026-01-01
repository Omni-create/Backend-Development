using System;
using System.ComponentModel.DataAnnotations;

namespace Backend_Dev.Transfer;

public class CreateInvoiceDto
{
    [Required]
    public int ReservationId { get; set; }

    public int? PaymentInfoId { get; set; }

    public string? Description { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "TotalCost must be greater than 0")]
    public decimal TotalCost { get; set; }
    
    public string PaymentStatus { get; set; } = "Pending";
}