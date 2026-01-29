namespace BookingOrchestrationApi.Models.DTOs
{
    public class CampingSpotDto
    {
        public int CampingID { get; set; }
        public string Regels { get; set; } = string.Empty;
        public decimal Lengte { get; set; }
        public decimal Breedte { get; set; }
        public decimal Stroom { get; set; }
        public bool Huisdieren { get; set; }
        public string Accommodatie { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }
        public decimal PricePerNight { get; set; }
        
        // Calculated availability based on dates
        public List<DateTime> AvailableDates { get; set; } = new();
        public List<DateTime> UnavailableDates { get; set; } = new();
    }
}