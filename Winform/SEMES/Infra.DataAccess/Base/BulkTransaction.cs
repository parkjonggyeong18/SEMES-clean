using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infra.DataAccess
{
    public class BulkTransaction
    {
        /// <summary>
        /// This class is simply responsible for storing a text command and a list of parameters
        /// for the text command (Transact-SQL or Stored Procedure) to perform bulk transactions.
        /// For more details see the BulkExecuteNonQuery () method of the MasterRepository class.
        /// </summary>
        /// 
        public string CommandText { get; set; } // Gets or sets a text command.
        public List<SqlParameter> Parameters { get; set; } // Gets or sets a collection of parameters for the text command.

        public BulkTransaction()
        {
            Parameters = new List<SqlParameter>(); // Initialize the list of parameters.
        }
    }
}
