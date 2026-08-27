using MediatR;

namespace Application.Rooms.Commands.AddServiceToRoom
{
    public record AddServiceToRoomCommand(Guid RoomId, Guid ServiceId) : IRequest;
}
