using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Rooms.Commands.AddServiceToRoom
{
    public class AddServiceToRoomCommandHandler : IRequestHandler<AddServiceToRoomCommand>
    {
        private readonly IBookingDbContext _context;

        public AddServiceToRoomCommandHandler(IBookingDbContext context)
        {
            _context = context;
        }

        public async Task Handle(AddServiceToRoomCommand request, CancellationToken cancellationToken)
        {
            var room = await _context.Rooms
                .FirstOrDefaultAsync(r => r.Id == request.RoomId, cancellationToken)
                ?? throw new KeyNotFoundException($"Room with ID '{request.RoomId}' was not found.");

            var serviceExists = await _context.Services
                .AnyAsync(s => s.Id == request.ServiceId, cancellationToken);

            if (!serviceExists)
            {
                throw new KeyNotFoundException($"Service with ID '{request.ServiceId}' was not found.");
            }

            room.AddService(request.ServiceId);

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
