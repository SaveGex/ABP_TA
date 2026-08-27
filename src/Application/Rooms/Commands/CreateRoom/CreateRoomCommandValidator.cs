using FluentValidation;

namespace Application.Rooms.Commands.CreateRoom;


/// <summary>
/// Validates criteria for creating a new conference room.
/// </summary>
public class CreateRoomCommandValidator : AbstractValidator<CreateRoomCommand>
{
    public CreateRoomCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("Room name must not be empty and cannot exceed 100 characters.");

        RuleFor(x => x.Capacity)
            .GreaterThan(0)
            .WithMessage("Capacity must be greater than zero.");

        RuleFor(x => x.BaseHourlyRate)
            .NotNull()
            .WithMessage("Base hourly rate is required.");

        RuleFor(x => x.BaseHourlyRate.Amount)
            .GreaterThan(0)
            .When(x => x.BaseHourlyRate != null)
            .WithMessage("Hourly rate amount must be greater than zero.");

        RuleFor(x => x.BaseHourlyRate.Currency)
            .NotEmpty()
            .Length(3)
            .When(x => x.BaseHourlyRate != null)
            .WithMessage("Currency must be a valid 3-character ISO code.");

        RuleFor(x => x.ServiceIds)
            .NotNull()
            .WithMessage("Service IDs collection must not be null.");
    }
}