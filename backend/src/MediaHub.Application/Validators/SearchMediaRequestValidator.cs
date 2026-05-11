using FluentValidation;
using MediaHub.Application.DTOs.Requests;

namespace MediaHub.Application.Validators;

public class SearchMediaRequestValidator : AbstractValidator<SearchMediaRequest>
{
    public SearchMediaRequestValidator()
    {
        RuleFor(x => x.Query)
            .NotEmpty().WithMessage("La recherche ne peut pas être vide")
            .MinimumLength(2).WithMessage("La recherche doit faire au moins 2 caractères")
            .MaximumLength(100);

        RuleFor(x => x.Type).IsInEnum();

        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 50);
    }
}