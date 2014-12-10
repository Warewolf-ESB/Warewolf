
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
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Dev2.Common.Interfaces.Services.Sql;

namespace Dev2.Tests.Runtime.ServiceModel.Esb.Brokers
{
    public class TestDbServer : IDbServer
    {
        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public virtual void Dispose()
        {
        }

        #endregion

        #region Implementation of IDbServer

        public virtual bool IsConnected { get { return true; } }
        public virtual string ConnectionString { get; private set; }

        public virtual bool Connect(string connectionString)
        {
            ConnectionString = connectionString;
            return true;
        }

        public virtual void BeginTransaction()
        {
        }

        public virtual void RollbackTransaction()
        {
        }

        public virtual DataTable FetchDataTable(IDbCommand command)
        {
            var dataTable = new DataTable();
            return dataTable;
        }

        public virtual List<string> FetchDatabases()
        {
            return new List<string>();
        }

        public virtual void FetchStoredProcedures(Func<IDbCommand, List<IDbDataParameter>, string,string, bool> procedureProcessor, Func<IDbCommand, List<IDbDataParameter>, string,string, bool> functionProcessor, bool continueOnProcessorException = false)
        {
        }

        public virtual IDbCommand CreateCommand()
        {
            return new SqlCommand();
        }

        #endregion
    }
}
