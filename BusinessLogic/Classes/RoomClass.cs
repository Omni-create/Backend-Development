namespace BusinessLogic.Classes
{
    public class Room
    {
        public int Id { get; set; }
        public string RoomTypeId { get; set; } // e.g., Single, Double, Suite
        public string Status { get; set; } // e.g., Available, Occupied, Maintenance
    };

    public Room Room(int id, string roomTypeId, string status)
    {
        Id = id;
        RoomTypeId = roomTypeId;
        Status = status;
    }
}