using MassTransit.MySqlOutbox.Entities;
using Microsoft.EntityFrameworkCore;

namespace MassTransit.MySqlOutbox.Abstractions;

public interface IOutboxDbContext
{
   public DbSet<OutboxMessage> OutboxMessages { get; set; }
}