using Application.DTOs;
using MediatR;

namespace Application.Revenues.Queries.RoomUtilizationReport
{
    public record GetRoomUtilizationReportQuery(
        DateTime From,
        DateTime To,
        Guid? RoomId = null
    ) : IRequest<RoomUtilizationReportDTO>;
}
