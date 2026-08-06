using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ticketing.Application.Features.SLAs.Commands.UpdateSLA
{
    public class UpdateSLACommandValidator : AbstractValidator<UpdateSLACommand>
    {
        public UpdateSLACommandValidator()
        {
            RuleFor(x => x.Request.DepartmentId)
            .NotEmpty();

            RuleForEach(x => x.Request.Priorities)
                .ChildRules(priority =>
                {
                    priority.RuleFor(x => x.Priority)
                        .IsInEnum();

                    priority.RuleFor(x => x.ResponseTimeMinutes)
                        .GreaterThan(0);

                    priority.RuleFor(x => x.ResolutionTimeMinutes)
                        .GreaterThan(0);

                    priority.RuleFor(x => x)
                        .Must(x => x.ResolutionTimeMinutes >= x.ResponseTimeMinutes);
                });
        }
    }
}