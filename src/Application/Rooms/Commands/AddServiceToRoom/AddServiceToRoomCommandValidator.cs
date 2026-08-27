using FluentValidation;

namespace Application.Rooms.Commands.AddServiceToRoom
{
    public class AddServiceToRoomCommandValidator : AbstractValidator<AddServiceToRoomCommand>
    {
        public AddServiceToRoomCommandValidator()
        {
            RuleFor(x => x.RoomId)
                .NotEmpty()
                .WithMessage("Room ID must not be empty.");

            RuleFor(x => x.ServiceId)
                .NotEmpty()
                .WithMessage("Service ID must not be empty.");
        }
    }
}
