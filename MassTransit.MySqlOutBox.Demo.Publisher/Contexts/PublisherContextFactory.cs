using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MassTransit.MySqlOutbox.Demo.Contexts;

public class PublisherContextFactory : IDesignTimeDbContextFactory<PublisherContext>
{
   public PublisherContext CreateDbContext(string[] args)
   {
      var configuration = new ConfigurationBuilder()
         .SetBasePath(Directory.GetCurrentDirectory())
         .AddJsonFile("appsettings.json")
         .AddJsonFile("appsettings.Development.json", optional: true)
         .Build();

      var optionsBuilder = new DbContextOptionsBuilder<PublisherContext>();

      string connectionString = configuration.GetConnectionString("DefaultConnection")
         ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

      optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

      return new PublisherContext(optionsBuilder.Options);
   }
}