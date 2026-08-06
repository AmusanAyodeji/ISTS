using FluentValidation;

namespace Ticketing.Application.Features.Tickets.Commands.CreateTicket;

public class CreateTicketCommandValidator : AbstractValidator<CreateTicketCommand>
{
    public CreateTicketCommandValidator()
    {
        RuleFor(x => x.Request.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Request.Description)
            .NotEmpty();

        RuleFor(x => x.Request.CategoryId)
            .NotEmpty();

        RuleFor(x => x.Request.DepartmentId)
            .NotEmpty();
    }
}
