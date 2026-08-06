using System.Text.Json.Serialization;
using Ticketing.Domain.Common;
namespace Ticketing.Domain.Entities;

public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public Guid DepartmentId { get; set; } 

    [JsonIgnore]
    public Department? Department { get; set; } 
}