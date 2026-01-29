using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using BookingOrchestrationApi.Models.DTOs;
using HotelApi.Models;

namespace BookingOrchestrationApi.Models.ApiClients.Hotel
{
    public class HotelApiClient : IHotelApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<HotelApiClient> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public HotelApiClient(HttpClient httpClient, ILogger<HotelApiClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = null,
                Converters = { new JsonStringEnumConverter() }
            };
        }

        public async Task<BookingResponse> CreateReservationAsync(BookingRequest request)
        {
            try
            {
                _logger.LogInformation("Creating hotel reservation for user {UserId}, room type {RoomTypeId}",
                    request.UserId, request.RoomTypeId);

                // Validate request has RoomTypeId
                if (!request.RoomTypeId.HasValue)
                {
                    throw new ArgumentException("RoomTypeId is required for hotel booking");
                }

                // Fetch user data
                var user = await GetUserAsync(request.UserId);
                if (user == null)
                {
                    _logger.LogError("User with ID {UserId} not found", request.UserId);
                    throw new KeyNotFoundException($"User with ID {request.UserId} not found");
                }

                // First, find an available room of the requested type
                var availableRooms = await GetAvailableRoomsAsync(
                    DateOnly.FromDateTime(request.StartDate),
                    DateOnly.FromDateTime(request.EndDate),
                    request.RoomTypeId);

                if (!availableRooms.Any())
                {
                    _logger.LogWarning("No available rooms of type {RoomTypeId} for dates {StartDate} to {EndDate}",
                        request.RoomTypeId, request.StartDate.ToString("yyyy-MM-dd"), request.EndDate.ToString("yyyy-MM-dd"));

                    throw new InvalidOperationException($"No available rooms of type {request.RoomTypeId} for the selected dates");
                }

                // Get first available room
                var availableRoom = availableRooms.First();

                // Create reservation payload with RoomId (required by the Reservation model)
                var reservationData = new Reservation
                {
                    UserId = request.UserId,
                    RoomId = availableRoom.RoomId,
                    StartDate = DateOnly.FromDateTime(request.StartDate),
                    EndDate = DateOnly.FromDateTime(request.EndDate),
                    Status = ReservationStatus.Confirmed  // This will be serialized as integer 1
                };

                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = null
                    // No JsonStringEnumConverter here - Status will be integer (1)
                };

                var json = JsonSerializer.Serialize(reservationData, jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _logger.LogDebug("Sending reservation request to Hotel API: {Json}", json);

                var response = await _httpClient.PostAsync("Reservation", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Hotel API returned error: {StatusCode} - {Error}", response.StatusCode, errorContent);
                    throw new Exception($"Failed to create reservation: {response.StatusCode}");
                }

                var responseString = await response.Content.ReadAsStringAsync();
                // Use original _jsonOptions (WITH JsonStringEnumConverter) for deserialization
                var reservation = JsonSerializer.Deserialize<Reservation>(responseString, _jsonOptions);

                if (reservation == null)
                {
                    _logger.LogError("Failed to deserialize reservation response");
                    throw new Exception("Invalid response from Hotel API");
                }

                // Rest of your code remains the same...
                var room = await GetRoomAsync(availableRoom.RoomId);
                if (room != null)
                {
                    room.Status = RoomStatus.Occupied;
                    await UpdateRoomAsync(room);
                }

                // Create invoice for the reservation
                var totalCost = await CalculateTotalCostAsync(request, availableRoom);
                var invoiceResponse = await CreateInvoiceAsync(reservation.ReservationId, totalCost, request.NumberOfPersons);

                // Add extra options if specified
                if (request.ExtraOptionIds?.Any() == true)
                {
                    await AddExtrasToReservationAsync(reservation.ReservationId, request.ExtraOptionIds);
                }

                // Add facilities if specified
                if (request.FacilityIds?.Any() == true)
                {
                    await AddFacilitiesToReservationAsync(reservation.ReservationId, request.FacilityIds);
                }

                return new BookingResponse
                {
                    ServiceType = "Hotel",
                    ReservationId = reservation.ReservationId,
                    InvoiceId = invoiceResponse?.InvoiceId,
                    Status = "Confirmed",
                    Message = "Hotel reservation created successfully",
                    CreatedAt = DateTime.Now,
                    TotalCost = totalCost
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating hotel reservation");
                throw;
            }
        }

        public async Task<BookingResponse> GetReservationByIdAsync(int reservationId)
        {
            try
            {
                _logger.LogInformation("Getting hotel reservation {ReservationId}", reservationId);

                var response = await _httpClient.GetAsync($"Reservation/{reservationId}");

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        throw new KeyNotFoundException($"Reservation {reservationId} not found");
                    }
                    throw new Exception($"Failed to get reservation: {response.StatusCode}");
                }

                var responseString = await response.Content.ReadAsStringAsync();
                var reservation = JsonSerializer.Deserialize<Reservation>(responseString, _jsonOptions);

                return new BookingResponse
                {
                    ServiceType = "Hotel",
                    ReservationId = reservationId,
                    Status = reservation?.Status.ToString() ?? "Unknown",
                    Message = "Hotel reservation retrieved successfully",
                    CreatedAt = DateTime.Now
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting hotel reservation {ReservationId}", reservationId);
                throw;
            }
        }

        public async Task<bool> CancelReservationAsync(int reservationId)
        {
            try
            {
                _logger.LogInformation("Cancelling hotel reservation {ReservationId}", reservationId);

                // First, get the reservation to update its status
                var getResponse = await _httpClient.GetAsync($"Reservation/{reservationId}");
                if (!getResponse.IsSuccessStatusCode)
                {
                    return false;
                }

                var reservationJson = await getResponse.Content.ReadAsStringAsync();
                var reservation = JsonSerializer.Deserialize<Reservation>(reservationJson, _jsonOptions);

                if (reservation == null)
                {
                    return false;
                }

                // Update the status to Cancelled
                reservation.Status = ReservationStatus.Cancelled;

                // Also update the room status back to Available
                var room = await GetRoomAsync(reservation.RoomId);
                if (room != null)
                {
                    room.Status = RoomStatus.Available;
                    await UpdateRoomAsync(room);
                }

                var json = JsonSerializer.Serialize(reservation, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var updateResponse = await _httpClient.PutAsync($"Reservation/{reservationId}", content);

                return updateResponse.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling hotel reservation {ReservationId}", reservationId);
                return false;
            }
        }

        public async Task<List<AvailableRoom>> GetAvailableRoomsAsync(DateOnly startDate, DateOnly endDate, int? roomTypeId)
        {
            try
            {
                _logger.LogInformation("Getting available rooms from {StartDate} to {EndDate}, room type: {RoomTypeId}",
                    startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"), roomTypeId);

                // Get all rooms
                var roomsResponse = await _httpClient.GetAsync("Room");
                if (!roomsResponse.IsSuccessStatusCode)
                {
                    return new List<AvailableRoom>();
                }

                var roomsJson = await roomsResponse.Content.ReadAsStringAsync();
                var allRooms = JsonSerializer.Deserialize<List<Room>>(roomsJson, _jsonOptions) ?? new List<Room>();

                // Get all reservations
                var reservationsResponse = await _httpClient.GetAsync("Reservation");
                if (!reservationsResponse.IsSuccessStatusCode)
                {
                    // If we can't get reservations, assume all rooms with Available status are available
                    return FilterAndMapRooms(allRooms, roomTypeId, new HashSet<int>());
                }

                var reservationsJson = await reservationsResponse.Content.ReadAsStringAsync();
                var allReservations = JsonSerializer.Deserialize<List<Reservation>>(reservationsJson, _jsonOptions) ?? new List<Reservation>();

                // Find rooms that are occupied in the given date range
                var occupiedRoomIds = new HashSet<int>();
                foreach (var reservation in allReservations)
                {
                    if (reservation.Status != ReservationStatus.Cancelled &&
                        !(endDate <= reservation.StartDate || startDate >= reservation.EndDate))
                    {
                        // Date ranges overlap, room is occupied
                        occupiedRoomIds.Add(reservation.RoomId);
                    }
                }

                return FilterAndMapRooms(allRooms, roomTypeId, occupiedRoomIds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available rooms");
                throw;
            }
        }

        private List<AvailableRoom> FilterAndMapRooms(List<Room> allRooms, int? roomTypeId, HashSet<int> occupiedRoomIds)
        {
            // Find available rooms (not occupied and status is Available)
            var availableRooms = allRooms
                .Where(room =>
                    !occupiedRoomIds.Contains(room.RoomId) &&
                    room.Status == RoomStatus.Available)
                .ToList();

            // Filter by room type if specified
            if (roomTypeId.HasValue)
            {
                availableRooms = availableRooms.Where(r => r.RoomTypeId == roomTypeId.Value).ToList();
            }

            // Get room types for additional info
            var roomTypes = GetRoomTypesAsync().Result; // Note: In production, you might want to avoid .Result

            // Convert to AvailableRoom DTO with room type info
            var result = new List<AvailableRoom>();
            foreach (var room in availableRooms)
            {
                var roomType = roomTypes.FirstOrDefault(rt => rt.RoomTypeId == room.RoomTypeId);

                result.Add(new AvailableRoom
                {
                    RoomId = room.RoomId,
                    RoomTypeId = room.RoomTypeId,
                    RoomType = roomType?.Type ?? "Unknown",
                    Status = room.Status.ToString(),
                    PricePerNight = roomType?.PricePerNight ?? 0,
                    Capacity = roomType?.Capacity ?? 1,
                    Description = roomType?.Description
                });
            }

            _logger.LogInformation("Found {Count} available rooms", result.Count);
            return result;
        }

        public async Task<List<RoomType>> GetRoomTypesAsync()
        {
            try
            {
                _logger.LogInformation("Getting room types from Hotel API");

                var response = await _httpClient.GetAsync("RoomTypes");
                if (!response.IsSuccessStatusCode)
                {
                    return new List<RoomType>();
                }

                var json = await response.Content.ReadAsStringAsync();
                var roomTypes = JsonSerializer.Deserialize<List<RoomType>>(json, _jsonOptions) ?? new List<RoomType>();

                return roomTypes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting room types");
                return new List<RoomType>();
            }
        }

        public async Task<User?> GetUserAsync(int userId)
        {
            try
            {
                _logger.LogInformation("Getting user {UserId} from Hotel API", userId);

                var response = await _httpClient.GetAsync($"Users/{userId}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("User {UserId} not found", userId);
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                var user = JsonSerializer.Deserialize<User>(json, _jsonOptions);

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user {UserId}", userId);
                return null;
            }
        }

        public async Task<Room?> GetRoomAsync(int roomId)
        {
            try
            {
                _logger.LogInformation("Getting room {RoomId} from Hotel API", roomId);

                var response = await _httpClient.GetAsync($"Room/{roomId}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Room {RoomId} not found", roomId);
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                var room = JsonSerializer.Deserialize<Room>(json, _jsonOptions);

                return room;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting room {RoomId}", roomId);
                return null;
            }
        }

        public async Task<bool> UpdateRoomAsync(Room room)
        {
            try
            {
                _logger.LogInformation("Updating room {RoomId}", room.RoomId);

                var json = JsonSerializer.Serialize(room, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"Room/{room.RoomId}", content);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating room {RoomId}", room.RoomId);
                return false;
            }
        }

        public async Task<List<ExtraOption>> GetExtraOptionsAsync()
        {
            try
            {
                _logger.LogInformation("Getting extra options from Hotel API");

                var response = await _httpClient.GetAsync("ExtraOption");
                if (!response.IsSuccessStatusCode)
                {
                    return new List<ExtraOption>();
                }

                var json = await response.Content.ReadAsStringAsync();
                var extraOptions = JsonSerializer.Deserialize<List<ExtraOption>>(json, _jsonOptions) ?? new List<ExtraOption>();

                return extraOptions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting extra options");
                return new List<ExtraOption>();
            }
        }

        public async Task<List<Facility>> GetFacilitiesAsync()
        {
            try
            {
                _logger.LogInformation("Getting facilities from Hotel API");

                var response = await _httpClient.GetAsync("Facility");
                if (!response.IsSuccessStatusCode)
                {
                    return new List<Facility>();
                }

                var json = await response.Content.ReadAsStringAsync();
                var facilities = JsonSerializer.Deserialize<List<Facility>>(json, _jsonOptions) ?? new List<Facility>();

                return facilities;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting facilities");
                return new List<Facility>();
            }
        }

        #region Private Helper Methods

        private async Task<decimal> CalculateTotalCostAsync(BookingRequest request, AvailableRoom availableRoom)
        {
            try
            {
                // Get room type for price
                var roomTypes = await GetRoomTypesAsync();
                var roomType = roomTypes.FirstOrDefault(rt => rt.RoomTypeId == availableRoom.RoomTypeId);

                decimal pricePerNight = roomType?.PricePerNight ?? 100m;
                var nights = (request.EndDate - request.StartDate).Days;
                var roomCost = pricePerNight * nights;

                // Add extra options cost
                decimal extrasCost = 0;
                if (request.ExtraOptionIds?.Any() == true)
                {
                    var extraOptions = await GetExtraOptionsAsync();
                    foreach (var option in extraOptions)
                    {
                        if (request.ExtraOptionIds.Contains(option.ExtraOptionId))
                        {
                            extrasCost += option.Price;
                        }
                    }
                }

                // Add facilities cost
                decimal facilitiesCost = 0;
                if (request.FacilityIds?.Any() == true)
                {
                    var facilities = await GetFacilitiesAsync();
                    foreach (var facility in facilities)
                    {
                        if (request.FacilityIds.Contains(facility.FacilityId))
                        {
                            facilitiesCost += facility.Price;
                        }
                    }
                }

                return roomCost + extrasCost + facilitiesCost;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating total cost, using default calculation");

                // Fallback calculation
                var nights = (request.EndDate - request.StartDate).Days;
                return 100m * nights;
            }
        }

        private async Task<Invoice?> CreateInvoiceAsync(int reservationId, decimal totalCost, int numberOfPersons)
        {
            try
            {
                var invoiceData = new Invoice
                {
                    ReservationId = reservationId,
                    Description = $"Hotel reservation for {numberOfPersons} persons",
                    TotalCost = totalCost,
                    PaymentStatus = PaymentStatus.Pending,
                    IssueDate = DateOnly.FromDateTime(DateTime.Now)
                };

                var json = JsonSerializer.Serialize(invoiceData, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("Invoice", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<Invoice>(responseString, _jsonOptions);
                }

                _logger.LogWarning("Failed to create invoice: {StatusCode}", response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating invoice");
                return null;
            }
        }

        private async Task AddExtrasToReservationAsync(int reservationId, List<int> extraOptionIds)
        {
            try
            {
                foreach (var extraOptionId in extraOptionIds)
                {
                    var data = new ReservedExtraOption
                    {
                        ExtraOptionId = extraOptionId,
                        ReservationId = reservationId
                    };

                    var json = JsonSerializer.Serialize(data, _jsonOptions);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    await _httpClient.PostAsync("ReservedExtraOption", content);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to add some extra options to reservation");
            }
        }

        private async Task AddFacilitiesToReservationAsync(int reservationId, List<int> facilityIds)
        {
            try
            {
                foreach (var facilityId in facilityIds)
                {
                    var data = new ReservedFacility
                    {
                        FacilityId = facilityId,
                        ReservationId = reservationId
                    };

                    var json = JsonSerializer.Serialize(data, _jsonOptions);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    await _httpClient.PostAsync("ReservedFacility", content);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to add some facilities to reservation");
            }
        }
        public async Task<List<Reservation>> GetReservationsAsync()
        {
            try
            {
                _logger.LogInformation("Getting all reservations from Hotel API");

                var response = await _httpClient.GetAsync("Reservation");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to get reservations. Status code: {StatusCode}", response.StatusCode);
                    return new List<Reservation>();
                }

                var json = await response.Content.ReadAsStringAsync();
                var reservations = JsonSerializer.Deserialize<List<Reservation>>(json, _jsonOptions) ?? new List<Reservation>();

                _logger.LogInformation("Retrieved {Count} reservations", reservations.Count);
                return reservations;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all reservations");
                return new List<Reservation>();
            }
        }

        #endregion
    }

    public class AvailableRoom
    {
        public int RoomId { get; set; }
        public int RoomTypeId { get; set; }
        public string RoomType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal PricePerNight { get; set; }
        public int Capacity { get; set; }
        public string? Description { get; set; }
    }
}