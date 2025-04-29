using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UI.WinForm.Utils
{
    public abstract class ExceptionManager
    {
        public static string GetMessage(Exception exception)
        {
            System.Data.SqlClient.SqlException sqlEx = exception as System.Data.SqlClient.SqlException;           

            if (sqlEx != null && sqlEx.Number == 2627)
            {
                string value = sqlEx.Message.Split('(', ')')[1];
                return "Duplicate record, try another.\nDuplicate value is:\n    ■ " + value;
            }
            else
            {
                return "An error has occurred.\nDetails:\n" + exception.Message;
            }
        }
    }
}
