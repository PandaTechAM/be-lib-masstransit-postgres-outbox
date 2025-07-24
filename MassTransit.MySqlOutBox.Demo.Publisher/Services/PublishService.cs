using MassTransit.MySqlOutbox.Demo.Contexts;
using MassTransit.MySqlOutbox.Demo.Shared.Events;
using MassTransit.MySqlOutbox.Extensions;

namespace MassTransit.MySqlOutbox.Demo.Services;

public class PublishService(PublisherContext dbContext)
{
   public async Task PublishComplexEventAsync()
   {
      var complexEvent = ComplexObjectEvent.Init();
      dbContext.AddToOutbox(complexEvent);
      await dbContext.SaveChangesAsync();
   }
}