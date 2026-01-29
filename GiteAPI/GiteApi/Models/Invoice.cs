namespace GiteApi.Models
{
    public class Invoice
    {
        public int InvoiceID { get; set; }
        public int ReservationID { get; set; }
        public int? PaymentInfoID { get; set; }
        public string? Description { get; set; }
        public decimal TotalCost { get; set; }
        public string PaymentStatus { get; set; } = null!;
        public DateTime IssueDate { get; set; }
    }
}
