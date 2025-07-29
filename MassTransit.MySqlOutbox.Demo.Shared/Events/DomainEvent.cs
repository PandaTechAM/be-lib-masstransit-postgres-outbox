namespace MassTransit.MySqlOutbox.Demo.Shared.Events;

public abstract record DomainEvent
{
   protected DomainEvent()
   {
      OccurredAt = TimeProvider.System.GetUtcNow();
   }

   public DateTimeOffset OccurredAt { get; }
}
