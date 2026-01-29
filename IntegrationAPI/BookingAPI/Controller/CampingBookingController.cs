using Microsoft.AspNetCore.Mvc;
using BookingOrchestrationApi.Models.DTOs;
using BookingOrchestrationApi.Services;

namespace BookingOrchestrationApi.Controllers
{
    [ApiController]
    [Route("api/booking/camping")]
    public class CampingBookingController : ControllerBase
    {
        private readonly ILogger<CampingBookingController> _logger;
        private readonly CampingBookingService _campingService;

        public CampingBookingController(
            CampingBookingService campingService,
            ILogger<CampingBookingController> logger)
        {
            _campingService = campingService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<CampingBookingResponse>> CreateBooking([FromBody] CampingBookingRequest request)
        {
            try
            {
                var response = await _campingService.CreateCampingBookingAsync(request);
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating camping booking");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpGet("{bookingId}")]
        public async Task<ActionResult<CampingBookingResponse>> GetBooking(
            int bookingId,
            [FromQuery] bool includeGebruiker = false,
            [FromQuery] bool includeAccommodatie = false,
            [FromQuery] bool includeBetalingen = false)
        {
            try
            {
                var response = await _campingService.GetCampingBookingAsync(bookingId, includeGebruiker, includeAccommodatie, includeBetalingen);
                return Ok(response);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting camping booking");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpGet("GetAllReservations")]
        public async Task<ActionResult<List<CampingBookingResponse>>> GetAllBookings(
            [FromQuery] int? gebruikerId = null,
            [FromQuery] int? accommodatieId = null,
            [FromQuery] bool includeGebruiker = false,
            [FromQuery] bool includeAccommodatie = false,
            [FromQuery] bool includeBetalingen = false)
        {
            try
            {
                var bookings = await _campingService.GetAllBookingsAsync(
                    gebruikerId, 
                    accommodatieId, 
                    includeGebruiker, 
                    includeAccommodatie, 
                    includeBetalingen);
                return Ok(bookings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all camping bookings");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpPut("{bookingId}")]
        public async Task<ActionResult<CampingBookingResponse>> UpdateBooking(
            int bookingId,
            [FromBody] CampingBookingRequest request)
        {
            try
            {
                var response = await _campingService.UpdateCampingBookingAsync(bookingId, request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating camping booking");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpDelete("{bookingId}")]
        public async Task<ActionResult> CancelBooking(int bookingId)
        {
            try
            {
                var result = await _campingService.CancelBookingAsync(bookingId);
                return result ? Ok(new { message = "Camping booking cancelled successfully" })
                              : BadRequest(new { error = "Failed to cancel booking" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling camping booking");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpGet("availability")]
        public async Task<ActionResult> CheckAvailability(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] int? stroom = null,
            [FromQuery] bool? huisdieren = null)
        {
            try
            {
                var parameters = new { stroom, huisdieren };
                var availability = await _campingService.GetAvailabilityAsync(startDate, endDate, parameters);
                return Ok(availability);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking camping availability");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpGet("spots")]
        public async Task<ActionResult> GetCampingSpots(
            [FromQuery] int? stroom = null,
            [FromQuery] bool? huisdieren = null)
        {
            try
            {
                var spots = await _campingService.GetAllCampingSpotsAsync(stroom, huisdieren);
                return Ok(spots);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting camping spots");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpPost("price")]
        public async Task<ActionResult<decimal>> CalculatePrice([FromBody] CampingBookingRequest request)
        {
            try
            {
                var price = await _campingService.CalculatePriceAsync(request);
                return Ok(new { totalPrice = price });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating camping price");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }
    }
}