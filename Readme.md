# Overview

Credit where it's due 🙂. This library is based on the [Pandatech.MassTransit.PostgresOutbox](https://github.com/PandaTechAM/be-lib-masstransit-postgres-outbox), adjusted for use with MySQL.

# PropertyMe.MassTransit.MySqlOutbox

The PropertyMe MySqlOutbox for [MassTransit](https://masstransit.io/) provides an alternative to MassTransit's
inbuilt transactional outbox implementation.

The default MassTransit transactional outbox assumes you are using a single DbContext, and that your DbContext has
change tracking enabled. This is fine for many people, but it you want to use multiple DbContexts, such as in a modular monolith,
or you wish to disable change tracking for performance reasons, then you need an alternative.

This library is one such alternative. It is built for MySQL and provides seamless integration with multiple DbContexts in Entity Framework Core
 and you can use it with DbContexts where you have disabled change tracking and manually attach entities when saving them.

## Features

- **Multiple DbContext Support**: Operate within complex systems using multiple data contexts without hassle.
- **Outbox Pattern Implementation**: Reliably handle message sending operations, ensuring no messages are lost in
  transit, even in the event of system failures.
- **Inbox Pattern Support**: Process incoming messages effectively, preventing duplicate processing and ensuring message
  consistency.
- **MySQL For Update Concurrency Handling**: Utilizes MySQL's For Update row locking feature for concurrency
  control, making your message handling processes more robust.
- **Simple Integration**: Designed to fit easily into existing MassTransit and EF Core based projects.

## Pre-Requisites

- .NET Core 8 or later

## Installation

The library can be installed via NuGet Package Manager. Use the following command:

```bash
Install-Package PropertyMe.MassTransit.MySqlOutbox
```

## Usage

The examples below are given for both inbox and outbox usage.
If you need only one of these, use one of the more specific methods available (i.e. instead of `services.AddOutboxInboxServices` use
`services.AddInboxServices`, for example).

### Configuration

#### DbContext interfaces 

Ensure your `DbContext` implements the `IOutboxDbContext` and `IInboxDbContext` interfaces.
Configure your entities and generate migrations.

*Note: If you use something other an EF migrations, you can refer to the `Migrations` folder in the Publisher test project to see what is required.*

Call `ConfigureInboxOutboxEntities` on your `ModelBuilder` to configure the necessary tables for inbox and outbox
patterns.

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.ConfigureInboxOutboxEntities();
}
```

Note: If multiple DbContexts share the same database and you use EF Migrations, you may need to ensure that only one DbContext is used for the migration generation. In `OnModelCreating()` use something like `modelBuilder.Entity<OutboxMessage>().ToTable("OutboxMessages", t => t.ExcludeFromMigrations());` to prevent duplicate migrations from being created.

**Service Registration:** Register essential services on startup, specifying the `DbContext` type.
You can optionally override settings(its optional parameter).

```csharp
services.AddOutboxInboxServices<YourDbContext>();
```

This registers the background services that will scan the outbox table for messages to send via MassTransit, and the clean up services to remove stale messages from the inbox/outbox tables. You only need to call this once, using just one DbContext.

### Publishing Messages

Rather than calling MassTransit's `publishEndpoint.Publish()` you will instead add your messages to the outbox table in your DbContext.

Call the `AddToOutbox` method on your `DbContext`, specifying your message.
Remember to call `SaveChanges()` to persist the message to the database.

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

### Consuming Messages (Inbox Pattern)

To consume messages using the inbox pattern, create a consumer that inherits from
`InboxConsumer<TMessage, TDbContext>` class, specifying the message type and `DbContext` type as generic arguments.

```csharp

public class YourConsumer : InboxConsumer<YourMessage, YourDbContext>
{
    private readonly YourDbContext _context;

    public YourConsumer(YourDbContext dbContext, IServiceProvider sp)
        : base(sp)
    {
        _context = dbContext;
    }

    public override async Task Consume(YourMessage message, IDbContextTransaction transaction)
    {
        // Implement your message processing logic here
    }
}
```

## Example Code

The test projects show how different outbox and inbox usage scenarios. Explore the test code and try different use cases out to see how things work.

#### Basic scenario

To see the basics in action, run both the publisher and consumer projects.

When you POST a request to the publisher's `/publish` endpoint, a complex message will be sent from the publisher to the consumer. Feel free to stop your message broker (e.g. RabbitMq) during the test to see the resiliency in action. Queue up multiple messages.

```sh
curl --request POST  --url http://publisher.localtest.me/publish --header 'Accept: */*'
```

#### With change tracking turned off

The publisher and consumer projects also have a domain class (DDDEntity) that emits a created events when an instance is created. The ContextForDDDEntity DbContext has disabled change tracking and instead attaches both the entity and the related events to the DbContext during save changes.

Run a single instance of the publisher and consumer and POST a message to the `/create` endpoint to see it in action.

#### Multiple publishers and subscribers

Finally, for a genuinely more complex scenario, you can run multiple publishers and multiple consumers (run thenm from different terminal windows) using the command line. The publisher instances will publish events every second, in a loop.

Run publishers using `dotnet run --no-build -- --id <someId> --auto` from the `MassTransit.MySqlOutbox.Demo.Publisher` folder.

Run consumers using  `dotnet run --no-build -- --id <someId>` from the `MassTransit.MySqlOutbox.Demo.Consumer` folder.

You should see the consumers processing messages from each of the publishers.

##### Exception handling

The `CreatedEventConsumer` has some commented out code in it. Uncomment this and the consumer will start to throw different exceptions when processing messages. The different exceptions will trigger either retries in MassTransit or faults within MassTransit, so you can play around and see what happens inbox retries and moving messages to error queues for exceptions, etc.

## License

PropertyMe.MassTransit.MySqlOutbox is licensed under the MIT License.
