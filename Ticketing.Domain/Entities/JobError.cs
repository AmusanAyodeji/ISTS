using Ticketing.Domain.Common;

namespace Ticketing.Domain.Entities;

public class JobError : BaseEntity
{
    public Guid JobId { get; set; }
    public Guid DepartmentId { get; set; }
    public string Message { get; set; }
}