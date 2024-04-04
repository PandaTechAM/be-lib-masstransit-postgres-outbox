using MassTransit.PostgresOutbox.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinHub.Mock1.Box.Abstractions
{
   public interface IOutboxDbContext
   {
      public DbSet<OutboxMessage> OutboxMessages { get; set; }
   }
}
