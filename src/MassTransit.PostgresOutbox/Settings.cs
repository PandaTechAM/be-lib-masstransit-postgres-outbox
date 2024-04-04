namespace MassTransit.PostgresOutbox;

public class Settings
{
   public TimeSpan PublisherTimerPeriod = TimeSpan.FromSeconds(1);
   public int PublisherBatchCount = 100;

   public TimeSpan OutboxRemovalTimerPeriod = TimeSpan.FromDays(1);
   public int OutboxRemovalBeforeInDays = 5;

   public TimeSpan InboxRemovalTimerPeriod = TimeSpan.FromDays(1);
   public int InboxRemovalBeforeInDays = 5;
}