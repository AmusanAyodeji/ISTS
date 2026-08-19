using CsvHelper.Configuration;
using Ticketing.Application.DTOs.Category;

namespace Ticketing.Application.Common.Mappings;

public sealed class CategoryInfoMap : ClassMap<CategoryInfo>
{
    public CategoryInfoMap()
    {
        Map(m => m.DepartmentName).Name("DepartmentName");
        Map(m => m.Name).Name("Name");
    }
}
