using MassTransit.PostgresOutbox.Entities;
using Microsoft.EntityFrameworkCore;

namespace MassTransit.PostgresOutbox.Abstractions;

public interface IInboxDbContext
{
   DbSet<InboxMessage> InboxMessages { get; set; }
}