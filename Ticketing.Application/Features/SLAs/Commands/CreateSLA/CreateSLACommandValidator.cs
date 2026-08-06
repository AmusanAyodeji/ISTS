using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ticketing.Domain.Entities;

namespace Ticketing.Application.Features.SLAs.Commands.CreateSLA;

public class CreateSLACommandValidator : AbstractValidator<CreateSLACommand>
{
    public CreateSLACommandValidator()
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