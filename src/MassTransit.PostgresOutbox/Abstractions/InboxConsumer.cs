using System.Data;
using EFCore.PostgresExtensions.Enums;
using EFCore.PostgresExtensions.Extensions;
using MassTransit.PostgresOutbox.Entities;
using MassTransit.PostgresOutbox.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MassTransit.PostgresOutbox.Abstractions;

public abstract class InboxConsumer<TMessage, TDbContext> : IConsumer<TMessage>
   where TMessage : class
   where TDbContext : DbContext, IInboxDbContext
{
   private readonly string _consumerId;
   private readonly IServiceProvider _sp;

   protected InboxConsumer(IServiceProvider sp)
   {
      _consumerId = GetType()
         .ToString();
      _sp = sp;
   }

   public async Task Consume(ConsumeContext<TMessage> context)
   {
      var messageId = context.Headers.Get<Guid>(Constants.OutboxMessageId) ?? context.MessageId;
      var dbContext = _sp.GetRequiredService<TDbContext>();
      var logger = _sp.GetRequiredService<ILogger<InboxConsumer<TMessage, TDbContext>>>();

      var exists =
         await dbContext.InboxMessages.AnyAsync(x => x.MessageId == messageId && x.ConsumerId == _consumerId);

      if (!exists)
      {
         dbContext.InboxMessages.Add(new InboxMessage
         {
            MessageId = messageId!.Value,
            CreatedAt = DateTime.UtcNow,
            State = MessageState.New,
            ConsumerId = _consumerId
         });

         await dbContext.SaveChangesAsync();
      }

      await using var transactionScope =
         await dbContext.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);

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
         await Consume(context.Message, transactionScope);

         inboxMessage.State = MessageState.Done;
         inboxMessage.UpdatedAt = DateTime.UtcNow;

         await dbContext.SaveChangesAsync();
         await transactionScope.CommitAsync();
      }
      catch (Exception ex)
      {
         logger.LogError(ex, "Exception thrown while consuming message {messageId} by {consumerId}",
            messageId,
            _consumerId);

         await transactionScope.RollbackAsync();

         await dbContext.InboxMessages
                        .Where(x => x.MessageId == messageId && x.ConsumerId == _consumerId)
                        .ExecuteUpdateAsync(x => x.SetProperty(x => x.UpdatedAt, x => DateTime.UtcNow));

         throw;
      }
   }

   protected abstract Task Consume(TMessage message, IDbContextTransaction transactionScope);
}