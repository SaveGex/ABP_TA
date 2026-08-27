using Application.DTOs;
using Domain.ValueObjects;
using MediatR;

namespace Application.Rooms.Commands.CreateRoom
{
    /// <summary>
    /// Command for creating a new conference room.
    /// </summary>
    public record CreateRoomCommand(string Name, int Capacity, Money BaseHourlyRate, List<Guid> ServiceIds) : IRequest<RoomResponseDTO>;
}
