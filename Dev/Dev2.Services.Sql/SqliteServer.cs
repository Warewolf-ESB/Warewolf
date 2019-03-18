#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
﻿/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Services.Sql;
using Warewolf.Resource.Errors;
using System.Data.Common;


namespace Dev2.Services.Sql
{
	public class SqliteServer : IDbServer
	{
		readonly IDbFactory _factory;
		IDbCommand _command;
		IDbConnection _connection;

		public bool IsConnected => _connection != null && _connection.State == ConnectionState.Open;
		public SqliteServer()
		{
			_factory = new SqliteServerFactory();
		}
		public SqliteServer(string connectionString)
		{
			_factory = new SqliteServerFactory();
			_connection = (SQLiteConnection)_factory.CreateConnection(connectionString);
			_connection.Open();
		}
		public SqliteServer(IDbFactory dbFactory)
		{
			_factory = dbFactory;
		}

        public int? CommandTimeout { get; set; }

		public string ConnectionString => _connection == null ? null : _connection.ConnectionString;
		public void Connect(string connectionString)
		{
			_connection = (SQLiteConnection)_factory.CreateConnection(connectionString);
			_connection.Open();
		}
		public bool Connect(string connectionString, CommandType commandType, string commandText)
		{
			_connection = (SQLiteConnection)_factory.CreateConnection(connectionString);
			var commandT = commandType;
			VerifyArgument.IsNotNull("commandText", commandText);
			if (commandText.ToLower().StartsWith("select "))
			{
				commandT = CommandType.Text;
			}
			_command = _factory.CreateCommand(_connection, commandT, commandText, CommandTimeout);
			_connection.Open();
			return true;
		}
		public IDbConnection Connection()
		{
			VerifyConnection();
          
			return _connection;
		}
        public IDbCommand CreateCommand()
        {
            VerifyConnection();
            var command = _connection.CreateCommand();
            return command;
        }
        public DataTable FetchDataTable(IDbCommand command)
		{
			VerifyArgument.IsNotNull("command", command);

			return ExecuteReader(command, reader => _factory.CreateTable(reader, LoadOption.OverwriteChanges));
		}
		public DataSet FetchDataSet(IDbCommand command)
		{
			VerifyArgument.IsNotNull("command", command);

			return  _factory.FetchDataSet(command);
		}
		public int ExecuteNonQuery(IDbCommand command)
		{
			VerifyArgument.IsNotNull("command", command);

			return _factory.ExecuteNonQuery(command);
		}
        
        public int ExecuteScalar(IDbCommand command)
		{
			VerifyArgument.IsNotNull("command", command);

			return _factory.ExecuteScalar(command);
		}
		static T ExecuteReader<T>(IDbCommand command, Func<IDataAdapter, T> handler)
		{
			try
			{
				var adapter = new SQLiteDataAdapter(command as SQLiteCommand);
				using (adapter)
				{
					return handler(adapter);
				}
			}
			catch (DbException e)
			{
				if (e.Message.Contains("There is no text for object "))
				{
					var exceptionDataTable = new DataTable("Error");
					exceptionDataTable.Columns.Add("ErrorText");
					exceptionDataTable.LoadDataRow(new object[] { e.Message }, true);
					return handler(new SQLiteDataAdapter());
				}
				throw;
			}
		}
		void VerifyConnection()
		{
			if (!IsConnected)
			{
				throw new Exception(ErrorResource.PleaseConnectFirst);
			}
		}
		
		#region IDisposable

		bool _disposed;

		// Implement IDisposable.
		// Do not make this method virtual.
		// A derived class should not be able to override this method.
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

		~SqliteServer()
		{
			// Do not re-create Dispose clean-up code here.
			// Calling Dispose(false) is optimal in terms of
			// readability and maintainability.
			Dispose(false);
		}

		// Dispose(bool disposing) executes in two distinct scenarios.
		// If disposing equals true, the method has been called directly
		// or indirectly by a user's code. Managed and unmanaged resources
		// can be disposed.
		// If disposing equals false, the method has been called by the
		// runtime from inside the finalizer and you should not reference
		// other objects. Only unmanaged resources can be disposed.
		void Dispose(bool disposing)
		{
            // Check to see if Dispose has already been called.
            if (_disposed)
            {
                return;
            }
			// If disposing equals true, dispose all managed
			// and unmanaged resources.
			if (disposing)
			{
				// Dispose managed resources.

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

		#endregion IDisposable

		public void BeginTransaction()
		{
			throw new NotImplementedException();
		}

		public void RollbackTransaction()
		{
			throw new NotImplementedException();
		}

		public List<string> FetchDatabases()
		{
			throw new NotImplementedException();
		}

		public void FetchStoredProcedures(Func<IDbCommand, List<IDbDataParameter>, string, string, bool> procedureProcessor, Func<IDbCommand, List<IDbDataParameter>, string, string, bool> functionProcessor)
		{
			throw new NotImplementedException();
		}

		public void FetchStoredProcedures(Func<IDbCommand, List<IDbDataParameter>, string, string, bool> procedureProcessor, Func<IDbCommand, List<IDbDataParameter>, string, string, bool> functionProcessor, bool continueOnProcessorException, string dbName)
		{
			throw new NotImplementedException();
		}

		public void FetchStoredProcedures(Func<IDbCommand, List<IDbDataParameter>, List<IDbDataParameter>, string, string, bool> procedureProcessor, Func<IDbCommand, List<IDbDataParameter>, List<IDbDataParameter>, string, string, bool> functionProcessor)
		{
			throw new NotImplementedException();
		}

		public void FetchStoredProcedures(Func<IDbCommand, List<IDbDataParameter>, List<IDbDataParameter>, string, string, bool> procedureProcessor, Func<IDbCommand, List<IDbDataParameter>, List<IDbDataParameter>, string, string, bool> functionProcessor, bool continueOnProcessorException, string dbName)
		{
			throw new NotImplementedException();
		}
	}
}
