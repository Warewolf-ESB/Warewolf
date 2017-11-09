using System.Data.SqlClient;
using System.Transactions;

namespace Dev2.Integration.Tests.Database_Tools_Refresh
{
    public static class SqlHelper
    {
        public static int RunSqlCommand(string command)
        {
            using (var trans = new TransactionScope())
            {
                using (var con = new SqlConnection("Data Source=RSAKLFSVRDEV,1433;Initial Catalog=Dev2TestingDB;User ID=testuser;Password=test123;"))
                {
                    var cmd = con.CreateCommand();
                    cmd.CommandText = command;
                    con.Open();
                    var a = cmd.ExecuteNonQuery();
                    trans.Complete();
                    return a;
                }
            }
        }
    }
}
