using System.ComponentModel.DataAnnotations;

namespace BookingOrchestrationApi.Models.DTOs.Hotel
{
    public class HotelBookingRequest
    {
        [Required]
        public int UserId { get; set; }
        
        [Required]
        public int RoomTypeId { get; set; }
        
        [Required]
        public DateTime StartDate { get; set; }
        
        [Required]
        public DateTime EndDate { get; set; }
        
        [Required]
        [Range(1, 10)]
        public int NumberOfPersons { get; set; }
        
        public List<int> ExtraOptionIds { get; set; } = new();
        public List<int> FacilityIds { get; set; } = new();
        
        public string? SpecialRequests { get; set; }
    }
}