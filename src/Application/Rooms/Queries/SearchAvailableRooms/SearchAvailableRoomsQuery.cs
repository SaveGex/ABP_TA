using Application.DTOs;
using MediatR;

namespace Application.Rooms.Queries.SearchAvailableRooms
{
    public record SearchAvailableRoomsQuery(DateTime date, TimeSpan from, TimeSpan to, int capacity) : IRequest<List<RoomResponseDTO>>;
}
