using System;
using System.Data;
using System.Data.SqlClient;

namespace Dev2.Services.Sql
{
    public interface ISqlConnection:IDisposable
    {
        bool FireInfoMessageEventOnUserErrors { get; set; }
        bool StatisticsEnabled { get; set; }
        event SqlInfoMessageEventHandler InfoMessage;
        ConnectionState State { get; }
        IDbTransaction BeginTransaction();
        void Open();
        DataTable GetSchema(string table);
        IDbCommand CreateCommand();
        void SetInfoMessage(SqlInfoMessageEventHandler a);
    }
}