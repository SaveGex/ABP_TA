using FluentValidation;

namespace Application.Revenues.Queries.RevenueReport
{
    public class GetRevenueReportQueryValidator : AbstractValidator<GetRevenueReportQuery>
    {
        public GetRevenueReportQueryValidator()
        {
            RuleFor(x => x.From)
                .LessThan(x => x.To)
                .WithMessage("Start date ('From') must be earlier than end date ('To').");
        }
    }
}
