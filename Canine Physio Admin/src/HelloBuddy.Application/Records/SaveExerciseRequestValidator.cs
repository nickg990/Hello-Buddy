using FluentValidation;
using HelloBuddy.Contracts;

namespace HelloBuddy.Application.Records;

public sealed class SaveExerciseRequestValidator : AbstractValidator<SaveExerciseRequest>
{
    public SaveExerciseRequestValidator()
    {
        RuleFor(x => x.ExerciseCategoryId).GreaterThan(0UL);
        RuleFor(x => x.Title).NotEmpty().MaximumLength(255);
        RuleFor(x => x.ObjectiveSummary).MaximumLength(4000);

        RuleFor(x => x.ImageUrl)
            .MaximumLength(500)
            .Must(BeValidUrl)
            .When(x => !string.IsNullOrWhiteSpace(x.ImageUrl));

        RuleFor(x => x.VideoUrl)
            .MaximumLength(500)
            .Must(BeValidUrl)
            .When(x => !string.IsNullOrWhiteSpace(x.VideoUrl));

        RuleFor(x => x.Instructions)
            .NotEmpty()
            .WithMessage("At least one instruction step is required.");

        RuleForEach(x => x.Instructions).ChildRules(step =>
        {
            step.RuleFor(x => x.InstructionText)
                .NotEmpty()
                .MaximumLength(5000);
        });
    }

    private static bool BeValidUrl(string? value)
    {
        return Uri.TryCreate(value, UriKind.Absolute, out _);
    }
}
