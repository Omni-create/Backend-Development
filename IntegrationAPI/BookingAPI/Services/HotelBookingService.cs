using BookingOrchestrationApi.Models.ApiClients.Hotel;
using BookingOrchestrationApi.Models.DTOs;

namespace BookingOrchestrationApi.Services.Hotel
{
    public class HotelBookingService : IHotelBookingService
    {
        private readonly IHotelApiClient _hotelApiClient;
        private readonly ILogger<HotelBookingService> _logger;

        public HotelBookingService(IHotelApiClient hotelApiClient, ILogger<HotelBookingService> logger)
        {
            _hotelApiClient = hotelApiClient;
            _logger = logger;
        }

        public async Task<BookingResponse> CreateBookingAsync(BookingRequest request)
        {
            _logger.LogInformation("Creating hotel booking for user {UserId}", request.UserId);
            return await _hotelApiClient.CreateReservationAsync(request);
        }

        public async Task<BookingResponse> GetBookingAsync(int bookingId)
        {
            _logger.LogInformation("Getting hotel booking {BookingId}", bookingId);
            return await _hotelApiClient.GetReservationByIdAsync(bookingId);
        }

        public async Task<List<dynamic>> GetBookingsAsync()
        {
            _logger.LogInformation("Getting all hotel bookings");
            var result = await _hotelApiClient.GetReservationsAsync();
            return result.Select(x => (dynamic)x).ToList();
        }

        public async Task<bool> CancelBookingAsync(int bookingId)
        {
            _logger.LogInformation("Cancelling hotel booking {BookingId}", bookingId);
            return await _hotelApiClient.CancelReservationAsync(bookingId);
        }

        public async Task<List<dynamic>> GetAvailabilityAsync(DateTime startDate, DateTime endDate, object? parameters)
        {
            _logger.LogInformation("Getting availability for hotel from {StartDate} to {EndDate}", startDate, endDate);

            int? roomTypeId = null;
            if (parameters != null)
            {
                if (parameters is Dictionary<string, object> dict && dict.ContainsKey("roomTypeId"))
                {
                    roomTypeId = Convert.ToInt32(dict["roomTypeId"]);
                }
                else if (parameters is int id)
                {
                    roomTypeId = id;
                }
            }

            var result = await _hotelApiClient.GetAvailableRoomsAsync(
                DateOnly.FromDateTime(startDate),
                DateOnly.FromDateTime(endDate),
                roomTypeId);
            return result.Select(x => (dynamic)x).ToList();
        }

        public async Task<List<dynamic>> GetRoomTypesAsync()
        {
            var result = await _hotelApiClient.GetRoomTypesAsync();
            return result.Select(x => (dynamic)x).ToList();
        }

        public async Task<List<dynamic>> GetExtraOptionsAsync()
        {
            var result = await _hotelApiClient.GetExtraOptionsAsync();
            return result.Select(x => (dynamic)x).ToList();
        }

        public async Task<List<dynamic>> GetFacilitiesAsync()
        {
            var result = await _hotelApiClient.GetFacilitiesAsync();
            return result.Select(x => (dynamic)x).ToList();
        }
    }
}