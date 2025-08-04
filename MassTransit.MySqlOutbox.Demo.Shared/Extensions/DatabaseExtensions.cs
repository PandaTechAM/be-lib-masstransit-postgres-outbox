using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MassTransit.MySqlOutbox.Demo.Shared.Extensions;

public static class DatabaseExtensions
{
   public static WebApplicationBuilder AddMySqlContext<TContext>(this WebApplicationBuilder builder,
      string connectionString) where TContext : DbContext
   {
      builder.Services.AddDbContextPool<TContext>((sp, options) =>
      {
         options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
         // options.UseLoggerFactory(LoggerFactory.Create(c => c.AddConsole().AddFilter(l => l >= LogLevel.Information)));
         // options.EnableSensitiveDataLogging();
         // options.EnableDetailedErrors();
         // options.EnableThreadSafetyChecks();
      });

      return builder;
   }

   public static IServiceCollection AddMySqlContext<TContext>(this IServiceCollection services,
      string connectionString) where TContext : DbContext
   {
      services.AddDbContextPool<TContext>((sp, options) =>
      {
         options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
      });

      return services;
   }


   public static WebApplication MigrateDatabase<TContext>(this WebApplication app)
      where TContext : DbContext
   {
      using var scope = app.Services.CreateScope();
      var dbContext = scope.ServiceProvider.GetRequiredService<TContext>();
      dbContext.Database.Migrate();
      return app;
   }
}