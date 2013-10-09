using System.Data;
using System.Data.SqlClient;

namespace Dev2.Activities
{
    public class SqlBulkInserter : ISqlBulkInserter
    {
        #region Implementation of ISqlBulkInserter

        public SqlBulkCopyOptions CurrentOptions { get; set; }

        public void Insert(SqlBulkCopy sqlBulkCopy, DataTable dataTableToInsert)
        {            
        }

        #endregion
    }
}