using Application.Bookings.Commands.CreateBooking;
using Application.Bookings.Queries.GetBookingById;
using Application.Bookings.Queries.GetBookings;
using Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace MainWeb.Controllers
{
    /// <summary>
    /// Manages room bookings and reservations.
    /// </summary>
    public class BookingsController : ApiControllerBase
    {
        /// <summary>
        /// Creates a new room booking reservation.
        /// </summary>
        /// <param name="command">Booking parameters including room ID, time slot, and optional add-on services.</param>
        /// <returns>The unique identifier of the newly created booking.</returns>
        /// <response code="201">Booking successfully created.</response>
        /// <response code="400">Invalid input parameters or validation failure.</response>
        /// <response code="409">The room is already booked for the specified time slot.</response>
        [HttpPost]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateBookingCommand command)
        {
            var data = await Mediator.Send(command);
            return Created(nameof(GetById), data);
        }

        /// <summary>
        /// Retrieves detailed information about a booking by its unique identifier.
        /// </summary>
        /// <param name="id">The unique GUID of the booking.</param>
        /// <response code="200">Booking details found and returned.</response>
        /// <response code="404">Booking with the specified ID was not found.</response>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(BookingResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BookingResponseDTO>> GetById(Guid id)
        {
            return Ok(await Mediator.Send(new GetBookingByIdQuery(id)));
        }

        /// <summary>
        /// Retrieves a list of bookings with optional filtering by room ID and date range.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<BookingResponseDTO>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<BookingResponseDTO>>> Get([FromQuery] GetBookingsQuery query)
        {
            return Ok(await Mediator.Send(query));
        }
    }
}
