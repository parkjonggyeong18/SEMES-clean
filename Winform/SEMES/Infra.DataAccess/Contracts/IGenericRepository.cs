using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infra.DataAccess.Contracts
{
    public interface IGenericRepository<Entity> where Entity : class
    {//This interface defines common public behaviors for all entities.

        int Add(Entity entity); // Add new entity.
        int Edit(Entity entity); // Edit an entity.
        int Remove(Entity entity); // Remove an entity.

        Entity GetSingle(string value); // Get an entity by value (Search).
        IEnumerable<Entity> GetAll(); // List all entities.
        IEnumerable<Entity> GetByValue(string value); // List entities by value (Filter).
    }
}
