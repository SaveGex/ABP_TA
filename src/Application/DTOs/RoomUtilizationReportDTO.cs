using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs
{
    public record RoomUtilizationReportDTO(DateTime From, DateTime To, decimal OverallUtilizationPercentage, List<RoomUtilizationDetailsDTO> RoomDetails);
}
