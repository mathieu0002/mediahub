using FluentValidation;
using MediaHub.Application.DTOs.Requests;

namespace MediaHub.Application.Validators;

public class UpdateUserMediaRequestValidator : AbstractValidator<UpdateUserMediaRequest>
{
    public UpdateUserMediaRequestValidator()
    {
        When(x => x.Status.HasValue, () =>
            RuleFor(x => x.Status!.Value).IsInEnum());

        When(x => x.Progress.HasValue, () =>
            RuleFor(x => x.Progress!.Value).GreaterThanOrEqualTo(0));

        When(x => x.UserScore.HasValue, () =>
            RuleFor(x => x.UserScore!.Value).InclusiveBetween(0, 100));

        When(x => x.Notes is not null, () =>
            RuleFor(x => x.Notes!).MaximumLength(2000));
    }
}