using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Data.SqlClient;
using System.Data;
using System.Configuration; 
using System.Collections.Concurrent;

namespace LSE_ClientCS
{
    class DbUtil
    {
        private string _connectionString = string.Empty;     //sql 접속정보
        public DbUtil()
        {
            this._connectionString = ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString;
        }
        /// <summary>
        /// 수신 데이터 입력
        /// </summary>
        /// <param name="dataTable"></param>
        public void WriteDataBase(int? t1, int? t2, int? t3, int? t4)
        {
             using (MySqlConnection conn = new MySqlConnection(_connectionString))
            { 
                var comm = new MySqlCommand("INSERT INTO t1 (t1, t2, t3, t4, currentDate) VALUES("
                 + t1 +  "," 
                 + t2 +  "," 
                 + t3 +  "," 
                 + t4 +  "," 
                 + "NOW()"
                 + ")", conn);
                conn.Open();
                comm.ExecuteNonQuery();

            }
        }
    }
}
