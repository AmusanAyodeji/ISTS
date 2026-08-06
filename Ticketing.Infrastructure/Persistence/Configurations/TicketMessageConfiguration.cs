using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ticketing.Domain.Entities;

namespace Ticketing.Infrastructure.Persistence.Configurations;

public class TicketMessageConfiguration : IEntityTypeConfiguration<TicketMessage>
{
    public void Configure(EntityTypeBuilder<TicketMessage> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Message)
            .IsRequired()
            .HasMaxLength(4000);

        builder.HasOne(x => x.Ticket)
            .WithMany(x => x.Messages)
            .HasForeignKey(x => x.TicketId)
            .OnDelete(DeleteBehavior.Cascade);


        builder.Property(x => x.AttachmentUrl)
            .HasMaxLength(500);
    }
}
