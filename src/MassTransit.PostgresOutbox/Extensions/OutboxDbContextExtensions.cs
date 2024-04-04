using MassTransit.PostgresOutbox.Abstractions;
using MassTransit.PostgresOutbox.Entities;
using Newtonsoft.Json;

namespace MassTransit.PostgresOutbox.Extensions
{
    public static class OutboxDbContextExtensions
   {
      public static Guid AddToOutbox<T>(this IOutboxDbContext dbContext, T message)
      {
         var entity = new OutboxMessage
         {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            State = Enums.MessageState.New,
            UpdatedAt = null,
            Payload = JsonConvert.SerializeObject(message),
            Type = typeof(T).AssemblyQualifiedName!
         };

         dbContext.OutboxMessages.Add(entity);

         return entity.Id;
      }
   }
}
