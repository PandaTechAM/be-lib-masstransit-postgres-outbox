using MassTransit.MySqlOutbox.Demo.Consumer.Contexts;
using MassTransit.MySqlOutbox.Demo.Shared.Extensions;
using MassTransit.MySqlOutbox.Extensions;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;
var connectionString = configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
   throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
}

builder.Services.AddEndpointsApiExplorer();
builder.AddMassTransit(configuration, typeof(Program).Assembly);

builder.AddMySqlContext<ConsumerContext>(connectionString);
builder.Services.AddOutboxInboxServices<ConsumerContext>();

var app = builder.Build();
// app.MigrateDatabase<ConsumerContext>(); <<- run migrations from the publisher project

app.Run();