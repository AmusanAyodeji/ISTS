using FluentValidation;

namespace Ticketing.Application.Features.Tickets.Commands.UpdateTicketStatus;

public class UpdateTicketStatusCommandValidator : AbstractValidator<UpdateTicketStatusCommand>
{
    public UpdateTicketStatusCommandValidator()
    {
        RuleFor(x => x.Request.Status)
            .IsInEnum();
    }
}
