using MassTransit.MySqlOutbox.Abstractions;
using MassTransit.MySqlOutbox.Demo.Consumer.Contexts;
using MassTransit.MySqlOutbox.Demo.Shared.Events;
using Microsoft.EntityFrameworkCore.Storage;

namespace MassTransit.MySqlOutbox.Demo.Consumer.Services;

#pragma warning disable CS9113 // Parameter is unread.

public class ConsumeService(ConsumerContext dbContext, IServiceProvider sp)
   : InboxConsumer<ComplexObjectEvent, ConsumerContext>(sp)

#pragma warning restore CS9113 // Parameter is unread.
{
   protected override Task Consume(ComplexObjectEvent message, IDbContextTransaction dbContextTransaction, CancellationToken ct)
   {
      Console.WriteLine($"Message received at {DateTime.Now}");
      var original = ComplexObjectEvent.Init();
      var match = message.Equals(original);
      Console.WriteLine($"Match: {match}");
      return Task.CompletedTask;
   }
}