using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ticketing.Domain.Entities;

namespace Ticketing.Infrastructure.Persistence.Configurations;

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public static readonly Guid ItDepartmentId = Guid.Parse("10000000-0000-0000-0000-000000000001");
    public static readonly Guid HrDepartmentId = Guid.Parse("10000000-0000-0000-0000-000000000002");
    public static readonly Guid FinanceDepartmentId = Guid.Parse("10000000-0000-0000-0000-000000000003");

    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.HasIndex(x => x.Name).IsUnique();

        var seedTimestamp = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        builder.HasData(
            new Department { Id = ItDepartmentId, Name = "IT", Description = "Information Technology department", CreatedAt = seedTimestamp },
            new Department { Id = HrDepartmentId, Name = "HR", Description = "Human Resources department", CreatedAt = seedTimestamp },
            new Department { Id = FinanceDepartmentId, Name = "Finance", Description = "Finance and Accounting department", CreatedAt = seedTimestamp }
        );
    }
}
