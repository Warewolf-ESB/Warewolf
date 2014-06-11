using System;
using System.Data;

namespace Dev2.Activities.SqlBulkInsert
{
    public interface ISqlBulkCopy : IDisposable
    {
        bool WriteToServer(DataTable dt);
    }
}
