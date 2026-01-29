using System.ComponentModel.DataAnnotations;
using BookingOrchestrationApi.Models.DTOs;

namespace BookingOrchestrationApi.Models.DTOs
{
    public class BookingRequest
    {
        [Required]
        public int UserId { get; set; }
        
        public int? BedroomId { get; set; }      // For Gite
        public int? RoomTypeId { get; set; }     // For Hotel
        public int? TableId { get; set; }        // For Restaurant
        
        [Required]
        public DateTime StartDate { get; set; }
        
        [Required]
        public DateTime EndDate { get; set; }
        
        [Required]
        [Range(1, 100)]
        public int NumberOfPersons { get; set; }
        
        public List<int> ExtraOptionIds { get; set; } = new();
        public List<int> FacilityIds { get; set; } = new();
        
        public string? SpecialRequests { get; set; }
        public string? PaymentMethod { get; set; }
        
        public string? ContactName { get; set; }
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
    }
}