using Infra.DataAccess.Contracts;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infra.DataAccess
{
    public abstract class MasterRepository : Repository, IMasterRepository
    {
        /// <summary>
        /// This abstract class inherits from the Repository class and implements the IMasterRepository interface.
        /// This class is a base class for all entity repositories and is responsible for making any query and 
        /// transaction to the SQL Server database, for this it implements 3 methods:
        /// 
        /// -ExecuteNonQuery method (...) -> Execute transaction commands (Create, Update and Delete).
        /// -BulkExecuteNonQuery method (...) -> Execute transaction commands (Create, Update and Delete)
        ///                                      to carry out massive transactions affecting multiple rows.
        /// -ExecuteReader method (...) -> Execute query commands (Select).
        ///
        /// These methods have 2 or more overloads (It is considered the pillar of polymorphism OOP).
        /// </summary>

        private DataTable dataTable;//Gets or sets the data table for the queries.   

        public MasterRepository()
        {

        }

        public int ExecuteNonQuery(string commandText, SqlParameter parameter, CommandType commandType)
        {/* This method is responsible for executing a transaction command (Create, Update and Delete) with ONLY ONE PARAMETER,
            either a Transact-SQL command or stored procedure. For this, specify the type of command at the time of invoking the method.
            You could use this method to delete a row where generally only one parameter is needed (@id)*/

            using (var connection = GetConnection())//Get connection.
            {
                connection.Open();//Open connection.
                using (var command = new SqlCommand()) // Create SqlCommand object.
                {
                    command.Connection = connection; // Establish the connection.
                    command.CommandText = commandText; // Set the text command.
                    command.CommandType = commandType; // Set the type of command (Transact-SQL or stored procedure).
                    command.Parameters.Add(parameter); // Add the parameter.
                    return command.ExecuteNonQuery(); // Execute the text command and return the number of rows affected.                 
                }
            }
            /*Note: The USING declaration guarantees that the objects that implement IDisposable are disposed of correctly,
             * therefore, the SQLConexion object and SqlCommnad will be discarded automatically once they fulfill their
             * mission, also it is not necessary to close the connection and clean the SqlCommand parameters explicitly.*/
        }
        public int ExecuteNonQuery(string commandText, List<SqlParameter> parameters, CommandType commandType)
        {/*This method is responsible for executing a transaction command (Create, Update and Delete) with VARIOUS PARAMETERS,
           be it a Transact-SQL command or stored procedure. For this, specify the type of command at the time of invoking the method.*/

            using (var connection = GetConnection())
            {
                connection.Open();
                using (var command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = commandText;
                    command.CommandType = commandType;
                    command.Parameters.AddRange(parameters.ToArray()); // Add the collection of parameters.
                    return command.ExecuteNonQuery(); // Execute the text command and return the number of rows affected.
                }
            }
        }
        public int BulkExecuteNonQuery(List<BulkTransaction> transactions, CommandType commandType)
        {/*This method is responsible for executing MULTIPLE transaction COMMANDS (Create, Update and Delete)
           with VARIOUS PARAMETERS, be it a Transact-SQL command or stored procedure. The use of SqlTransaction
           is very important, it guarantees that all the data is stored correctly, in case of an error, 
           it will be in charge of reverting all the changes.
           Use this method if you want to insert, edit or delete data in bulk (Multiple rows and tables)*/

            int result = 0;

            using (var connection = GetConnection())//Get the connection.
            {
                connection.Open();//Open connection.

                using (SqlTransaction sqlTransaction = connection.BeginTransaction())//Initialize the transaction.
                using (var command = new SqlCommand())//Initialize a SqlCommand object.
                {
                    command.Connection = connection;
                    command.Transaction = sqlTransaction;//Set the transaction
                    command.CommandType = commandType;
                    try
                    {
                       foreach (var trans in transactions) // Go through the collection of transactions and get the text commands with their respective parameters.
                         {
                             command.CommandText = trans.CommandText; // Set the text command.
                             command.Parameters.AddRange (trans.Parameters.ToArray ()); // Add the collection of parameters.
                             result += command.ExecuteNonQuery (); // Execute the text command and accumulate the number of rows affected in each cycle.
                             command.Parameters.Clear (); // Clear the parameters of the SQL command.
                         }
                         sqlTransaction.Commit (); // Once all the commands have been executed, commit (save) the transaction.
                    }
                    catch (Exception)
                    {
                        // In case of an exception, abort the transaction to revert the changes.
                        sqlTransaction.Rollback();                        
                        throw;                         
                    }
                }
            }
            return result;
        }

        public DataTable ExecuteReader(string commandText, CommandType commandType)
        {/* This method is responsible for executing query commands WITHOUT parameters,
           * either a Transact-SQL command or stored procedure*/

            dataTable = new DataTable();//Initialize the data table.

            using (var connection = GetConnection())//Get the connection.
            {
                connection.Open();
                using (var command = new SqlCommand())//Create the SQL Command object.
                {
                    command.Connection = connection;
                    command.CommandText = commandText;
                    command.CommandType = commandType;

                    using (var reader = command.ExecuteReader()) // Execute the command in reader mode.
                        dataTable.Load(reader); // Fill the data table with the result stored in the data reader.
                }
            }
            return dataTable;//Return the data table
        }
        public DataTable ExecuteReader(string commandText, SqlParameter parameter, CommandType commandType)
        {/* This method takes care of executing query commands WITH a parameter,
           * either a Transact-SQL command or stored procedure*/

            dataTable = new DataTable();

            using (var connection = GetConnection())
            {
                connection.Open();
                using (var command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = commandText;
                    command.CommandType = commandType;
                    command.Parameters.Add(parameter);

                    using (var reader = command.ExecuteReader())
                        dataTable.Load(reader);
                }
            }
            return dataTable;
        }
        public DataTable ExecuteReader(string commandText, List<SqlParameter> parameters, CommandType commandType)
        {/* This method takes care of executing query commands with various parameters,
           * either a Transact-SQL command or stored procedure*/

            dataTable = new DataTable();

            using (var connection = GetConnection())
            {
                connection.Open();
                using (var command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = commandText;
                    command.CommandType = commandType;
                    command.Parameters.AddRange(parameters.ToArray());

                    using (var reader = command.ExecuteReader())
                        dataTable.Load(reader);
                }
            }
            return dataTable;
        }
    }
}
