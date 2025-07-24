using MassTransit.MySqlOutbox.Abstractions;
using MassTransit.MySqlOutbox.Jobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MassTransit.MySqlOutbox.Extensions;

public static class ServiceCollectionExtensions
{
   public static IServiceCollection AddOutboxInboxServices<TDbContext>(this IServiceCollection services,
      Settings settings = null!)
      where TDbContext : DbContext, IOutboxDbContext, IInboxDbContext
   {
      return services.AddSettings(settings)
                     .AddHostedService<OutboxMessagePublisherService<TDbContext>>()
                     .AddHostedService<OutboxMessageRemovalService<TDbContext>>()
                     .AddHostedService<InboxMessageRemovalService<TDbContext>>();
   }

   public static IServiceCollection AddOutboxPublisherJob<TDbContext>(this IServiceCollection services,
      Settings settings = null!)
      where TDbContext : DbContext, IOutboxDbContext
   {
      services.AddSettings(settings);
      services.AddHostedService<OutboxMessagePublisherService<TDbContext>>();
      return services;
   }

   public static IServiceCollection AddOutboxRemovalJob<TDbContext>(this IServiceCollection services,
      Settings settings = null!)
      where TDbContext : DbContext, IOutboxDbContext
   {
      services.AddSettings(settings);
      services.AddHostedService<OutboxMessageRemovalService<TDbContext>>();
      return services;
   }

   public static IServiceCollection AddInboxRemovalJob<TDbContext>(this IServiceCollection services,
      Settings settings = null!)
      where TDbContext : DbContext, IInboxDbContext
   {
      services.AddSettings(settings);
      services.AddHostedService<InboxMessageRemovalService<TDbContext>>();
      return services;
   }

   public static IServiceCollection AddSettings(this IServiceCollection services, Settings settings)
   {
      settings ??= new Settings();

      services.AddSingleton(settings);

      return services;
   }
}