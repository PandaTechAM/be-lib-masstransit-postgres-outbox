using MassTransit.PostgresOutbox.Enums;

namespace MassTransit.PostgresOutbox.Entities
{
    public class OutboxMessage
    {
        public required Guid Id { get; set; }
        public MessageState State { get; set; } = MessageState.New;
        public required string Payload { get; set; }
        public required string Type { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

    }
}
