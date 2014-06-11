using System.Data;

namespace Dev2.Activities.SqlBulkInsert
{
    public interface ISqlBulkInserter
    {
        bool Insert(ISqlBulkCopy sqlBulkCopy, DataTable dataTableToInsert);
    }
}