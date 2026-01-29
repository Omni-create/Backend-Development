using Microsoft.AspNetCore.Mvc;
using BookingOrchestrationApi.Models.DTOs;
using BookingOrchestrationApi.Services;

namespace BookingOrchestrationApi.Controllers
{
    [ApiController]
    [Route("api/booking/gite")]
    public class GiteBookingController : ControllerBase
    {
        private readonly ILogger<GiteBookingController> _logger;
        private readonly GiteBookingService _giteService;

        public GiteBookingController(
            GiteBookingService giteService,
            ILogger<GiteBookingController> logger)
        {
            _giteService = giteService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<BookingResponse>> CreateBooking([FromBody] BookingRequest request)
        {
            try
            {
                _logger.LogInformation("Creating gite booking for user {UserId}", request.UserId);
                
                var response = await _giteService.CreateBookingAsync(request);
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation error creating gite booking");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating gite booking");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpGet("{bookingId}")]
        public async Task<ActionResult<BookingResponse>> GetBooking(int bookingId)
        {
            try
            {
                _logger.LogInformation("Getting gite booking {BookingId}", bookingId);
                
                var response = await _giteService.GetBookingAsync(bookingId);
                return Ok(response);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Gite booking not found");
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting gite booking");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpGet("GetAllReservations")]
        public async Task<ActionResult<List<BookingResponse>>> GetAllBookings([FromQuery] int? userId = null)
        {
            try
            {
                _logger.LogInformation("Getting all gite bookings");
                
                var bookings = await _giteService.GetAllBookingsAsync(userId);
                return Ok(bookings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all gite bookings");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpDelete("{bookingId}")]
        public async Task<ActionResult> CancelBooking(int bookingId)
        {
            try
            {
                _logger.LogInformation("Cancelling gite booking {BookingId}", bookingId);
                
                var result = await _giteService.CancelBookingAsync(bookingId);
                
                if (result)
                {
                    return Ok(new { message = $"Booking {bookingId} cancelled successfully" });
                }
                else
                {
                    return StatusCode(500, new { error = "Failed to cancel booking" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling gite booking");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpGet("availability")]
        public async Task<ActionResult> CheckAvailability(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] int? capacity = null)
        {
            try
            {
                _logger.LogInformation("Checking gite availability from {StartDate} to {EndDate}", startDate, endDate);
                
                if (startDate >= endDate)
                {
                    return BadRequest(new { error = "EndDate must be after StartDate" });
                }

                object? parameters = null;
                if (capacity.HasValue)
                {
                    parameters = new Dictionary<string, object> { { "capacity", capacity.Value } };
                }

                var availability = await _giteService.GetAvailabilityAsync(startDate, endDate, parameters);
                
                return Ok(new 
                { 
                    startDate, 
                    endDate, 
                    capacity,
                    availableBedrooms = availability.Count,
                    bedrooms = availability
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking gite availability");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }
    }
}