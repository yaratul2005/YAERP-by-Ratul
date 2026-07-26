using System;
using FluentValidation;

namespace YAERP.Application.Finance.Queries.GetFinancialAnomalies;

public class GetFinancialAnomaliesQueryValidator : AbstractValidator<GetFinancialAnomaliesQuery>
{
    public GetFinancialAnomaliesQueryValidator()
    {
        When(q => q.StartDate.HasValue && q.EndDate.HasValue, () =>
        {
            RuleFor(q => q.EndDate!.Value)
                .GreaterThanOrEqualTo(q => q.StartDate!.Value)
                .WithMessage("EndDate must be on or after StartDate.");
        });
    }
}
