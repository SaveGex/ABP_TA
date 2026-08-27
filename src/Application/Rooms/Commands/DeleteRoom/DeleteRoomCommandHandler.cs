using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Rooms.Commands.DeleteRoom
{
    public class DeleteRoomCommandHandler : IRequestHandler<DeleteRoomCommand>
    {
        private readonly IBookingDbContext _context;

        public DeleteRoomCommandHandler(IBookingDbContext context)
        {
            _context = context;
        }

        public async Task Handle(DeleteRoomCommand request, CancellationToken cancellationToken)
        {
            var room = await _context.Rooms
                .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken)
                ?? throw new KeyNotFoundException($"Room with ID '{request.Id}' was not found.");

            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
