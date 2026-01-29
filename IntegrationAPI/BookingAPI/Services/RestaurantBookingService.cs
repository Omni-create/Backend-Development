using BookingOrchestrationApi.Models.ApiClients.Restaurant;
using BookingOrchestrationApi.Models.DTOs;
using BookingOrchestrationApi.Models.DTOs.Restaurant;
using Microsoft.Extensions.Logging;

namespace BookingOrchestrationApi.Services.Restaurant
{
    public class RestaurantBookingService : IBookingService
    {
        private readonly IRestaurantApiClient _restaurantApiClient;
        private readonly ILogger<RestaurantBookingService> _logger;

        public RestaurantBookingService(
            IRestaurantApiClient restaurantApiClient,
            ILogger<RestaurantBookingService> logger)
        {
            _restaurantApiClient = restaurantApiClient;
            _logger = logger;
        }

        public async Task<BookingResponse> CreateBookingAsync(BookingRequest request)
        {
            try
            {
                _logger.LogInformation("Creating restaurant booking for user {UserId}", request.UserId);
                
                if (!request.TableId.HasValue)
                {
                    throw new ArgumentException("TableId is required for restaurant booking");
                }

                var restaurantRequest = new RestaurantBookingRequest
                {
                    GebruikerID = request.UserId,
                    DatumTijd = request.StartDate,
                    AantalVolwassenen = request.NumberOfPersons,
                    TafelID = request.TableId.Value,
                    SpecialeWensen = request.SpecialRequests,
                    BetaalMethode = request.PaymentMethod ?? "Later"
                };

                var reservation = await _restaurantApiClient.CreateReservationAsync(restaurantRequest);
                
                return new BookingResponse
                {
                    ServiceType = "Restaurant",
                    ReservationId = reservation.ReserveringID,
                    Status = reservation.IsGeannuleerd ? "Cancelled" : "Confirmed",
                    Message = "Restaurant reservation created successfully",
                    CreatedAt = DateTime.UtcNow,
                    TotalCost = reservation.Rekening?.TotaalBetaald ?? 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating restaurant booking");
                throw;
            }
        }

        public async Task<bool> CancelBookingAsync(int bookingId)
        {
            try
            {
                _logger.LogInformation("Cancelling restaurant booking {BookingId}", bookingId);
                
                // NOTE: This implementation doesn't actually cancel the booking in the API
                // The external API doesn't appear to have a DELETE or cancellation endpoint
                // You may need to implement this differently based on your API's capabilities
                
                _logger.LogWarning("CancelBookingAsync called but external API has no cancellation endpoint");
                throw new NotImplementedException(
                    "Cancellation is not supported - external API has no DELETE or cancellation endpoint");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling restaurant booking");
                throw;
            }
        }

        public async Task<List<dynamic>> GetAvailabilityAsync(DateTime startDate, DateTime endDate, object? parameters)
        {
            try
            {
                _logger.LogInformation("Getting restaurant availability from {StartDate} to {EndDate}", 
                    startDate, endDate);
                
                var allTables = await _restaurantApiClient.GetAllTablesAsync();
                var allReservations = await _restaurantApiClient.GetAllReservationsAsync();
                
                var reservationsInRange = allReservations
                    .Where(r => !r.IsGeannuleerd && 
                                r.DatumTijd.Date >= startDate.Date && 
                                r.DatumTijd.Date <= endDate.Date)
                    .ToList();
                
                int? requiredSeats = null;
                
                if (parameters != null)
                {
                    var properties = parameters.GetType().GetProperties();
                    foreach (var prop in properties)
                    {
                        if (prop.Name.ToLower() == "seats" || prop.Name.ToLower() == "aantalplaatsen")
                            requiredSeats = (int?)prop.GetValue(parameters);
                    }
                }
                
                var availableTables = new List<dynamic>();
                
                foreach (var table in allTables)
                {
                    if (requiredSeats.HasValue && table.AantalPlaatsen < requiredSeats.Value)
                        continue;
                    
                    var tableReservations = reservationsInRange
                        .Where(r => r.TafelID == table.TafelID)
                        .ToList();
                    
                    bool isAvailable = true;
                    string availabilityMessage = "Available";
                    
                    if (tableReservations.Any())
                    {
                        var conflictingReservations = tableReservations
                            .Where(r => Math.Abs((r.DatumTijd - startDate).TotalHours) < 2)
                            .ToList();
                        
                        if (conflictingReservations.Any())
                        {
                            isAvailable = false;
                            var nextReservation = conflictingReservations.First();
                            availabilityMessage = $"Reserved at {nextReservation.DatumTijd:HH:mm}";
                        }
                    }
                    
                    availableTables.Add(new
                    {
                        TableId = table.TafelID,
                        TableNumber = table.Tafelnummer,
                        Seats = table.AantalPlaatsen,
                        IsAvailable = isAvailable,
                        Message = availabilityMessage,
                        ExistingReservations = tableReservations.Select(r => new
                        {
                            Time = r.DatumTijd,
                            People = r.AantalPersonen,
                            Status = r.RekeningStatus
                        }).ToList()
                    });
                }
                
                return availableTables.Select(x => (dynamic)x).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting restaurant availability");
                throw;
            }
        }

        public async Task<List<TafelDto>> GetAllTablesAsync()
        {
            return await _restaurantApiClient.GetAllTablesAsync();
        }

        public async Task<List<RestaurantReservationDto>> GetAllReservationsAsync()
        {
            return await _restaurantApiClient.GetAllReservationsAsync();
        }

        public Task<BookingResponse> GetBookingAsync(int bookingId)
        {
            throw new NotImplementedException();
        }
    }
}