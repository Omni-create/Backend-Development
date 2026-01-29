using BookingOrchestrationApi.Models.ApiClients;
using BookingOrchestrationApi.Models.DTOs;

namespace BookingOrchestrationApi.Services
{
    public class CampingBookingService : IBookingService
    {
        private readonly ICampingApiClient _campingApiClient;
        private readonly ILogger<CampingBookingService> _logger;

        public CampingBookingService(ICampingApiClient campingApiClient, ILogger<CampingBookingService> logger)
        {
            _campingApiClient = campingApiClient;
            _logger = logger;
        }

        public async Task<CampingBookingResponse> CreateCampingBookingAsync(CampingBookingRequest request)
        {
            _logger.LogInformation("Creating camping booking for user {GebruikerID}", request.GebruikerID);
            
            if (request.AccommodatieID <= 0)
            {
                throw new ArgumentException("AccommodatieID is required for camping booking");
            }

            return await _campingApiClient.CreateReservationAsync(request);
        }

        public async Task<CampingBookingResponse> GetCampingBookingAsync(
            int bookingId, 
            bool includeGebruiker = false, 
            bool includeAccommodatie = false, 
            bool includeBetalingen = false)
        {
            return await _campingApiClient.GetReservationAsync(
                bookingId, includeGebruiker, includeAccommodatie, includeBetalingen);
        }

        public async Task<List<CampingBookingResponse>> GetAllBookingsAsync(
            int? gebruikerId = null,
            int? accommodatieId = null,
            bool includeGebruiker = false,
            bool includeAccommodatie = false,
            bool includeBetalingen = false)
        {
            _logger.LogInformation("Getting all camping bookings");
            
            // Get basic bookings list
            var bookings = await _campingApiClient.GetBookingsAsync(gebruikerId, accommodatieId);

            // If any includes are requested, fetch full details for each booking
            if (includeGebruiker || includeAccommodatie || includeBetalingen)
            {
                var detailedBookings = new List<CampingBookingResponse>();
                
                foreach (var booking in bookings)
                {
                    try
                    {
                        var detailedBooking = await _campingApiClient.GetReservationAsync(
                            booking.BoekingID,
                            includeGebruiker,
                            includeAccommodatie,
                            includeBetalingen);
                        
                        detailedBookings.Add(detailedBooking);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to get details for booking {BookingId}, using basic data", booking.BoekingID);
                        detailedBookings.Add(booking);
                    }
                }
                
                return detailedBookings;
            }

            return bookings;
        }

        public async Task<CampingBookingResponse> UpdateCampingBookingAsync(int bookingId, CampingBookingRequest request)
        {
            return await _campingApiClient.UpdateReservationAsync(bookingId, request);
        }

        public async Task<bool> CancelCampingBookingAsync(int bookingId)
        {
            _logger.LogInformation("Cancelling camping booking {BookingId}", bookingId);
            return await _campingApiClient.CancelReservationAsync(bookingId);
        }

        public async Task<bool> CancelBookingAsync(int bookingId)
        {
            return await CancelCampingBookingAsync(bookingId);
        }

        public async Task<BookingResponse> GetBookingAsync(int bookingId)
        {
            throw new NotImplementedException("Use GetCampingBookingAsync instead");
        }

        public async Task<BookingResponse> CreateBookingAsync(BookingRequest request)
        {
            throw new NotImplementedException("Use CreateCampingBookingAsync instead");
        }

        public async Task<List<CampingSpotDto>> GetAllCampingSpotsAsync(int? stroom = null, bool? huisdieren = null)
        {
            return await _campingApiClient.GetAllCampingSpotsAsync(stroom, huisdieren);
        }

        public async Task<decimal> CalculatePriceAsync(CampingBookingRequest request)
        {
            return await _campingApiClient.CalculatePriceAsync(request);
        }

        public async Task<List<dynamic>> GetAvailabilityAsync(DateTime startDate, DateTime endDate, object? parameters)
        {
            int? stroom = null;
            bool? huisdieren = null;

            if (parameters != null)
            {
                var properties = parameters.GetType().GetProperties();
                foreach (var prop in properties)
                {
                    if (prop.Name == "stroom") stroom = (int?)prop.GetValue(parameters);
                    if (prop.Name == "huisdieren") huisdieren = (bool?)prop.GetValue(parameters);
                }
            }

            var result = await _campingApiClient.GetAvailableCampingSpotsAsync(startDate, endDate, stroom, huisdieren);
            return result.Select(x => (dynamic)x).ToList();
        }
    }
}