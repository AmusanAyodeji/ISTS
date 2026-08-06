using FluentValidation;

namespace Ticketing.Application.Features.Departments.Commands.CreateDepartment;

public class CreateDepartmentCommandValidator
    : AbstractValidator<CreateDepartmentCommand>
{
    public CreateDepartmentCommandValidator()
    {
        RuleFor(x => x.Request.Name)
            .NotEmpty()
            .MaximumLength(100);
    }
}