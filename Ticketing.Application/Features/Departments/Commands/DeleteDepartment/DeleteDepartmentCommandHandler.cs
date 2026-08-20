using MediatR;
using Ticketing.Application.Interfaces.Persistence;

namespace Ticketing.Application.Features.Departments.Commands.DeleteDepartment;

public class DeleteDepartmentCommandHandler
    : IRequestHandler<DeleteDepartmentCommand, Unit>
{
    private readonly IDepartmentRepository _departmentRepository;

    public DeleteDepartmentCommandHandler(IDepartmentRepository departmentRepository)
    {
        _departmentRepository = departmentRepository;
    }

    public async Task<Unit> Handle(
        DeleteDepartmentCommand request,
        CancellationToken cancellationToken)
    {
        var department = await _departmentRepository.GetByIdWithCategoriesAsync(request.Id, cancellationToken);

        if (department is null)
        {
            throw new KeyNotFoundException($"Department '{request.Id}' not found.");
        }

        if (department.Categories.Any())
        {
            throw new InvalidOperationException(
                "Cannot delete a department that still has categories. Remove or reassign the categories first.");
        }

        if (await _departmentRepository.HasTicketsAsync(request.Id, cancellationToken))
        {
            throw new InvalidOperationException(
                "Cannot delete a department that has tickets assigned to it.");
        }

        _departmentRepository.Delete(department);
        await _departmentRepository.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
