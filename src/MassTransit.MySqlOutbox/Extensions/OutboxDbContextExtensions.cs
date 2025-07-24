using System.Text.Json;
using MassTransit.MySqlOutbox.Abstractions;
using MassTransit.MySqlOutbox.Entities;
using MassTransit.MySqlOutbox.Enums;

namespace MassTransit.MySqlOutbox.Extensions;

public static class OutboxDbContextExtensions
{
   public static Guid AddToOutbox<T>(this IOutboxDbContext dbContext, T message)
   {
      var entity = new OutboxMessage
      {
         Id = Guid.NewGuid(),
         CreatedAt = DateTime.UtcNow,
         State = MessageState.New,
         UpdatedAt = null,
         Payload = JsonSerializer.Serialize(message),
         Type = typeof(T).AssemblyQualifiedName!
      };

      dbContext.OutboxMessages.Add(entity);

      return entity.Id;
   }
}