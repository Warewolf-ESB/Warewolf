using System.Data;
using System.Linq;
using Microsoft.SqlServer.Server;
using Warewolf.ComponentModel;

namespace Warewolf.Sql
{
    public class SqlCtx : ISqlCtx
    {
        #region SendRow

        public void SendRow(SqlDataRecord dataRecord, object[] items)
        {
            for(var i = 0; i < items.Length; i++)
            {
                var value = items[i].ToStringSafe();
                if(string.IsNullOrEmpty(value))
                {
                    dataRecord.SetDBNull(i);
                }
                else
                {
                    dataRecord.SetString(i, value);
                }
            }

            if(SqlContext.IsAvailable && SqlContext.Pipe != null)
            {
                // Send the row back to the client.
                SqlContext.Pipe.SendResultsRow(dataRecord);
            }
        }

        #endregion

        #region SendStart

        public SqlDataRecord SendStart(DataTable dt)
        {
            if(SqlContext.IsAvailable && SqlContext.Pipe != null)
            {
                var metaData = dt.Columns.Cast<DataColumn>().Select(c => new SqlMetaData(c.ColumnName, SqlDbType.VarChar, SqlMetaData.Max));
                var dataRecord = new SqlDataRecord(metaData.ToArray());

                SqlContext.Pipe.SendResultsStart(dataRecord);
                return dataRecord;
            }
            return null;
        }

        #endregion

        #region SendEnd

        public void SendEnd()
        {
            if(SqlContext.IsAvailable && SqlContext.Pipe != null)
            {
                // Mark the end of the result-set.
                SqlContext.Pipe.SendResultsEnd();
            }
        }

        #endregion

    }
}