using Application.DTOs;
using MediatR;

namespace Application.Revenues.Queries.RevenueReport
{
    public record GetRevenueReportQuery(
        DateTime From,
        DateTime To,
        Guid? RoomId = null
    ) : IRequest<RevenueReportDTO>;
}
