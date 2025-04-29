using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infra.DataAccess.Contracts;
using Infra.DataAccess.Entities;
using Infra.Common;
using System.Data.SqlClient;
using System.Data;

namespace Infra.DataAccess.Repositories
{
    public class UserRepository : MasterRepository, IUserRepository
    {
        /// <summary>
        /// This class inherits from the MasterRepository class and implements the IUserRepository interface.
        /// Here the different transactions and queries the user entity are made to the database.
        /// </summary>
        /// 

        public User Login(string username, string password)
        {// Example of a query with several parameters using a stored procedure:
            // Validate the username and password of the user for the login.

            string commandText = "LoginUser"; // Set the text command (Transact-SQL or stored procedure).
            CommandType commandType = CommandType.StoredProcedure; // Set the command type.
            var parameters = new List<SqlParameter>(); // Create a generic list of for the query parameters.
            parameters.Add(new SqlParameter("@user", username)); // Create and add the user parameter (Parameter name and value).
            parameters.Add(new SqlParameter("@password", password)); // Create and add the password parameter (Parameter name and value).

            var table = ExecuteReader(commandText, parameters, commandType);//Execute the reader method of the MasterRepository base class and send the necessary parameters.

            if (table.Rows.Count > 0)//If the query was successful (valid username and password).
            {
                var user = new User(); // Create user entity-object.
                foreach (DataRow row in table.Rows) // Loop through the rows of the table and assign the respective values of the user object.
                {
                    user.Id = (int)(row[0]);//Cell position [0].
                    user.Username = row[1].ToString();
                    user.Password = row[2].ToString();
                    user.FirstName = row[3].ToString();
                    user.LastName = row[4].ToString();
                    user.Position = row[5].ToString();
                    user.Email = row[6].ToString();
                    if (row[7] != DBNull.Value) user.Photo = (byte[])row[7];//Set value if cell value is other than null.
                }
                return user;//Return user object.
            }
            else //If the query was not successful - return a null object.
                return null;
        }

        public int Add(User entity)
        {// Example of a transaction with various parameters using a stored procedure:
            // Add a new user.

            var parameters = new List<SqlParameter>();//Create a list for the transaction parameters.
            parameters.Add(new SqlParameter("@userName", entity.Username));
            parameters.Add(new SqlParameter("@password", entity.Password));
            parameters.Add(new SqlParameter("@firstName", entity.FirstName));
            parameters.Add(new SqlParameter("@lastName", entity.LastName));
            parameters.Add(new SqlParameter("@position", entity.Position));
            parameters.Add(new SqlParameter("@email", entity.Email));
            if (entity.Photo != null)//If the Photo property is other than null, assign the property value.
                parameters.Add(new SqlParameter("@photo", entity.Photo) { SqlDbType = SqlDbType.VarBinary });//In this case of the Photo field, it is important to explicitly specify the SQL data type,
            else //Otherwise assign a null value from SQL.                                                    //You can do the same with the other parameters, however it is optional,
                parameters.Add(new SqlParameter("@photo", DBNull.Value) { SqlDbType = SqlDbType.VarBinary }); //The data type will be derived from the data type of its value.

            // Execute the ExecuteNonQuery method of the MasterRepository class to perform an insert transaction,
            // and send the necessary parameters (Text command, parameters and type of command).
            return ExecuteNonQuery("AddUser", parameters, CommandType.StoredProcedure);
        }
        public int Edit(User entity)
        {// Example of a transaction with various parameters using a stored procedure:
            // Edit user.

            var parameters = new List<SqlParameter>();//Create a list for the transaction parameters.
            parameters.Add(new SqlParameter("@id", entity.Id));
            parameters.Add(new SqlParameter("@userName", entity.Username));
            parameters.Add(new SqlParameter("@password", entity.Password));
            parameters.Add(new SqlParameter("@firstName", entity.FirstName));
            parameters.Add(new SqlParameter("@lastName", entity.LastName));
            parameters.Add(new SqlParameter("@position", entity.Position));
            parameters.Add(new SqlParameter("@email", entity.Email));
            if (entity.Photo != null)
                parameters.Add(new SqlParameter("@photo", entity.Photo) { SqlDbType = SqlDbType.VarBinary });
            else parameters.Add(new SqlParameter("@photo", DBNull.Value) { SqlDbType = SqlDbType.VarBinary });

            // Execute the ExecuteNonQuery method of the MasterRepository class to perform an update transaction,
            // and send the necessary parameters (Text command, parameters and type of command).
            return ExecuteNonQuery("EditUser", parameters, CommandType.StoredProcedure);
        }
        public int Remove(User entity)
        {// Example of a transaction with a single parameter using a Transact-SQL command:
            // Delete user.

            string sqlCommand = "delete from Users where id=@id";//Command of type text (Transact-SQL)
            return ExecuteNonQuery(sqlCommand, new SqlParameter("@id", entity.Id), CommandType.Text);
        }
        public int AddRange(List<User> users)
        {// Example of a bulk transaction using a stored procedure:
            // Add multiple users.

            var transactions = new List<BulkTransaction>();//Create a generic list for transactions.

            foreach (var user in users)//Loop through the list of users and add the instructions to the list of transactions.
            {
                var trans = new BulkTransaction();//Create a transaction object.
                var parameters = new List<SqlParameter>();//Create a list for the transaction parameters.
                //In this case of a bulk transaction, it is convenient to specify the data type of the parameter.
                parameters.Add(new SqlParameter("@userName", user.Username) { SqlDbType = SqlDbType.NVarChar });
                parameters.Add(new SqlParameter("@password", user.Password) { SqlDbType = SqlDbType.NVarChar });
                parameters.Add(new SqlParameter("@firstName", user.FirstName) { SqlDbType = SqlDbType.NVarChar });
                parameters.Add(new SqlParameter("@lastName", user.LastName) { SqlDbType = SqlDbType.NVarChar });
                parameters.Add(new SqlParameter("@position", user.Position) { SqlDbType = SqlDbType.NVarChar });
                parameters.Add(new SqlParameter("@email", user.Email) { SqlDbType = SqlDbType.NVarChar });
                if (user.Photo != null)
                    parameters.Add(new SqlParameter("@photo", user.Photo) { SqlDbType = SqlDbType.VarBinary });
                else parameters.Add(new SqlParameter("@photo", DBNull.Value) { SqlDbType = SqlDbType.VarBinary });

                trans.CommandText = "AddUser"; // Set the text command (In this case a stored procedure).
                trans.Parameters = parameters; // Set the parameters of the instruction (Text command).

                transactions.Add(trans); // Add the transaction to the list of transactions.
            }
            //You can continue adding more transactions to other tables to the generic list of transactions.

            //Finally execute all the instructions of the transaction list using the BulkExecuteNonQuery method 
            //of the MasterRepository base class, send the necessary parameters (List of transactions and the type of command.)
            return BulkExecuteNonQuery(transactions, CommandType.StoredProcedure);
        }
        public int RemoveRange(List<User> users)
        {// Example of a bulk transaction using a Transact-SQL command:
            // Delete multiple users.

            var transactions = new List<BulkTransaction>();

            foreach (var user in users)
            {
                var trans = new BulkTransaction();
                trans.CommandText = "delete from Users where id=@id";
                trans.Parameters = new List<SqlParameter> { new SqlParameter("@id", user.Id) { SqlDbType = SqlDbType.Int } };

                transactions.Add(trans);
            }
            return BulkExecuteNonQuery(transactions, CommandType.Text);
        }

