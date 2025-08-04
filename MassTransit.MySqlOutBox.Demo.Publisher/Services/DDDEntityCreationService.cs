using MassTransit.MySqlOutbox.Demo.Shared.Entities;

namespace MassTransit.MySqlOutbox.Demo.Services;

public class DDDEntityCreationService(ContextForDDDEntity dbContext)
{
   public async Task CreateNewEntity(string publisherNumber)
   {
      var entity = new DDDEntity(publisherNumber);
      dbContext.Add(entity);
      await dbContext.SaveChangesAsync(); //We publish events automatically during SaveChangesAsync for this dbContext
      Console.WriteLine($"New DDDEntity created with Id: {entity.Id} created at {DateTime.Now}");
   }
}