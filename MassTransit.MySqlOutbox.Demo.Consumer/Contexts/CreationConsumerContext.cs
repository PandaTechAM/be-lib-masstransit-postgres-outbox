using MassTransit.MySqlOutbox.Abstractions;
using MassTransit.MySqlOutbox.Entities;
using MassTransit.MySqlOutbox.Extensions;
using Microsoft.EntityFrameworkCore;

namespace MassTransit.MySqlOutbox.Demo.Consumer.Contexts;
public class CreationConsumerContext(DbContextOptions<CreationConsumerContext> options) : DbContext(options)
   , IInboxDbContext
{
   public DbSet<InboxMessage> InboxMessages { get; set; }
   //public DbSet<OutboxMessage> OutboxMessages { get; set; } // We don't publish messages in this context

   protected override void OnModelCreating(ModelBuilder modelBuilder)
   {
      base.OnModelCreating(modelBuilder);

      modelBuilder.ApplyConfigurationsFromAssembly(typeof(CreationConsumerContext).Assembly);
      modelBuilder.ConfigureInboxOutboxEntities();
   }
}