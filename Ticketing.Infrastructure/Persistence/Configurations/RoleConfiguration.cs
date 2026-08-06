using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ticketing.Domain.Constants;
using Ticketing.Domain.Entities;

namespace Ticketing.Infrastructure.Persistence.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public static readonly Guid StaffRoleId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid AgentRoleId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid ManagerRoleId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    public static readonly Guid AdminRoleId = Guid.Parse("44444444-4444-4444-4444-444444444444");

    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.HasIndex(x => x.Name).IsUnique();

        var seedTimestamp = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        builder.HasData(
            new Role { Id = StaffRoleId, Name = SystemRoles.Staff, Description = "Regular internal staff member.", CreatedAt = seedTimestamp },
            new Role { Id = AgentRoleId, Name = SystemRoles.Agent, Description = "Support agent handling tickets.", CreatedAt = seedTimestamp },
            new Role { Id = ManagerRoleId, Name = SystemRoles.Manager, Description = "Team manager with elevated access.", CreatedAt = seedTimestamp },
            new Role { Id = AdminRoleId, Name = SystemRoles.Admin, Description = "System administrator.", CreatedAt = seedTimestamp });
    }
}
