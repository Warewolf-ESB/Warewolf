using System;
using System.Data;
using System.Data.SqlClient;

namespace Dev2.Activities.SqlBulkInsert
{
    public class SqlBulkCopyWrapper : ISqlBulkCopy
    {
        #region Implementation of IDisposable

        private readonly SqlBulkCopy _sbc;
        public SqlBulkCopyWrapper(SqlBulkCopy copyTool)
        {
            _sbc = copyTool;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if(_sbc != null)
            {
                _sbc.Close();
            }
        }

        public bool WriteToServer(DataTable dt)
        {
            if(_sbc == null)
            {
                throw new ArgumentException("_sbc");
            }

            using(_sbc)
            {
                _sbc.WriteToServer(dt);
            }

            return true;
        }

        #endregion
    }
}
