namespace BookingOrchestrationApi.Models.DTOs
{
    public class BookingResponse
    {
        public string ServiceType { get; set; } = string.Empty;
        public int ReservationId { get; set; }
        public int? InvoiceId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public decimal TotalCost { get; set; }
    }
}