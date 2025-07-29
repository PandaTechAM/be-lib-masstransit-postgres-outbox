using MassTransit.MySqlOutbox.Entities;
using Microsoft.EntityFrameworkCore;

namespace MassTransit.MySqlOutbox.Extensions;

public static class ModelBuilderExtensions
{
   public static ModelBuilder ConfigureOutboxMessageEntity(this ModelBuilder modelBuilder)
   {
      var entity = modelBuilder.Entity<OutboxMessage>();


      entity.HasKey(x => x.Id);
      entity.Property(x => x.Id)
            .HasColumnType("binary(16)")
            .ValueGeneratedNever();

      return modelBuilder;
   }

   public static ModelBuilder ConfigureInboxMessageEntity(this ModelBuilder modelBuilder)
   {
      var entity = modelBuilder.Entity<InboxMessage>();

      entity.Property(x => x.MessageId)
            .HasColumnType("binary(16)")
            .ValueGeneratedNever();

      entity.HasKey(x => new
      {
         x.MessageId,
         x.ConsumerId
      });

      return modelBuilder;
   }

   public static ModelBuilder ConfigureInboxOutboxEntities(this ModelBuilder modelBuilder)
   {
      modelBuilder.ConfigureOutboxMessageEntity();
      modelBuilder.ConfigureInboxMessageEntity();

      return modelBuilder;
   }
}