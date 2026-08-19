using MediatR;
using Ticketing.Application.DTOs.Category;

namespace Ticketing.Application.Features.Categories.Commands.CreateCategoriesBulk;

public record CreateCategoriesBulkCommand(IList<CategoryInfo> Categories) : IRequest<BulkImportResult>;

public class BulkImportResult
{
    public int Created { get; set; }
    public int Skipped { get; set; }
}
