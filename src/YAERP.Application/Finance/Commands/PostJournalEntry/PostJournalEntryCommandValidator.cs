using FluentValidation;

namespace YAERP.Application.Finance.Commands.PostJournalEntry;

public class PostJournalEntryCommandValidator : AbstractValidator<PostJournalEntryCommand>
{
    public PostJournalEntryCommandValidator()
    {
        RuleFor(v => v.EntryNumber).NotEmpty().MaximumLength(50);
        RuleFor(v => v.Lines).NotEmpty();
        RuleForEach(v => v.Lines).ChildRules(lines =>
        {
            lines.RuleFor(l => l.AccountId).NotEmpty();
            lines.RuleFor(l => l.Debit).GreaterThanOrEqualTo(0);
            lines.RuleFor(l => l.Credit).GreaterThanOrEqualTo(0);
            lines.RuleFor(l => l).Must(l => (l.Debit > 0 && l.Credit == 0) || (l.Credit > 0 && l.Debit == 0))
                .WithMessage("Line must have either a debit or a credit, but not both or neither.");
        });
    }
}
