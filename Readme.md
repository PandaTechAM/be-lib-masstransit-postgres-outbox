# Pandatech.MassTransit.PostgresOutbox

Welcome to the Pandatech MassTransit PostgreSQL Outbox Extension repository. This library is designed to enhance
MassTransit's capabilities by introducing robust support for the Outbox and Inbox patterns with a particular focus on
PostgreSQL, alongside seamless integration with multiple DbContexts in Entity Framework Core. This extension is ideal
for developers seeking to ensure reliable message delivery and processing in distributed, microservice-oriented
architectures.

## Features

- **Multiple DbContext Support**: Operate within complex systems using multiple data contexts without hassle.
- **Outbox Pattern Implementation**: Reliably handle message sending operations, ensuring no messages are lost in
  transit, even in the event of system failures.
- **Inbox Pattern Support**: Process incoming messages effectively, preventing duplicate processing and ensuring message
  consistency.
- **PostgreSQL ForUpdate Concurrency Handling**: Utilize PostgreSQL's ForUpdate feature for enhanced concurrency
  control, making your message handling processes more robust.
- **Seamless Integration**: Designed to fit effortlessly into existing MassTransit and EF Core based projects.

## Getting Started

To get started with the Pandatech MassTransit PostgreSQL Outbox Extension, ensure you have the following prerequisites:

- .NET Core 8 or later
- An existing MassTransit project
- PostgreSQL database

## Installation

The library can be installed via NuGet Package Manager. Use the following command:

```bash
Install-Package Pandatech.MassTransit.PostgresOutbox
``` 

## Configuration

Before diving into the usage, it's essential to configure the Pandatech MassTransit PostgreSQL Outbox Extension in your
application. This involves setting up your DbContexts, configuring MassTransit to use the extension, and initializing
the Outbox and Inbox features.

Stay tuned for the next sections where we'll cover the usage details, showcasing how you can leverage this powerful
extension to enhance your distributed systems.

## Usage
Take into account that examples below are given for configuring both inbox and outbox patterns.
If you need only one of those , consider using appropriate methods available(eg. instead of AddOutboxInboxServices use AddInboxServices and etc).

Configuration

Entity Configuration: Ensure your DbContext implements the IOutboxDbContext and IInboxDbContext interfaces.
Configure your entities and generate migrations. 
Call ConfigureInboxOutboxEntities on your ModelBuilder to configure the necessary tables for inbox and outbox patterns.

```csharp

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.ConfigureInboxOutboxEntities();
}
```

And you need to call UseQueryLocks() inside AddDbContext or AddDbContextPool , this needs for enabling ForUpdate feature.

```csharp
  builder.Services.AddDbContextPool<PostgresContext>(options =>
         options.UseNpgsql(connectionString)
                .UseQueryLocks());
```

Service Registration: Register essential services on startup, specifying the DbContext type. 
You can optionally override settings(its optional parameter).

```csharp

services.AddOutboxInboxServices<PostgresContext>();
```

Publishing Messages (Outbox Pattern)

To publish a message using the outbox pattern, call the AddToOutbox method on your DbContext, 
specifying your message. Remember to call SaveChanges() to persist the message to the database.

```csharp

dbContext.Orders.Add(new Order
{
    Amount = 555,
    CreatedAt = DateTime.UtcNow,
});

// Add message to the outbox
dbContext.AddToOutbox(new OrderCreatedEvent());

// Save changes to the database
dbContext.SaveChanges();
```

Consuming Messages (Inbox Pattern)

To consume messages using the inbox pattern, create a consumer that inherits from
InboxConsumer<TMessage, TDbContext> class, specifying the message type and DbContext type as generic arguments.

```csharp

public class YourConsumer : InboxConsumer<YourMessage, PostgresContext>
{
    private readonly PostgresContext _context;

    public YourConsumer(PostgresContext dbContext, IServiceScopeFactory serviceScopeFactory)
        : base(serviceScopeFactory)
    {
        _context = dbContext;
    }

    public override async Task Consume(YourMessage message)
    {
        // Implement your message processing logic here
    }
}
```

## License

Pandatech.MassTransit.PostgresOutbox is licensed under the MIT License.