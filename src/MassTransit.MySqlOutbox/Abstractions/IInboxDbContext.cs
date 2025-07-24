using MassTransit.MySqlOutbox.Entities;
using Microsoft.EntityFrameworkCore;

namespace MassTransit.MySqlOutbox.Abstractions;

public interface IInboxDbContext
{
   public DbSet<InboxMessage> InboxMessages { get; set; }
}