        public User GetSingle(string value)
        {// Example of a query using a Transact-SQL command with a parameter:
            // Get a user according to the specified value (Search).

            string sqlCommand;
            DataTable table;
            int idUser;

            bool isNumeric = int.TryParse(value, out idUser);//Determine if the value parameter is an integer.
            if (isNumeric)//If the value is a number, query using the user's id.
            {
                sqlCommand = "select *from Users where id= @idUser";
                table = ExecuteReader(sqlCommand, new SqlParameter("@idUser", idUser), CommandType.Text);
            }
            else //Otherwise, make the query using the username or email.
            {
                sqlCommand = "select *from Users where userName= @findValue or email=@findValue";
                table = ExecuteReader(sqlCommand, new SqlParameter("@findValue", value), CommandType.Text);
            }

            if (table.Rows.Count > 0)//If the query is successful
            {
                var user = new User();//Create a user object and assign the values.
                foreach (DataRow row in table.Rows)
                {
                    user.Id = Convert.ToInt32(row[0]);
                    user.Username = row[1].ToString();
                    user.Password = row[2].ToString();
                    user.FirstName = row[3].ToString();
                    user.LastName = row[4].ToString();
                    user.Position = row[5].ToString();
                    user.Email = row[6].ToString();
                    if (row[7] != DBNull.Value) user.Photo = (byte[])row[7];
                }
                //Optionally disposing the table to free memory (Dispose() method doesn't work on DataTable, DataSet and others).
                table.Clear();
                table = null;

                return user;//Return user found.
            }
            else//If the query was not successful, return null object.
                return null;
        }
        public IEnumerable<User> GetAll()
        {
            var userList = new List<User>();
            var table = ExecuteReader("SelectAllUsers", CommandType.StoredProcedure);

            foreach (DataRow row in table.Rows)
            {
                var user = new User();
                user.Id = Convert.ToInt32(row[0]);
                user.Username = row[1].ToString();
                user.Password = row[2].ToString();
                user.FirstName = row[3].ToString();
                user.LastName = row[4].ToString();
                user.Position = row[5].ToString();
                user.Email = row[6].ToString();
                if (row[7] != DBNull.Value) user.Photo = (byte[])row[7];

                userList.Add(user);
            }
            table.Clear();
            table = null;

            return userList;
        }
        public IEnumerable<User> GetByValue(string value)
        {
            var userList = new List<User>();
            var table = ExecuteReader("SelectUser", new SqlParameter("@findValue", value), CommandType.StoredProcedure);

            foreach (DataRow row in table.Rows)
            {
                var user = new User();
                user.Id = Convert.ToInt32(row[0]);
                user.Username = row[1].ToString();
                user.Password = row[2].ToString();
                user.FirstName = row[3].ToString();
                user.LastName = row[4].ToString();
                user.Position = row[5].ToString();
                user.Email = row[6].ToString();
                if (row[7] != DBNull.Value) user.Photo = (byte[])row[7];

                userList.Add(user);
            }
            table.Clear();
            table = null;

            return userList;
        }
    }
}
