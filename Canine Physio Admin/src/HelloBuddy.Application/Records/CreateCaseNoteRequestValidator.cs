using FluentValidation;
using HelloBuddy.Contracts;

namespace HelloBuddy.Application.Records;

public sealed class CreateCaseNoteRequestValidator : AbstractValidator<CreateCaseNoteRequest>
{
    public CreateCaseNoteRequestValidator()
    {
        RuleFor(x => x.NoteType).MaximumLength(50);
        RuleFor(x => x.NoteText).NotEmpty();
    }
}
