using EFCore.PostgresExtensions.Enums;
using EFCore.PostgresExtensions.Extensions;
using MassTransit.PostgresOutbox.Entities;
using MassTransit.PostgresOutbox.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MassTransit.PostgresOutbox.Abstractions;

public abstract class InboxConsumer<TMessage, TDbContext> : IConsumer<TMessage>
    where TMessage : class
    where TDbContext : DbContext, IInboxDbContext
{
    private readonly string _consumerId;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    protected InboxConsumer(IServiceScopeFactory serviceScopeFactory)
    {
        _consumerId = GetType().ToString();
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task Consume(ConsumeContext<TMessage> context)
    {
        var messageId = context.Headers.Get<Guid>(Constants.OutboxMessageId);

        if (messageId is null)
        {
            await Consume(context.Message);
            return;
        }

        using var scope = _serviceScopeFactory.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<InboxConsumer<TMessage, TDbContext>>>();

        var exists =
            await dbContext.InboxMessages.AnyAsync(x => x.MessageId == messageId && x.ConsumerId == _consumerId);

        if (!exists)
        {
            dbContext.InboxMessages.Add(new InboxMessage
            {
                MessageId = messageId.Value,
                CreatedAt = DateTime.UtcNow,
                State = MessageState.New,
                ConsumerId = _consumerId,
            });

            await dbContext.SaveChangesAsync();
        }

        await using var transactionScope =
            await dbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted);

        var inboxMessage = await dbContext.InboxMessages
            .Where(x => x.MessageId == messageId)
            .Where(x => x.ConsumerId == _consumerId)
            .Where(x => x.State == MessageState.New)
            .ForUpdate(LockBehavior.SkipLocked)
            .FirstOrDefaultAsync();

        if (inboxMessage == null)
        {
            return;
        }

        try
        {
            await Consume(context.Message);
            inboxMessage.State = MessageState.Done;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception thrown while consuming message");
            throw;
        }
        finally
        {
            inboxMessage.UpdatedAt = DateTime.UtcNow;
            await dbContext.SaveChangesAsync();
            await transactionScope.CommitAsync();
        }
    }

    protected abstract Task Consume(TMessage message);
}