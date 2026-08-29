namespace Application.DTOs
{
    public record RoomUtilizationDetailsDTO(Guid RoomId, string RoomName, decimal TotalBookedHours, decimal TotalAvailableHours, decimal UtilizationPercentage);
}
