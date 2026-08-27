using Application.Common.Interfaces;
using Application.DTOs;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Revenues.Queries.RoomUtilizationReport
{
    public class GetRoomUtilizationReportQueryHandler : IRequestHandler<GetRoomUtilizationReportQuery, RoomUtilizationReportDTO>
    {
        private readonly IBookingDbContext _context;

        public GetRoomUtilizationReportQueryHandler(IBookingDbContext context)
        {
            _context = context;
        }

        public async Task<RoomUtilizationReportDTO> Handle(GetRoomUtilizationReportQuery request, CancellationToken cancellationToken)
        {
            var periodHours = (decimal)(request.To - request.From).TotalHours;

            var roomsQuery = _context.Rooms.AsNoTracking();
            if (request.RoomId.HasValue)
            {
                roomsQuery = roomsQuery.Where(r => r.Id == request.RoomId.Value);
            }

            var rooms = await roomsQuery.ToListAsync(cancellationToken);

            var bookings = await _context.Bookings
                .AsNoTracking()
                .Where(b => b.Status != BookingStatus.Cancelled
                         && b.Slot.Start >= request.From
                         && b.Slot.End <= request.To)
                .ToListAsync(cancellationToken);

            var roomDetails = new List<RoomUtilizationDetailsDTO>();
            decimal totalBookedHoursAllRooms = 0m;

            foreach (var room in rooms)
            {
                var roomBookings = bookings.Where(b => b.RoomId == room.Id).ToList();
                var bookedHours = roomBookings.Sum(b => b.Slot.DurationInHours);

                totalBookedHoursAllRooms += bookedHours;

                var utilizationPercentage = periodHours > 0
                    ? Math.Round((bookedHours / periodHours) * 100m, 2, MidpointRounding.AwayFromZero)
                    : 0m;

                roomDetails.Add(new RoomUtilizationDetailsDTO(
                    room.Id,
                    room.Name,
                    bookedHours,
                    periodHours,
                    utilizationPercentage
                ));
            }

            var totalAvailableHoursAllRooms = periodHours * rooms.Count;
            var overallUtilization = totalAvailableHoursAllRooms > 0
                ? Math.Round((totalBookedHoursAllRooms / totalAvailableHoursAllRooms) * 100m, 2, MidpointRounding.AwayFromZero)
                : 0m;

            return new RoomUtilizationReportDTO(
                request.From,
                request.To,
                overallUtilization,
                roomDetails
            );
        }
    }
}
