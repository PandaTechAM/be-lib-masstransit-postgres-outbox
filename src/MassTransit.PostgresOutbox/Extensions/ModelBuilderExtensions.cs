using MassTransit.PostgresOutbox.Entities;
using Microsoft.EntityFrameworkCore;

namespace MassTransit.PostgresOutbox.Extensions;

public static class ModelBuilderExtensions
{
    public static ModelBuilder ConfigureOutboxMessageEntity(this ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<OutboxMessage>();

        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).ValueGeneratedNever();

        return modelBuilder;
    }

    public static ModelBuilder ConfigureInboxMessageEntity(this ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<InboxMessage>();

        entity.HasKey(x => new { x.MessageId, x.ConsumerId });

        return modelBuilder;
    }

    public static ModelBuilder ConfigureInboxOutboxEntities(this ModelBuilder modelBuilder)
    {
        modelBuilder.ConfigureOutboxMessageEntity();
        modelBuilder.ConfigureInboxMessageEntity();

        return modelBuilder;
    }
}