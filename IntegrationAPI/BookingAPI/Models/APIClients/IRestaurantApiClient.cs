using BookingOrchestrationApi.Models.DTOs.Restaurant;

namespace BookingOrchestrationApi.Models.ApiClients.Restaurant
{
    public interface IRestaurantApiClient
    {
        Task<List<TafelDto>> GetAllTablesAsync();
        Task<List<RestaurantReservationDto>> GetAllReservationsAsync();
        Task<RestaurantReservationDto> CreateReservationAsync(RestaurantBookingRequest request);
    }
}