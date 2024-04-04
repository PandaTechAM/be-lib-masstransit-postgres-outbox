using MassTransit.PostgresOutbox.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MassTransit.PostgresOutbox.Jobs;

internal class InboxMessageRemovalService<TDbContext>(
    IServiceScopeFactory serviceScopeFactory,
    ILogger<InboxMessageRemovalService<TDbContext>> logger,
    Settings settings)
    : BackgroundService
    where TDbContext : DbContext, IInboxDbContext
{
    private readonly int _beforeInDays = settings.InboxRemovalBeforeInDays;
    private readonly PeriodicTimer _timer = new(settings.InboxRemovalTimerPeriod);

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (await _timer.WaitForNextTickAsync(cancellationToken))
        {
            logger.LogInformation($"{nameof(InboxMessageRemovalService<TDbContext>)} started iteration");

            using var scope = serviceScopeFactory.CreateScope();
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
                logger.LogError(ex, ex.Message);
            }

            logger.LogInformation($"{nameof(InboxMessageRemovalService<TDbContext>)} finished iteration");
        }
    }
}