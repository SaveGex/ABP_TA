namespace Application.DTOs
{
    public record BookingResponseDTO(
        Guid Id,
        Guid RoomId,
        DateTime Start,
        DateTime End,
        IReadOnlyCollection<BookedServiceDTO> Services,
        decimal TotalPrice,
        string Currency,
        string Status
    );
}
