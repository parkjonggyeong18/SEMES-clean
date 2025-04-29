using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infra.DataAccess.Entities
{
   public class User
   {// The entities have the same fields as the database table,
       // also this allows you to easily change to Entity Framework.

        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Position { get; set; }
        public string Email { get; set; }
        public byte[] Photo { get; set; }

    }
}
