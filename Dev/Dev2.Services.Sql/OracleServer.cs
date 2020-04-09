#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
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
using System.Linq;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Services.Sql;
using Oracle.ManagedDataAccess.Client;

namespace Dev2.Services.Sql
{
    public sealed class OracleServer : IDbServer
    {
        readonly IDbFactory _factory;
        IDbCommand _command;
        IDbConnection _connection;
        IDbTransaction _transaction;
        string _owner;
        readonly bool _isTesting;

        public OracleServer(IDbFactory factory, IDbCommand command, IDbTransaction transaction)
        {
            _factory = factory;
            _command = command;

            var con = new OracleConnection
            {
                ConnectionString = string.Empty
            };

            _isTesting = true;
            _connection = con;
            _transaction = transaction;
        }

        public int? CommandTimeout { get; set; }

        public bool IsConnected
        {
            get
            {
                if (_isTesting)
                {
                    return true;
                }

                return _connection != null && _connection.State == ConnectionState.Open;
            }
        }
        public string ConnectionString => _connection?.ConnectionString;

        public void FetchStoredProcedures(Func<IDbCommand, List<IDbDataParameter>, List<IDbDataParameter>, string, string, bool> procedureProcessor, Func<IDbCommand, List<IDbDataParameter>, List<IDbDataParameter>, string, string, bool> functionProcessor) => FetchStoredProcedures(procedureProcessor, functionProcessor, false, "");

        public void FetchStoredProcedures(Func<IDbCommand, List<IDbDataParameter>, List<IDbDataParameter>, string, string, bool> procedureProcessor, Func<IDbCommand, List<IDbDataParameter>, List<IDbDataParameter>, string, string, bool> functionProcessor, bool continueOnProcessorException, string dbName)
        {
            VerifyArgument.IsNotNull("procedureProcessor", procedureProcessor);
            VerifyArgument.IsNotNull("functionProcessor", functionProcessor);
            VerifyConnection();
            _owner = dbName;
            var proceduresDataTable = GetSchema(_connection);

            foreach (DataRow row in proceduresDataTable.Rows)
            {
                var type = row["ROUTINE_TYPE"];
                if (type.ToString().ToUpperInvariant() == "FUNCTION")
                {
                    continue;
                }

                var fullProcedureName = row["NAME"].ToString();

                if (row["DB"].ToString().Equals(dbName, StringComparison.OrdinalIgnoreCase))
                {
                    using (IDbCommand command = _factory.CreateCommand(_connection, CommandType.StoredProcedure, _owner + "." + fullProcedureName, CommandTimeout))
                    {
                        TryProcessProcedure(procedureProcessor, continueOnProcessorException, dbName, fullProcedureName, command);
                    }
                }
            }
        }

        private void TryProcessProcedure(Func<IDbCommand, List<IDbDataParameter>, List<IDbDataParameter>, string, string, bool> procedureProcessor, bool continueOnProcessorException, string dbName, string fullProcedureName, IDbCommand command)
        {
            try
            {
                var parameters = GetProcedureParameters(command, dbName, fullProcedureName, out List<IDbDataParameter> outParameters);
                var helpText = FetchHelpTextContinueOnException(fullProcedureName, _connection);
                procedureProcessor(command, parameters, outParameters, helpText, fullProcedureName);
            }
            catch (Exception)
            {
                if (!continueOnProcessorException)
                {
                    throw;
                }
            }
        }

        public IDbCommand CreateCommand()
        {
            VerifyConnection();
            var command = _connection.CreateCommand();
            if (!_isTesting)
            {
                command.Transaction = _transaction;
            }
            return command;
        }

        public void BeginTransaction()
        {
            if (IsConnected)
            {
                _transaction = _connection.BeginTransaction();
            }
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

        public List<string> FetchDatabases()
        {
            VerifyConnection();
            OracleDataReader reader = null;
            var result = new List<string>();
            var cmd = new OracleCommand("SELECT DISTINCT(OWNER) AS DATABASE_NAME FROM DBA_SEGMENTS WHERE OWNER IN (SELECT USERNAME FROM DBA_USERS WHERE DEFAULT_TABLESPACE NOT IN ('SYSTEM','SYSAUX'))", (OracleConnection)_connection);
            try
            {
                if (!_isTesting)
                {
                    reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        result.Add(reader.GetString(0));
                    }
                }
            }
            finally
            {
                reader?.Close();
            }

            return result;
        }

        
        public DataTable FetchDataTable(IDbCommand command)
        {
            VerifyArgument.IsNotNull("command", command);

            return ExecuteReader(command, reader => _factory.CreateTable(reader, LoadOption.OverwriteChanges));
        }

