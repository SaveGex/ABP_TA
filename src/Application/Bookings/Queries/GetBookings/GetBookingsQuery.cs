using Application.DTOs;
using MediatR;

namespace Application.Bookings.Queries.GetBookings
{
    public record GetBookingsQuery(
        Guid? RoomId = null,
        DateTime? From = null,
        DateTime? To = null
    ) : IRequest<List<BookingResponseDTO>>;
}
