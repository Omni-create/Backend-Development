using BookingOrchestrationApi.Models.DTOs;

namespace BookingOrchestrationApi.Models.ApiClients
{
    public interface IGiteApiClient
    {
        Task<BookingResponse> CreateReservationAsync(BookingRequest request);
        Task<BookingResponse> GetReservationAsync(int reservationId);
        Task<List<BookingResponse>> GetAllReservationsAsync(int? userId = null);
        Task<bool> CancelReservationAsync(int reservationId);
        Task<List<dynamic>> GetAvailableBedroomsAsync(DateTime startDate, DateTime endDate, int? capacity);
        
    }
}