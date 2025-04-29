using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
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
        public string CommandText { get; set; }
        public List<MySqlParameter> Parameters { get; set; }   // ★ 수정
        public BulkTransaction() => Parameters = new List<MySqlParameter>();
    }
    }

