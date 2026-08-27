using Domain.ValueObjects;
using MediatR;

namespace Application.Rooms.Commands.UpdateRoom
{
    public record UpdateRoomCommand(
        Guid Id,
        string Name,
        int Capacity,
        Money BaseHourlyRate
    ) : IRequest;
}
