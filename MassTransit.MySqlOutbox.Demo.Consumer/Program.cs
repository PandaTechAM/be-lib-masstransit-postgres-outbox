using CommandLine;
using MassTransit;
using MassTransit.MySqlOutbox.Demo.Consumer.Contexts;
using MassTransit.MySqlOutbox.Demo.Shared.Extensions;
using MassTransit.MySqlOutbox.Extensions;

var parserResult = Parser.Default.ParseArguments<Options>(args);

if (parserResult.Errors.Any())
{
   Console.WriteLine(parserResult.Errors.ToString());// Handle errors
   return;
}

var consumerId = parserResult.Value.ConsumerId;

var configuration = new ConfigurationBuilder()
   .AddJsonFile("appsettings.json")
   .AddEnvironmentVariables()
   .Build();

var connectionString = configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
   throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
}

var host = Host.CreateDefaultBuilder(args)
   .ConfigureLogging(logging =>
   {
      logging.AddConsole();
      logging.AddDebug();
      logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);
   })
   .ConfigureServices((context, services) =>
   {
      services.AddSingleton(new Options() { ConsumerId = consumerId });
      services.AddMassTransit(x =>
      {
         x.AddConsumers(typeof(Program).Assembly);
         x.SetKebabCaseEndpointNameFormatter();
         x.UsingRabbitMq((context, cfg) =>
         {
            cfg.Host(configuration.GetConnectionString("RabbitMq"));
            cfg.ConfigureEndpoints(context);
            cfg.UseMessageRetry(r =>
            {
               r.Ignore<ApplicationException>();
               r.Exponential(10, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(100), TimeSpan.FromSeconds(2));
            });
         });
      });

      services.AddMySqlContext<ConsumerContext>(connectionString);
      services.AddOutboxInboxServices<ConsumerContext>();
      services.AddMySqlContext<CreationConsumerContext>(connectionString);
   })
   .Build();


Console.WriteLine($"Consumer {consumerId} is running...");
Console.WriteLine("Waiting for messages");

await host.RunAsync();

public record Options
{
   [Option('i', "id", Required = false, HelpText = "Identifier for the publisher instance", Default = "1")]
   public string? ConsumerId { get; init; }
};