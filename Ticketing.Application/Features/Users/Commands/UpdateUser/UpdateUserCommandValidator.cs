using FluentValidation;
using Ticketing.Domain.Constants;

namespace Ticketing.Application.Features.Users.Commands.UpdateUser;

public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();

        RuleFor(x => x.Request.FirstName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Request.LastName)
            .NotEmpty()
            .MaximumLength(100);

        RuleForEach(x => x.Request.Roles)
            .Must(role => SystemRoles.All.Any(r => r.Equals(role, StringComparison.OrdinalIgnoreCase)))
            .WithMessage("Invalid role provided. Allowed roles: Staff, Agent, Manager, Admin.");
    }
}
