using MassTransit.MySqlOutbox.Demo.Contexts;
using MassTransit.MySqlOutbox.Demo.Shared.Events;
using MassTransit.MySqlOutbox.Extensions;

namespace MassTransit.MySqlOutbox.Demo.Services;

public class PublishService(PublisherContext dbContext)
{
   public async Task PublishComplexEventAsync()
   {
      var complexEvent = ComplexObjectEvent.Init();
      var messageId = dbContext.AddToOutbox(complexEvent);
      Console.WriteLine($"Message added to outbox at {DateTime.Now} (Id: {messageId})");
      await dbContext.SaveChangesAsync();
   }
}