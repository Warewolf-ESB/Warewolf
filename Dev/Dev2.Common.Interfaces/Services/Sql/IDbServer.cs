/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Data;

namespace Dev2.Common.Interfaces.Services.Sql
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

        void FetchStoredProcedures(Func<IDbCommand, List<IDbDataParameter>, string, string, bool> procedureProcessor,
            Func<IDbCommand, List<IDbDataParameter>, string, string, bool> functionProcessor,
            bool continueOnProcessorException = false,string dbName="" );

        void FetchStoredProcedures(Func<IDbCommand, List<IDbDataParameter>, List<IDbDataParameter>, string, string, bool> procedureProcessor,
        Func<IDbCommand, List<IDbDataParameter>, List<IDbDataParameter>, string, string, bool> functionProcessor,
        bool continueOnProcessorException = false, string dbName = "");

        IDbCommand CreateCommand();

        bool Connect(string connectionString, CommandType commandType, string commandText);
    }
}