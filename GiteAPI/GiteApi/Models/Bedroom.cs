namespace GiteApi.Models
{
    public class Bedroom
    {
        public int BedroomID { get; set; }
        public string BedroomName { get; set; } = null!;
        public int Capacity { get; set; }
        public string? Description { get; set; }
        public string AvailabilityStatus { get; set; } = "Available";
        public ICollection<Reservation>? Reservation { get; set; } = new List<Reservation>();
    }
}
