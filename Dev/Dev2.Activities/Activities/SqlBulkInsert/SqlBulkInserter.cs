using System.Data;

namespace Dev2.Activities.SqlBulkInsert
{
    public class SqlBulkInserter : ISqlBulkInserter
    {
        public bool Insert(ISqlBulkCopy sqlBulkCopy, DataTable dataTableToInsert)
        {
            using(sqlBulkCopy)
            {
                return sqlBulkCopy.WriteToServer(dataTableToInsert);
            }
        }
    }
}