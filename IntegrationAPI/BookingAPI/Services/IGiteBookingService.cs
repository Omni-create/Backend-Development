using BookingOrchestrationApi.Models.DTOs;

namespace BookingOrchestrationApi.Services
{
    public interface IBookingService
    {
        // Base interface for common booking operations
        Task<BookingResponse> CreateBookingAsync(BookingRequest request);
        Task<BookingResponse> GetBookingAsync(int bookingId);
        Task<bool> CancelBookingAsync(int bookingId);
        Task<List<dynamic>> GetAvailabilityAsync(DateTime startDate, DateTime endDate, object? parameters);
    }
}