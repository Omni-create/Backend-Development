using System;
using System.Collections.Generic;

namespace Backend_Dev.Models;

public partial class PaymentInfo
{
    public int PaymentInfoId { get; set; }

    public int UserId { get; set; }

    public string? LastFourDigits { get; set; }

    public string? BankHolderName { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public string? PaymentToken { get; set; }

    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    public virtual User User { get; set; } = null!;
}
