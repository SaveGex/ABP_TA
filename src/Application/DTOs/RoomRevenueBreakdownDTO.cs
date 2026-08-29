namespace Application.DTOs
{
    public record RoomRevenueBreakdownDTO(Guid RoomId, string RoomName, int BookingCount, decimal TotalRevenue);
}
