using System.Runtime.Serialization;
using MassTransit.MySqlOutbox.Demo.Shared.Events;

namespace MassTransit.MySqlOutbox.Demo.Shared.Entities;

public class DDDEntity
{
   private readonly List<DomainEvent> _domainEvents = new();
   public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents;

   protected void RaiseDomainEvent(DomainEvent domainEvent) => _domainEvents.Add(domainEvent);
   public void ClearDomainEvents() => _domainEvents.Clear();
   private static readonly Random random = new(Guid.NewGuid().GetHashCode());

   public int Id { get; private init; }

   public DDDEntity()
   {
   }

   public DDDEntity(string publisherId)
   {
      Id = random.Next(1, 1000000);
      RaiseDomainEvent(new EntityCreated(Id, publisherId ?? "", DateTimeOffset.UtcNow));
   }

}