using FluentValidation;

namespace YAERP.Application.Finance.Queries.GetFinancialAnomalies;

public class GetFinancialAnomaliesQueryValidator : AbstractValidator<GetFinancialAnomaliesQuery>
{
    public GetFinancialAnomaliesQueryValidator()
    {
        RuleFor(v => v.ThresholdZScore).GreaterThan(0);
    }
}
