using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Rooms.Commands.UpdateRoom
{
    public class UpdateRoomCommandHandler : IRequestHandler<UpdateRoomCommand>
    {
        private readonly IBookingDbContext _context;

        public UpdateRoomCommandHandler(IBookingDbContext context)
        {
            _context = context;
        }

        public async Task Handle(UpdateRoomCommand request, CancellationToken cancellationToken)
        {
            var room = await _context.Rooms
                .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken)
                ?? throw new KeyNotFoundException($"Room with ID '{request.Id}' was not found.");

            room.UpdateRate(request.BaseHourlyRate);

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
