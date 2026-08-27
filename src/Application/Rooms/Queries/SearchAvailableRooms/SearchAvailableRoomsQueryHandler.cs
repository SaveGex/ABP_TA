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
            _context = context;
        }

        public async Task<List<RoomResponseDTO>> Handle(SearchAvailableRoomsQuery request, CancellationToken cancellationToken)
        {
            var startDateTime = request.date.Date.Add(request.from);
            var endDateTime = request.date.Date.Add(request.to);

            var requestedSlot = new TimeSlot(startDateTime, endDateTime);

            // 1. Get room IDs that have active (non-cancelled) bookings overlapping with the requested slot
            var occupiedRoomIds = await _context.Bookings
                .AsNoTracking()
                .Where(b => b.Status != BookingStatus.Cancelled
                         && b.Slot.Start < requestedSlot.End
                         && b.Slot.End > requestedSlot.Start)
                .Select(b => b.RoomId)
                .Distinct()
                .ToListAsync(cancellationToken);

            // 2. Fetch available rooms with required capacity
            var availableRooms = await _context.Rooms
                .AsNoTracking()
                .Where(r => r.Capacity >= request.capacity && !occupiedRoomIds.Contains(r.Id))
                .ToListAsync(cancellationToken);

            // 3. Fetch all services to map full DTO details
            var allServices = await _context.Services
                .AsNoTracking()
                .ToDictionaryAsync(s => s.Id, cancellationToken);

            var result = new List<RoomResponseDTO>();

            foreach (var room in availableRooms)
            {
                var roomServices = room.ServiceIds
                    .Where(id => allServices.ContainsKey(id))
                    .Select(id => new BookedServiceDTO(
                        id,
                        allServices[id].Name,
                        allServices[id].Price.Amount,
                        allServices[id].Price.Currency))
                    .ToList();

                result.Add(new RoomResponseDTO(
                    Id: room.Id,
                    Name: room.Name,
                    Capacity: room.Capacity,
                    BaseHourlyRate: room.BaseHourlyRate,
                    Services: roomServices
                ));
            }

            return result;
        }
    }
}
