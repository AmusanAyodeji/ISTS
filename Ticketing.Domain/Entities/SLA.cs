using Ticketing.Domain.Common;
using Ticketing.Domain.Enums;

namespace Ticketing.Domain.Entities
{
    public class SLA : BaseEntity
    {
        public Guid DepartmentId { get; set; }
        public Department Department { get; set; } = null!;

        public TicketPriority Priority { get; set; }

        public int ResponseTimeMinutes { get; set; }
        public int ResolutionTimeMinutes { get; set; }
    }
}