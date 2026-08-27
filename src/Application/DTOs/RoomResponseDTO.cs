using Domain.ValueObjects;

namespace Application.DTOs
{
    public record RoomResponseDTO(Guid Id, string Name, int Capacity, Money BaseHourlyRate, IReadOnlyCollection<BookedServiceDTO> Services);
}
