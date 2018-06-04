/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
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

        public virtual bool IsConnected => true;
        public virtual string ConnectionString { get; private set; }

        public virtual void Connect(string connectionString)
        {
            ConnectionString = connectionString;
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
		public virtual DataSet FetchDataSet(IDbCommand command)
		{
			var dataSet = new DataSet();
			return dataSet;
		}
		public virtual List<string> FetchDatabases()
        {
            return new List<string>();
        }
		public int ExecuteNonQuery(IDbCommand command)
		{


			return  0;
		}

		public int ExecuteScalar(IDbCommand command)
		{
			return 0;
		}
		public virtual void FetchStoredProcedures(Func<IDbCommand, List<IDbDataParameter>, string, string, bool> procedureProcessor, Func<IDbCommand, List<IDbDataParameter>, string, string, bool> functionProcessor)
        {
        }

        public virtual void FetchStoredProcedures(Func<IDbCommand, List<IDbDataParameter>, string,string, bool> procedureProcessor, Func<IDbCommand, List<IDbDataParameter>, string,string, bool> functionProcessor, bool continueOnProcessorException = false, string a="" )
        {
        }

        public void FetchStoredProcedures(Func<IDbCommand, List<IDbDataParameter>, List<IDbDataParameter>, string, string, bool> procedureProcessor, Func<IDbCommand, List<IDbDataParameter>, List<IDbDataParameter>, string, string, bool> functionProcessor)
        {
        }

        public void FetchStoredProcedures(Func<IDbCommand, List<IDbDataParameter>, List<IDbDataParameter>, string, string, bool> procedureProcessor, Func<IDbCommand, List<IDbDataParameter>, List<IDbDataParameter>, string, string, bool> functionProcessor, bool continueOnProcessorException, string dbName)
        {
        }

        public virtual IDbCommand CreateCommand()
        {
            return new SqlCommand();
        }

        public bool Connect(string connectionString, CommandType commandType, string commandText)
        {
            return false;
        }

        #endregion
    }
}
