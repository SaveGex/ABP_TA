using FluentValidation;

namespace Application.Rooms.Queries.SearchAvailableRooms
{
    public class SearchAvailableRoomsQueryValidator : AbstractValidator<SearchAvailableRoomsQuery>
    {
        public SearchAvailableRoomsQueryValidator()
        {
            RuleFor(x => x.date)
                .NotEmpty()
                .WithMessage("Date is required.");

            RuleFor(x => x.from)
                .LessThan(x => x.to)
                .WithMessage("Start time ('from') must be strictly earlier than end time ('to').");

            RuleFor(x => x.capacity)
                .GreaterThan(0)
                .WithMessage("Required capacity must be greater than zero.");
        }
    }
}
