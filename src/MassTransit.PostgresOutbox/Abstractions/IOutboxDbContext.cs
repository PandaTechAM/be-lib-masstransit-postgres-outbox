using MassTransit.PostgresOutbox.Entities;
using Microsoft.EntityFrameworkCore;

namespace MassTransit.PostgresOutbox.Abstractions
{
    public interface IOutboxDbContext
    {
        public DbSet<OutboxMessage> OutboxMessages { get; set; }
    }
}
