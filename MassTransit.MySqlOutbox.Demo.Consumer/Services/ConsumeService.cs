using MassTransit.MySqlOutbox.Abstractions;
using MassTransit.MySqlOutbox.Demo.Consumer.Contexts;
using MassTransit.MySqlOutbox.Demo.Shared.Events;
using Microsoft.EntityFrameworkCore.Storage;

namespace MassTransit.MySqlOutbox.Demo.Consumer.Services;

public class ConsumeService(ConsumerContext dbContext, IServiceProvider sp)
   : InboxConsumer<ComplexObjectEvent, ConsumerContext>(sp)
{
   protected override Task Consume(ComplexObjectEvent message, IDbContextTransaction dbContextTransaction)
   {
      var original = ComplexObjectEvent.Init();
      var match = message.Equals(original);
      Console.WriteLine($"Match: {match}");
      return Task.CompletedTask;
   }
}