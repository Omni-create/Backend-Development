using System.Text.Json;
using BookingOrchestrationApi.Models.ApiClients;
using BookingOrchestrationApi.Models.DTOs;

namespace BookingOrchestrationApi.Services
{
    public class GiteBookingService : IBookingService
    {
        private readonly IGiteApiClient _giteApiClient;
        private readonly ILogger<GiteBookingService> _logger;

        public GiteBookingService(IGiteApiClient giteApiClient, ILogger<GiteBookingService> logger)
        {
            _giteApiClient = giteApiClient;
            _logger = logger;
        }

        public async Task<BookingResponse> CreateBookingAsync(BookingRequest request)
        {
            _logger.LogInformation("Creating gite booking for user {UserId}", request.UserId);
            
            if (!request.BedroomId.HasValue)
            {
                throw new ArgumentException("BedroomId is required for gite booking");
            }

            if (request.StartDate >= request.EndDate)
            {
                throw new ArgumentException("EndDate must be after StartDate");
            }

            if (request.NumberOfPersons < 1)
            {
                throw new ArgumentException("NumberOfPersons must be at least 1");
            }

            return await _giteApiClient.CreateReservationAsync(request);
        }

        public async Task<BookingResponse> GetBookingAsync(int bookingId)
        {
            _logger.LogInformation("Getting gite booking {BookingId}", bookingId);
            return await _giteApiClient.GetReservationAsync(bookingId);
        }

        public async Task<List<BookingResponse>> GetAllBookingsAsync(int? userId = null)
        {
            _logger.LogInformation("Getting all gite bookings");
            return await _giteApiClient.GetAllReservationsAsync(userId);
        }

        public async Task<bool> CancelBookingAsync(int bookingId)
        {
            _logger.LogInformation("Cancelling gite booking {BookingId}", bookingId);
            return await _giteApiClient.CancelReservationAsync(bookingId);
        }

        public async Task<List<dynamic>> GetAvailabilityAsync(DateTime startDate, DateTime endDate, object? parameters)
        {
            _logger.LogInformation("Getting availability for gite from {StartDate} to {EndDate}", startDate, endDate);
            
            int? capacity = null;
            if (parameters != null)
            {
                if (parameters is Dictionary<string, object> dict && dict.ContainsKey("capacity"))
                {
                    capacity = Convert.ToInt32(dict["capacity"]);
                }
                else if (parameters is IDictionary<string, object> idict && idict.ContainsKey("capacity"))
                {
                    capacity = Convert.ToInt32(idict["capacity"]);
                }
            }

            return await _giteApiClient.GetAvailableBedroomsAsync(startDate, endDate, capacity);
        }
    }
}