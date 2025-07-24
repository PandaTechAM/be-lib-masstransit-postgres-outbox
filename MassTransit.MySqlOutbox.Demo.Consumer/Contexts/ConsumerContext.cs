using MassTransit.MySqlOutbox.Abstractions;
using MassTransit.MySqlOutbox.Entities;
using MassTransit.MySqlOutbox.Extensions;
using Microsoft.EntityFrameworkCore;

namespace MassTransit.MySqlOutbox.Demo.Consumer.Contexts;
public class ConsumerContext(DbContextOptions<ConsumerContext> options) : DbContext(options)
   , IOutboxDbContext, IInboxDbContext
{
   public DbSet<InboxMessage> InboxMessages { get; set; }
   public DbSet<OutboxMessage> OutboxMessages { get; set; }
   
   protected override void OnModelCreating(ModelBuilder modelBuilder)
   {
      base.OnModelCreating(modelBuilder);

      modelBuilder.ApplyConfigurationsFromAssembly(typeof(ConsumerContext).Assembly);
      modelBuilder.ConfigureInboxOutboxEntities();
   }
}