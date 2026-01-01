namespace Presentation.Transfer
{
    public class CreatePaymentInfoDto
    {
        public int UserId { get; set; }
        public string? LastFourDigits { get; set; }
        public string? BankHolderName { get; set; }
        public string PaymentMethod { get; set; } = null!;
        public string? PaymentToken { get; set; }
    }
}
