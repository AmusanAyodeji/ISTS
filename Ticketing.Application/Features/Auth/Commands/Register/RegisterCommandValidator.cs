using FluentValidation;

namespace Ticketing.Application.Features.Auth.Commands.Register;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Request.FirstName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Request.LastName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Request.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(255);

        RuleFor(x => x.Request.Password)
            .NotEmpty()
            .MinimumLength(8)
            .MaximumLength(128);
    }
}
