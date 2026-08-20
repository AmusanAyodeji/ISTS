using MediatR;
using Ticketing.Application.DTOs.Department;

namespace Ticketing.Application.Features.Departments.Commands.CreateDepartmentsBulk;

public record CreateDepartmentsBulkCommand(IList<DepartmentInfo> Departments) : IRequest<BulkImportResult>;

public class BulkImportResult
{
    public int Created { get; set; }
    public int Skipped { get; set; }
}
