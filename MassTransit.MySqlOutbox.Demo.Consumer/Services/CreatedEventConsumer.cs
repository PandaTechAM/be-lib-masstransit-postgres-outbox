using MassTransit.MySqlOutbox.Abstractions;
using MassTransit.MySqlOutbox.Demo.Consumer.Contexts;
using MassTransit.MySqlOutbox.Demo.Shared.Events;
using Microsoft.EntityFrameworkCore.Storage;

namespace MassTransit.MySqlOutbox.Demo.Consumer.Services;

#pragma warning disable CS9113 // Parameter is unread.

public class CreatedEventConsumer(CreationConsumerContext dbContext, IServiceProvider sp, Options options)
   : InboxConsumer<EntityCreated, CreationConsumerContext>(sp)
#pragma warning restore CS9113 // Parameter is unread.
{
   protected override Task Consume(EntityCreated message, IDbContextTransaction dbContextTransaction, CancellationToken ct)
   {
      var id  = message.EntityId;
      Console.WriteLine($"[{options.ConsumerId}] DDDEntity with id {id} was created at {message.OccurredAt}. Event received at {DateTime.Now} from publisher {message.PublisherId}.");
      switch (DateTime.Now.Millisecond % 3)
      {
         // case 0:
         //    Console.WriteLine($" ------ >>>> Application Exception thrown for {id}");
         //    throw new ApplicationException("Exception throwing simulation");
         // case 1:
         //    Console.WriteLine($" ------ >>>> End of stream exception thrown for {id}");
         //    throw new EndOfStreamException();
         default:
            return Task.CompletedTask;
      }

   }
}