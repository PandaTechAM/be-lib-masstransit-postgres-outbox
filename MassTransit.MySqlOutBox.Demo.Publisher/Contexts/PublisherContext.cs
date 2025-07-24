using MassTransit.MySqlOutbox.Abstractions;
using MassTransit.MySqlOutbox.Entities;
using MassTransit.MySqlOutbox.Extensions;
using Microsoft.EntityFrameworkCore;

namespace MassTransit.MySqlOutbox.Demo.Contexts;
public class PublisherContext(DbContextOptions<PublisherContext> options) : DbContext(options)
   , IOutboxDbContext, IInboxDbContext
{
   public DbSet<InboxMessage> InboxMessages { get; set; }
   public DbSet<OutboxMessage> OutboxMessages { get; set; }
   
   protected override void OnModelCreating(ModelBuilder modelBuilder)
   {
      base.OnModelCreating(modelBuilder);

      modelBuilder.ApplyConfigurationsFromAssembly(typeof(PublisherContext).Assembly);
      modelBuilder.ConfigureInboxOutboxEntities();
   }
}