using FluentValidation;

namespace Ticketing.Application.Features.Tickets.Commands.EscalateTicket;

public class EscalateTicketCommandValidator : AbstractValidator<EscalateTicketCommand>
{
    public EscalateTicketCommandValidator()
    {
        RuleFor(x => x.TicketId).NotEmpty();
    }
}
