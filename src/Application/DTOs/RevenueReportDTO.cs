namespace Application.DTOs
{
    public record RevenueReportDTO(
        DateTime From,
        DateTime To,
        decimal TotalRevenue,
        decimal RoomRevenue,
        decimal ServicesRevenue,
        int TotalBookings,
        List<RoomRevenueBreakdownDTO> RoomBreakdowns,
        string Currency);
}
