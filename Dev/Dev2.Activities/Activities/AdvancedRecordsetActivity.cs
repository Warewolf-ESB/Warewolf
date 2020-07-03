#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Data;
using Dev2.Data.TO;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.Diagnostics;
using Dev2.Interfaces;
using Dev2.Util;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using TSQL;
using TSQL.Clauses;
using TSQL.Statements;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Core;
using Warewolf.Storage;
using Warewolf.Storage.Interfaces;

namespace Dev2.Activities
{
    [ToolDescriptorInfo("AdvancedRecordset", "Advanced Recordset", ToolType.Native, "8999E59B-38A3-43BB-A98F-6090C5C9EA1E", "Dev2.Activities", "1.0.0.0", "Legacy", "Recordset", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_AdvancedRecordset")]
    public class AdvancedRecordsetActivity : DsfActivity, IEquatable<AdvancedRecordsetActivity>, IDisposable
    {
        public IExecutionEnvironment ExecutionEnvironment { get; protected set; }
        public string SqlQuery { get; set; }
        public string RecordsetName { get; set; }
        public IList<INameValue> DeclareVariables { get; set; }
        public string ExecuteActionString { get; set; }
        IAdvancedRecordsetActivityWorker _worker;

        public AdvancedRecordsetActivity()
        {
            Construct(new AdvancedRecordsetActivityWorker(this));
        }
        public AdvancedRecordsetActivity(IAdvancedRecordsetActivityWorker worker)
        {
            Construct(worker);
        }
        private void Construct(IAdvancedRecordsetActivityWorker worker)
        {
            _worker = worker;
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
                _worker.AddValidationErrors(allErrors);
                if (!allErrors.HasErrors())
                {
                    if (dataObject.IsDebugMode())
                    {
                        ExecuteToolAddDebugItems(dataObject, update);
                    }
                    _worker.ExecuteRecordset(dataObject, update);
                }
            }
            catch (Exception e)
            {
                Dev2Logger.Error("AdvancedRecordset", e, GlobalConstants.WarewolfError);
                allErrors.AddError(e.Message);
            }
            finally
            {
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

        [ExcludeFromCodeCoverage]
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

        public void ExecuteToolAddDebugItems(IDSFDataObject dataObject, int update)
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

        public Dictionary<string, List<string>> GetIdentifiers()
        {
            return _worker.GetIdentifiers();
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
                if (SqlQuery != null)
                {
                    hashCode = (hashCode * 397) ^ (SqlQuery.GetHashCode());
                }
                return hashCode;
            }
        }

        public void Dispose()
        {
            if (_worker != null)
            {
                _worker.Dispose();
                _worker = null;
            }
        }
    }

    public interface IAdvancedRecordsetActivityWorker : IDisposable
    {
        IAdvancedRecordset AdvancedRecordset { get; set; }
        void LoadRecordset(string tableName);
        void AddDeclarations(string varName, string varValue);
        void AddValidationErrors(ErrorResultTO allErrors);
        void ExecuteSql(int update, ref bool started);
        void ExecuteRecordset(IDSFDataObject dataObject, int update);
        Dictionary<string, List<string>> GetIdentifiers();
    }

    internal class AdvancedRecordsetActivityWorker : IAdvancedRecordsetActivityWorker
    {
        int _recordsAffected;
        AdvancedRecordsetActivity _activity;
        private IAdvancedRecordset _advancedRecordset;
        private readonly IAdvancedRecordsetFactory _advancedRecordsetFactory;

        [ExcludeFromCodeCoverage]
        public AdvancedRecordsetActivityWorker(AdvancedRecordsetActivity activity)
            : this(activity, new AdvancedRecordset(), new AdvancedRecordsetFactory())
        {
        }

        public AdvancedRecordsetActivityWorker(AdvancedRecordsetActivity activity, IAdvancedRecordset recordset)
            : this(activity, recordset, new AdvancedRecordsetFactory())
        {
        }

        public AdvancedRecordsetActivityWorker(AdvancedRecordsetActivity activity, IAdvancedRecordset advancedrecordset, IAdvancedRecordsetFactory advancedRecordsetFactory)
        {
            _activity = activity;
            _advancedRecordset = advancedrecordset;
            _advancedRecordsetFactory = advancedRecordsetFactory;
        }

        public IAdvancedRecordset AdvancedRecordset
        {
            get => _advancedRecordset;
            set => _advancedRecordset = value;
        }

        public void LoadRecordset(string tableName)
        {
            _advancedRecordset.LoadRecordsetAsTable(tableName);
        }

        public void AddDeclarations(string varName, string varValue)
        {
            try
            {
                _advancedRecordset.CreateVariableTable();
                InsertIntoVariableTable(varName, varValue);
            }
            catch (Exception e)
            {
                Dev2Logger.Error("AdvancedRecorset", e, GlobalConstants.WarewolfError);
            }
        }

        string AddSqlForVariables(string queryText) => Regex.Replace(queryText, @"\@\w+\b", match =>
        {
            if (_activity.DeclareVariables.FirstOrDefault(nv => "@" + nv.Name == match.Value) != null)
            {
                return _advancedRecordset.GetVariableValue(match.Value);
            }
            return match.Value;
        });

        void InsertIntoVariableTable(string varName, string value)
        {
            try
            {
                _advancedRecordset.InsertIntoVariableTable(varName, value);
            }
            catch (Exception e)
            {
                Dev2Logger.Error("AdvancedRecorset", e, GlobalConstants.WarewolfError);
            }
        }

        void ProcessSelectStatement(TSQLSelectStatement selectStatement, int update, ref bool started)
        {
            var sqlQuery = _advancedRecordset.UpdateSqlWithHashCodes(selectStatement);
            var results = _advancedRecordset.ExecuteQuery(sqlQuery);
            foreach (DataTable dt in results.Tables)
            {
                _advancedRecordset.ApplyResultToEnvironment(dt.TableName, _activity.Outputs, dt.Rows.Cast<DataRow>().ToList(), false, update, ref started);
            }
        }

#pragma warning disable S1541 // Methods and properties should not be too complex
        void ProcessComplexStatement(TSQLUnknownStatement complexStatement, int update, ref bool started)
#pragma warning restore S1541 // Methods and properties should not be too complex
        {
            var tokens = complexStatement.Tokens;
            for (int i = 0; i < complexStatement.Tokens.Count; i++)
            {
                //(i == 1 && tokens[i].Type.ToString() == "Identifier" && (tokens[i - 1].Text.ToUpper() == "TABLE"))
                if (tokens[i].Type.ToString() == "Keyword" && (tokens[i].Text.ToUpper() == "CREATE"))
                {
                    ProcessCreateTableStatement(complexStatement);
                }
                if (tokens[i].Type.ToString() == "Keyword" && (tokens[i].Text.ToUpper() == "UPDATE"))
                {
                    ProcessUpdateStatement(complexStatement, update, ref started);
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
            recordset.Rows.Add(_advancedRecordset.ExecuteNonQuery(_advancedRecordset.ReturnSql(complexStatement.Tokens)));
            var outputName = _activity.Outputs.FirstOrDefault(e => e.MappedFrom == "records_affected").MappedTo;
            _advancedRecordset.ApplyScalarResultToEnvironment(outputName, int.Parse(recordset.Rows[0].ItemArray[0].ToString()));
        }

#pragma warning disable S1541 // Methods and properties should not be too complex
        void ProcessUpdateStatement(TSQLUnknownStatement complexStatement, int update, ref bool started)
#pragma warning restore S1541 // Methods and properties should not be too complex
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

            var sqlQuery = _advancedRecordset.UpdateSqlWithHashCodes(complexStatement);

            var recordset = new DataTable();
            recordset.Columns.Add("records_affected", typeof(int));
            recordset.Rows.Add(_advancedRecordset.ExecuteNonQuery(sqlQuery));
            object sumObject;
            sumObject = recordset.Compute("Sum(records_affected)", "");
            _recordsAffected += Convert.ToInt16(sumObject.ToString());
            var mapping = _activity.Outputs.FirstOrDefault(e => e.MappedFrom == "records_affected");


            if (mapping != null)
            {
                _advancedRecordset.ApplyScalarResultToEnvironment(mapping.MappedTo, _recordsAffected);
            }
            var results = _advancedRecordset.ExecuteQuery("SELECT * FROM " + _advancedRecordset.HashedRecSets.FirstOrDefault(x => x.recSet == outputRecordsetName).hashCode);
            foreach (DataTable dt in results.Tables)
            {
                _advancedRecordset.ApplyResultToEnvironment(outputRecordsetName, _activity.Outputs, dt.Rows.Cast<DataRow>().ToList(), true, update, ref started);
            }
        }

        public void AddValidationErrors(ErrorResultTO allErrors)
        {
            if (DataListUtil.HasNegativeIndex(_activity.SqlQuery))
            {
                allErrors.AddError(string.Format("Negative Recordset Index for SqlQuery: {0}", _activity.SqlQuery));
            }
        }

        public void ExecuteSql(int update, ref bool started)
        {
            var queryText = AddSqlForVariables(_activity.SqlQuery);
            var statements = TSQLStatementReader.ParseStatements(queryText);

            if (queryText.Contains("UNION") && statements.Count == 2)
            {
                var tables = statements[0].GetAllTables();
                foreach (var table in tables)
                {
                    LoadRecordset(table.TableName);
                }
                var sqlQueryToUpdate = queryText;
                foreach (var item in _advancedRecordset.HashedRecSets)
                {
                    sqlQueryToUpdate = sqlQueryToUpdate.Replace(item.recSet, item.hashCode);
                }
                var results = _advancedRecordset.ExecuteQuery(sqlQueryToUpdate);
                foreach (DataTable dt in results.Tables)
                {
                    _advancedRecordset.ApplyResultToEnvironment(dt.TableName, _activity.Outputs, dt.Rows.Cast<DataRow>().ToList(), false, update, ref started);
                }
            }
            else
            {
                ExecuteAllSqlStatements(update, statements, ref started);
            }
        }

        private void ExecuteAllSqlStatements(int update, List<TSQLStatement> statements, ref bool started)
        {
            foreach (var statement in statements)
            {
                var tables = statement.GetAllTables();
                foreach (var table in tables)
                {
                    LoadRecordset(table.TableName);
                }
                if (statement.Type == TSQLStatementType.Select)
                {
                    var selectStatement = statement as TSQLSelectStatement;
                    ProcessSelectStatement(selectStatement, update, ref started);
                }
                else
                {
                    var unknownStatement = statement as TSQLUnknownStatement;
                    ProcessComplexStatement(unknownStatement, update, ref started);
                }
            }
        }

        public void ExecuteRecordset(IDSFDataObject dataObject, int update)
        {
            var env = dataObject.Environment;
            AdvancedRecordset = _advancedRecordsetFactory.New(env);
            var iter = new WarewolfListIterator();
            var started = false;
            var itemsToIterateOver = new Dictionary<string, IWarewolfIterator>();
            if (_activity.DeclareVariables == null || _activity.DeclareVariables.All(d => String.IsNullOrEmpty(d.Value)))
            {
                ExecuteSql(update, ref started);
            }
            else
            {
                _recordsAffected = 0;
                foreach (var declare in _activity.DeclareVariables)
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
                    ExecuteSql(update, ref started);
                    started = true;
                }
            }
            foreach (var hashedRecSet in _advancedRecordset.HashedRecSets)
            {
                _advancedRecordset.DeleteTableInSqlite(hashedRecSet.hashCode);
            }
        }

        public Dictionary<string, List<string>> GetIdentifiers()
        {
            Dictionary<string, List<string>> identifiers = new Dictionary<string, List<string>>();

            try
            {
                var queryText = AddSqlForVariables(_activity.SqlQuery);
                var statements = TSQLStatementReader.ParseStatements(queryText);

                List<string> identifierVariables = null;

                foreach (var statement in statements)
                {
                    if (statement.Type == TSQLStatementType.Select)
                    {
                        foreach (var table in statement.GetAllTables())
                        {
                            var key = table.TableName;

                            identifierVariables = new List<string>();

                            identifierVariables.AddRange(statement.Tokens.Where(a => a.Type == TSQL.Tokens.TSQLTokenType.Identifier && a.Text != key).Select(a => a.Text));

                            identifiers.Add(key, identifierVariables);
                        }
                    }
                }
            }

            catch { }

            return identifiers;
        }

        public void Dispose()
        {
            _activity = null;
        }
    }
}
