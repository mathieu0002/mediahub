using FluentValidation;
using MediaHub.Application.DTOs.Requests;

namespace MediaHub.Application.Validators;

public class CreateUserMediaRequestValidator : AbstractValidator<CreateUserMediaRequest>
{
    public CreateUserMediaRequestValidator()
    {
        RuleFor(x => x.ExternalId).NotEmpty();
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Source).IsInEnum();
        RuleFor(x => x.Status).IsInEnum();
    }
}