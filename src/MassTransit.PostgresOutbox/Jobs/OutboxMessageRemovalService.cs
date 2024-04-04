using FinHub.Mock1.Box.Abstractions;
using MassTransit.PostgresOutbox;
using MassTransit.PostgresOutbox.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MassTransit.PostgresOutbox.Jobs
{
   internal class OutboxMessageRemovalService<TDbContext> : BackgroundService
      where TDbContext : DbContext, IOutboxDbContext
   {
      private readonly int _beforeInDays;
      private readonly PeriodicTimer _timer;
      private readonly ILogger<OutboxMessageRemovalService<TDbContext>> _logger;
      private readonly IServiceScopeFactory _serviceScopeFactory;

      public OutboxMessageRemovalService(IServiceScopeFactory serviceScopeFactory, ILogger<OutboxMessageRemovalService<TDbContext>> logger, Settings settings)
      {
         _serviceScopeFactory = serviceScopeFactory;
         _logger = logger;
         _timer = new(settings.OutboxRemovalTimerPeriod);
         _beforeInDays = settings.OutboxRemovalBeforeInDays;
      }

      protected override async Task ExecuteAsync(CancellationToken cancellationToken)
      {
         while (await _timer.WaitForNextTickAsync(cancellationToken))
         {
            _logger.LogInformation($"{nameof(OutboxMessageRemovalService<TDbContext>)} started iteration");

            using var scope = _serviceScopeFactory.CreateScope();
            using var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();

            try
            {
               var daysBefore = DateTime.UtcNow.AddDays(-_beforeInDays);

               await dbContext.OutboxMessages
                              .Where(x => x.State == MessageState.Done)
                              .Where(x => x.UpdatedAt < daysBefore)
                              .ExecuteDeleteAsync(cancellationToken);
            }
            catch (Exception ex)
            {
               _logger.LogError(ex, ex.Message);
            }

            _logger.LogInformation($"{nameof(OutboxMessageRemovalService<TDbContext>)} finished iteration");
         }
      }
   }
}
