namespace BookingOrchestrationApi.Services.Hotel
{
    public interface IHotelBookingService : IBookingService
    {
        Task<List<dynamic>> GetBookingsAsync();
        
        Task<List<dynamic>> GetRoomTypesAsync();
        Task<List<dynamic>> GetExtraOptionsAsync();
        Task<List<dynamic>> GetFacilitiesAsync();
    }
}