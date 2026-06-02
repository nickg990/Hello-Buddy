using FluentValidation;
using HelloBuddy.Contracts;

namespace HelloBuddy.Application.Records;

public sealed class SaveTreatmentCaseRequestValidator : AbstractValidator<SaveTreatmentCaseRequest>
{
    private static readonly string[] ValidStatuses = ["planned", "active", "completed", "cancelled"];

    public SaveTreatmentCaseRequestValidator()
    {
        RuleFor(x => x.PetId).GreaterThan(0UL);
        RuleFor(x => x.CaseTitle).NotEmpty().MaximumLength(255);
        RuleFor(x => x.StartDate).NotEqual(default(DateOnly));
        RuleFor(x => x.Status).NotEmpty().Must(value => ValidStatuses.Contains(value.Trim(), StringComparer.OrdinalIgnoreCase));
        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .When(x => x.EndDate.HasValue);
    }
}
