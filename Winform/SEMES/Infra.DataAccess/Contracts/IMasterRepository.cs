using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infra.DataAccess.Contracts
{
    public interface IMasterRepository
    {//This interface defines the public behaviors of the MasterRepository class.

        //Methods for executing Insert, Update and Delete commands.
        int ExecuteNonQuery(string commandText, SqlParameter parameter, CommandType commandType);
        int ExecuteNonQuery(string commandText, List<SqlParameter> parameters, CommandType commandType);
        int BulkExecuteNonQuery(List<BulkTransaction> transactions, CommandType commandType);

        //Methods for running query commands (Select)
        DataTable ExecuteReader(string commandText, CommandType commandType);
        DataTable ExecuteReader(string commandText, SqlParameter parameter, CommandType commandType);
        DataTable ExecuteReader(string commandText, List<SqlParameter> parameters, CommandType commandType);  
    }
}
