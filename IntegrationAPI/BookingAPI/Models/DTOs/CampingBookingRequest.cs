using System.ComponentModel.DataAnnotations;

namespace BookingOrchestrationApi.Models.DTOs
{
    public class CampingBookingRequest
    {
        [Required]
        public int GebruikerID { get; set; }
        
        [Required]
        public int AccommodatieID { get; set; }
        
        [Required]
        public DateTime CheckInDatum { get; set; }
        
        [Required]
        public DateTime CheckOutDatum { get; set; }
        
        [Required]
        [Range(1, 100)]
        public int AantalVolwassenen { get; set; }
        
        [Range(0, 100)]
        public int AantalJongeKinderen { get; set; } = 0;
        
        [Range(0, 100)]
        public int AantalOudereKinderen { get; set; } = 0;
        
        public string? Opmerking { get; set; }
        
        // Optional fields for filtering when getting bookings
        public bool IncludeGebruiker { get; set; } = false;
        public bool IncludeAccommodatie { get; set; } = false;
        public bool IncludeBetalingen { get; set; } = false;
        
        // Optional filter parameters for getting camping spots
        public int? Stroom { get; set; }
        public bool? Huisdieren { get; set; }
    }
}