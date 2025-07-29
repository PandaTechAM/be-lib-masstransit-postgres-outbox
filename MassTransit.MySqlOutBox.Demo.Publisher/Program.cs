using System.Reflection;
using MassTransit.MySqlOutbox.Demo;
using MassTransit.MySqlOutbox.Demo.Contexts;
using MassTransit.MySqlOutbox.Demo.Services;
using MassTransit.MySqlOutbox.Demo.Shared.Extensions;
using MassTransit.MySqlOutbox.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;
var connectionString = configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
   throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
}

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
      await service.CreateNewEntity();
      return Results.Ok();
   });

Console.WriteLine("Publisher is running...");

app.Run();