using Application.DTOs;
using Application.Rooms.Commands.AddServiceToRoom;
using Application.Rooms.Commands.CreateRoom;
using Application.Rooms.Commands.DeleteRoom;
using Application.Rooms.Commands.UpdateRoom;
using Application.Rooms.Queries.SearchAvailableRooms;
using Microsoft.AspNetCore.Mvc;

namespace MainWeb.Controllers
{
    /// <summary>
    /// Manages conference room configurations and availability.
    /// </summary>
    public class RoomsController : ApiControllerBase
    {
        /// <summary>
        /// Registers a new conference room in the system.
        /// </summary>
        /// <param name="command">Room details including name, capacity, base hourly rate, and initial services.</param>
        /// <returns>The unique identifier of the created room.</returns>
        /// <response code="201">Room successfully created.</response>
        /// <response code="400">Validation failed for the request payload.</response>
        [HttpPost]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateRoomCommand command)
        {
            var data = await Mediator.Send(command);
            return Created(string.Empty, data);
        }

        /// <summary>
        /// Updates basic information for an existing room.
        /// </summary>
        /// <param name="id">The unique identifier of the room to update.</param>
        /// <param name="command">Updated room data.</param>
        /// <response code="204">Room updated successfully.</response>
        /// <response code="400">Route ID does not match request body ID or invalid payload.</response>
        /// <response code="404">Room was not found.</response>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRoomCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest("Route ID and payload ID mismatch.");
            }

            await Mediator.Send(command);
            return NoContent();
        }

        /// <summary>
        /// Assigns an available service to a specific conference room.
        /// </summary>
        /// <param name="id">The target room ID.</param>
        /// <param name="command">Service assignment payload.</param>
        /// <response code="204">Service added to room successfully.</response>
        /// <response code="400">Route ID mismatch or validation failure.</response>
        /// <response code="404">Room or Service not found.</response>
        [HttpPost("{id:guid}/services")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddService(Guid id, [FromBody] AddServiceToRoomCommand command)
        {
            if (id != command.RoomId)
            {
                return BadRequest("Route Room ID and payload Room ID mismatch.");
            }

            await Mediator.Send(command);
            return NoContent();
        }

        /// <summary>
        /// Searches for available rooms matching specified date, time range, and minimum seating capacity.
        /// </summary>
        [HttpGet("search", Name = nameof(Search))]
        [ProducesResponseType(typeof(List<RoomResponseDTO>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<RoomResponseDTO>>> Search([FromQuery] SearchAvailableRoomsQuery query)
        {
            return Ok(await Mediator.Send(query));
        }

        /// <summary>
        /// Removes a conference room by its unique identifier.
        /// </summary>
        /// <param name="id">The unique GUID of the room to delete.</param>
        /// <response code="204">Room successfully deleted.</response>
        /// <response code="404">Room not found.</response>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id)
        {
            await Mediator.Send(new DeleteRoomCommand(id));
            return NoContent();
        }
    }
}
