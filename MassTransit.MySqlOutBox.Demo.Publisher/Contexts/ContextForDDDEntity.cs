using MassTransit.MySqlOutbox.Demo.Shared.Entities;
using MassTransit.MySqlOutbox.Entities;
using Microsoft.EntityFrameworkCore;
using MassTransit.MySqlOutbox.Extensions;
using MassTransit.MySqlOutbox.Abstractions;
using MassTransit.MySqlOutbox.Demo.Shared.Events;

public class ContextForDDDEntity : DbContext, IOutboxDbContext
{
   public DbSet<OutboxMessage> OutboxMessages { get; set; }
   public DbSet<DDDEntity> DDDEntities { get; set; }

   public ContextForDDDEntity(DbContextOptions<ContextForDDDEntity> options) : base(options)
   {
   }

   protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
   {
      configurationBuilder.IgnoreAny<DomainEvent>();
   }

   public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
   {
      var updatedRecords = 0;

      try
      {
         var entitiesWithEvents = ChangeTracker.Entries<DDDEntity>().Where(e => e.Entity.DomainEvents.Count > 0).Select(e => e.Entity).ToList();

         foreach (var entity in entitiesWithEvents)
         {
            foreach (var domainEvent in entity.DomainEvents)
            {
               //make generic so we can use the subclassed domain event type
               //otherwise the type is `DomainEvent`
               var method = typeof(OutboxDbContextExtensions)
                  .GetMethod(nameof(OutboxDbContextExtensions.AddToOutbox))!
                  .MakeGenericMethod(domainEvent.GetType());

               method.Invoke(null, [this, domainEvent]);
            }
            entity.ClearDomainEvents();
         }
         updatedRecords = await base.SaveChangesAsync(cancellationToken);
      }
      catch (System.Exception)
      {
         throw;
      }
      return updatedRecords;

   }
}