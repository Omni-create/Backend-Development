namespace Presentation.Transfer
{
    public class RoomTypeDto
    {
        public int RoomTypeId { get; set; }
        public int Capacity { get; set; }
        public string? Description { get; set; }
        public string Type { get; set; } = null!;
        public decimal PricePerNight { get; set; }
    }
}
