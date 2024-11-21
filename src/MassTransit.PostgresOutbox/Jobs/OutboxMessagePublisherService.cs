using System.Data;
using EFCore.PostgresExtensions.Enums;
using EFCore.PostgresExtensions.Extensions;
using MassTransit.PostgresOutbox.Abstractions;
using MassTransit.PostgresOutbox.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MassTransit.PostgresOutbox.Jobs;

internal class OutboxMessagePublisherService<TDbContext>(
   IServiceScopeFactory serviceScopeFactory,
   ILogger<OutboxMessagePublisherService<TDbContext>> logger,
   Settings settings)
   : BackgroundService
   where TDbContext : DbContext, IOutboxDbContext
{
   private readonly int _batchCount = settings.PublisherBatchCount;
   private readonly PeriodicTimer _timer = new(settings.PublisherTimerPeriod);

   protected override async Task ExecuteAsync(CancellationToken cancellationToken)
   {
      while (await _timer.WaitForNextTickAsync(cancellationToken))
      {
         logger.LogDebug($"{nameof(OutboxMessagePublisherService<TDbContext>)} started iteration");

         using var scope = serviceScopeFactory.CreateScope();
         await using var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();
         var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
         await using var transactionScope =
            await dbContext.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted,
               cancellationToken);

         try
         {
            var messages = await dbContext.OutboxMessages
                                          .Where(x => x.State == MessageState.New)
                                          .OrderBy(x => x.CreatedAt)
                                          .ForUpdate(LockBehavior.SkipLocked)
                                          .Take(_batchCount)
                                          .ToListAsync(cancellationToken);

            if (messages.Count == 0)
            {
               continue;
            }

            var publishedMessageIds = new List<Guid>(messages.Count);

            foreach (var message in messages)
            {
               try
               {
                  var type = Type.GetType(message.Type);

                  var messageObject = JsonConvert.DeserializeObject(message.Payload, type!);

                  await publishEndpoint.Publish(messageObject!,
                     type!,
                     x => x.Headers.Set(Constants.OutboxMessageId, message.Id),
                     cancellationToken);

                  publishedMessageIds.Add(message.Id);
               }
               catch (Exception ex)
               {
                  logger.LogError(ex, ex.Message);
               }
            }

            if (publishedMessageIds.Count == 0)
            {
               continue;
            }

            var utcNow = DateTime.UtcNow;

            await dbContext.OutboxMessages
                           .Where(b => publishedMessageIds.Contains(b.Id))
                           .ExecuteUpdateAsync(x => x.SetProperty(m => m.State, MessageState.Done)
                                                     .SetProperty(m => m.UpdatedAt, utcNow),
                              cancellationToken);

            await dbContext.SaveChangesAsync(cancellationToken);
            await transactionScope.CommitAsync(cancellationToken);
         }
         catch (Exception ex)
         {
            logger.LogError(ex, ex.Message);
            await transactionScope.RollbackAsync(cancellationToken);
         }

         logger.LogDebug($"{nameof(OutboxMessagePublisherService<TDbContext>)} finished iteration");
      }
   }
}