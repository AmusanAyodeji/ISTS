using FluentValidation;

namespace Ticketing.Application.Features.Messages.Commands.SendMessage;

public class SendTicketMessageCommandValidator : AbstractValidator<SendTicketMessageCommand>
{
    public SendTicketMessageCommandValidator()
    {
        RuleFor(x => x.TicketId).NotEmpty();
        RuleFor(x => x.Message).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.SenderUserId).NotEmpty();
    }
}