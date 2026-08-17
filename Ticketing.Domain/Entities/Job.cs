using Ticketing.Domain.Common;
using Ticketing.Domain.Enums;

namespace Ticketing.Domain.Entities;

public class Job:BaseEntity
{
    public Guid JobId { get; set; }
    public Guid DepartmentId { get; set; }
    public JobStatus Status { get; set; }
    public string FileName { get; set; }
}