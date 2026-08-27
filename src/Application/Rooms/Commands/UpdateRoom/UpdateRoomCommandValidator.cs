using FluentValidation;

namespace Application.Rooms.Commands.UpdateRoom
{
    public class UpdateRoomCommandValidator : AbstractValidator<UpdateRoomCommand>
    {
        public UpdateRoomCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("Room ID must not be empty.");

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
        }
    }
}
