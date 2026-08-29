using Application.Common.Interfaces;
using Application.DTOs;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Revenues.Queries.RoomUtilizationReport
{
    public class GetRoomUtilizationReportQueryHandler : IRequestHandler<GetRoomUtilizationReportQuery, RoomUtilizationReportDTO>
    {
        private readonly IBookingDbContext _context;

        public GetRoomUtilizationReportQueryHandler(IBookingDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<RoomUtilizationReportDTO> Handle(
            GetRoomUtilizationReportQuery request,
            CancellationToken cancellationToken)
        {
            var periodHours = CalculatePeriodHours(request.From, request.To);

            var rooms = await GetRoomsAsync(request.RoomId, cancellationToken);

            if (!rooms.Any() || periodHours <= 0)
            {
                return CreateEmptyReport(request.From, request.To);
            }

            var roomIds = rooms.Select(r => r.Id).ToList();
            var bookings = await GetBookingsAsync(request.From, request.To, roomIds, cancellationToken);

            return BuildReport(request.From, request.To, rooms, bookings, periodHours);
        }

        private static decimal CalculatePeriodHours(DateTime start, DateTime end)
        {
            return (decimal)(end - start).TotalHours;
        }

        private async Task<List<Room>> GetRoomsAsync(Guid? roomId, CancellationToken cancellationToken)
        {
            var query = _context.Rooms.AsNoTracking();

            if (roomId.HasValue)
            {
                query = query.Where(r => r.Id == roomId.Value);
            }

            return await query.ToListAsync(cancellationToken);
        }

        private async Task<List<Booking>> GetBookingsAsync(
            DateTime startDateTime,
            DateTime endDateTime,
            List<Guid> roomIds,
            CancellationToken cancellationToken)
        {
            // We are looking for any overlapping reservations within the range [startDateTime, endDateTime]
            return await _context.Bookings
                .AsNoTracking()
                .Where(b => b.Status != BookingStatus.Cancelled
                         && b.Slot.Start < endDateTime
                         && b.Slot.End > startDateTime
                         && roomIds.Contains(b.RoomId))
                .ToListAsync(cancellationToken);
        }

        private static RoomUtilizationReportDTO BuildReport(
            DateTime from,
            DateTime to,
            List<Room> rooms,
            List<Booking> bookings,
            decimal periodHours)
        {
            var roomDetails = new List<RoomUtilizationDetailsDTO>();
            decimal totalBookedHoursAllRooms = 0m;

            foreach (var room in rooms)
            {
                var roomBookings = bookings.Where(b => b.RoomId == room.Id);
                var bookedHours = CalculateBookedHoursForRoom(from, to, roomBookings);
                totalBookedHoursAllRooms += bookedHours;

                roomDetails.Add(new RoomUtilizationDetailsDTO(
                    room.Id,
                    room.Name,
                    bookedHours,
                    periodHours,
                    CalculatePercentage(bookedHours, periodHours)
                ));
            }

            var totalAvailableHoursAllRooms = periodHours * rooms.Count;
            var overallUtilization = CalculatePercentage(totalBookedHoursAllRooms, totalAvailableHoursAllRooms);

            return new RoomUtilizationReportDTO(
                from,
                to,
                overallUtilization,
                roomDetails
            );
        }

        private static decimal CalculateBookedHoursForRoom(
            DateTime periodStart,
            DateTime periodEnd,
            IEnumerable<Booking> roomBookings)
        {
            decimal totalHours = 0m;

            foreach (var booking in roomBookings)
            {
                // We determine the exact boundaries of the reporting period and the reservation
                var effectiveStart = booking.Slot.Start > periodStart ? booking.Slot.Start : periodStart;
                var effectiveEnd = booking.Slot.End < periodEnd ? booking.Slot.End : periodEnd;

                if (effectiveEnd > effectiveStart)
                {
                    totalHours += (decimal)(effectiveEnd - effectiveStart).TotalHours;
                }
            }

            return Math.Round(totalHours, 2, MidpointRounding.AwayFromZero);
        }

        private static decimal CalculatePercentage(decimal part, decimal total)
        {
            if (total <= 0)
                return 0m;

            return Math.Round((part / total) * 100m, 2, MidpointRounding.AwayFromZero);
        }

        private static RoomUtilizationReportDTO CreateEmptyReport(DateTime from, DateTime to)
        {
            return new RoomUtilizationReportDTO(
                from,
                to,
                0m,
                new List<RoomUtilizationDetailsDTO>()
            );
        }
    }
}