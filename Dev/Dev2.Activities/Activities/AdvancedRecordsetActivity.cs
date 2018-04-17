using System;
using System.Activities;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dev2.Activities.Debug;
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
using TSQL.Tokens;
using TSQL.Statements;
using Warewolf.Storage.Interfaces;
using Dev2.Common.Interfaces;
using System.Text.RegularExpressions;
using Dev2.Common.Interfaces.DB;
using Dev2.DataList.Contract;
using System.Globalization;
using Dev2.Util;

namespace Dev2.Activities
{

    [ToolDescriptorInfo("AdvancedRecordset", "Advanced Recordset", ToolType.Native, "8999E59B-38A3-43BB-A98F-6090C5C9EA1E", "Dev2.Acitivities", "1.0.0.0", "Legacy", "Recordset", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_AdvancedRecordset")]
    public class AdvancedRecordsetActivity : DsfActivity, IEquatable<AdvancedRecordsetActivity>
    {
        public IExecutionEnvironment ExecutionEnvironment { get; protected set; }
        private AdvancedRecordset AdvancedRecordset { get; set; }
        public string SqlQuery { get; set; }
        public string ObjectName { get; set; }
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
        void ExecuteRecordset(IDSFDataObject dataObject, int update)
        {
            AdvancedRecordset = new AdvancedRecordset(dataObject.Environment);
            AddDeclarations(dataObject, update);

			var queryText = AddSqlForVariables(SqlQuery);
			var statements = TSQLStatementReader.ParseStatements(queryText);
			var allTables = new List<string>();
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
					AdvancedRecordset.ApplyResultToEnvironment(dt.TableName, Outputs, dt.Rows.Cast<DataRow>().ToList(), false, update);
				}
			}
			else
			{
				foreach (TSQLStatement statement in statements)
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
						ProcessSelectStatement(selectStatement, update);
					}
					else
					{
						var unknownStatement = statement as TSQLUnknownStatement;
						ProcessComplexStatement(unknownStatement, update);
					}
				}
			}
            foreach (var table in allTables)
            {
                AdvancedRecordset.DeleteTableInSqlite(table);
            }
        }

		void ProcessSelectStatement(TSQLSelectStatement selectStatement, int update)
		{
			var results = AdvancedRecordset.ExecuteQuery(AdvancedRecordset.ReturnSql(selectStatement.Tokens));
			foreach (DataTable dt in results.Tables)
			{
				AdvancedRecordset.ApplyResultToEnvironment(dt.TableName, Outputs, dt.Rows.Cast<DataRow>().ToList(), false, update);
			}

		}
		void ProcessComplexStatement(TSQLUnknownStatement complexStatement, int update)
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
                    ProcessUpdateStatement(complexStatement, update);
                }
                if (tokens[i].Type.ToString() == "Keyword" && (tokens[i].Text.ToUpper() == "DELETE"))
                {
                    ProcessUpdateStatement(complexStatement, update);
                }
				if (tokens[i].Type.ToString() == "Keyword" && (tokens[i].Text.ToUpper() == "INSERT"))
				{
					ProcessUpdateStatement(complexStatement, update);
				}
				if (tokens[i].Type.ToString() == "Identifier" && (tokens[i].Text.ToUpper() == "REPLACE"))
				{
					ProcessUpdateStatement(complexStatement, update);
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
        void ProcessUpdateStatement(TSQLUnknownStatement complexStatement, int update)
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
                AdvancedRecordset.ApplyResultToEnvironment(outputRecordsetName, Outputs, dt.Rows.Cast<DataRow>().ToList(), true, update);
            }
        }

        void LoadRecordset(string tableName)
        {

            AdvancedRecordset.LoadRecordsetAsTable(tableName);
        }
       
        void AddDeclarations(IDSFDataObject dataObject, int update)
        {
            AdvancedRecordset.CreateVariableTable();
            try
            {
                foreach (var variable in DeclareVariables)
                {
                    if (variable.Name.Length > 0 && variable.Value.Length > 0)
                    {
                        InsertIntoVariableTable(variable.Name, variable.Value, dataObject, update);
                    }
                }
            }
            catch (Exception e)
            {
                Dev2Logger.Error("AdvancedRecorset", e, GlobalConstants.WarewolfError);
            }
        }
        string AddSqlForVariables(string queryText) => Regex.Replace(queryText, @"\@\w+\b", match => AdvancedRecordset.GetVariableValue(match.Value));
        void InsertIntoVariableTable(string varName, string varValue, IDSFDataObject dataObject, int update)
        {
            try
            {
                var value = varValue;
                if (DataListUtil.IsFullyEvaluated(value))
                {
                    var variableValue = dataObject.Environment.Eval(value, update);
                    if (variableValue.IsWarewolfAtomResult && variableValue is CommonFunctions.WarewolfEvalResult.WarewolfAtomResult data && !data.Item.IsNothing)
                    {
                        AdvancedRecordset.InsertIntoVariableTable(varName, data.Item.ToString());
                    }
                    if (variableValue.IsWarewolfAtomResult && variableValue is CommonFunctions.WarewolfEvalResult.WarewolfAtomResult dataValue && dataValue.Item.IsNothing)
                    {
                        AdvancedRecordset.InsertIntoVariableTable(varName, value);
                    }
                }
                else
                {
                    AdvancedRecordset.InsertIntoVariableTable(varName, value);
                }
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
            var execErrors = new ErrorResultTO();

            tmpErrors = new ErrorResultTO();
            tmpErrors.MergeErrors(execErrors);

            var allErrors = new ErrorResultTO();
            InitializeDebug(dataObject);
            try
            {
                var advancedRecordset = new AdvancedRecordset(dataObject.Environment);
                var personRecordsetName = "person";
                var addressRecordsetName = "address";
                advancedRecordset.LoadRecordsetAsTable(personRecordsetName);
                advancedRecordset.LoadRecordsetAsTable(addressRecordsetName);
                var results = advancedRecordset.ExecuteQuery(dataObject.QueryString);
                // apply sql results to environment
                advancedRecordset.ApplyResultToEnvironment("", Outputs, results.Tables[0].Rows.Cast<DataRow>().ToList(), false, 0);
                var fetchErrors = allErrors.FetchErrors();
                foreach (var error in fetchErrors)
                {
                    dataObject.Environment.Errors.Add(error);
                }
            }
            catch (Exception e)
            {
                allErrors.AddError(e.Message);
            }

            tmpErrors.MergeErrors(execErrors);

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
            if (ReferenceEquals(null, other))
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
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((AdvancedRecordsetActivity)obj);
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
