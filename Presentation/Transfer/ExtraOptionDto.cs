namespace Presentation.Transfer
{
    public class ExtraOptionDto
    {
        public int ExtraOptionId { get; set; }
        public string OptionName { get; set; } = null!;
        public decimal Price { get; set; }
    }
}
