using MassTransit.PostgresOutbox.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MassTransit.PostgresOutbox.Jobs
{
    internal class InboxMessageRemovalService<TDbContext> : BackgroundService
       where TDbContext : DbContext, IInboxDbContext
    {
        private readonly int _beforeInDays;
        private readonly PeriodicTimer _timer;
        private readonly ILogger<InboxMessageRemovalService<TDbContext>> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public InboxMessageRemovalService(IServiceScopeFactory serviceScopeFactory, ILogger<InboxMessageRemovalService<TDbContext>> logger, Settings settings)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
            _timer = new(settings.InboxRemovalTimerPeriod);
            _beforeInDays = settings.InboxRemovalBeforeInDays;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (await _timer.WaitForNextTickAsync(cancellationToken))
            {
                _logger.LogInformation($"{nameof(InboxMessageRemovalService<TDbContext>)} started iteration");

                using var scope = _serviceScopeFactory.CreateScope();
                using var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();

                try
                {
                    var daysBefore = DateTime.UtcNow.AddDays(-_beforeInDays);

                    await dbContext.InboxMessages
                                   .Where(x => x.State == Enums.MessageState.Done)
                                   .Where(x => x.UpdatedAt < daysBefore)
                                   .ExecuteDeleteAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                }

                _logger.LogInformation($"{nameof(InboxMessageRemovalService<TDbContext>)} finished iteration");
            }
        }
    }
}
