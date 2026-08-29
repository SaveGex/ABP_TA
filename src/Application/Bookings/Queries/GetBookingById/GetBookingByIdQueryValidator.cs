using FluentValidation;

namespace Application.Bookings.Queries.GetBookingById
{
    public class GetBookingByIdQueryValidator : AbstractValidator<GetBookingByIdQuery>
    {
        public GetBookingByIdQueryValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("Booking ID must not be empty.");
        }
    }
}
