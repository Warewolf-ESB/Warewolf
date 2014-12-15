
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

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
