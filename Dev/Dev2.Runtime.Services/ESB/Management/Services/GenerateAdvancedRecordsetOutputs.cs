#pragma warning disable
/*
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
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.Enums;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.Services.Sql;
using Dev2.Workspaces;
using TSQL;
using TSQL.Statements;
using Warewolf.Core;

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    /// Generates output mappings for Advanced Recordset SQL queries by analyzing the query structure
    /// This service creates an in-memory SQLite database, loads recordset schemas, executes the query,
    /// and returns the resulting column definitions as output mappings
    /// </summary>
    public class GenerateAdvancedRecordsetOutputs : IEsbManagementEndpoint
    {
        public Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs) => Guid.Empty;

        public AuthorizationContext GetAuthorizationContextForService() => AuthorizationContext.Contribute;

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var msg = new ExecuteMessage();
            var serializer = new Dev2JsonSerializer();
            
            try
            {
                Dev2Logger.Info("Generate Advanced Recordset Outputs", GlobalConstants.WarewolfInfo);

                values.TryGetValue("AdvancedRecordsetService", out StringBuilder serviceDefinition);
                
                if (serviceDefinition == null || serviceDefinition.Length == 0)
                {
                    throw new ArgumentException("AdvancedRecordsetService parameter is required");
                }

                // Remove extra curly braces if present (double-serialization issue)
                var jsonString = serviceDefinition.ToString().Trim();
                if (jsonString.StartsWith("{{") && jsonString.EndsWith("}}"))
                {
                    jsonString = jsonString.Substring(1, jsonString.Length - 2);
                }

                var serviceRequest = serializer.Deserialize<AdvancedRecordsetServiceRequest>(jsonString);
                
                if (string.IsNullOrWhiteSpace(serviceRequest.SqlQuery))
                {
                    throw new ArgumentException("SQL query cannot be empty");
                }

                ValidateDeclareVariables(serviceRequest.DeclareVariables);

                var outputs = GenerateOutputMappings(serviceRequest);

                var response = new AdvancedRecordsetOutputsResponse
                {
                    Outputs = outputs.Outputs,
                    RecordsetName = outputs.RecordsetName,
                    Success = true
                };

                msg.HasError = false;
                msg.Message = serializer.SerializeToBuilder(response);
            }
            catch (Exception err)
            {
                msg.HasError = true;
                var errorResponse = new AdvancedRecordsetOutputsResponse
                {
                    Success = false,
                    Error = err.Message
                };
                msg.Message = serializer.SerializeToBuilder(errorResponse);
                Dev2Logger.Error(err, GlobalConstants.WarewolfError);
            }

            return serializer.SerializeToBuilder(msg);
        }

        private void ValidateDeclareVariables(List<NameValue> declareVariables)
        {
            if (declareVariables != null)
            {
                var invalidVariable = declareVariables.FirstOrDefault(v => 
                    !string.IsNullOrEmpty(v.Name) && string.IsNullOrEmpty(v.Value));
                
                if (invalidVariable != null)
                {
                    throw new ArgumentException($"Variable '{invalidVariable.Name}' must have a value");
                }
            }
        }

        private AdvancedRecordsetOutputsResult GenerateOutputMappings(AdvancedRecordsetServiceRequest request)
        {
            using (var dbManager = new SqliteServer("Data Source=:memory:"))
            {
                var statements = TSQLStatementReader.ParseStatements(request.SqlQuery);
                
                if (statements.Count == 0)
                {
                    throw new ArgumentException("Invalid SQL query - no statements found");
                }

                if (request.Recordsets == null || request.Recordsets.Count == 0)
                {
                    throw new ArgumentException("Recordsets parameter is required. Please provide the recordset definitions from your DataList.");
                }

                var hashedRecSets = new List<(string hashCode, string recSetName)>();
                
                foreach (var recordset in request.Recordsets)
                {
                    var recSetHash = "A" + recordset.Name.GetHashCode().ToString().Replace("-", "B");
                    hashedRecSets.Add((recSetHash, recordset.Name));
                    
                    AddRecordsetAsTable(dbManager, recSetHash, recordset.Fields.ToList());
                }

                DataSet result;
                var countOfStatements = statements.Count;

                if (request.SqlQuery.Contains("UNION") && countOfStatements == 2)
                {
                    result = ProcessUnionQuery(request.SqlQuery, hashedRecSets, dbManager);
                }
                else
                {
                    result = ProcessRegularQuery(statements, hashedRecSets, dbManager);
                }

                var table = result.Tables[0];
                var fields = GetFields(table);
                
                var recordsetName = string.IsNullOrEmpty(request.RecordsetName) 
                    ? table.TableName + "Copy" 
                    : request.RecordsetName;
                
                var outputs = fields.Select(field => new ServiceOutputMapping
                {
                    MappedFrom = field,
                    MappedTo = field,
                    RecordSetName = recordsetName
                }).ToList();

                return new AdvancedRecordsetOutputsResult
                {
                    Outputs = outputs,
                    RecordsetName = recordsetName
                };
            }
        }

        private void AddRecordsetAsTable(SqliteServer dbManager, string recordsetName, List<string> fields)
        {
            ExecuteQuery(dbManager, "CREATE TABLE IF NOT EXISTS " + recordsetName + "([" + recordsetName + "_Primary_Id] INTEGER NOT NULL, CONSTRAINT[PK_" + recordsetName + "] PRIMARY KEY([" + recordsetName + "_Primary_Id]))");
            foreach (var field in fields)
            {
                if (!string.IsNullOrEmpty(field))
                {
                    ExecuteNonQuery(dbManager, "ALTER TABLE  " + recordsetName + " ADD COLUMN " + field + " string;");
                }
            }
        }

        private void ExecuteNonQuery(SqliteServer dbManager, string sqlQuery)
        {
            using (var cmd = dbManager.CreateCommand())
            {
                cmd.CommandText = sqlQuery;
                cmd.CommandType = CommandType.Text;
                dbManager.ExecuteNonQuery(cmd);
            }
        }

        private DataSet ProcessUnionQuery(string sqlQuery, List<(string hashCode, string recSetName)> hashedRecSets, 
            SqliteServer dbManager)
        {
            var sqlQueryToUpdate = sqlQuery;
            
            foreach (var item in hashedRecSets)
            {
                sqlQueryToUpdate = sqlQueryToUpdate.Replace(item.recSetName, item.hashCode);
            }
            
            var sql = Regex.Replace(sqlQueryToUpdate, @"\@\w+\b", match => "''");
            
            return ExecuteQuery(dbManager, sql);
        }

        private DataSet ProcessRegularQuery(List<TSQLStatement> statements, 
            List<(string hashCode, string recSetName)> hashedRecSets, 
            SqliteServer dbManager)
        {
            var countOfStatements = statements.Count;
            DataSet result = null;

            for (var i = 0; i < countOfStatements; i++)
            {
                var statement = statements[i];
                var sql = UpdateSqlWithHashCodes(statement, hashedRecSets);
                
                sql = Regex.Replace(sql, @"\@\w+\b", match => "''");
                
                result = ExecuteStatement(dbManager, statement, sql);
                
                if (i != countOfStatements - 1)
                {
                    continue;
                }
            }

            return result;
        }

        private DataSet ExecuteStatement(SqliteServer dbManager, TSQLStatement sqlStatement, string query)
        {
            if (sqlStatement.Type == TSQLStatementType.Select)
            {
                return ExecuteQuery(dbManager, query);
            }
            var recordset = new DataTable();
            recordset.Columns.Add("records_affected", typeof(int));
            recordset.Rows.Add(ExecuteNonQueryWithReturn(dbManager, query));
            var ds = new DataSet();
            ds.Tables.Add(recordset);
            return ds;
        }

        private int ExecuteNonQueryWithReturn(SqliteServer dbManager, string sqlQuery)
        {
            using (var cmd = dbManager.CreateCommand())
            {
                cmd.CommandText = sqlQuery;
                cmd.CommandType = CommandType.Text;
                return dbManager.ExecuteNonQuery(cmd);
            }
        }

        private DataSet ExecuteQuery(SqliteServer dbManager, string sqlQuery)
        {
            try
            {
                var command = dbManager.CreateCommand();
                command.CommandText = sqlQuery;
                command.CommandType = CommandType.Text;
                var ds = dbManager.FetchDataSet(command);
                return ds;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        private string UpdateSqlWithHashCodes(TSQLStatement statement, 
            List<(string hashCode, string recSetName)> hashedRecSets)
        {
            var sqlBuildUp = new List<string>();
            
            foreach (var token in statement.Tokens)
            {
                if (token.Type == TSQL.Tokens.TSQLTokenType.Identifier && sqlBuildUp.Count >= 1)
                {
                    if (sqlBuildUp[sqlBuildUp.Count - 1] == ".")
                    {
                        sqlBuildUp.Add(token.Text);
                    }
                    else
                    {
                        var hash = hashedRecSets.FirstOrDefault(x => x.recSetName == token.Text);
                        sqlBuildUp.Add(hash != default ? hash.hashCode : token.Text);
                    }
                }
                else
                {
                    sqlBuildUp.Add(token.Text);
                }
            }
            
            return string.Join(" ", sqlBuildUp);
        }

        private List<string> GetFields(DataTable table)
        {
            var fields = new List<string>();
            
            foreach (DataColumn column in table.Columns)
            {
                if (!column.ColumnName.Contains("Primary_Id"))
                {
                    fields.Add(column.ColumnName);
                }
            }
            
            return fields;
        }

        public DynamicService CreateServiceEntry() => 
            EsbManagementServiceEntry.CreateESBManagementServiceEntry(
                HandlesType(), 
                "<DataList><AdvancedRecordsetService ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public string HandlesType() => "GenerateAdvancedRecordsetOutputs";
    }

    #region Request/Response Data Transfer Objects

    /// <summary>
    /// Request payload for generating Advanced Recordset outputs
    /// Contains the SQL query, variables, and recordset definitions
    /// </summary>
    public class AdvancedRecordsetServiceRequest
    {
        public string SqlQuery { get; set; }
        public List<NameValue> DeclareVariables { get; set; }
        public List<RecordsetDefinition> Recordsets { get; set; }
        public string RecordsetName { get; set; }
    }
    
    /// <summary>
    /// Concrete implementation of name-value pair for JSON deserialization
    /// </summary>
    public class NameValue
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    /// <summary>
    /// Represents a recordset definition with its name and field list
    /// </summary>
    public class RecordsetDefinition
    {
        public string Name { get; set; }
        public List<string> Fields { get; set; }
    }

    /// <summary>
    /// Response payload containing the generated output mappings
    /// </summary>
    public class AdvancedRecordsetOutputsResponse
    {
        public bool Success { get; set; }
        public List<ServiceOutputMapping> Outputs { get; set; }
        public string RecordsetName { get; set; }
        public string Error { get; set; }
    }

    /// <summary>
    /// Internal result object for output generation
    /// </summary>
    internal class AdvancedRecordsetOutputsResult
    {
        public List<ServiceOutputMapping> Outputs { get; set; }
        public string RecordsetName { get; set; }
    }

    #endregion
}
