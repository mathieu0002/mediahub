using FluentValidation;
using MediaHub.Application.DTOs.Requests;

namespace MediaHub.Application.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(255);

        RuleFor(x => x.Username)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(50)
            .Matches("^[a-zA-Z0-9_-]+$")
            .WithMessage("Lettres, chiffres, _ ou - uniquement");

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .MaximumLength(100)
            .Matches("[A-Z]").WithMessage("Doit contenir au moins une majuscule")
            .Matches("[a-z]").WithMessage("Doit contenir au moins une minuscule")
            .Matches("[0-9]").WithMessage("Doit contenir au moins un chiffre");
    }
}