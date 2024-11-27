using MassTransit.PostgresOutbox.Abstractions;
using MassTransit.PostgresOutbox.Demo.Consumer.Contexts;
using MassTransit.PostgresOutbox.Demo.Shared.Events;

namespace MassTransit.PostgresOutbox.Demo.Consumer.Services;

public class ConsumeService(ConsumerContext dbContext, IServiceScopeFactory serviceScopeFactory)
   : InboxConsumer<ComplexObjectEvent, ConsumerContext>(serviceScopeFactory)
{
   protected override Task Consume(ComplexObjectEvent message)
   {
      var original = ComplexObjectEvent.Init();
      var match = message.Equals(original);
      Console.WriteLine($"Match: {match}");
      return Task.CompletedTask;
   }
}