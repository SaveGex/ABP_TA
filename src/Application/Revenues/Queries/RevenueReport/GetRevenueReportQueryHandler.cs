using Application.Common.Interfaces;
using Application.DTOs;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Revenues.Queries.RevenueReport
{
    public class GetRevenueReportQueryHandler : IRequestHandler<GetRevenueReportQuery, RevenueReportDTO>
    {
        private readonly IBookingDbContext _context;

        public GetRevenueReportQueryHandler(IBookingDbContext context)
        {
            _context = context;
        }

        public async Task<RevenueReportDTO> Handle(GetRevenueReportQuery request, CancellationToken cancellationToken)
        {
            var bookingsQuery = _context.Bookings
                .AsNoTracking()
                .Where(b => b.Status != BookingStatus.Cancelled
                         && b.Slot.Start >= request.From
                         && b.Slot.End <= request.To);

            if (request.RoomId.HasValue)
            {
                bookingsQuery = bookingsQuery.Where(b => b.RoomId == request.RoomId.Value);
            }

            var bookings = await bookingsQuery.ToListAsync(cancellationToken);
            var rooms = await _context.Rooms.AsNoTracking().ToDictionaryAsync(r => r.Id, cancellationToken);

            decimal totalRevenue = 0m;
            decimal servicesRevenue = 0m;

            foreach (var booking in bookings)
            {
                totalRevenue += booking.TotalPrice.Amount;
                servicesRevenue += booking.Services.Sum(s => s.Price.Amount);
            }

            var roomRevenue = totalRevenue - servicesRevenue;

            var roomBreakdowns = bookings
                .GroupBy(b => b.RoomId)
                .Select(g => new RoomRevenueBreakdownDTO(
                    g.Key,
                    rooms.TryGetValue(g.Key, out var room) ? room.Name : "Unknown Room",
                    g.Count(),
                    g.Sum(b => b.TotalPrice.Amount)
                ))
                .ToList();

            var currency = bookings.FirstOrDefault()?.TotalPrice.Currency ?? "USD";

            return new RevenueReportDTO(
                From: request.From,
                To: request.To,
                TotalRevenue: totalRevenue,
                RoomRevenue: roomRevenue,
                ServicesRevenue: servicesRevenue,
                TotalBookings: bookings.Count,
                RoomBreakdowns: roomBreakdowns,
                Currency: currency
            );
        }
    }
}
