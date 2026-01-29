using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using BookingOrchestrationApi.Models.DTOs;

namespace BookingOrchestrationApi.Models.ApiClients
{
    public class GiteApiClient : IGiteApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<GiteApiClient> _logger;
        private readonly string _baseUrl;

        public GiteApiClient(HttpClient httpClient, IConfiguration configuration, ILogger<GiteApiClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _baseUrl = configuration["ExternalApis:GiteApi:BaseUrl"] 
                ?? "http://localhost:5000/api";
            
            _httpClient.BaseAddress = new Uri(_baseUrl);
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        public async Task<BookingResponse> CreateReservationAsync(BookingRequest request)
        {
            try
            {
                var isAvailable = await CheckBedroomAvailabilityAsync(
                    request.BedroomId.Value, 
                    request.StartDate, 
                    request.EndDate
                );

                if (!isAvailable)
                {
                    throw new ArgumentException($"Bedroom ID {request.BedroomId} is not available for the selected dates");
                }

                var reservationData = new
                {
                    userID = request.UserId,
                    reservationType = "Standard",
                    bedroomID = request.BedroomId,
                    startDate = request.StartDate,
                    endDate = request.EndDate,
                    numberOfPersons = request.NumberOfPersons,
                    reservationStatus = "Confirmed"
                };

                var options = new JsonSerializerOptions
                {
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var json = JsonSerializer.Serialize(reservationData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _logger.LogInformation("Creating reservation at {BaseUrl}/Reservation", _baseUrl);

                var response = await _httpClient.PostAsync("Reservation", content);
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to create reservation. Status: {StatusCode}, Response: {Error}", 
                        response.StatusCode, errorContent);
                    throw new Exception($"Failed to create reservation: {response.StatusCode}");
                }

                var responseString = await response.Content.ReadAsStringAsync();
                var reservation = JsonSerializer.Deserialize<JsonElement>(responseString);
                var reservationId = reservation.GetProperty("reservationID").GetInt32();
                
                var invoiceResponse = await CreateInvoiceAsync(reservationId, request);
                
                return new BookingResponse
                {
                    ServiceType = "Gite",
                    ReservationId = reservationId,
                    InvoiceId = invoiceResponse?.GetProperty("invoiceID").GetInt32(),
                    Status = "Confirmed",
                    Message = "Gite reservation created successfully",
                    CreatedAt = DateTime.UtcNow,
                    TotalCost = CalculateTotalCost(request)
                };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error creating gite reservation");
                throw new Exception($"Failed to create reservation: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating gite reservation");
                throw;
            }
        }

        public async Task<BookingResponse> GetReservationAsync(int reservationId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"Reservation/{reservationId}");
                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        throw new KeyNotFoundException($"Reservation with ID {reservationId} not found");
                    }
                    
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to get reservation. Status: {StatusCode}, Response: {Error}", 
                        response.StatusCode, errorContent);
                    throw new Exception($"Failed to get reservation: {response.StatusCode}");
                }

                var responseString = await response.Content.ReadAsStringAsync();
                var reservation = JsonSerializer.Deserialize<JsonElement>(responseString);

                decimal totalCost = 0;
                if (reservation.TryGetProperty("invoice", out var invoices) && invoices.GetArrayLength() > 0)
                {
                    totalCost = invoices[0].GetProperty("totalCost").GetDecimal();
                }

                return new BookingResponse
                {
                    ServiceType = "Gite",
                    ReservationId = reservationId,
                    InvoiceId = reservation.TryGetProperty("invoice", out var invs) && invs.GetArrayLength() > 0 
                        ? invs[0].GetProperty("invoiceID").GetInt32() 
                        : null,
                    Status = reservation.GetProperty("reservationStatus").GetString() ?? "Unknown",
                    Message = "Reservation retrieved successfully",
                    CreatedAt = DateTime.UtcNow,
                    TotalCost = totalCost
                };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error getting gite reservation {ReservationId}", reservationId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting gite reservation {ReservationId}", reservationId);
                throw;
            }
        }

        public async Task<List<BookingResponse>> GetAllReservationsAsync(int? userId = null)
        {
            try
            {
                _logger.LogInformation("Getting all gite reservations");

                var response = await _httpClient.GetAsync("Reservation");
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to get reservations. Status: {StatusCode}", response.StatusCode);
                    throw new Exception($"Failed to get reservations: {response.StatusCode}");
                }

                var responseString = await response.Content.ReadAsStringAsync();
                var reservations = JsonSerializer.Deserialize<List<JsonElement>>(responseString);

                if (reservations == null)
                    return new List<BookingResponse>();

                var bookingResponses = new List<BookingResponse>();

                foreach (var reservation in reservations)
                {
                    try
                    {
                        var reservationId = reservation.GetProperty("reservationID").GetInt32();
                        var reservationUserId = reservation.GetProperty("userID").GetInt32();

                        // Filter by userId if provided
                        if (userId.HasValue && reservationUserId != userId.Value)
                            continue;

                        decimal totalCost = 0;
                        int? invoiceId = null;

                        if (reservation.TryGetProperty("invoice", out var invoices) && invoices.GetArrayLength() > 0)
                        {
                            invoiceId = invoices[0].GetProperty("invoiceID").GetInt32();
                            totalCost = invoices[0].GetProperty("totalCost").GetDecimal();
                        }

                        var bookingResponse = new BookingResponse
                        {
                            ServiceType = "Gite",
                            ReservationId = reservationId,
                            InvoiceId = invoiceId,
                            Status = reservation.GetProperty("reservationStatus").GetString() ?? "Unknown",
                            Message = "Reservation retrieved successfully",
                            CreatedAt = DateTime.UtcNow,
                            TotalCost = totalCost
                        };

                        bookingResponses.Add(bookingResponse);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to parse reservation, skipping");
                    }
                }

                _logger.LogInformation("Retrieved {Count} gite reservations", bookingResponses.Count);
                return bookingResponses;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all gite reservations");
                throw;
            }
        }

        public async Task<bool> CancelReservationAsync(int reservationId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"Reservation/{reservationId}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling gite reservation {ReservationId}", reservationId);
                return false;
            }
        }

        public async Task<List<dynamic>> GetAvailableBedroomsAsync(DateTime startDate, DateTime endDate, int? capacity)
        {
            try
            {
                var response = await _httpClient.GetAsync("Bedroom");
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to get bedrooms. Status: {StatusCode}", response.StatusCode);
                    return new List<dynamic>();
                }

                var responseString = await response.Content.ReadAsStringAsync();
                var allBedrooms = JsonSerializer.Deserialize<List<JsonElement>>(responseString);

                if (allBedrooms == null)
                    return new List<dynamic>();

                var reservationsResponse = await _httpClient.GetAsync("Reservation");
                var reservationsString = await reservationsResponse.Content.ReadAsStringAsync();
                var allReservations = JsonSerializer.Deserialize<List<JsonElement>>(reservationsString);

                var availableBedrooms = new List<dynamic>();

                foreach (var bedroom in allBedrooms)
                {
                    var bedroomCapacity = bedroom.GetProperty("capacity").GetInt32();
                    if (capacity.HasValue && bedroomCapacity < capacity.Value)
                        continue;

                    var availabilityStatus = bedroom.GetProperty("availabilityStatus").GetString();
                    if (availabilityStatus?.ToLower() != "available")
                        continue;

                    var bedroomId = bedroom.GetProperty("bedroomID").GetInt32();
                    var isAvailable = true;

                    if (allReservations != null)
                    {
                        foreach (var reservation in allReservations)
                        {
                            var reservationStatus = reservation.GetProperty("reservationStatus").GetString();
                            if (reservationStatus?.ToLower() == "cancelled")
                                continue;

                            var resBedroomId = reservation.GetProperty("bedroomID").GetInt32();
                            if (resBedroomId != bedroomId)
                                continue;

                            var resStartDate = reservation.GetProperty("startDate").GetDateTime();
                            var resEndDate = reservation.GetProperty("endDate").GetDateTime();

                            if (!(resEndDate <= startDate || resStartDate >= endDate))
                            {
                                isAvailable = false;
                                break;
                            }
                        }
                    }

                    if (isAvailable)
                    {
                        var availableBedroom = new
                        {
                            bedroomID = bedroomId,
                            bedroomName = bedroom.GetProperty("bedroomName").GetString(),
                            capacity = bedroomCapacity,
                            description = bedroom.GetProperty("description").GetString(),
                            availabilityStatus = "Available"
                        };
                        availableBedrooms.Add(availableBedroom);
                    }
                }

                return availableBedrooms;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available bedrooms");
                throw;
            }
        }

        private async Task<bool> CheckBedroomAvailabilityAsync(int bedroomId, DateTime startDate, DateTime endDate)
        {
            try
            {
                var availableBedrooms = await GetAvailableBedroomsAsync(startDate, endDate, null);
                return availableBedrooms.Any(b =>
                {
                    dynamic bDynamic = b;
                    return bDynamic.bedroomID == bedroomId;
                });
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async Task<JsonElement?> CreateInvoiceAsync(int reservationId, BookingRequest request)
        {
            try
            {
                var totalCost = CalculateTotalCost(request);
                
                var invoiceData = new
                {
                    reservationID = reservationId,
                    description = $"Reservation for {request.NumberOfPersons} persons",
                    totalCost = totalCost,
                    paymentStatus = "Pending",
                    issueDate = DateTime.UtcNow
                };

                var json = JsonSerializer.Serialize(invoiceData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("Invoice", content);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to create invoice for reservation {ReservationId}", reservationId);
                    return null;
                }

                var responseString = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<JsonElement>(responseString);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating invoice for reservation {ReservationId}", reservationId);
                return null;
            }
        }

        private decimal CalculateTotalCost(BookingRequest request)
        {
            var nights = (request.EndDate - request.StartDate).Days;
            if (nights <= 0) nights = 1;

            var baseRate = 100m;
            var personSurcharge = Math.Max(0, request.NumberOfPersons - 2) * 20m * nights;
            
            return (baseRate * nights) + personSurcharge;
        }
    }
}