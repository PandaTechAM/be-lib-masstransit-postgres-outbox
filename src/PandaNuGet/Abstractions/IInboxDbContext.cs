using MassTransit.PostgresOutbox.Entities;
using Microsoft.EntityFrameworkCore;

namespace MassTransit.PostgresOutbox.Abstractions
{
    public interface IInboxDbContext
    {
        public DbSet<InboxMessage> InboxMessages { get; set; }
    }
}
