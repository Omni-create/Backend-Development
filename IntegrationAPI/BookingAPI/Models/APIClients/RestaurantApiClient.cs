using System.Text;
using System.Text.Json;
using BookingOrchestrationApi.Models.DTOs.Restaurant;

namespace BookingOrchestrationApi.Models.ApiClients.Restaurant
{
    public class RestaurantApiClient : IRestaurantApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<RestaurantApiClient> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public RestaurantApiClient(HttpClient httpClient, ILogger<RestaurantApiClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        public async Task<List<TafelDto>> GetAllTablesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/Tafels");
                response.EnsureSuccessStatusCode();
                
                var content = await response.Content.ReadAsStringAsync();
                var tables = JsonSerializer.Deserialize<List<TafelDto>>(content, _jsonOptions);
                
                return tables ?? new List<TafelDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all tables from restaurant API");
                throw;
            }
        }

        public async Task<List<RestaurantReservationDto>> GetAllReservationsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/Reserveringen");
                response.EnsureSuccessStatusCode();
                
                var content = await response.Content.ReadAsStringAsync();
                var reservations = JsonSerializer.Deserialize<List<RestaurantReservationDto>>(content, _jsonOptions);
                
                return reservations ?? new List<RestaurantReservationDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all reservations from restaurant API");
                throw;
            }
        }

        public async Task<RestaurantReservationDto> CreateReservationAsync(RestaurantBookingRequest request)
        {
            try
            {
                var jsonContent = JsonSerializer.Serialize(request, _jsonOptions);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync("/api/Reserveringen", content);
                response.EnsureSuccessStatusCode();
                
                var responseContent = await response.Content.ReadAsStringAsync();
                var reservation = JsonSerializer.Deserialize<RestaurantReservationDto>(responseContent, _jsonOptions);
                
                return reservation ?? new RestaurantReservationDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating reservation in restaurant API");
                throw;
            }
        }
    }
}