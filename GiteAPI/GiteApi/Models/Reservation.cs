namespace GiteApi.Models
{
    public class Reservation
    {
        public int ReservationID { get; set; }
        public int UserID { get; set; }
        public string ReservationType { get; set; } = null!;
        public int? BedroomID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int NumberOfPersons { get; set; }
        public string ReservationStatus { get; set; } = null!;
        public ICollection<Invoice>? Invoice { get; set; } = new List<Invoice>();
    }
}
