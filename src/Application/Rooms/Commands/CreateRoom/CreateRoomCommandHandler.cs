using Application.Common.Interfaces;
using Application.DTOs;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Rooms.Commands.CreateRoom;

/// <summary>
/// Handles the creation logic for conference rooms.
/// </summary>
public class CreateRoomCommandHandler : IRequestHandler<CreateRoomCommand, RoomResponseDTO>
{
    private readonly IBookingDbContext _context;

    public CreateRoomCommandHandler(IBookingDbContext context)
    {
        _context = context;
    }

    public async Task<RoomResponseDTO> Handle(CreateRoomCommand request, CancellationToken cancellationToken)
    {
        // Validate if all assigned services actually exist in the system database
        if (request.ServiceIds.Count > 0)
        {
            var existingServiceIds = await _context.Services
                .Where(s => request.ServiceIds.Contains(s.Id))
                .Select(s => s.Id)
                .ToListAsync(cancellationToken);

            var missingServiceIds = request.ServiceIds.Except(existingServiceIds).ToList();
            if (missingServiceIds.Count > 0)
            {
                throw new KeyNotFoundException(
                    $"Services with IDs [{string.Join(", ", missingServiceIds)}] do not exist.");
            }
        }

        var room = Room.Create(request.Name, request.Capacity, request.BaseHourlyRate);

        foreach (var serviceId in request.ServiceIds)
        {
            room.AddService(serviceId);
        }

        _context.Rooms.Add(room);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToResponseDTO(room);
    }

    private RoomResponseDTO MapToResponseDTO(Room room)
    {
        var services = _context.Services
            .AsNoTracking()
            .Where(s => room.ServiceIds.Contains(s.Id))
            .Select(s => new BookedServiceDTO(s.Id, s.Name, s.Price.Amount, s.Price.Currency)).ToList();

        return new RoomResponseDTO(
            Id: room.Id,
            Name: room.Name,
            Capacity: room.Capacity,
            BaseHourlyRate: room.BaseHourlyRate,
            Services: services,
            CreatedAt: room.CreatedAt
        );
    }
}
