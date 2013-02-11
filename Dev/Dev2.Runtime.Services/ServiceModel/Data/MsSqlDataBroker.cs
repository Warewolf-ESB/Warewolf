using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace Dev2.Runtime.ServiceModel.Data
{
    public class MsSqlDataBroker : DataBroker
    {
        #region Override Methods

        public override void GetStoredProcedures(IDbConnection connection, Func<IDbCommand, IList<IDataParameter>, string, bool> procedureProcessor, 
            Func<IDbCommand, IList<IDataParameter>, string, bool> functionProcessor, bool continueOnProcessorException = false)
        {
            var sqlConnection = connection as SqlConnection;
            if (sqlConnection == null)
            {
                throw new ArgumentException("Expected type SqlConnection.", "connection");
            }

            var proceduresDataTable = GetSchema(sqlConnection, "Procedures");
            var procedureDataColumn = GetDataColumn(proceduresDataTable, "ROUTINE_NAME");
            var procedureTypeColumn = GetDataColumn(proceduresDataTable, "ROUTINE_TYPE");
            var procedureSchemaColumn = GetDataColumn(proceduresDataTable, "SPECIFIC_SCHEMA"); // ROUTINE_CATALOG - ROUTINE_SCHEMA ,SPECIFIC_SCHEMA

            foreach (DataRow row in proceduresDataTable.Rows)
            {
                //
                // Create the procedure command
                //
                var fullProcedureName = GetFullProcedureName(row, procedureDataColumn, procedureSchemaColumn);
                var procedureCommand = CreateProcedureCommand(sqlConnection, fullProcedureName);

                //
                // Get procedures parameters
                //
                var parameters = GetProcedureParameters(procedureCommand);

                //
                // Get procedure help text
                //
                var procedureHelpText = GetProcedureHelpText(sqlConnection, fullProcedureName);

                //
                // Determine procedure type, stored procedure or function, and invoke the appropriate processor
                //
                if (IsStoredProcedure(row, procedureTypeColumn))
                {
                    procedureProcessor(procedureCommand, parameters, procedureHelpText);
                }
                else if (IsFunction(row, procedureTypeColumn))
                {
                    functionProcessor(procedureCommand, parameters, procedureHelpText);
                }
            }
        }

        public override DataSet ExecuteSelect(IDbCommand command)
        {
            var sqlCommand = command as SqlCommand;
            if (command == null)
            {
                throw new ArgumentException(string.Format("Expected type {0}.", typeof(SqlCommand)), "command");
            }

            var dataset = new DataSet();
            var adapter = new SqlDataAdapter(sqlCommand);

            adapter.Fill(dataset);

            return dataset;
        }

        #endregion

        #region Private Methods

        private bool IsStoredProcedure(DataRow row, DataColumn procedureTypeColumn)
        {
            if (row == null || procedureTypeColumn == null)
            {
                return false;
            }

            return row[procedureTypeColumn].ToString().Equals("PROCEDURE");
        }

        private bool IsFunction(DataRow row, DataColumn procedureTypeColumn)
        {
            if (row == null || procedureTypeColumn == null)
            {
                return false;
            }

            return !row[procedureTypeColumn].ToString().Equals("PROCEDURE");
        }

        private string GetProcedureHelpText(SqlConnection sqlConnection, string fullProcedureName)
        {
            StringBuilder sb = new StringBuilder();

            SqlCommand helpTextCommand = CreateHelpTextCommand(sqlConnection, fullProcedureName);
            try
            {
                Func<IDataReader, bool> helptextProcessor = reader =>
                {
                    object value = reader.GetValue(0);
                    if (value != null)
                    {
                        sb.Append(value.ToString());
                    }
                    return true;
                };
                ExecuteSelect(helpTextCommand, helptextProcessor, false, CommandBehavior.Default);
            }
            finally
            {
                helpTextCommand.Dispose();
            }

            return sb.ToString();
        }

        private List<IDataParameter> GetProcedureParameters(SqlCommand procedureCommand)
        {
            List<IDataParameter> parameters = new List<IDataParameter>();

            if (TryLoadCommandParameters(procedureCommand))
            {
                foreach (SqlParameter parameter in procedureCommand.Parameters)
                {
                    if (!parameter.ParameterName.Equals("@RETURN_VALUE"))
                    {
                        parameters.Add(parameter);
                    }
                }
            }

            return parameters;
        }

        private DataColumn GetDataColumn(DataTable dataTable, string columnName)
        {
            DataColumn dataColumn = dataTable.Columns[columnName];
            if(dataColumn == null)
            {
                throw new Exception(string.Format("MSSQL Intergate : Unable to load '{0}' column of '{1}'.", columnName, dataTable.TableName));
            }
            return dataColumn;
        }

        private DataTable GetSchema(SqlConnection sqlConnection, string collectionName)
        {
            DataTable proceduresDataTable = sqlConnection.GetSchema(collectionName);
            if(proceduresDataTable == null)
            {
                throw new Exception(string.Format("MSSQL Intergate : Unable to load the '{0}' schema.", collectionName));
            }
            return proceduresDataTable;
        }

        private SqlCommand CreateProcedureCommand(SqlConnection sqlConnection, string fullCallName)
        {
            SqlCommand cmd = new SqlCommand("sp_helptext '" + fullCallName + "'", sqlConnection) 
            { 
                CommandType = CommandType.Text 
            };

            return cmd;
        }

        private SqlCommand CreateHelpTextCommand(SqlConnection sqlConnection, string fullCallName)
        {
            SqlCommand cmd = new SqlCommand(fullCallName, sqlConnection) 
            { 
                CommandType = CommandType.StoredProcedure 
            };

            return cmd;
        }

        private string GetFullProcedureName(DataRow row, DataColumn procedureDataColumn, DataColumn procedureSchemaColumn)
        {
            string procedureName = row[procedureDataColumn].ToString();
            string schemaName = row[procedureSchemaColumn].ToString();
            return schemaName + "." + procedureName;
        }

        private bool TryLoadCommandParameters(SqlCommand command)
        {
            try
            {
                SqlCommandBuilder.DeriveParameters(command);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion

    }
}
