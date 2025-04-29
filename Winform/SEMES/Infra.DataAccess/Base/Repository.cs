using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infra.DataAccess
{
    public abstract class Repository
    {
        /// <summary>
        /// This abstract class is responsible for establishing the connection string
        /// and get the SQL connection.
        /// </summary>

        private readonly string connectionString;//Gets or sets the connection string.

        public Repository()
        {
            connectionString = "Server=(local);DataBase= MyCompanyTest; integrated security= true";//Set the connection string.
        }

        protected SqlConnection GetConnection()
        {//This method is responsible for establishing and returning the connection object to SQL Server.
            return new SqlConnection(connectionString);
        }
    }
}
