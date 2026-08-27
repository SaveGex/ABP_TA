using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs
{
    public record RoomRevenueBreakdownDTO(Guid RoomId, string RoomName, int BookingCount, decimal TotalRevenue);
}
