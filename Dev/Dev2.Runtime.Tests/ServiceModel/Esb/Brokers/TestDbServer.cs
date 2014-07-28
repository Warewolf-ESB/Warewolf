using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Dev2.Services.Sql;

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