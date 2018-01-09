using Oracle.ManagedDataAccess.Client;
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

        public static int RunOracleSqlCommand(string command)
        {
            using (var trans = new TransactionScope())
            {
                var conStr1 = "User Id=Testuser;Password=test123;Data Source=rsaklfsvrdev;";
                using (var con = new OracleConnection(conStr1))
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
