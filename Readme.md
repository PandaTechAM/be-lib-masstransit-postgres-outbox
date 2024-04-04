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

## License

Pandatech.MassTransit.PostgresOutbox is licensed under the MIT License.