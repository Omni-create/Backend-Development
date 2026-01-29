using System.ComponentModel.DataAnnotations;

namespace BookingOrchestrationApi.Models.DTOs.Restaurant
{
    public class RestaurantBookingRequest
    {
        [Required]
        public int GebruikerID { get; set; }
        
        [Required]
        public DateTime DatumTijd { get; set; }
        
        [Required]
        [Range(1, 20)]
        public int AantalVolwassenen { get; set; }
        
        [Range(0, 10)]
        public int AantalJongeKinderen { get; set; } = 0;
        
        [Range(0, 10)]
        public int AantalOudereKinderen { get; set; } = 0;
        
        [Required]
        public int TafelID { get; set; }
        
        public string? SpecialeWensen { get; set; }
        public string? BetaalMethode { get; set; } = "Later";
    }
}