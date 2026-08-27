using FluentValidation;

namespace Application.Bookings.Commands;

/// <summary>
/// Validates input criteria for <see cref="CreateBookingCommand"/>.
/// </summary>
public class CreateBookingCommandValidator : AbstractValidator<CreateBookingCommand>
{
    public CreateBookingCommandValidator()
    {
        RuleFor(x => x.roomId)
            .NotEmpty()
            .WithMessage("Room ID must not be empty.");

        RuleFor(x => x.slot)
            .NotNull()
            .WithMessage("Time slot is required.");

        RuleFor(x => x.slot.Start)
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("Booking start time must be in the future.");

        RuleFor(x => x.slot.End)
            .GreaterThan(x => x.slot.Start)
            .WithMessage("Booking end time must be after start time.");

        RuleFor(x => x.serviceIds)
            .NotNull()
            .WithMessage("Service IDs collection cannot be null.");
    }
}
