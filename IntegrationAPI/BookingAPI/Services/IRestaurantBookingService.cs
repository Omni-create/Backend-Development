using BookingOrchestrationApi.Models.DTOs.Restaurant;

namespace BookingOrchestrationApi.Services.Restaurant
{
    public interface IRestaurantBookingService : IBookingService
    {
        Task<List<TafelDto>> GetAllTablesAsync();
        Task<List<RestaurantReservationDto>> GetAllReservationsAsync();
    }
}