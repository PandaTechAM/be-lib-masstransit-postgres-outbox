using System.Text.Json.Serialization;

namespace MassTransit.MySqlOutbox.Demo.Shared.Events;

public record EntityCreated : DomainEvent
{
   public int EntityId { get; init; }
   public DateTimeOffset EventTime { get; init; }

   [JsonConstructor]
   public EntityCreated(int EntityId, DateTimeOffset EventTime)
   {
      this.EntityId = EntityId;
      this.EventTime = EventTime;
   }

   public EntityCreated() : this(0, DateTimeOffset.UtcNow) { }

   public override string ToString() => $"EntityCreated: {EntityId} at {EventTime}";
}