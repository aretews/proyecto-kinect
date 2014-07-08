using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;

namespace KinectFinalProyect
{
    class Conexion
    {

        public SqlConnection getConnection(){
            string data = "Data Source=Server_Name;Initial Catalog=Database_Name;User ID=XXXX;Password=XXXX;Integrated Security=True;";
            SqlConnection connection = new SqlConnection(data);
            return connection;

        }


    }
}
