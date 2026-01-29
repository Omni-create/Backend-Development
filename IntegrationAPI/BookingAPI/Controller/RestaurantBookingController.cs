using Microsoft.AspNetCore.Mvc;
using BookingOrchestrationApi.Models.DTOs.Restaurant;
using BookingOrchestrationApi.Models.DTOs;
using BookingOrchestrationApi.Services.Restaurant;

namespace BookingOrchestrationApi.Controllers.Restaurant
{
    [ApiController]
    [Route("api/booking/restaurant")]
    public class RestaurantBookingController : ControllerBase
    {
        private readonly ILogger<RestaurantBookingController> _logger;
        private readonly RestaurantBookingService _restaurantService;

        public RestaurantBookingController(
            RestaurantBookingService restaurantService,
            ILogger<RestaurantBookingController> logger)
        {
            _restaurantService = restaurantService;
            _logger = logger;
        }

        [HttpGet("GetAllReservations")]
        public async Task<ActionResult> GetAllReservations(
            [FromQuery] int? userId = null,
            [FromQuery] DateTime? date = null)
        {
            try
            {
                var reservations = await _restaurantService.GetAllReservationsAsync();
                
                if (userId.HasValue)
                {
                    reservations = reservations.Where(r => r.BoekingID == userId.Value).ToList();
                }
                
                if (date.HasValue)
                {
                    reservations = reservations.Where(r => r.DatumTijd.Date == date.Value.Date).ToList();
                }
                
                return Ok(new
                {
                    totalReservations = reservations.Count,
                    activeReservations = reservations.Count(r => !r.IsGeannuleerd && r.RekeningStatus != "Paid"),
                    reservations = reservations
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting restaurant reservations");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpPost]
        public async Task<ActionResult<RestaurantReservationDto>> CreateReservation([FromBody] RestaurantBookingRequest request)
        {
            try
            {
                _logger.LogInformation("Creating restaurant reservation for user {GebruikerID}", request.GebruikerID);
                
                // Get all tables to validate table exists
                var tables = await _restaurantService.GetAllTablesAsync();
                var table = tables.FirstOrDefault(t => t.TafelID == request.TafelID);
                
                if (table == null)
                {
                    return BadRequest(new { error = $"Table with ID {request.TafelID} not found" });
                }
                
                // Check if table is available
                var availability = await _restaurantService.GetAvailabilityAsync(
                    request.DatumTijd, 
                    request.DatumTijd.AddHours(2), 
                    new { seats = request.AantalVolwassenen + request.AantalJongeKinderen + request.AantalOudereKinderen });
                
                var tableAvailability = availability.FirstOrDefault(a => a.TableId == request.TafelID);
                
                if (tableAvailability == null || !tableAvailability.IsAvailable)
                {
                    return BadRequest(new { error = $"Table {request.TafelID} is not available at the requested time" });
                }
                
                // Convert to BookingRequest and create booking
                var bookingRequest = new BookingRequest
                {
                    UserId = request.GebruikerID,
                    TableId = request.TafelID,
                    StartDate = request.DatumTijd,
                    EndDate = request.DatumTijd.AddHours(2), // Assuming 2-hour slot
                    NumberOfPersons = request.AantalVolwassenen,
                    SpecialRequests = request.SpecialeWensen,
                    PaymentMethod = request.BetaalMethode
                };
                
                var response = await _restaurantService.CreateBookingAsync(bookingRequest);
                
                // Get the actual reservation details
                var reservation = await _restaurantService.GetAllReservationsAsync();
                var newReservation = reservation
                    .Where(r => r.BoekingID == request.GebruikerID)
                    .OrderByDescending(r => r.DatumTijd)
                    .FirstOrDefault();
                
                return Ok(newReservation ?? new RestaurantReservationDto { 
                    ReserveringID = response.ReservationId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating restaurant reservation");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpDelete("{bookingId}")]
        public async Task<ActionResult> CancelBooking(int bookingId)
        {
            try
            {
                _logger.LogInformation("Cancelling restaurant booking {BookingId}", bookingId);
                
                var result = await _restaurantService.CancelBookingAsync(bookingId);
                
                if (result)
                {
                    return Ok(new { message = $"Booking {bookingId} cancelled successfully" });
                }
                else
                {
                    return BadRequest(new { error = "Failed to cancel booking" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling restaurant booking");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpGet("availability")]
        public async Task<ActionResult> CheckAvailability(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] int? seats = null)
        {
            try
            {
                _logger.LogInformation("Checking restaurant availability from {StartDate} to {EndDate}", 
                    startDate, endDate);
                
                if (startDate >= endDate)
                {
                    return BadRequest(new { error = "EndDate must be after StartDate" });
                }
                
                object? parameters = null;
                if (seats.HasValue)
                {
                    parameters = new { seats = seats.Value };
                }
                
                var availability = await _restaurantService.GetAvailabilityAsync(startDate, endDate, parameters);
                
                return Ok(new 
                { 
                    startDate, 
                    endDate, 
                    requiredSeats = seats,
                    availableTables = availability.Where(a => a.IsAvailable).Count(),
                    totalTables = availability.Count,
                    tables = availability
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking restaurant availability");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpGet("tables")]
        public async Task<ActionResult> GetAllTables()
        {
            try
            {
                var tables = await _restaurantService.GetAllTablesAsync();
                return Ok(tables);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting restaurant tables");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }
    }
}