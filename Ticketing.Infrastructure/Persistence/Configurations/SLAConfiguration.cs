using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ticketing.Domain.Entities;

public class SLAConfiguration : IEntityTypeConfiguration<SLA>
{
    public void Configure(EntityTypeBuilder<SLA> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.Department)
               .WithMany(x => x.SLAs)
               .HasForeignKey(x => x.DepartmentId);

        builder.HasIndex(x => new { x.DepartmentId, x.Priority })
               .IsUnique();

        builder.Property(x => x.ResponseTimeMinutes)
               .IsRequired();

        builder.Property(x => x.ResolutionTimeMinutes)
               .IsRequired();

        builder.Property(x => x.Priority)
               .IsRequired();
    }
}