using FluentValidation;

namespace Ticketing.Application.Features.Categories.Commands.CreateCategory;

public class CreateCategoryCommandValidator
    : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(x => x.Request.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Request.DepartmentId)
            .NotEmpty();
    }
}