using BookingOrchestrationApi.Models.DTOs;
using HotelApi.Models;

namespace BookingOrchestrationApi.Models.ApiClients.Hotel
{
    public interface IHotelApiClient
    {
        Task<BookingResponse> CreateReservationAsync(BookingRequest request);
        Task<BookingResponse> GetReservationByIdAsync(int reservationId);
        Task<bool> CancelReservationAsync(int reservationId);

        Task<List<Reservation>> GetReservationsAsync();
        
        Task<List<AvailableRoom>> GetAvailableRoomsAsync(DateOnly startDate, DateOnly endDate, int? roomTypeId);
        Task<List<RoomType>> GetRoomTypesAsync();
        Task<List<ExtraOption>> GetExtraOptionsAsync();
        Task<List<Facility>> GetFacilitiesAsync();
    }
}