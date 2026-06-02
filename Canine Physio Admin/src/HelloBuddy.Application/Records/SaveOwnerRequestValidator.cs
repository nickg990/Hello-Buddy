using FluentValidation;
using HelloBuddy.Contracts;

namespace HelloBuddy.Application.Records;

public sealed class SaveOwnerRequestValidator : AbstractValidator<SaveOwnerRequest>
{
    public SaveOwnerRequestValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(255);
        RuleFor(x => x.PhoneNumber).MaximumLength(30);
        RuleFor(x => x.AddressLine1).MaximumLength(255);
        RuleFor(x => x.AddressLine2).MaximumLength(255);
        RuleFor(x => x.Town).MaximumLength(100);
        RuleFor(x => x.Postcode).MaximumLength(20);
    }
}
