using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ticketing.Domain.Entities;

namespace Ticketing.Infrastructure.Persistence.Configurations;

public class RatingConfiguration : IEntityTypeConfiguration<Ratings>
{
    public void Configure(EntityTypeBuilder<Ratings> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.Ticket)
               .WithMany()
               .HasForeignKey(x => x.TicketId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.Rating).IsRequired();
        builder.Property(x => x.Comment).IsRequired(false);
        builder.Property(x => x.UserId).IsRequired();
    }
}
