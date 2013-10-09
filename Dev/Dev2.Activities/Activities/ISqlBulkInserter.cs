using System.Data;
using System.Data.SqlClient;

namespace Dev2.Activities
{
    public interface ISqlBulkInserter
    {
        SqlBulkCopyOptions CurrentOptions { get; set; }
        void Insert(SqlBulkCopy sqlBulkCopy,DataTable dataTableToInsert);
    }
}