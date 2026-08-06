using FluentValidation;

namespace Ticketing.Application.Features.Rating.Commands
{
    public class RatingCreationCommandValidator : AbstractValidator<RatingCreationCommand>
    {
        public RatingCreationCommandValidator()
        {
            RuleFor(x => x.request.TicketId)
                .NotEmpty();
            RuleFor(x => x.request.Rating)
                .GreaterThan(-1);
        }
    }
}