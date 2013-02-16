using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Xml;

namespace Dev2.Runtime.ServiceModel.Esb.Brokers
{
    /// <summary>
    /// A Microsoft SQL specific database broker implementation
    /// </summary>
    public class MsSqlBroker : AbstractDatabaseBroker
    {
        #region Override Methods

        /// <summary>
        /// Returns all stored procedures.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="procedureProcessor">The procedure processor.</param>
        /// <param name="functionProcessor">The function processor.</param>
        /// <param name="continueOnProcessorException">if set to <c>true</c> [continue on processor exception].</param>
        /// <exception cref="System.ArgumentException">Expected type SqlConnection.;connection</exception>
        protected override void GetStoredProcedures(IDbConnection connection, Func<IDbCommand, IList<IDataParameter>, string, bool> procedureProcessor, 
            Func<IDbCommand, IList<IDataParameter>, string, bool> functionProcessor, bool continueOnProcessorException = false)
        {
            var sqlConnection = connection as SqlConnection;
            if (sqlConnection == null)
            {
                throw new ArgumentException(string.Format("Expected type '{0}', received '{1}'.", typeof(SqlConnection), connection), "connection");
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

        /// <summary>
        /// Executes a select command.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <exception cref="System.ArgumentException">command</exception>
        protected override DataSet ExecuteSelect(IDbCommand command)
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

        /// <summary>
        /// Creates a connection.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        protected override IDbConnection CreateConnection(string connectionString)
        {
            return new SqlConnection(connectionString);
        }

        /// <summary>
        /// Mormalizes a data payload.
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <returns></returns>
        protected override string NormalizeXmlPayload(string payload)
        {
            DataTable dt;
            StringBuilder result = new StringBuilder();

            XmlDocument xDoc = new XmlDocument();
            xDoc.LoadXml(payload);
            XmlNodeList nl = xDoc.SelectNodes("//NewDataSet/Table/*[starts-with(local-name(),'XML_')]");
            int foundXMLFrags = 0;

            foreach (XmlNode n in nl)
            {
                string tmp = n.InnerXml;
                result = result.Append(tmp);
                foundXMLFrags++;
            }

            string res = result.ToString();

            if (foundXMLFrags >= 1)
            {
                res = "<FromXMLPayloads>" + res + "</FromXMLPayloads>";
            }
            else if (foundXMLFrags == 0)
            {
                res = payload;
            }

            return base.NormalizeXmlPayload(res);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Determines whether the specified row represents a stored procedure.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <param name="procedureTypeColumn">The procedure type column.</param>
        /// <returns>
        ///   <c>true</c> if the specified row is a stored procedure; otherwise, <c>false</c>.
        /// </returns>
        private bool IsStoredProcedure(DataRow row, DataColumn procedureTypeColumn)
        {
            if (row == null || procedureTypeColumn == null)
            {
                return false;
            }

            return row[procedureTypeColumn].ToString().Equals("PROCEDURE");
        }

        /// <summary>
        /// Determines whether the specified row represents a stored function.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <param name="procedureTypeColumn">The procedure type column.</param>
        /// <returns>
        ///   <c>true</c> if the specified row is a stored function; otherwise, <c>false</c>.
        /// </returns>
        private bool IsFunction(DataRow row, DataColumn procedureTypeColumn)
        {
            if (row == null || procedureTypeColumn == null)
            {
                return false;
            }

            return !row[procedureTypeColumn].ToString().Equals("PROCEDURE");
        }

        /// <summary>
        /// Gets the help text for a procedure/function.
        /// </summary>
        /// <param name="sqlConnection">The SQL connection.</param>
        /// <param name="fullProcedureName">Full name of the procedure.</param>
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

        /// <summary>
        /// Gets the parameters for a procedure/function.
        /// </summary>
        /// <param name="procedureCommand">The procedure command.</param>
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

        /// <summary>
        /// Gets the data column from a data table.
        /// </summary>
        /// <param name="dataTable">The data table.</param>
        /// <param name="columnName">Name of the column.</param>
        /// <exception cref="System.Exception"></exception>
        private DataColumn GetDataColumn(DataTable dataTable, string columnName)
        {
            DataColumn dataColumn = dataTable.Columns[columnName];
            if(dataColumn == null)
            {
                throw new Exception(string.Format("MSSQL Intergate : Unable to load '{0}' column of '{1}'.", columnName, dataTable.TableName));
            }
            return dataColumn;
        }

        /// <summary>
        /// Gets the schema from a data table.
        /// </summary>
        /// <param name="sqlConnection">The SQL connection.</param>
        /// <param name="collectionName">Name of the collection.</param>
        /// <exception cref="System.Exception"></exception>
        private DataTable GetSchema(SqlConnection sqlConnection, string collectionName)
        {
            DataTable proceduresDataTable = sqlConnection.GetSchema(collectionName);
            if(proceduresDataTable == null)
            {
                throw new Exception(string.Format("MSSQL Intergate : Unable to load the '{0}' schema.", collectionName));
            }
            return proceduresDataTable;
        }

        /// <summary>
        /// Creates a command for retrieving the help text of a procedure/function.
        /// </summary>
        /// <param name="sqlConnection">The SQL connection.</param>
        /// <param name="fullProcedureName">Full name of the procedure.</param>
        private SqlCommand CreateHelpTextCommand(SqlConnection sqlConnection, string fullProcedureName)    
        {
            SqlCommand cmd = new SqlCommand("sp_helptext '" + fullProcedureName + "'", sqlConnection) 
            { 
                CommandType = CommandType.Text 
            };

            return cmd;
        }

        /// <summary>
        /// Creates a command for executing a procedure/function.
        /// </summary>
        /// <param name="sqlConnection">The SQL connection.</param>
        /// <param name="fullCallName">Full name of the call.</param>
        private SqlCommand CreateProcedureCommand(SqlConnection sqlConnection, string fullCallName)    
        {
            SqlCommand cmd = new SqlCommand(fullCallName, sqlConnection) 
            { 
                CommandType = CommandType.StoredProcedure 
            };

            return cmd;
        }

        /// <summary>
        /// Gets the full name of the procedure/function.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <param name="procedureDataColumn">The procedure data column.</param>
        /// <param name="procedureSchemaColumn">The procedure schema column.</param>
        private string GetFullProcedureName(DataRow row, DataColumn procedureDataColumn, DataColumn procedureSchemaColumn)
        {
            string procedureName = row[procedureDataColumn].ToString();
            string schemaName = row[procedureSchemaColumn].ToString();
            return schemaName + "." + procedureName;
        }

        /// <summary>
        /// Load parameters of a command.
        /// </summary>
        /// <param name="command">The command.</param>
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
