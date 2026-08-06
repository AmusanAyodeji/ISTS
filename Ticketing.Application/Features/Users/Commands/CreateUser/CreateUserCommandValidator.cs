using FluentValidation;
using Ticketing.Domain.Constants;

namespace Ticketing.Application.Features.Users.Commands.CreateUser;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
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

        RuleForEach(x => x.Request.Roles)
            .Must(role => SystemRoles.All.Contains(role))
            .WithMessage("Invalid role provided. Allowed roles: Staff, Agent, Manager, Admin.");
    }
}
