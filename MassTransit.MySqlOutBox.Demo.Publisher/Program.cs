using System.Net;
using System.Net.Sockets;
using CommandLine;
using MassTransit.MySqlOutbox.Demo.Contexts;
using MassTransit.MySqlOutbox.Demo.Publisher;
using MassTransit.MySqlOutbox.Demo.Services;
using MassTransit.MySqlOutbox.Demo.Shared.Extensions;
using MassTransit.MySqlOutbox.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var parserResult = Parser.Default.ParseArguments<Options>(args);

if (parserResult.Errors.Any())
{
   Console.WriteLine(parserResult.Errors.ToString());// Handle errors
   return;
}

var publisherId = parserResult.Value.Id;
var automaticallyPublish = parserResult.Value.Auto;

var builder = WebApplication.CreateBuilder(args);

var defaultPort = 5000;
var portToUse = FindAvailablePort(defaultPort);

builder.WebHost.UseUrls($"http://localhost:{portToUse}");

Console.WriteLine($"Using port {portToUse}");

var configuration = builder.Configuration;
var connectionString = configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
   throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
}

if (automaticallyPublish)
{
   builder.Services.AddHostedService<AutoPublisherService>();
}

builder.AddMassTransit(configuration, typeof(Program).Assembly);

builder.AddMySqlContext<PublisherContext>(connectionString);
builder.AddMySqlContext<PublisherContext>(connectionString);

builder.Services.AddOutboxInboxServices<PublisherContext>();

builder.Services.AddDbContext<ContextForDDDEntity>(o =>
   {
      o.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
      o.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
   });

builder.Services.AddScoped<PublishService>();
builder.Services.AddScoped<DDDEntityCreationService>();
builder.Services.AddSingleton<Options>(parserResult.Value);

var app = builder.Build();

app.MigrateDatabase<PublisherContext>();
app.UseSwagger();
app.UseSwaggerUI();

app.MapPost("/publish",
   async ([FromServices] PublishService service) =>
   {
      await service.PublishComplexEventAsync();
      return Results.Ok();
   });

app.MapPost("/create",
   async ([FromServices] DDDEntityCreationService service) =>
   {
      await service.CreateNewEntity(publisherId);
      return Results.Ok();
   });


Console.WriteLine($"Publisher {publisherId} is running...");

app.Run();


static int FindAvailablePort(int startingPort)
{
   for (int port = startingPort; port < startingPort + 100; port++)
   {
      try
      {
         TcpListener listener = new TcpListener(IPAddress.Loopback, port);
         listener.Start();
         listener.Stop();
         return port;
      }
      catch (SocketException)
      {
         // Port is in use, try next
      }
   }
   throw new Exception("No available ports found.");
}


public class Options
{
   [Option('i', "id", Required = false, HelpText = "Identifier for the publisher instance", Default = "1")]
   public string Id { get; set; } = "";

   [Option("auto", Required = false, HelpText = "Automatically send messages in a loop.", Default = false)]
   public bool Auto { get; set; } = false;
}