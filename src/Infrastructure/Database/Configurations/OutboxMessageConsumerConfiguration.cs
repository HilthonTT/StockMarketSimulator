using Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations;

public sealed class OutboxMessageConsumerConfiguration : IEntityTypeConfiguration<OutboxMessageConsumer>
{
    public void Configure(EntityTypeBuilder<OutboxMessageConsumer> builder)
    {
        builder.ToTable("outbox_message_consumers");

        // A single domain event can be consumed by several handlers, so the identity of a
        // consumption record is the (event id, handler name) pair, not the event id alone.
        builder.HasKey(x => new { x.Id, x.Name });

        builder.Property(x => x.Id).ValueGeneratedNever();
    }
}
