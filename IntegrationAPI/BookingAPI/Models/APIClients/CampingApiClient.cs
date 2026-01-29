using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using BookingOrchestrationApi.Models.DTOs;

namespace BookingOrchestrationApi.Models.ApiClients
{
    public class CampingApiClient : ICampingApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CampingApiClient> _logger;
        private readonly string _baseUrl;
        private readonly JsonSerializerOptions _jsonOptions;

        public CampingApiClient(HttpClient httpClient, IConfiguration configuration, ILogger<CampingApiClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _baseUrl = configuration["ExternalApis:CampingApi:BaseUrl"]
                ?? "https://campingef-api-bnfxe6egdfhac5ck.westeurope-01.azurewebsites.net/api/";

            _httpClient.BaseAddress = new Uri(_baseUrl);
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new JsonStringEnumConverter() }
            };
        }

        public async Task<CampingBookingResponse> CreateReservationAsync(CampingBookingRequest request)
        {
            try
            {
                _logger.LogInformation("Creating camping reservation for user {GebruikerID}", request.GebruikerID);

                // Check if the camping spot is available
                var availableSpots = await GetAvailableCampingSpotsAsync(
                    request.CheckInDatum,
                    request.CheckOutDatum,
                    request.Stroom,
                    request.Huisdieren);

                var isAvailable = availableSpots.Any(spot => spot.CampingID == request.AccommodatieID);

                if (!isAvailable)
                {
                    throw new InvalidOperationException($"Camping spot {request.AccommodatieID} is not available for the selected dates");
                }

                // Map to camping API's booking model
                var boekingData = new
                {
                    gebruikerID = request.GebruikerID,
                    datum = DateTime.Now,
                    accommodatieID = request.AccommodatieID,
                    checkInDatum = request.CheckInDatum,
                    checkOutDatum = request.CheckOutDatum,
                    aantalVolwassenen = request.AantalVolwassenen,
                    aantalJongeKinderen = request.AantalJongeKinderen,
                    aantalOudereKinderen = request.AantalOudereKinderen,
                    opmerking = request.Opmerking ?? string.Empty,
                    cancelled = false
                };

                var json = JsonSerializer.Serialize(boekingData, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _logger.LogDebug("Sending booking request to Camping API: {Json}", json);
                var response = await _httpClient.PostAsync("Boeking", content);
                response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Camping API Response: {Response}", responseString);
                
                // Handle if API returns just true/false instead of object
                CampingBookingResponse? bookingResponse = null;
                if (responseString.Equals("true", StringComparison.OrdinalIgnoreCase))
                {
                    // API returned true, create a response object manually
                    bookingResponse = new CampingBookingResponse
                    {
                        BoekingID = 0, // Will be updated from GetReservation if needed
                        GebruikerID = request.GebruikerID,
                        AccommodatieID = request.AccommodatieID,
                        Datum = DateTime.Now,
                        CheckInDatum = request.CheckInDatum,
                        CheckOutDatum = request.CheckOutDatum,
                        AantalVolwassenen = request.AantalVolwassenen,
                        AantalJongeKinderen = request.AantalJongeKinderen,
                        AantalOudereKinderen = request.AantalOudereKinderen,
                        Opmerking = request.Opmerking,
                        Cancelled = false
                    };
                }
                else if (responseString.Equals("false", StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("Camping API rejected the booking request");
                }
                else
                {
                    // Try to deserialize as JSON object
                    bookingResponse = JsonSerializer.Deserialize<CampingBookingResponse>(responseString, _jsonOptions);
                }

                if (bookingResponse == null)
                {
                    throw new Exception("Failed to deserialize booking response");
                }

                // Calculate total cost
                var totalCost = await CalculatePriceAsync(request);

                // Create payment (if payment endpoint exists)
                try
                {
                    await CreatePaymentAsync(bookingResponse.BoekingID, totalCost);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to create payment, but booking was successful");
                }

                // Update the response with additional information
                bookingResponse.Status = "Confirmed";
                bookingResponse.Message = "Camping reservation created successfully";
                bookingResponse.TotalCost = totalCost;

                return bookingResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating camping reservation");
                throw;
            }
        }

        public async Task<CampingBookingResponse> GetReservationAsync(int reservationId, bool includeGebruiker = false, bool includeAccommodatie = false, bool includeBetalingen = false)
        {
            try
            {
                _logger.LogInformation("Getting camping reservation {ReservationId}", reservationId);

                // For getting a specific reservation, use the reservation ID as the first parameter
                // Second parameter: use 0 to not include gebruiker details
                // Third parameter: use 0 to not include accommodatie details
                int gebruikerParam = includeGebruiker ? 1 : 0; // Use 1 to include, 0 to exclude
                int accommodatieParam = includeAccommodatie ? 1 : 0; // Use 1 to include, 0 to exclude

                // The endpoint seems to be: /api/Boeking/{bookingId}/{includeGebruiker}/{includeAccommodatie}
                // But actually looking at your example, it seems to return full details regardless
                // Let me test with just the booking ID first

                // Try with just the booking ID (no inclusion parameters)
                var url = $"Boeking/{reservationId}";
                _logger.LogInformation("Getting booking from URL: {Url}", url);

                var response = await _httpClient.GetAsync(url);

                // If that fails, try with inclusion parameters
                if (!response.IsSuccessStatusCode)
                {
                    url = $"Boeking/{reservationId}/{gebruikerParam}/{accommodatieParam}";
                    _logger.LogInformation("Trying alternative URL: {Url}", url);
                    response = await _httpClient.GetAsync(url);
                }

                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("Booking response JSON: {Json}", json);

                // The response might be a single object or an array
                CampingBookingResponse? booking;

                try
                {
                    // First try to deserialize as a single object
                    booking = JsonSerializer.Deserialize<CampingBookingResponse>(json, _jsonOptions);

                    if (booking == null)
                    {
                        // Try as an array
                        var bookings = JsonSerializer.Deserialize<List<CampingBookingResponse>>(json, _jsonOptions);
                        booking = bookings?.FirstOrDefault(b => b.BoekingID == reservationId);
                    }
                }
                catch (JsonException)
                {
                    // If that fails, try as an array
                    var bookings = JsonSerializer.Deserialize<List<CampingBookingResponse>>(json, _jsonOptions);
                    booking = bookings?.FirstOrDefault(b => b.BoekingID == reservationId);
                }

                if (booking == null)
                {
                    throw new KeyNotFoundException($"Reservation {reservationId} not found");
                }

                // Get payments if requested
                if (includeBetalingen)
                {
                    try
                    {
                        var paymentsResponse = await _httpClient.GetAsync($"Betaling/Boeking/{reservationId}");
                        if (paymentsResponse.IsSuccessStatusCode)
                        {
                            var paymentsJson = await paymentsResponse.Content.ReadAsStringAsync();
                            booking.Betalingen = JsonSerializer.Deserialize<List<BetalingDto>>(paymentsJson, _jsonOptions) ?? new List<BetalingDto>();
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to get payments for booking {ReservationId}", reservationId);
                    }
                }

                booking.Status = booking.Cancelled ? "Cancelled" : "Confirmed";
                booking.Message = "Camping reservation retrieved successfully";

                return booking;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting camping reservation {ReservationId}", reservationId);
                throw;
            }
        }

        public async Task<CampingBookingResponse> UpdateReservationAsync(int reservationId, CampingBookingRequest request)
        {
            try
            {
                _logger.LogInformation("Updating camping reservation {ReservationId}", reservationId);

                // Cancel the existing reservation
                await CancelReservationAsync(reservationId);

                // Create a new reservation with updated details
                return await CreateReservationAsync(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating camping reservation {ReservationId}", reservationId);
                throw;
            }
        }

        public async Task<bool> CancelReservationAsync(int reservationId)
        {
            try
            {
                _logger.LogInformation("Cancelling camping reservation {ReservationId}", reservationId);

                // First, get the booking
                var booking = await GetReservationAsync(reservationId);
                if (booking == null)
                {
                    return false;
                }

                // Update the booking to set cancelled = true
                var updateData = new
                {
                    boekingID = reservationId,
                    gebruikerID = booking.GebruikerID,
                    datum = booking.Datum,
                    accommodatieID = booking.AccommodatieID,
                    checkInDatum = booking.CheckInDatum,
                    checkOutDatum = booking.CheckOutDatum,
                    aantalVolwassenen = booking.AantalVolwassenen,
                    aantalJongeKinderen = booking.AantalJongeKinderen,
                    aantalOudereKinderen = booking.AantalOudereKinderen,
                    opmerking = booking.Opmerking,
                    cancelled = true
                };

                var json = JsonSerializer.Serialize(updateData, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"Boeking/{reservationId}", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling camping reservation {ReservationId}", reservationId);
                return false;
            }
        }

        public async Task<List<CampingSpotDto>> GetAvailableCampingSpotsAsync(
            DateTime startDate, DateTime endDate, int? stroom = null, bool? huisdieren = null)
        {
            try
            {
                _logger.LogInformation("Getting available camping spots from {StartDate} to {EndDate}", startDate, endDate);

                // Get all camping spots with filters
                var allSpots = await GetAllCampingSpotsAsync(stroom, huisdieren);

                // Get all bookings for the date range
                var allBookings = await GetBookingsAsync(null, null);

                // Filter out bookings that are cancelled
                var activeBookings = allBookings.Where(b => !b.Cancelled).ToList();

                // Calculate availability for each spot
                var availableSpots = new List<CampingSpotDto>();

                foreach (var spot in allSpots)
                {
                    // Check if there are any active bookings for this spot that overlap with the requested dates
                    var conflictingBookings = activeBookings.Where(b =>
                    {
                        if (b.AccommodatieID != spot.CampingID) return false;

                        // Check for date overlap
                        return (startDate < b.CheckOutDatum && endDate > b.CheckInDatum);
                    }).ToList();

                    if (!conflictingBookings.Any())
                    {
                        spot.IsAvailable = true;
                        availableSpots.Add(spot);
                    }
                    else
                    {
                        spot.IsAvailable = false;
                        // Calculate unavailable dates for this spot
                        var unavailableDates = new List<DateTime>();
                        foreach (var booking in conflictingBookings)
                        {
                            for (var date = booking.CheckInDatum; date < booking.CheckOutDatum; date = date.AddDays(1))
                            {
                                unavailableDates.Add(date);
                            }
                        }
                        spot.UnavailableDates = unavailableDates.Distinct().ToList();
                    }
                }

                return availableSpots;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available camping spots");
                throw;
            }
        }

        public async Task<List<CampingSpotDto>> GetAllCampingSpotsAsync(int? stroom = null, bool? huisdieren = null)
        {
            try
            {
                // According to your API: /api/Camping/{id}/{stroom}/{huisdieren}
                // To get all spots, use id=0, stroom=0, huisdieren=false (0 for false)
                // stroom: 0 means no filter, any other number filters by stroom value
                // huisdieren: 0 means false (no pets), 1 means true (pets allowed)

                int campingId = 0; // 0 means get all
                int stroomValue = stroom ?? 0; // 0 means no filter
                string huisdierenValue = (huisdieren ?? false).ToString().ToLower();

                var url = $"Camping/{campingId}/{stroomValue}/{huisdierenValue}";
                _logger.LogInformation("Getting camping spots from URL: {Url}", url);

                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to get camping spots. Status: {StatusCode}, Reason: {Reason}",
                        response.StatusCode, response.ReasonPhrase);

                    // Try without parameters
                    var fallbackUrl = "Camping";
                    _logger.LogInformation("Trying fallback URL: {Url}", fallbackUrl);
                    response = await _httpClient.GetAsync(fallbackUrl);
                }

                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("Response JSON: {Json}", json);

                var spots = JsonSerializer.Deserialize<List<CampingSpotDto>>(json, _jsonOptions) ?? new List<CampingSpotDto>();

                // Calculate price for each spot
                foreach (var spot in spots)
                {
                    spot.PricePerNight = CalculateSpotPrice(spot);
                }

                _logger.LogInformation("Retrieved {Count} camping spots", spots.Count);
                return spots;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all camping spots");
                throw;
            }
        }

        public async Task<List<CampingBookingResponse>> GetBookingsAsync(int? gebruikerId = null, int? accommodatieId = null)
        {
            try
            {
                // According to your API: /api/Boeking/{id}/{GebruikerID}/{AccommodatieID}
                // To get all bookings, use id=0, GebruikerID=0, AccommodatieID=0
                // To filter by gebruiker, use actual gebruikerID
                // To filter by accommodatie, use actual accommodatieID

                int boekingsId = 0; // 0 means get all bookings
                int gebruikerIdParam = gebruikerId ?? 0; // 0 means don't filter by user
                int accommodatieIdParam = accommodatieId ?? 0; // 0 means don't filter by accommodation

                var url = $"Boeking/{boekingsId}/{gebruikerIdParam}/{accommodatieIdParam}";
                _logger.LogInformation("Getting bookings from URL: {Url}", url);

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("Bookings response JSON: {Json}", json);

                var bookings = JsonSerializer.Deserialize<List<CampingBookingResponse>>(json, _jsonOptions) ?? new List<CampingBookingResponse>();

                _logger.LogInformation("Retrieved {Count} bookings", bookings.Count);
                return bookings;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting bookings");
                throw;
            }
        }

        public async Task<decimal> CalculatePriceAsync(CampingBookingRequest request)
        {
            try
            {
                // Get camping spot details
                var spots = await GetAllCampingSpotsAsync();
                var spot = spots.FirstOrDefault(s => s.CampingID == request.AccommodatieID);

                if (spot == null)
                {
                    _logger.LogWarning("Camping spot {CampingSpotId} not found, using default pricing", request.AccommodatieID);
                    return CalculateDefaultPrice(request);
                }

                // Calculate base price
                var nights = (request.CheckOutDatum - request.CheckInDatum).Days;
                if (nights < 1) nights = 1; // Minimum 1 night

                var basePricePerNight = spot.PricePerNight;

                // Calculate guest-based pricing
                var adultPrice = 15m; // Price per adult per night
                var olderChildPrice = 10m; // Price per older child per night
                var youngChildPrice = 5m; // Price per young child per night

                var guestCostPerNight = (request.AantalVolwassenen * adultPrice) +
                                        (request.AantalOudereKinderen * olderChildPrice) +
                                        (request.AantalJongeKinderen * youngChildPrice);

                var totalCost = nights * (basePricePerNight + guestCostPerNight);

                // Add electricity fee if applicable
                if (spot.Stroom > 0)
                {
                    totalCost += spot.Stroom * nights;
                }

                // Add pet fee if applicable and requested
                if (spot.Huisdieren && request.Opmerking?.ToLower().Contains("huisdier") == true)
                {
                    totalCost += 5m * nights; // Pet fee per night
                }

                _logger.LogInformation("Calculated price: {TotalCost} for {Nights} nights", totalCost, nights);
                return totalCost;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating price, using default calculation");
                return CalculateDefaultPrice(request);
            }
        }

        private async Task CreatePaymentAsync(int boekingId, decimal totalCost)
        {
            try
            {
                var betalingData = new
                {
                    boekingID = boekingId,
                    type = "Reservation",
                    bedrag = totalCost,
                    methode = "Credit Card",
                    status = "Pending",
                    korting = 0,
                    datumOrigine = DateTime.Now
                };

                var json = JsonSerializer.Serialize(betalingData, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Note: Adjust endpoint if needed based on actual API
                var response = await _httpClient.PostAsync("Betaling", content);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to create payment: {StatusCode}", response.StatusCode);
                }
                else
                {
                    _logger.LogInformation("Payment created successfully for booking {BoekingId}", boekingId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to create payment for booking {BoekingId}", boekingId);
            }
        }

        private decimal CalculateSpotPrice(CampingSpotDto spot)
        {
            // Base price calculation based on spot features
            var basePrice = 20m; // Minimum price

            // Add premium for electricity
            if (spot.Stroom > 0)
            {
                basePrice += 5m;
            }

            // Add premium for pet allowance
            if (spot.Huisdieren)
            {
                basePrice += 3m;
            }

            // Add premium for size
            var size = spot.Lengte * spot.Breedte;
            if (size > 100) // If larger than 100m²
            {
                basePrice += 10m;
            }
            else if (size > 50) // If larger than 50m²
            {
                basePrice += 5m;
            }

            return basePrice;
        }

        private decimal CalculateDefaultPrice(CampingBookingRequest request)
        {
            var nights = (request.CheckOutDatum - request.CheckInDatum).Days;
            if (nights < 1) nights = 1;

            var basePricePerNight = 30m;
            var totalPersons = request.AantalVolwassenen + request.AantalOudereKinderen + request.AantalJongeKinderen;
            var personSurcharge = Math.Max(0, totalPersons - 1) * 5m;

            return (basePricePerNight * nights) + personSurcharge;
        }
    }
}