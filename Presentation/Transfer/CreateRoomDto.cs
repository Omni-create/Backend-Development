namespace Presentation.Transfer
{
    public class CreateRoomDto
    {
        public int RoomTypeId { get; set; }
        public string Status { get; set; } = null!;
    }
}
