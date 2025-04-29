using MySql.Data.MySqlClient;    // ★
using System.Collections.Generic;
using System.Data;

namespace Infra.DataAccess
{
    /// <summary>
    ///  SQL Server 전용 코드 → MySQL 로 치환(이름만 바뀜, 로직 동일).
    /// </summary>
    public abstract class MasterRepository : Repository
    {
        private DataTable dataTable;

        /*---------------------- ExecuteNonQuery ----------------------*/
        public int ExecuteNonQuery(string commandText,
                                   MySqlParameter parameter,
                                   CommandType commandType)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var command = new MySqlCommand(commandText, connection))
                {
                    command.CommandType = commandType;
                    command.Parameters.Add(parameter);
                    return command.ExecuteNonQuery();
                }
            }
        }

        public int ExecuteNonQuery(string commandText,
                                   List<MySqlParameter> parameters,
                                   CommandType commandType)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var command = new MySqlCommand(commandText, connection))
                {
                    command.CommandType = commandType;
                    command.Parameters.AddRange(parameters.ToArray());
                    return command.ExecuteNonQuery();
                }
            }
        }

        /*---------------------- BulkExecuteNonQuery ------------------*/
        public int BulkExecuteNonQuery(List<BulkTransaction> transactions,
                                       CommandType commandType)
        {
            int result = 0;
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var trx = connection.BeginTransaction())
                using (var cmd = new MySqlCommand
                {
                    Connection = connection,
                    Transaction = trx,
                    CommandType = commandType
                })
                {
                    try
                    {
                        foreach (var t in transactions)
                        {
                            cmd.CommandText = t.CommandText;
                            cmd.Parameters.AddRange(t.Parameters.ToArray());
                            result += cmd.ExecuteNonQuery();
                            cmd.Parameters.Clear();
                        }
                        trx.Commit();
                    }
                    catch
                    {
                        trx.Rollback();
                        throw;
                    }
                }
            }
            return result;
        }

        /*--------------------------- Reader --------------------------*/
        public DataTable ExecuteReader(string commandText,
                                       CommandType commandType)
        {
            dataTable = new DataTable();
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var cmd = new MySqlCommand(commandText, connection))
                {
                    cmd.CommandType = commandType;
                    using (var rdr = cmd.ExecuteReader())
                        dataTable.Load(rdr);
                }
            }
            return dataTable;
        }

        public DataTable ExecuteReader(string commandText,
                                       MySqlParameter parameter,
                                       CommandType commandType)
        {
            dataTable = new DataTable();
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var cmd = new MySqlCommand(commandText, connection))
                {
                    cmd.CommandType = commandType;
                    cmd.Parameters.Add(parameter);
                    using (var rdr = cmd.ExecuteReader())
                        dataTable.Load(rdr);
                }
            }
            return dataTable;
        }

        public DataTable ExecuteReader(string commandText,
                                       List<MySqlParameter> parameters,
                                       CommandType commandType)
        {
            dataTable = new DataTable();
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var cmd = new MySqlCommand(commandText, connection))
                {
                    cmd.CommandType = commandType;
                    cmd.Parameters.AddRange(parameters.ToArray());
                    using (var rdr = cmd.ExecuteReader())
                        dataTable.Load(rdr);
                }
            }
            return dataTable;
        }
    }
}
