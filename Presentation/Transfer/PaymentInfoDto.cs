namespace Presentation.Transfer
{
    public class PaymentInfoDto
    {
        public int PaymentInfoId { get; set; }
        public int UserId { get; set; }
        public string? LastFourDigits { get; set; }
        public string? BankHolderName { get; set; }
        public string PaymentMethod { get; set; } = null!;
        public string? PaymentToken { get; set; }
    }
}
