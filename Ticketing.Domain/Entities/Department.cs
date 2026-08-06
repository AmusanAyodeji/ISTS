using Ticketing.Domain.Common;

namespace Ticketing.Domain.Entities;

public class Department : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public ICollection<Category> Categories { get; set; } = new List<Category>(); 
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    public ICollection<SLA> SLAs { get; set; } = new List<SLA>();
}
