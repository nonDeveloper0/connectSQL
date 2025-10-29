using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Microsoft.Data.SqlClient;

namespace ConnectSQL
{
    public class SQLDBConnect
    {
        public static SqlConnection connect;

        public void ConnectDB()
        {
            string connStr = "Data Source=DESKTOP-KUULHH8\\SQLEXPRESS;Initial Catalog=TutorialDB;Integrated Security=True;Pooling=False;Encrypt=True;Trust Server Certificate=True";
            connect = new SqlConnection(connStr);
            connect.Open();
        }

        public SqlCommand Query(string sqlQuery)
        {
            SqlCommand command = new SqlCommand(sqlQuery, connect);
            return command;
        }
    }
}
// 추가 v251029