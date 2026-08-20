using CsvHelper.Configuration;
using Ticketing.Application.DTOs.Department;

namespace Ticketing.Application.Common.Mappings;

public sealed class DepartmentInfoMap : ClassMap<DepartmentInfo>
{
    public DepartmentInfoMap()
    {
        Map(m => m.Name).Name("Name");
        Map(m => m.Description).Name("Description");
    }
}
