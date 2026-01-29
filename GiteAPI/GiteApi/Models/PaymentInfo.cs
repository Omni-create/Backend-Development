namespace GiteApi.Models
{
    public class PaymentInfo
    {
        public int PaymentInfoID { get; set; }
        public int UserID { get; set; }
        public string? LastFourDigits { get; set; }
        public string? BankHolderName { get; set; }
        public string PaymentMethod { get; set; } = null!;
        public string? PaymentToken { get; set; }
        public ICollection<Invoice>? Invoice { get; set; } = new List<Invoice>();
    }
}
