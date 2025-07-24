namespace MassTransit.MySqlOutbox;

public class Settings
{
   public int InboxRemovalBeforeInDays = 5;

   public TimeSpan InboxRemovalTimerPeriod = TimeSpan.FromDays(1);
   public int OutboxRemovalBeforeInDays = 5;

   public TimeSpan OutboxRemovalTimerPeriod = TimeSpan.FromDays(1);
   public int PublisherBatchCount = 100;
   public TimeSpan PublisherTimerPeriod = TimeSpan.FromSeconds(10);
}