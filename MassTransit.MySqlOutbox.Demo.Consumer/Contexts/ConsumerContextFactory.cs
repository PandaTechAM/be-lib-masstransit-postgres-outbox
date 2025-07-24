using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace MassTransit.MySqlOutbox.Demo.Consumer.Contexts;

public class ConsumerContextFactory : IDesignTimeDbContextFactory<ConsumerContext>
{
   public ConsumerContext CreateDbContext(string[] args)
   {
      var configuration = new ConfigurationBuilder()
         .SetBasePath(Directory.GetCurrentDirectory())
         .AddJsonFile("appsettings.json")
         .AddJsonFile("appsettings.Development.json", optional: true)
         .Build();
      var connectionString = configuration.GetConnectionString("DefaultConnection")
         ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

      var optionsBuilder = new DbContextOptionsBuilder<ConsumerContext>();
      optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

      return new ConsumerContext(optionsBuilder.Options);
   }
}