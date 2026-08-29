using Application.Common.Interfaces;
using Application.DTOs;
using Domain.Entities;
using Domain.Enums;
using Domain.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Bookings.Commands.CreateBooking;

public class CreateBookingCommandHandler : IRequestHandler<CreateBookingCommand, BookingResponseDTO>
{
    private readonly IBookingDbContext _context;
    private readonly IRentalPricingService _rentalPricingService;

    public CreateBookingCommandHandler(IBookingDbContext context, IRentalPricingService rentalPricingService)
    {
        _context = context;
        _rentalPricingService = rentalPricingService;
    }

    public async Task<BookingResponseDTO> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
    {
        // 1. Validate room existence
        var room = await _context.Rooms
            .FirstOrDefaultAsync(r => r.Id == request.roomId, cancellationToken)
            ?? throw new KeyNotFoundException($"Room with ID '{request.roomId}' was not found.");

        // 2. Validate that all requested services are assigned to this room
        var invalidServiceIds = request.serviceIds.Except(room.ServiceIds).ToList();
        if (invalidServiceIds.Count > 0)
        {
            throw new InvalidOperationException(
                $"Services [{string.Join(", ", invalidServiceIds)}] are not available for this room.");
        }

        // 3. Fetch service entities to capture historical snapshots (Name, Price)
        var selectedServices = await _context.Services
            .Where(s => request.serviceIds.Contains(s.Id))
            .ToListAsync(cancellationToken);

        // 4. Validate slot availability
        var isSlotOccupied = await _context.Bookings
            .AnyAsync(b => b.RoomId == request.roomId
                        && b.Status != BookingStatus.Cancelled
                        && b.Slot.Start < request.slot.End
                        && b.Slot.End > request.slot.Start,
                      cancellationToken);

        if (isSlotOccupied)
        {
            throw new InvalidOperationException("The requested room is already booked for the specified time slot.");
        }

        var roomCost = _rentalPricingService.CalculateRoomCost(room.BaseHourlyRate, request.slot);

        // 5. Create domain aggregate
        var booking = Booking.Create(room, request.slot, selectedServices, roomCost);

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToResponseDto(booking);
    }

    private static BookingResponseDTO MapToResponseDto(Booking booking)
    {
        var serviceDtos = booking.Services

            .Select(s => new BookedServiceDTO(s.ServiceId, s.Name, s.Price.Amount, s.Price.Currency))
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