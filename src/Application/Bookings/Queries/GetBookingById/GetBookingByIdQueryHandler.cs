using Application.Common.Interfaces;
using Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Bookings.Queries.GetBookingById
{
    public class GetBookingByIdQueryHandler : IRequestHandler<GetBookingByIdQuery, BookingResponseDTO>
    {
        private readonly IBookingDbContext _context;

        public GetBookingByIdQueryHandler(IBookingDbContext context)
        {
            _context = context;
        }

        public async Task<BookingResponseDTO> Handle(GetBookingByIdQuery request, CancellationToken cancellationToken)
        {
            var booking = await _context.Bookings
                .AsNoTracking()
                .Include(b => b.Services)
                .FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken)
                ?? throw new KeyNotFoundException($"Booking with ID '{request.Id}' was not found.");

            var serviceDtos = booking.Services
                .Select(s => new BookedServiceDTO(
                    Id: s.ServiceId,
                    Name: s.Name,
                    Price: s.Price.Amount,
                    Currency: s.Price.Currency))
                .ToList();

            return new BookingResponseDTO(
                Id: booking.Id,
                RoomId: booking.RoomId,
                Start: booking.Slot.Start,
                End: booking.Slot.End,
                TotalPrice: booking.TotalPrice.Amount,
                Currency: booking.TotalPrice.Currency,
                Status: booking.Status.ToString(),
                Services: serviceDtos.AsReadOnly()
            );
        }
    }
}
