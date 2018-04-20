using System;
using System.Activities;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dev2.Common;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Data.TO;
using Dev2.Data.Util;
using Dev2.Diagnostics;
using Dev2.Interfaces;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Core;
using TSQL;
using TSQL.Statements;
using Warewolf.Storage.Interfaces;
using Dev2.Common.Interfaces;
using System.Text.RegularExpressions;
using Dev2.DataList.Contract;
using Dev2.Util;
using Warewolf.Storage;
using Dev2.Data;

namespace Dev2.Activities
{

    [ToolDescriptorInfo("AdvancedRecordset", "Advanced Recordset", ToolType.Native, "8999E59B-38A3-43BB-A98F-6090C5C9EA1E", "Dev2.Acitivities", "1.0.0.0", "Legacy", "Recordset", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_AdvancedRecordset")]
    public class AdvancedRecordsetActivity : DsfActivity, IEquatable<AdvancedRecordsetActivity>
    {
        public IExecutionEnvironment ExecutionEnvironment { get; protected set; }
        private AdvancedRecordset AdvancedRecordset { get; set; }
        public string SqlQuery { get; set; }
        public string RecordsetName { get; set; }
        public IList<INameValue> DeclareVariables { get; set; }
        public string ExecuteActionString { get; set; }
        public AdvancedRecordsetActivity()
        {
            Type = "Advanced Recordset";
            DisplayName = "Advanced Recordset";
            DeclareVariables = new List<INameValue>();
        }
        public override enFindMissingType GetFindMissingType() => enFindMissingType.DataGridActivity;
        protected override void OnExecute(NativeActivityContext context)
        {
            var dataObject = context.GetExtension<IDSFDataObject>();
            ExecuteTool(dataObject, 0);
        }
        protected override void ExecuteTool(IDSFDataObject dataObject, int update)
        {
            var allErrors = new ErrorResultTO();
            InitializeDebug(dataObject);
            try
            {
                AddValidationErrors(allErrors);
                if (!allErrors.HasErrors())
                {
                    if (dataObject.IsDebugMode())
                    {
                        ExecuteToolAddDebugItems(dataObject, update);
                    }
                    ExecuteRecordset(dataObject, update);
                }
            }
            catch (Exception e)
            {
                Dev2Logger.Error("AdvancedRecordset", e, GlobalConstants.WarewolfError);
                allErrors.AddError(e.Message);
            }
            finally
            {
                // Handle Errors
                var hasErrors = allErrors.HasErrors();

                if (hasErrors)
                {
                    DisplayAndWriteError("AdvancedRecordset", allErrors);
                    var errorString = allErrors.MakeDisplayReady();
                    dataObject.Environment.AddError(errorString);
                }
                if (dataObject.IsDebugMode())
                {
                    DispatchDebugState(dataObject, StateType.Before, update);
                    DispatchDebugState(dataObject, StateType.After, update);
                }
            }
        }

        private void ExecuteToolAddDebugItems(IDSFDataObject dataObject, int update)
        {
            var resDebug = new DebugEvalResult(SqlQuery, "Query", dataObject.Environment, update);
            AddDebugInputItem(resDebug);
            if (DeclareVariables != null)
            {
                foreach (var item in DeclareVariables)
                {
                    if (!string.IsNullOrEmpty(item.Name))
                    {
                        var decVarDebug = new DebugEvalResult(item.Value, item.Name, dataObject.Environment, update);
                        AddDebugInputItem(decVarDebug);
                    }
                }
            }
        }

        void ExecuteRecordset(IDSFDataObject dataObject, int update)
        {
            var env = dataObject.Environment;
            AdvancedRecordset = new AdvancedRecordset(env);
            var iter = new WarewolfListIterator();
            var started = false;
            var itemsToIterateOver = new Dictionary<string, IWarewolfIterator>();
            var allTables = new List<string>();
            if (DeclareVariables == null || DeclareVariables.All(d=>String.IsNullOrEmpty(d.Value)))
            {
                ExecuteSql(update, allTables,ref started);
            }
            else
            {
                foreach (var declare in DeclareVariables)
                {
                    if (string.IsNullOrEmpty(declare.Value))
                    {
                        continue;
                    }
                    var res = new WarewolfIterator(env.Eval(declare.Value, update));
                    iter.AddVariableToIterateOn(res);
                    itemsToIterateOver.Add(declare.Name, res);
                }
                
                while (iter.HasMoreData())
                {
                    foreach (var item in itemsToIterateOver)
                    {
                        AddDeclarations(item.Key, iter.FetchNextValue(item.Value));
                    }
                    ExecuteSql(update, allTables,ref started);
                    started = true;
                }
            }
            foreach (var table in allTables)
            {
                AdvancedRecordset.DeleteTableInSqlite(table);
            }

        }

        private void ExecuteSql(int update, List<string> allTables,ref bool started)
        {
            var queryText = AddSqlForVariables(SqlQuery);
            var statements = TSQLStatementReader.ParseStatements(queryText);

            if (queryText.Contains("UNION") && statements.Count == 2)
            {
                var tables = statements[0].GetAllTables();
                foreach (var table in tables)
                {
                    LoadRecordset(table.TableName);
                    allTables.Add(table.TableName);
                }
                var results = AdvancedRecordset.ExecuteQuery(queryText);
                foreach (DataTable dt in results.Tables)
                {
                    AdvancedRecordset.ApplyResultToEnvironment(dt.TableName, Outputs, dt.Rows.Cast<DataRow>().ToList(), false, update,ref started);
                }
            }
            else
            {
                ExecuteAllSqlStatements(update, allTables, statements, ref started);
            }
        }

        private void ExecuteAllSqlStatements(int update, List<string> allTables, List<TSQLStatement> statements,ref bool started)
        {
            foreach (var statement in statements)
            {
                var tables = statement.GetAllTables();
                foreach (var table in tables)
                {
                    LoadRecordset(table.TableName);
                    allTables.Add(table.TableName);
                }
                if (statement.Type == TSQLStatementType.Select)
                {
                    var selectStatement = statement as TSQLSelectStatement;
                    ProcessSelectStatement(selectStatement, update,ref started);
                }
                else
                {
                    var unknownStatement = statement as TSQLUnknownStatement;
                    ProcessComplexStatement(unknownStatement, update, ref started);
                }
            }
        }

        void ProcessSelectStatement(TSQLSelectStatement selectStatement, int update,ref bool started)
        {
            var results = AdvancedRecordset.ExecuteQuery(AdvancedRecordset.ReturnSql(selectStatement.Tokens));
            foreach (DataTable dt in results.Tables)
            {
                AdvancedRecordset.ApplyResultToEnvironment(dt.TableName, Outputs, dt.Rows.Cast<DataRow>().ToList(), false, update,ref started);
            }

        }
        void ProcessComplexStatement(TSQLUnknownStatement complexStatement, int update,ref bool started)
        {
            var tokens = complexStatement.Tokens;
            for (int i = 0; i < complexStatement.Tokens.Count; i++)
            {
                if (i == 1 && tokens[i].Type.ToString() == "Identifier" && (tokens[i - 1].Text.ToUpper() == "TABLE"))
                {
                    ProcessCreateTableStatement(complexStatement);
                }
                if (tokens[i].Type.ToString() == "Keyword" && (tokens[i].Text.ToUpper() == "UPDATE"))
                {
                    ProcessUpdateStatement(complexStatement, update,ref started);
                }
                if (tokens[i].Type.ToString() == "Keyword" && (tokens[i].Text.ToUpper() == "DELETE"))
                {
                    ProcessUpdateStatement(complexStatement, update, ref started);
                }
                if (tokens[i].Type.ToString() == "Keyword" && (tokens[i].Text.ToUpper() == "INSERT"))
                {
                    ProcessUpdateStatement(complexStatement, update, ref started);
                }
                if (tokens[i].Type.ToString() == "Identifier" && (tokens[i].Text.ToUpper() == "REPLACE"))
                {
                    ProcessUpdateStatement(complexStatement, update, ref started);
                }
            }
        }
        void ProcessCreateTableStatement(TSQLUnknownStatement complexStatement)
        {
            var recordset = new DataTable();
            recordset.Columns.Add("records_affected", typeof(int));
            recordset.Rows.Add(AdvancedRecordset.ExecuteNonQuery(AdvancedRecordset.ReturnSql(complexStatement.Tokens)));
            var outputName = Outputs.FirstOrDefault(e => e.MappedFrom == "records_affected").MappedTo;
            AdvancedRecordset.ApplyScalarResultToEnvironment(outputName, recordset.Rows.Cast<DataRow>().ToList());
        }
        void ProcessUpdateStatement(TSQLUnknownStatement complexStatement, int update,ref bool started)
        {
            var tokens = complexStatement.Tokens;
            var outputRecordsetName = "";
            for (int i = 0; i < complexStatement.Tokens.Count; i++)
            {
                if (i == 1 && tokens[i].Type.ToString() == "Identifier" && (tokens[i - 1].Text.ToUpper() == "UPDATE"))
                {
                    outputRecordsetName = tokens[i].Text;
                }
                if (tokens[i].Type.ToString() == "Keyword" && (tokens[i].Text.ToUpper() == "INSERT"))
                {
                    outputRecordsetName = tokens[i + 2].Text;
                }
                if (tokens[i].Type.ToString() == "Identifier" && (tokens[i].Text.ToUpper() == "REPLACE"))
                {
                    outputRecordsetName = tokens[i + 2].Text;
                }
                if (tokens[i].Type.ToString() == "Keyword" && (tokens[i].Text.ToUpper() == "DELETE"))
                {
                    outputRecordsetName = tokens[i + 2].Text;
                }
            }
            var recordset = new DataTable();
            recordset.Columns.Add("records_affected", typeof(int));
            recordset.Rows.Add(AdvancedRecordset.ExecuteNonQuery(AdvancedRecordset.ReturnSql(complexStatement.Tokens)));
            var mapping = Outputs.FirstOrDefault(e => e.MappedFrom == "records_affected");
            if (mapping != null)
            {
                AdvancedRecordset.ApplyScalarResultToEnvironment(mapping.MappedTo, recordset.Rows.Cast<DataRow>().ToList());
            }
            var results = AdvancedRecordset.ExecuteQuery("SELECT * FROM " + outputRecordsetName);
            foreach (DataTable dt in results.Tables)
            {
                AdvancedRecordset.ApplyResultToEnvironment(outputRecordsetName, Outputs, dt.Rows.Cast<DataRow>().ToList(), true, update,ref started);
            }
        }

        void LoadRecordset(string tableName)
        {

            AdvancedRecordset.LoadRecordsetAsTable(tableName);
        }
       
        void AddDeclarations(string varName,string varValue)
        {
            try
            {
                AdvancedRecordset.CreateVariableTable();
                InsertIntoVariableTable(varName, varValue);
            }
            catch (Exception e)
            {
                Dev2Logger.Error("AdvancedRecorset", e, GlobalConstants.WarewolfError);
            }
        }
        string AddSqlForVariables(string queryText) => Regex.Replace(queryText, @"\@\w+\b", match => AdvancedRecordset.GetVariableValue(match.Value));
        void InsertIntoVariableTable(string varName, string value)
        {
            try
            {
                AdvancedRecordset.InsertIntoVariableTable(varName, value);
            }
            catch (Exception e)
            {
                Dev2Logger.Error("AdvancedRecorset", e, GlobalConstants.WarewolfError);
            }
        }
        void AddValidationErrors(ErrorResultTO allErrors)
        {
            if (DataListUtil.HasNegativeIndex(SqlQuery))
            {
                allErrors.AddError(string.Format("Negative Recordset Index for SqlQuery: {0}", SqlQuery));
            }
        }
        public override List<string> GetOutputs()
        {
            if (Outputs == null)
            {
                if (IsObject)
                {
                    return new List<string> { ObjectName };
                }
                var parser = DataListFactory.CreateOutputParser();
                var outputs = parser.Parse(OutputMapping);
                return outputs.Select(definition => definition.RawValue).ToList();
            }
            return Outputs.Select(mapping => mapping.MappedTo).ToList();
        }
        protected override void ExecutionImpl(IEsbChannel esbChannel, IDSFDataObject dataObject, string inputs, string outputs, out ErrorResultTO tmpErrors, int update)
        {
            throw new NotImplementedException();
        }
        public override List<DebugItem> GetDebugInputs(IExecutionEnvironment env, int update)
        {
            foreach (IDebugItem debugInput in _debugInputs)
            {
                debugInput.FlushStringBuilder();
            }
            return _debugInputs;
        }
        public bool Equals(AdvancedRecordsetActivity other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return base.Equals(other) && string.Equals(SqlQuery, other.SqlQuery);
        }
        public override bool Equals(object obj)
        {
            if (obj is AdvancedRecordsetActivity act)
            {
                return Equals(act);
            }
            return false;
        }
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (SourceId.GetHashCode());
                if (ExecuteActionString != null)
                {
                    hashCode = (hashCode * 397) ^ (ExecuteActionString.GetHashCode());
                }
                return hashCode;
            }
        }
    }
}
