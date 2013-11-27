using System.Data;
using Microsoft.SqlServer.Server;

namespace Warewolf.Sql
{
    public interface ISqlCtx
    {
        SqlDataRecord SendStart(DataTable dt);

        void SendRow(SqlDataRecord dataRecord, object[] items);

        void SendEnd();
    }
}