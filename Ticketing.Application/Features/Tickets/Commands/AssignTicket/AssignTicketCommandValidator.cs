using FluentValidation;

namespace Ticketing.Application.Features.Tickets.Commands.AssignTicket;

public class AssignTicketCommandValidator : AbstractValidator<AssignTicketCommand>
{
    public AssignTicketCommandValidator()
    {
        RuleFor(x => x.Request.AgentId)
            .NotEmpty();
    }
}
