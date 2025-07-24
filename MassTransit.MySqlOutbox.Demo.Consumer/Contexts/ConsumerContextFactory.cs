using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MassTransit.MySqlOutbox.Demo.Consumer.Contexts;

public class ConsumerContextFactory : IDesignTimeDbContextFactory<ConsumerContext>
{
   public ConsumerContext CreateDbContext(string[] args)
   {
      var optionsBuilder = new DbContextOptionsBuilder<ConsumerContext>();

      optionsBuilder
         .UseMySql();

      return new ConsumerContext(optionsBuilder.Options);
   }
}