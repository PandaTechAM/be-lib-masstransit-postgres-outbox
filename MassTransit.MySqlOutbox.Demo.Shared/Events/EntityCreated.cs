using System.Text.Json.Serialization;

namespace MassTransit.MySqlOutbox.Demo.Shared.Events;

public record EntityCreated : DomainEvent
{
   public int EntityId { get; init; }
   public string PublisherId { get; init; }
   public DateTimeOffset EventTime { get; init; }

   [JsonConstructor]
   public EntityCreated(int EntityId, string publisherId, DateTimeOffset EventTime)
   {
      this.EntityId = EntityId;
      this.PublisherId = publisherId;
      this.EventTime = EventTime;
   }

   public EntityCreated() : this(0, "", DateTimeOffset.UtcNow) { }

   public override string ToString() => $"EntityCreated: {EntityId} at {EventTime} by publisher {PublisherId}";
}