        public DataTable FetchDataTable(IDbDataParameter[] parameters, IEnumerable<IDbDataParameter> outparameters)
        {
            VerifyConnection();
            if (!_isTesting)
            {
                AddParameters(_command, parameters);

                foreach (var par in outparameters)
                {
                    _command.Parameters.Add(par);
                }
            }
            return FetchDataTable(_command);
        }
		public DataSet FetchDataSet(IDbCommand command)
		{
			VerifyArgument.IsNotNull("command", command);

			return _factory.FetchDataSet(command);
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
		public void FetchStoredProcedures(Func<IDbCommand, List<IDbDataParameter>, string, string, bool> procedureProcessor,
            Func<IDbCommand, List<IDbDataParameter>, string, string, bool> functionProcessor) => FetchStoredProcedures(procedureProcessor, functionProcessor, false, "");

        public void FetchStoredProcedures(Func<IDbCommand, List<IDbDataParameter>, string, string, bool> procedureProcessor,
            Func<IDbCommand, List<IDbDataParameter>, string, string, bool> functionProcessor,
            bool continueOnProcessorException, string dbName)
        {
            VerifyArgument.IsNotNull("procedureProcessor", procedureProcessor);
            VerifyArgument.IsNotNull("functionProcessor", functionProcessor);
            VerifyConnection();
            _owner = dbName;
            var proceduresDataTable = GetSchema(_connection);

            foreach (DataRow row in proceduresDataTable.Rows)//Procedure 2
            {
                var fullProcedureName = row["NAME"].ToString();

                if (row["DB"].ToString().Equals(dbName, StringComparison.OrdinalIgnoreCase))
                {
                    using (IDbCommand command = _factory.CreateCommand(_connection, CommandType.StoredProcedure, _owner + "." + fullProcedureName, CommandTimeout))
                    {
                        TryProcessProcedure(procedureProcessor, continueOnProcessorException, dbName, fullProcedureName, command);
                    }
                }
            }
        }

        private void TryProcessProcedure(Func<IDbCommand, List<IDbDataParameter>, string, string, bool> procedureProcessor, bool continueOnProcessorException, string dbName, string fullProcedureName, IDbCommand command)
        {
            try
            {
                var parameters = DbDataParameters(dbName, command, fullProcedureName, out List<IDbDataParameter> isOut, out string helpText);
                procedureProcessor(command, parameters, helpText, fullProcedureName);
            }
            catch (Exception)
            {
                if (!continueOnProcessorException)
                {
                    throw;
                }
            }
        }

        List<IDbDataParameter> DbDataParameters(string dbName, IDbCommand command, string fullProcedureName, out List<IDbDataParameter> isOut,
            out string helpText)
        {
            var parameters = GetProcedureParameters(command, dbName, fullProcedureName, out isOut);
            helpText = FetchHelpTextContinueOnException(fullProcedureName, _connection);
            return parameters;
        }



        string FetchHelpTextContinueOnException(string fullProcedureName, IDbConnection con)
        {
            var helpText = "";

            try
            {
                if (!_isTesting)
                {
                    helpText = GetHelpText(con, fullProcedureName);
                }
            }
            catch (Exception e)
            {
                helpText = "Could not fetch because of : " + e.Message;
            }

            return helpText;
        }

        void VerifyConnection()
        {
            if (!IsConnected)
            {
                throw new Exception("Please connect first.");
            }
        }

        public void Connect(string connectionString)
        {
            if (connectionString.Contains("Database"))
            {
                connectionString = connectionString.Replace(connectionString.Substring(connectionString.IndexOf("Database", StringComparison.Ordinal)), "");
            }
            _connection = (OracleConnection)_factory.CreateConnection(connectionString);
            if (!_isTesting)
            {
                _connection.Open();
            }
        }
        public bool Connect(string connectionString, CommandType commandType, string commandText)
        {
            if (connectionString != null && connectionString.Contains("Database"))
            {
                connectionString = connectionString.Replace(connectionString.Substring(connectionString.IndexOf("Database", StringComparison.Ordinal)), "");
            }


            CreateConnect(connectionString, commandType, commandText);

            return true;
        }

        void CreateConnect(string connectionString, CommandType commandType, string commandText)
        {
            if (!_isTesting)
            {
                _connection = _factory.CreateConnection(connectionString);

                VerifyArgument.IsNotNull("commandText", commandText);
                if (commandText.ToLower().StartsWith("select "))
                {
                    commandType = CommandType.Text;
                }

                _command = _factory.CreateCommand(_connection, commandType, commandText, CommandTimeout);

                _connection.Open();
            }
        }

        static T ExecuteReader<T>(IDbCommand command, Func<IDataAdapter, T> handler)
        {
            try
            {
                var singleOutParams = new List<IDataParameter>();
                if (command.CommandType == CommandType.StoredProcedure)
                {
                    var obj = new OracleParameter();
                    obj = GetProcInputs(command, singleOutParams, obj);
                    command.ExecuteNonQuery();
                    if (obj.ParameterName.Length > 0)
                    {
                        return ExecuteAsOracleCommand(command, handler);
                    }
                    if (singleOutParams.Count > 0)
                    {
                        var table = new DataTable("SingleValues");
                        table.Columns.AddRange(singleOutParams.Select(parameter => new DataColumn(parameter.ParameterName)).ToArray());
                        table.LoadDataRow(singleOutParams.Select(parameter => parameter.Value).ToArray(), true);
                        return handler(new OracleDataAdapter());
                    }
                }
                var oraDa = new OracleDataAdapter(command as OracleCommand);
                using (oraDa)
                {
                    return handler(oraDa);
                }

            }
            catch (DbException e)
            {
                if (e.Message.Contains("There is no text for object "))
                {
                    var exceptionDataTable = new DataTable("Error");
                    exceptionDataTable.Columns.Add("ErrorText");
                    exceptionDataTable.LoadDataRow(new object[] { e.Message }, true);
                    return handler(new OracleDataAdapter());
                }
                throw;
            }
        }

        private static T ExecuteAsOracleCommand<T>(IDbCommand command, Func<IDataAdapter, T> handler)
        {
            try
            {
                var da = new OracleDataAdapter(command as OracleCommand);
                using (da)
                {
                    return handler(da);
                }
            }
            catch (Exception e)
            {
                Dev2Logger.Warn("Error executing oracle command. " + e.Message, GlobalConstants.WarewolfWarn);
                var da = new OracleDataAdapter(command as OracleCommand);
                using (da)
                {
                    return handler(da);
                }
            }
        }

        private static OracleParameter GetProcInputs(IDbCommand command, List<IDataParameter> singleOutParams, OracleParameter obj)
        {
            for (int i = 0; i < command.Parameters.Count; i++)
            {
                var temp = (OracleParameter)command.Parameters[i];

                if (temp.OracleDbType == OracleDbType.RefCursor)
                {
                    obj = (OracleParameter)command.Parameters[i];
                }
                else
                {
                    if (temp.Direction != ParameterDirection.Input)
                    {
                        singleOutParams.Add(temp);
                    }
                }
            }

            return obj;
        }

        public static void AddParameters(IDbCommand command, ICollection<IDbDataParameter> parameters)
        {
            command.Parameters.Clear();
            if (parameters != null && parameters.Count > 0)
            {
                foreach (IDbDataParameter parameter in parameters)
                {
                    command.Parameters.Add(parameter);
                }
            }
        }

        DataTable GetSchema(IDbConnection connection)
        {
            var CommandText = GlobalConstants.SchemaQueryOracle;
            if (_isTesting)
            {
                var proceduresDataTable = new DataTable();
                proceduresDataTable.Columns.Add("NAME");
                proceduresDataTable.Columns.Add("DB");
                proceduresDataTable.Rows.Add("Test", "Test");
                return proceduresDataTable;
            }
            using (IDbCommand command = _factory.CreateCommand(connection, CommandType.Text, String.Format(CommandText, _owner), CommandTimeout))
            {
                return FetchDataTable(command);
            }
        }

        string GetHelpText(IDbConnection connection, string objectName)
        {
            using (IDbCommand command = _factory.CreateCommand(connection, CommandType.Text,
                $"SELECT text FROM all_source WHERE name='{objectName}' ORDER BY line", CommandTimeout))
            {
                return ExecuteReader(command, GetStringBuilder);
            }
        }

        string GetStringBuilder(IDataAdapter reader)
        {
            var sb = new StringBuilder();
            var ds = new DataSet(); //conn is opened by dataadapter
            reader.Fill(ds);
            var t = ds.Tables[0];
            var dataTableReader = t.CreateDataReader();
            while (dataTableReader.Read())
            {
                var value = dataTableReader.GetValue(0);
                if (value != null)
                {
                    sb.Append(value);
                }
            }
            return sb.ToString();
        }

        public List<OracleParameter> GetProcedureOutParams(string fullProcedureName, string dbName)
        {
            using (IDbCommand command = _factory.CreateCommand(_connection, CommandType.StoredProcedure, fullProcedureName, CommandTimeout))
            {
                if (!_isTesting)
                {
                    GetProcedureParameters(command, dbName, fullProcedureName, out List<IDbDataParameter> isOut);
                    return isOut.Select(a => a as OracleParameter).ToList();
                }
            }

            return new List<OracleParameter>();
        }
        public List<IDbDataParameter> GetProcedureInputParameters(IDbCommand command, string dbName, string procedureName)
        {
            var parameteres = new List<IDbDataParameter>();
            command.CommandType = CommandType.Text;
            command.CommandText = $"SELECT * from all_arguments where owner = '{dbName}' and object_name = '{procedureName.Substring(procedureName.IndexOf(".", StringComparison.Ordinal) + 1)}'";
            var dataTable = FetchDataTable(command);
            foreach (DataRow row in dataTable.Rows)
            {
                var parameterName = row["ARGUMENT_NAME"] as string;
                var InOut = row["IN_OUT"] as string;

                var isout = GetIsout(InOut);
                if (isout)
                {
                    continue;
                }


                Enum.TryParse(((string)row["DATA_TYPE"]).Replace(" ", ""), true, out OracleDbType OracleType);
                var OracleParameter = GetOracleParameter(OracleType, row, parameterName, ParameterDirection.Input);
                parameteres.Add(OracleParameter);
            }
            return parameteres;
        }

        List<IDbDataParameter> GetProcedureParameters(IDbCommand command, string dbName, string procedureName, out List<IDbDataParameter> outParams)
        {
            outParams = new List<IDbDataParameter>();
            //Please do not use SqlCommandBuilder.DeriveParameters(command); as it does not handle CLR procedures correctly.
            var originalCommandText = command.CommandText;
            var parameters = new List<IDbDataParameter>();
            command.CommandType = CommandType.Text;
            command.CommandText = $"SELECT * from all_arguments where owner = '{dbName}' and object_name = '{procedureName.Substring(procedureName.IndexOf(".", StringComparison.Ordinal) + 1)}'";

            var dataTable = FetchDataTable(command);
            if (!_isTesting)
            {
                GetParamValues(command, outParams, dataTable, parameters);
            }
            command.CommandText = originalCommandText;
            return parameters;
        }

        void GetParamValues(IDbCommand command, List<IDbDataParameter> outParams, DataTable dataTable, List<IDbDataParameter> parameters)
        {
            foreach (DataRow row in dataTable.Rows)
            {
                var parameterName = row["ARGUMENT_NAME"] as string;
                var InOut = row["IN_OUT"] as string;

                var isout = GetIsout(InOut);
                const ParameterDirection direction = ParameterDirection.Output;

                if (!String.IsNullOrEmpty(parameterName))
                {

                    Enum.TryParse(((string)row["DATA_TYPE"]).Replace(" ", ""), true, out OracleDbType OracleType);
                    var OracleParameter = GetOracleParameter(OracleType, row, parameterName, direction);

                    GetOutParamProperties(command, outParams, parameters, isout, OracleParameter);
                }
            }
        }

        OracleParameter GetOracleParameter(OracleDbType OracleType, DataRow row, string parameterName, ParameterDirection direction)
        {
            OracleParameter oracleParameter;
            if (OracleType == 0)
            {
                var dataType = row["DATA_TYPE"].ToString();
                OracleType = GetOracleDbType(dataType);

                oracleParameter = new OracleParameter(parameterName, OracleType) { Direction = direction };
            }
            else
            {
                oracleParameter = new OracleParameter(parameterName, OracleType) { Direction = direction };
            }
            var size = "";
            try
            {
                var sizeVal = row["SIZE"];
                if (sizeVal != null)
                {
                    size = sizeVal.ToString();
                }

            }
            catch (Exception)
            {
                size = string.Empty;
            }
            var sizeValue = string.IsNullOrEmpty(size) ? GetSizeForType(OracleType) : int.Parse(size);
            oracleParameter.Size = sizeValue;
            return oracleParameter;
        }

        void GetOutParamProperties(IDbCommand command, List<IDbDataParameter> outParams, List<IDbDataParameter> parameters, bool isout, OracleParameter OracleParameter)
        {
            if (!isout)
            {
                OracleParameter.Direction = ParameterDirection.Input;
                if (!_isTesting)
                {
                    command.Parameters.Add(OracleParameter);
                }
                parameters.Add(OracleParameter);
            }
            else
            {
                outParams.Add(OracleParameter);
            }
        }

        OracleDbType GetOracleDbType(string dataType)
        {
            OracleDbType OracleType;
            switch (dataType.ToUpper())
            {
                case "NUMBER":
                    {
                        OracleType = OracleDbType.Decimal;
                        break;
                    }
                case "DATE":
                    {
                        OracleType = OracleDbType.Date;
                        break;
                    }
                case "FLOAT":
                    {
                        OracleType = OracleDbType.Single;
                        break;
                    }
                case "BYTE":
                    {
                        OracleType = OracleDbType.Int16;
                        break;
                    }
                case "LONG":
                    {
                        OracleType = OracleDbType.Int64;
                        break;
                    }
                case "INT":
                    {
                        OracleType = OracleDbType.Int32;
                        break;
                    }
                case "SHORT":
                    {
                        OracleType = OracleDbType.Int16;
                        break;
                    }
                case "CHAR":
                    {
                        OracleType = OracleDbType.Char;
                        break;
                    }
                default:
                    {
                        OracleType = OracleDbType.Varchar2;
                        break;
                    }
            }
            return OracleType;
        }

        bool GetIsout(string InOut)
        {
            var isout = !(InOut != null && InOut.Contains("IN/OUT"));
            if (InOut != null && InOut.Trim().ToUpper() == "IN ".Trim().ToUpper())
            {
                return false;
            }
            return isout;
        }

#pragma warning disable S1541 // Methods and properties should not be too complex
        static int GetSizeForType(OracleDbType dbType)
#pragma warning restore S1541 // Methods and properties should not be too complex
        {
            switch (dbType)
            {
                case OracleDbType.BFile:
                case OracleDbType.Blob:
                case OracleDbType.Clob:
                    return int.MaxValue;
                case OracleDbType.Byte:
                    return 8;
                case OracleDbType.Char:
                    return 1;
                case OracleDbType.Date:
                    return 100;
                case OracleDbType.Decimal:
                case OracleDbType.Double:
                    return int.MaxValue;
                case OracleDbType.Long:
                case OracleDbType.LongRaw:
                    return int.MaxValue;
                case OracleDbType.Int16:
                    return Int16.MaxValue;
                case OracleDbType.Int32:
                    return int.MaxValue;
                case OracleDbType.Int64:
                    return int.MaxValue;
                case OracleDbType.IntervalDS:
                    break;
                case OracleDbType.IntervalYM:
                    break;
                case OracleDbType.NClob:
                case OracleDbType.NChar:
                    return 3000;
                case OracleDbType.NVarchar2:
                    return 3000;
                case OracleDbType.Raw:
                    break;
                case OracleDbType.Single:
                    break;
                case OracleDbType.TimeStamp:
                case OracleDbType.TimeStampLTZ:
                case OracleDbType.TimeStampTZ:
                    return 100;
                case OracleDbType.XmlType:
                case OracleDbType.Varchar2:
                    return 4000;
                case OracleDbType.BinaryDouble:
                    break;
                case OracleDbType.BinaryFloat:
                    break;
                case OracleDbType.RefCursor:
                    break;
                default:
                    return 4000;
            }

            return 4000;
        }

        bool _disposed;

        public OracleServer()
        {
            _factory = new OracleSqlFactory();
        }

        public OracleServer(IDbFactory dbFactory)
        {
            _factory = dbFactory;
        }

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

        ~OracleServer()
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
            if(!_disposed)
            {
                // If disposing equals true, dispose all managed 
                // and unmanaged resources. 
                if(disposing)
                {
                    // Dispose managed resources.
                    _transaction?.Dispose();

                    _command?.Dispose();
                    DisposeConnection();
                }

                // Call the appropriate methods to clean up 
                // unmanaged resources here. 
                // If disposing is false, 
                // only the following code is executed.

                // Note disposing has been done.
                _disposed = true;
            }
        }

        private void DisposeConnection()
        {
            if (_connection != null)
            {
                if (_connection.State != ConnectionState.Closed)
                {
                    _connection.Close();
                }
                _connection.Dispose();
            }
        }
    }
}
