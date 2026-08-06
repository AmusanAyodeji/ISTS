using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ticketing.Domain.Entities;

namespace Ticketing.Infrastructure.Persistence.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
public static readonly Guid ScreenIssueId = Guid.Parse("20000000-0000-0000-0000-000000000001");
public static readonly Guid PrinterIssueId = Guid.Parse("20000000-0000-0000-0000-000000000002");
public static readonly Guid PasswordResetId = Guid.Parse("20000000-0000-0000-0000-000000000003");
public static readonly Guid ItOtherId = Guid.Parse("20000000-0000-0000-0000-000000000004");

public static readonly Guid PayrollIssueId = Guid.Parse("20000000-0000-0000-0000-000000000005");
public static readonly Guid HrOtherId = Guid.Parse("20000000-0000-0000-0000-000000000006");

public static readonly Guid InvoiceIssueId = Guid.Parse("20000000-0000-0000-0000-000000000007");
public static readonly Guid FinanceOtherId = Guid.Parse("20000000-0000-0000-0000-000000000008");

    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasOne(x => x.Department)
            .WithMany(x => x.Categories)
            .HasForeignKey(x => x.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        var seedTimestamp = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        builder.HasData(

    new Category
    {
        Id = ScreenIssueId,
        Name = "Screen Issue",
        DepartmentId = DepartmentConfiguration.ItDepartmentId,
        CreatedAt = seedTimestamp
    },
    new Category
    {
        Id = PrinterIssueId,
        Name = "Printer Issue",
        DepartmentId = DepartmentConfiguration.ItDepartmentId,
        CreatedAt = seedTimestamp
    },
    new Category
    {
        Id = PasswordResetId,
        Name = "Password Reset",
        DepartmentId = DepartmentConfiguration.ItDepartmentId,
        CreatedAt = seedTimestamp
    },
    new Category
    {
        Id = ItOtherId,
        Name = "Other",
        DepartmentId = DepartmentConfiguration.ItDepartmentId,
        CreatedAt = seedTimestamp
    },

    new Category
    {
        Id = PayrollIssueId,
        Name = "Payroll Issue",
        DepartmentId = DepartmentConfiguration.HrDepartmentId,
        CreatedAt = seedTimestamp
    },
    new Category
    {
        Id = HrOtherId,
        Name = "Other",
        DepartmentId = DepartmentConfiguration.HrDepartmentId,
        CreatedAt = seedTimestamp
    },

    new Category
    {
        Id = InvoiceIssueId,
        Name = "Invoice Issue",
        DepartmentId = DepartmentConfiguration.FinanceDepartmentId,
        CreatedAt = seedTimestamp
    },
    new Category
    {
        Id = FinanceOtherId,
        Name = "Other",
        DepartmentId = DepartmentConfiguration.FinanceDepartmentId,
        CreatedAt = seedTimestamp
    }

        );
    }
}