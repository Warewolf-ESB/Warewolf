using System;
using System.Collections.Generic;
using System.Data;

namespace Dev2.Services.Sql
{
    public interface IDbServer : IDisposable
    {
        bool IsConnected { get; }
        string ConnectionString { get; }

        bool Connect(string connectionString);

        void BeginTransaction();

        void RollbackTransaction();

        DataTable FetchDataTable(IDbCommand command);

        List<string> FetchDatabases();

        void FetchStoredProcedures(Func<IDbCommand, List<IDbDataParameter>, string,string, bool> procedureProcessor,
            Func<IDbCommand, List<IDbDataParameter>, string,string, bool> functionProcessor, bool continueOnProcessorException = false);

        IDbCommand CreateCommand();

    }
}