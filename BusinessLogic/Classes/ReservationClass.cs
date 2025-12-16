namespace BusinessLogic.Classes
{
    public class Reservation
    {
        public int Id { get; set; }
        public int InvoiceId { get; set; }
        public int RoomId { get; set; }
        public int UserId { get; set; }
        public double PaymentAmount { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public string Status { get; set; } // e.g., Confirmed, Cancelled, Completed
    };

    public Reservation Reservation(int id, int invoiceId, int roomId, int userId, double paymentAmount, DateTime checkInDate, DateTime checkOutDate, string status)
    {
        Id = id;
        InvoiceId = invoiceId;
        RoomId = roomId;
        UserId = userId;
        PaymentAmount = paymentAmount;
        CheckInDate = checkInDate;
        CheckOutDate = checkOutDate;
        Status = status;
    }
}