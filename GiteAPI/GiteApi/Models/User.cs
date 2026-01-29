namespace GiteApi.Models
{
    public class User
    {
        public int UserID { get; set; }
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
        public DateTime CreatedDate { get; set; }
        public string UserRole { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public ICollection<Reservation>? Reservations { get; set; } = new List<Reservation>();
        public ICollection<PaymentInfo>? PaymentInfo { get; set; } = new List<PaymentInfo>();
    }
}