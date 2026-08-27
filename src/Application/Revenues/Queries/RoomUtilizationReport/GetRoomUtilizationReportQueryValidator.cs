using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Revenues.Queries.RoomUtilizationReport
{
    public class GetRoomUtilizationReportQueryValidator : AbstractValidator<GetRoomUtilizationReportQuery>
    {
        public GetRoomUtilizationReportQueryValidator()
        {
            RuleFor(x => x.From)
                .LessThan(x => x.To)
                .WithMessage("Start date ('From') must be earlier than end date ('To').");
        }
    }
}
