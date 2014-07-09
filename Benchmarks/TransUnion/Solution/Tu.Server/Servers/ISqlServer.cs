using System;
using System.Data;
using System.Data.SqlClient;

namespace Tu.Servers
{
    public interface ISqlServer : IDisposable
    {
        DataTable FetchDataTable(string commandText, CommandType commandType, params SqlParameter[] parameters);

        void ExecuteNonQuery(string commandText, CommandType commandType, params SqlParameter[] parameters);
    }
}