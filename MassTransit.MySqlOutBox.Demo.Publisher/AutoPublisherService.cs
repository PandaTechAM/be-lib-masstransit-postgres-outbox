using MassTransit.MySqlOutbox.Demo.Services;

namespace MassTransit.MySqlOutbox.Demo.Publisher;

internal class AutoPublisherService(
   IServiceScopeFactory serviceScopeFactory,
   ILogger<AutoPublisherService> logger)
   : BackgroundService
{
   private readonly PeriodicTimer _timer = new(TimeSpan.FromMilliseconds(1000));

   protected override async Task ExecuteAsync(CancellationToken cancellationToken)
   {
      while (await _timer.WaitForNextTickAsync(cancellationToken))
      {
         try
         {
            using var scope = serviceScopeFactory.CreateScope();
            var entityCreationService = scope.ServiceProvider.GetRequiredService<DDDEntityCreationService>();
            var options = scope.ServiceProvider.GetRequiredService<Options>();
            await entityCreationService.CreateNewEntity(options.Id);
         }
         catch (Exception ex)
         {
            logger.LogError(ex, "Exception while publishing");
         }
      }
   }
}
