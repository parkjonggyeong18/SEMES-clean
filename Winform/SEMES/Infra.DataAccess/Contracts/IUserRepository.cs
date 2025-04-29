using Infra.DataAccess.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infra.DataAccess.Contracts
{
    public interface IUserRepository : IGenericRepository<User>
    {// This interface implements the generic interface IGenericRepository (Don't forget to specify the entity class).
        // Additionally, define other specific public behaviors of the user entity.

        int AddRange(List<User> users); // Add a collection of users (Bulk insert)
        int RemoveRange(List<User> users); // Remove a collection of users (bulk removal)
        User Login(string username, string password); // Validate the login data.
    }
}
