using FluentValidation;

namespace Application.Bookings.Queries.GetBookings
{
    public class GetBookingsQueryValidator : AbstractValidator<GetBookingsQuery>
    {
        public GetBookingsQueryValidator()
        {
            When(x => x.From.HasValue && x.To.HasValue, () =>
            {
                RuleFor(x => x.From!.Value)
                    .LessThan(x => x.To!.Value)
                    .WithMessage("Start date ('From') must be earlier than end date ('To').");
            });
        }
    }
}
