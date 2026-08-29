namespace Application.DTOs
{
    public record RoomUtilizationReportDTO(DateTime From, DateTime To, decimal OverallUtilizationPercentage, List<RoomUtilizationDetailsDTO> RoomDetails);
}
