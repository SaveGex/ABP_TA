using Application.DTOs;
using MediatR;

namespace Application.Bookings.Queries.GetBookingById
{
    public record GetBookingByIdQuery(Guid Id) : IRequest<BookingResponseDTO>;
}
