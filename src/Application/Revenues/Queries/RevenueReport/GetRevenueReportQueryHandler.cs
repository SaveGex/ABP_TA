using Application.Common.Interfaces;
using Application.DTOs;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Revenues.Queries.RevenueReport
{
    public class GetRevenueReportQueryHandler : IRequestHandler<GetRevenueReportQuery, RevenueReportDTO>
    {
        private readonly IBookingDbContext _context;

        public GetRevenueReportQueryHandler(IBookingDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<RevenueReportDTO> Handle(GetRevenueReportQuery request, CancellationToken cancellationToken)
        {
            var bookingsQuery = _context.Bookings
                .AsNoTracking()
                .Where(b => b.Status != BookingStatus.Cancelled
                         && b.CreatedAt >= request.From
                         && b.CreatedAt <= request.To);

            if (request.RoomId.HasValue)
            {
                bookingsQuery = bookingsQuery.Where(b => b.RoomId == request.RoomId.Value);
            }

            var roomsMap = await GetRoomsMapAsync(request.RoomId, cancellationToken);

            var rawBreakdowns = await bookingsQuery
                .GroupBy(b => b.RoomId)
                .Select(g => new
                {
                    RoomId = g.Key,
                    TotalBookings = g.Count(),
                    TotalRevenue = g.Sum(b => b.TotalPrice.Amount),
                    ServicesRevenue = g.SelectMany(b => b.Services).Sum(s => s.Price.Amount)
                })
                .ToListAsync(cancellationToken);

            var totalBookingsCount = rawBreakdowns.Sum(r => r.TotalBookings);
            var totalRevenue = rawBreakdowns.Sum(r => r.TotalRevenue);
            var servicesRevenue = rawBreakdowns.Sum(r => r.ServicesRevenue);
            var roomRevenue = totalRevenue - servicesRevenue;

            var breakdownsDto = rawBreakdowns
                .Select(r => new RoomRevenueBreakdownDTO(
                    r.RoomId,
                    roomsMap.GetValueOrDefault(r.RoomId, "Unknown Room"),
                    r.TotalBookings,
                    r.TotalRevenue))
                .ToList();

            var currency = await GetReportCurrencyAsync(bookingsQuery, cancellationToken);

            return new RevenueReportDTO(
                From: request.From,
                To: request.To,
                TotalRevenue: totalRevenue,
                RoomRevenue: roomRevenue,
                ServicesRevenue: servicesRevenue,
                TotalBookings: totalBookingsCount,
                RoomBreakdowns: breakdownsDto,
                Currency: currency
            );
        }

        private async Task<Dictionary<Guid, string>> GetRoomsMapAsync(Guid? roomId, CancellationToken cancellationToken)
        {
            var query = _context.Rooms.AsNoTracking();

            if (roomId.HasValue)
            {
                query = query.Where(r => r.Id == roomId.Value);
            }

            return await query.ToDictionaryAsync(r => r.Id, r => r.Name, cancellationToken);
        }

        private static async Task<string> GetReportCurrencyAsync(
            IQueryable<Domain.Entities.Booking> bookingsQuery,
            CancellationToken cancellationToken)
        {
            return await bookingsQuery
                .Select(b => b.TotalPrice.Currency)
                .FirstOrDefaultAsync(cancellationToken) ?? "UAH";
        }
    }
}
