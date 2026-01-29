using BookingOrchestrationApi.Models.DTOs;

namespace BookingOrchestrationApi.Services
{
    public interface ICampingBookingService : IBookingService
    {
        Task<CampingBookingResponse> CreateCampingBookingAsync(CampingBookingRequest request);
        Task<CampingBookingResponse> GetCampingBookingAsync(
            int bookingId, 
            bool includeGebruiker = false, 
            bool includeAccommodatie = false, 
            bool includeBetalingen = false);
        
        Task<List<CampingBookingResponse>> GetAllBookingsAsync(
            int? gebruikerId = null,
            int? accommodatieId = null,
            bool includeGebruiker = false,
            bool includeAccommodatie = false,
            bool includeBetalingen = false);
            
        Task<CampingBookingResponse> UpdateCampingBookingAsync(int bookingId, CampingBookingRequest request);
        Task<bool> CancelCampingBookingAsync(int bookingId);
        Task<List<CampingSpotDto>> GetAllCampingSpotsAsync(int? stroom = null, bool? huisdieren = null);
        Task<decimal> CalculatePriceAsync(CampingBookingRequest request);
    }
}