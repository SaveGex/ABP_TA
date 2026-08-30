using Application.Common.Interfaces;
using Application.DTOs;
using Domain.Entities;
using Domain.Enums;
using Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Rooms.Queries.SearchAvailableRooms
{
    public class SearchAvailableRoomsQueryHandler : IRequestHandler<SearchAvailableRoomsQuery, List<RoomResponseDTO>>
    {
        private readonly IBookingDbContext _context;

        public SearchAvailableRoomsQueryHandler(IBookingDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<List<RoomResponseDTO>> Handle(
            SearchAvailableRoomsQuery request,
            CancellationToken cancellationToken)
        {
            ValidateTimeRange(request.from, request.to);

            var roomsQuery = _context.Rooms.AsNoTracking();

            roomsQuery = ApplyCapacityFilter(roomsQuery, request.capacity);
            roomsQuery = ApplyAvailabilityFilter(roomsQuery, request);

            var availableRooms = await roomsQuery.ToListAsync(cancellationToken);

            if (!availableRooms.Any())
            {
                return new List<RoomResponseDTO>();
            }

            return await MapToRoomResponseDtosAsync(availableRooms, cancellationToken);
        }

        private static void ValidateTimeRange(TimeSpan? from, TimeSpan? to)
        {
            if (from.HasValue && to.HasValue && from.Value >= to.Value)
            {
                throw new ArgumentException("'From' time must be earlier than 'To' time.");
            }
        }

        private static IQueryable<Room> ApplyCapacityFilter(IQueryable<Room> query, int? capacity)
        {
            return capacity.HasValue
                ? query.Where(r => r.Capacity >= capacity.Value)
                : query;
        }

        private IQueryable<Room> ApplyAvailabilityFilter(IQueryable<Room> query, SearchAvailableRoomsQuery request)
        {
            if (!request.from.HasValue || !request.to.HasValue)
            {
                return query;
            }

            var requestedSlot = BuildTimeSlot(request.date, request.from.Value, request.to.Value);

            var occupiedRoomIdsQuery = _context.Bookings
                .AsNoTracking()
                .Where(b => b.Status != BookingStatus.Cancelled
                         && b.Slot.Start < requestedSlot.End
                         && b.Slot.End > requestedSlot.Start)
                .Select(b => b.RoomId)
                .Distinct();

            return query.Where(r => !occupiedRoomIdsQuery.Contains(r.Id));
        }

        private static TimeSlot BuildTimeSlot(DateOnly? date, TimeSpan from, TimeSpan to)
        {
            var baseDate = date ?? DateOnly.FromDateTime(DateTime.UtcNow);
            var startDateTime = baseDate.ToDateTime(TimeOnly.FromTimeSpan(from));
            var endDateTime = baseDate.ToDateTime(TimeOnly.FromTimeSpan(to));

            return new TimeSlot(startDateTime, endDateTime);
        }

        private async Task<List<RoomResponseDTO>> MapToRoomResponseDtosAsync(
            List<Room> rooms,
            CancellationToken cancellationToken)
        {
            var requiredServiceIds = rooms
                .SelectMany(r => r.ServiceIds)
                .Distinct()
                .ToList();

            var relevantServices = await _context.Services
                .AsNoTracking()
                .Where(s => requiredServiceIds.Contains(s.Id))
                .ToDictionaryAsync(s => s.Id, cancellationToken);

            return rooms.Select(room => new RoomResponseDTO(
                Id: room.Id,
                Name: room.Name,
                Capacity: room.Capacity,
                BaseHourlyRate: room.BaseHourlyRate,
                Services: MapRoomServices(room.ServiceIds, relevantServices),
                CreatedAt: room.CreatedAt
            )).ToList();
        }

        private static List<BookedServiceDTO> MapRoomServices(
            IEnumerable<Guid> serviceIds,
            IReadOnlyDictionary<Guid, Service> servicesMap)
        {
            return serviceIds
                .Where(servicesMap.ContainsKey)
                .Select(id => servicesMap[id])
                .Select(s => new BookedServiceDTO(
                    s.Id,
                    s.Name,
                    s.Price.Amount,
                    s.Price.Currency))
                .ToList();
        }
    }
}
