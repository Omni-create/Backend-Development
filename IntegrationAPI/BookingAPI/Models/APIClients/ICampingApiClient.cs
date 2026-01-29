using BookingOrchestrationApi.Models.DTOs;

namespace BookingOrchestrationApi.Models.ApiClients
{
    public interface ICampingApiClient
    {
        // Booking operations
        Task<CampingBookingResponse> CreateReservationAsync(CampingBookingRequest request);
        Task<CampingBookingResponse> GetReservationAsync(int reservationId, bool includeGebruiker = false, bool includeAccommodatie = false, bool includeBetalingen = false);
        Task<CampingBookingResponse> UpdateReservationAsync(int reservationId, CampingBookingRequest request);
        Task<bool> CancelReservationAsync(int reservationId);
        
        // Availability and spot operations
        Task<List<CampingSpotDto>> GetAvailableCampingSpotsAsync(
            DateTime startDate, 
            DateTime endDate, 
            int? stroom = null, 
            bool? huisdieren = null);
        Task<List<CampingSpotDto>> GetAllCampingSpotsAsync(int? stroom = null, bool? huisdieren = null);
        Task<List<CampingBookingResponse>> GetBookingsAsync(int? gebruikerId = null, int? accommodatieId = null);
        
        // Utility methods
        Task<decimal> CalculatePriceAsync(CampingBookingRequest request);
    }
}