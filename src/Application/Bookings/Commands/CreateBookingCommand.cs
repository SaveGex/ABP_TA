using Application.DTOs;
using Domain.ValueObjects;
using MediatR;

namespace Application.Bookings.Commands
{
    public record CreateBookingCommand(Guid roomId, TimeSlot slot, List<Guid> serviceIds) : IRequest<BookingResponseDTO>;
}
