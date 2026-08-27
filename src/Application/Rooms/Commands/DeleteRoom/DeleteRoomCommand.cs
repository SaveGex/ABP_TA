using MediatR;

namespace Application.Rooms.Commands.DeleteRoom
{
    public record DeleteRoomCommand(Guid Id) : IRequest;
}
