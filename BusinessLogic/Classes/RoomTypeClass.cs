namespace BusinessLogic.Classes
{
    public class RoomType
    {
        public int Id { get; set; }
        public string Name { get; set; } // e.g., Single, Double, Suite
        public int Capacity { get; set; }
        public double PricePerNight { get; set; }
    };

    public RoomType RoomType(int id, string name, int capacity, double pricePerNight)
    {
        Id = id;
        Name = name;
        Capacity = capacity;
        PricePerNight = pricePerNight;
    }
}