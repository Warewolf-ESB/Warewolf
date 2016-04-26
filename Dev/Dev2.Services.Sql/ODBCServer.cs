/*
*  Warewolf - The Easy Service Bus
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Odbc;
using Dev2.Common.Interfaces.Services.Sql;
using Microsoft.Win32;

namespace Dev2.Services.Sql
{
    public class ODBCServer : IDbServer
    {
        private readonly IDbFactory _factory;
        private OdbcConnection _connection;
        private IDbTransaction _transaction;
        private bool _disposed;
        private IDbCommand _command;


        public ODBCServer(IDbFactory dbFactory)
        {
            _factory = dbFactory;
        }
        public ODBCServer()
        {
            _factory = new ODBCFactory();
        }
        public bool Connect(string connectionString, CommandType commandType, string commandText)
        {
            _connection = (OdbcConnection)_factory.CreateConnection(connectionString);

            VerifyArgument.IsNotNull("commandText", commandText);
            if (commandText.ToLower().StartsWith("select "))
            {
                commandType = CommandType.Text;
            }

            _command = _factory.CreateCommand(_connection, commandType, commandText);

            _connection.Open();
            return true;
        }
        public string ConnectionString
        {
            get { return _connection == null ? null : _connection.ConnectionString; }
        }

        public bool IsConnected
        {
            get { return _connection != null && _connection.State == ConnectionState.Open; }
        }

        public void BeginTransaction()
        {
            if (IsConnected)
            {
                _transaction = _connection.BeginTransaction();
            }
        }

        public bool Connect(string connectionString)
        {

            _connection = (OdbcConnection)_factory.CreateConnection(connectionString);
            _connection.Open();
            return true;
        }

        public IDbCommand CreateCommand()
        {
            VerifyConnection();
            IDbCommand command = _connection.CreateCommand();
            command.Transaction = _transaction;
            return command;
        }

        #region VerifyConnection

        private void VerifyConnection()
        {
            if (!IsConnected)
            {
                throw new Exception("Please connect first.");
            }
        }

        #endregion

        public void Dispose()
        {
            Dispose(true);
            // This object will be cleaned up by the Dispose method. 
            // Therefore, you should call GC.SupressFinalize to 
            // take this object off the finalization queue 
            // and prevent finalization code for this object 
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called. 
            if (!_disposed)
            {
                // If disposing equals true, dispose all managed 
                // and unmanaged resources. 
                if (disposing)
                {
                    // Dispose managed resources.
                    if (_transaction != null)
                    {
                        _transaction.Dispose();
                    }

                    if (_command != null)
                    {
                        _command.Dispose();
                    }

                    if (_connection != null)
                    {
                        if (_connection.State != ConnectionState.Closed)
                        {
                            _connection.Close();
                        }
                        _connection.Dispose();
                    }
                }

                // Call the appropriate methods to clean up 
                // unmanaged resources here. 
                // If disposing is false, 
                // only the following code is executed.

                // Note disposing has been done.
                _disposed = true;
            }
        }

        public List<string> FetchDatabases()
        {
            return GetDSN();
        }

        public DataTable FetchDataTable(IDbCommand command)
        {
            VerifyArgument.IsNotNull("command", command);

            return ExecuteReader(command, CommandBehavior.SchemaOnly & CommandBehavior.KeyInfo,
                reader => _factory.CreateTable(reader, LoadOption.OverwriteChanges));
        }
        public DataTable FetchDataTable()
        {


            return ExecuteReader(_command, CommandBehavior.SchemaOnly & CommandBehavior.KeyInfo,
                reader => _factory.CreateTable(reader, LoadOption.OverwriteChanges));
        }
        private static T ExecuteReader<T>(IDbCommand command, CommandBehavior commandBehavior,
            Func<IDataReader, T> handler)
        {
            try
            {
                if (command.CommandType == CommandType.StoredProcedure)
                {
                    command.CommandType = CommandType.Text;

                    using (IDataReader reader = command.ExecuteReader(commandBehavior))
                    {
                        return handler(reader);
                    }
                }
                else
                {
                    using (IDataReader reader = command.ExecuteReader(commandBehavior))
                    {
                        return handler(reader);
                    }
                }
            }
            catch (DbException e)
            {
                if (e.Message.Contains("There is no text for object "))
                {
                    var exceptionDataTable = new DataTable("Error");
                    exceptionDataTable.Columns.Add("ErrorText");
                    exceptionDataTable.LoadDataRow(new object[] { e.Message }, true);
                    return handler(new DataTableReader(exceptionDataTable));
                }
                throw;
            }
        }

        public void FetchStoredProcedures(Func<IDbCommand, List<IDbDataParameter>, List<IDbDataParameter>, string, string, bool> procedureProcessor, Func<IDbCommand, List<IDbDataParameter>, List<IDbDataParameter>, string, string, bool> functionProcessor, bool continueOnProcessorException = false, string dbName = "")
        {
            throw new NotImplementedException();
        }

        public void FetchStoredProcedures(Func<IDbCommand, List<IDbDataParameter>, string, string, bool> procedureProcessor, Func<IDbCommand, List<IDbDataParameter>, string, string, bool> functionProcessor, bool continueOnProcessorException = false, string dbName = "")
        {
            throw new NotImplementedException();
        }

        public void RollbackTransaction()
        {
            if (_transaction != null)
            {
                _transaction.Rollback();
                _transaction.Dispose();
                _transaction = null;
            }
        }



        #region helper methods



        public List<string> GetDSN()
        {
            List<string> list = new List<string>();
            list.AddRange(EnumDsn(Registry.CurrentUser, false));
            list.AddRange(EnumDsn(Registry.LocalMachine, false));
            if (Environment.Is64BitOperatingSystem)
            {
                list.AddRange(EnumDsn(Registry.CurrentUser, true));
                list.AddRange(EnumDsn(Registry.LocalMachine, true));
            }
            return list;
        }

        private IEnumerable<string> EnumDsn(RegistryKey rootKey, bool is64)
        {
            RegistryKey regKey = rootKey.OpenSubKey(@"Software\ODBC\ODBC.INI\ODBC Data Sources");
            if (is64)
            {
                regKey = rootKey.OpenSubKey(@"Software\Wow6432Node\ODBC\ODBC.INI\ODBC Data Sources");
            }
            if (regKey != null)
            {
                foreach (string name in regKey.GetValueNames())
                {
                    yield return name;
                }
            }
        }



        #endregion
    }


}
