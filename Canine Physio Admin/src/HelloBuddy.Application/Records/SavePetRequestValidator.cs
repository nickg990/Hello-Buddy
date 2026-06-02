using FluentValidation;
using HelloBuddy.Contracts;

namespace HelloBuddy.Application.Records;

public sealed class SavePetRequestValidator : AbstractValidator<SavePetRequest>
{
    private static readonly string[] ValidSexes = ["male", "female", "unknown"];

    public SavePetRequestValidator()
    {
        RuleFor(x => x.OwnerId).GreaterThan(0UL);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Breed).MaximumLength(100);
        RuleFor(x => x.Sex).NotEmpty().Must(value => ValidSexes.Contains(value.Trim(), StringComparer.OrdinalIgnoreCase));
        RuleFor(x => x.Weight).GreaterThanOrEqualTo(0m).LessThanOrEqualTo(999.99m).When(x => x.Weight.HasValue);
    }
}
