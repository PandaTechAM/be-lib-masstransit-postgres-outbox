﻿using MassTransit.PostgresOutbox;
using MassTransit.PostgresOutbox.Abstractions;
using MassTransit.PostgresOutbox.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MassTransit.PostgresOutbox.Jobs;

internal class OutboxMessageRemovalService<TDbContext>(
    IServiceScopeFactory serviceScopeFactory,
    ILogger<OutboxMessageRemovalService<TDbContext>> logger,
    Settings settings)
    : BackgroundService
    where TDbContext : DbContext, IOutboxDbContext
{
    private readonly int _beforeInDays = settings.OutboxRemovalBeforeInDays;
    private readonly PeriodicTimer _timer = new(settings.OutboxRemovalTimerPeriod);

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (await _timer.WaitForNextTickAsync(cancellationToken))
        {
            logger.LogDebug($"{nameof(OutboxMessageRemovalService<TDbContext>)} started iteration");

            using var scope = serviceScopeFactory.CreateScope();
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
                logger.LogError(ex, ex.Message);
            }

            logger.LogDebug($"{nameof(OutboxMessageRemovalService<TDbContext>)} finished iteration");
        }
    }
}