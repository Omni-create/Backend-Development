using Microsoft.AspNetCore.Mvc;
using BookingOrchestrationApi.Models.DTOs;
using BookingOrchestrationApi.Services.Hotel;

namespace BookingOrchestrationApi.Controllers.Hotel
{
    [ApiController]
    [Route("api/booking/hotel")]
    public class HotelBookingController : ControllerBase
    {
        private readonly ILogger<HotelBookingController> _logger;
        private readonly IHotelBookingService _hotelService;

        public HotelBookingController(
            IHotelBookingService hotelService,
            ILogger<HotelBookingController> logger)
        {
            _hotelService = hotelService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<BookingResponse>> CreateBooking([FromBody] BookingRequest request)
        {
            try
            {
                var response = await _hotelService.CreateBookingAsync(request);
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating hotel booking");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpGet("GetAllReservations")]
        public async Task<ActionResult> GetBookings()
        {
            try
            {
                var bookings = await _hotelService.GetBookingsAsync();
                return Ok(bookings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all hotel bookings");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }
        [HttpGet("{bookingId}")]
        public async Task<ActionResult<BookingResponse>> GetBooking(int bookingId)
        {
            try
            {
                var response = await _hotelService.GetBookingAsync(bookingId);
                return Ok(response);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting hotel booking");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpGet("availability")]
        public async Task<ActionResult> CheckAvailability(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] int? roomTypeId = null)
        {
            try
            {
                var availability = await _hotelService.GetAvailabilityAsync(startDate, endDate, roomTypeId);
                return Ok(availability);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking hotel availability");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpGet("roomtypes")]
        public async Task<ActionResult> GetRoomTypes()
        {
            try
            {
                var roomTypes = await _hotelService.GetRoomTypesAsync();
                return Ok(roomTypes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting hotel room types");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpGet("extras")]
        public async Task<ActionResult> GetExtraOptions()
        {
            try
            {
                var extras = await _hotelService.GetExtraOptionsAsync();
                return Ok(extras);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting hotel extra options");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpGet("facilities")]
        public async Task<ActionResult> GetFacilities()
        {
            try
            {
                var facilities = await _hotelService.GetFacilitiesAsync();
                return Ok(facilities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting hotel facilities");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }
    }
}