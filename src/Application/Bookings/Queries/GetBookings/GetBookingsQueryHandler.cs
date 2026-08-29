using Application.Common.Interfaces;
using Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Bookings.Queries.GetBookings
{
    public class GetBookingsQueryHandler : IRequestHandler<GetBookingsQuery, List<BookingResponseDTO>>
    {
        private readonly IBookingDbContext _context;

        public GetBookingsQueryHandler(IBookingDbContext context)
        {
            _context = context;
        }

        public async Task<List<BookingResponseDTO>> Handle(GetBookingsQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Bookings.AsNoTracking();

            if (request.RoomId.HasValue)
            {
                query = query.Where(b => b.RoomId == request.RoomId.Value);
            }

            if (request.From.HasValue)
            {
                query = query.Where(b => b.Slot.Start >= request.From.Value);
            }

            if (request.To.HasValue)
            {
                query = query.Where(b => b.Slot.End <= request.To.Value);
            }

            var bookings = await query.ToListAsync(cancellationToken);

            return bookings.Select(b =>
            {
                var serviceDtos = b.Services
                    .Select(s => new BookedServiceDTO(
                        Id: s.ServiceId,
                        Name: s.Name,
                        Price: s.Price.Amount,
                        Currency: s.Price.Currency))
                    .ToList();

                return new BookingResponseDTO(
                    Id: b.Id,
                    RoomId: b.RoomId,
                    Start: b.Slot.Start,
                    End: b.Slot.End,
                    TotalPrice: b.TotalPrice.Amount,
                    Currency: b.TotalPrice.Currency,
                    Status: b.Status.ToString(),
                    Services: serviceDtos.AsReadOnly()
                );
            }).ToList();
        }
    }
}
