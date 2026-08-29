using Application.DTOs;
using MediatR;

namespace Application.Rooms.Queries.SearchAvailableRooms
{
    public record SearchAvailableRoomsQuery(DateOnly? date = null, TimeSpan? from = null, TimeSpan? to = null, int? capacity = null) : IRequest<List<RoomResponseDTO>>;
}
