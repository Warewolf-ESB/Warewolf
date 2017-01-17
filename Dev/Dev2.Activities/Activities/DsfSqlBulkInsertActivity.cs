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
using System.Activities;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using Dev2.Activities.Debug;
using Dev2.Activities.SqlBulkInsert;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Data;
using Dev2.Data.TO;
using Dev2.Diagnostics;
using Dev2.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.TO;
using Dev2.Util;
using MySql.Data.MySqlClient;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;
using Warewolf.Core;
using Warewolf.Resource.Errors;
using Warewolf.Storage;
// ReSharper disable MemberCanBePrivate.Global

namespace Dev2.Activities
{
    [ToolDescriptorInfo("MicrosoftSQL", "SQL Bulk Insert", ToolType.Native, "8999E59A-38A3-43BB-A98F-6090C5C9EA1E", "Dev2.Acitivities", "1.0.0.0", "Legacy", "Database", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_Database_SQL_Bulk_Insert")]
    public class DsfSqlBulkInsertActivity : DsfActivityAbstract<string>
    {
        [NonSerialized]
        ISqlBulkInserter _sqlBulkInserter;

        public DsfSqlBulkInsertActivity()
            : base("SQL Bulk Insert")
        {
            InputMappings = new List<DataColumnMapping>();
            Timeout = "0";
            BatchSize = "0";
            IgnoreBlankRows = true;
        }

        public IList<DataColumnMapping> InputMappings { get; set; }

        [Inputs("Database")]
        public DbSource Database { get; set; }

        [Inputs("TableName")]
        public string TableName { get; set; }

        [Outputs("Result")]
        [FindMissing]
        public new string Result { get; set; }

        public bool CheckConstraints { get; set; }

        public bool FireTriggers { get; set; }

        public bool UseInternalTransaction { get; set; }

        public bool KeepIdentity { get; set; }

        public bool KeepTableLock { get; set; }

        public string Timeout { get; set; }

        public string BatchSize { get; set; }

        public override List<string> GetOutputs()
        {
            return new List<string> { Result };
        }

        internal ISqlBulkInserter SqlBulkInserter
        {
            get
            {
                return _sqlBulkInserter ?? (_sqlBulkInserter = new SqlBulkInserter());
            }
            set
            {
                _sqlBulkInserter = value;
            }
        }

        public bool IgnoreBlankRows { get; set; }

        public override enFindMissingType GetFindMissingType()
        {
            return enFindMissingType.MixedActivity;
        }

        #region Overrides of DsfNativeActivity<string>

        /// <summary>
        /// When overridden runs the activity's execution logic 
        /// </summary>
        /// <param name="context">The context to be used.</param>
        protected override void OnExecute(NativeActivityContext context)
        {
            var dataObject = context.GetExtension<IDSFDataObject>();
            ExecuteTool(dataObject, 0);
        }

        protected override void ExecuteTool(IDSFDataObject dataObject, int update)
        {


            var errorResultTo = new ErrorResultTO();
            var allErrors = new ErrorResultTO();
            bool addExceptionToErrorList = true;
            InitializeDebug(dataObject);
            try
            {
                IWarewolfIterator batchItr;
                IWarewolfIterator timeoutItr;
                var parametersIteratorCollection = BuildParametersIteratorCollection(dataObject.Environment, out batchItr, out timeoutItr, update);

                var currentOptions = BuildSqlBulkCopyOptions();
                var runtimeDatabase = ResourceCatalog.GetResource<DbSource>(dataObject.WorkspaceID, Database.ResourceID);
                if(runtimeDatabase.ServerType == enSourceType.MySqlDatabase)
                {
                    DoInsertForMySql(runtimeDatabase, currentOptions, parametersIteratorCollection, batchItr, timeoutItr, dataObject, errorResultTo, allErrors, ref addExceptionToErrorList, update);
                }
                else
                {
                    DoInsertForSqlServer(runtimeDatabase, currentOptions, dataObject, allErrors, batchItr, parametersIteratorCollection, timeoutItr, ref errorResultTo, ref addExceptionToErrorList, update);
                }

                allErrors.MergeErrors(errorResultTo);
            }
            catch(Exception e)
            {
                if(addExceptionToErrorList)
                {
                    allErrors.AddError(e.Message);
                }
                // ReSharper disable InvokeAsExtensionMethod
                Dev2Logger.Error(this, e);
                // ReSharper restore InvokeAsExtensionMethod
            }
            finally
            {
                // Handle Errors
                if(allErrors.HasErrors())
                {
                    DisplayAndWriteError("DsfSqlBulkInsertActivity", allErrors);
                    dataObject.Environment.Assign(Result, null, update);
                    var errorString = allErrors.MakeDisplayReady();
                    dataObject.Environment.AddError(errorString);
                    if(dataObject.IsDebugMode())
                    {
                        AddDebugOutputItem(new DebugItemStaticDataParams("Failure", Result, "", "="));
                    }
                }
                if(dataObject.IsDebugMode())
                {
                    DispatchDebugState(dataObject, StateType.Before, update);
                    DispatchDebugState(dataObject, StateType.After, update);
                }
            }
        }

        void DoInsertForSqlServer(DbSource runtimeDatabase, SqlBulkCopyOptions currentOptions, IDSFDataObject dataObject, ErrorResultTO allErrors, IWarewolfIterator batchItr, IWarewolfListIterator parametersIteratorCollection, IWarewolfIterator timeoutItr, ref ErrorResultTO errorResultTo, ref bool addExceptionToErrorList, int update)
        {

            SqlBulkCopy sqlBulkCopy;
            if(string.IsNullOrEmpty(BatchSize) && string.IsNullOrEmpty(Timeout))
            {
                sqlBulkCopy = new SqlBulkCopy(runtimeDatabase.ConnectionString, currentOptions) { DestinationTableName = TableName };
            }
            else
            {
                sqlBulkCopy = SetupSqlBulkCopy(batchItr, parametersIteratorCollection, timeoutItr, runtimeDatabase, currentOptions);
            }
            
            if(sqlBulkCopy != null)
            {
                var dataTableToInsert = BuildDataTableToInsert();
               
                var types = GETTypesFromMappingTypes();
                var columns = GetNamesFromMappings();
                if(InputMappings != null && InputMappings.Count > 0)
                {
                    var iteratorCollection = new WarewolfListIterator();
                    var listOfIterators = GetIteratorsFromInputMappings(dataObject, iteratorCollection, out errorResultTo, update);
                    iteratorCollection.Types = types;
                    iteratorCollection.Names = columns;
                    allErrors.MergeErrors(errorResultTo);
                    FillDataTableWithDataFromDataList(iteratorCollection, dataTableToInsert, listOfIterators);
                    // oh no, we have an issue, bubble it out ;)
                    if(allErrors.HasErrors())
                    {
                        addExceptionToErrorList = false;
                        throw new Exception(string.Format(ErrorResource.ProblemsWithIterators,"SQLBulkInsert"));
                    }

                    // emit options to debug as per acceptance test ;)
                    if(dataObject.IsDebugMode())
                    {
                        AddBatchSizeAndTimeOutToDebug(dataObject.Environment, update);
                        AddOptionsDebugItems();
                    }
                    
                  
                    if(InputMappings != null)
                    {
                        foreach(var dataColumnMapping in InputMappings)
                        {
                            if(!string.IsNullOrEmpty(dataColumnMapping.InputColumn))
                            {
                                sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping(dataColumnMapping.OutputColumn.ColumnName, dataColumnMapping.OutputColumn.ColumnName));
                            }
                        }
                    }
                    var wrapper = new SqlBulkCopyWrapper(sqlBulkCopy);
                    SqlBulkInserter.Insert(wrapper, dataTableToInsert);
                    dataObject.Environment.Assign(Result, "Success", update);
                    if (dataObject.IsDebugMode())
                    {
                        AddDebugOutputItem(new DebugItemWarewolfAtomResult("Success", Result, ""));
                    }
                }
                dataTableToInsert?.Dispose();
            }
        }

        void DoInsertForMySql(DbSource runtimeDatabase, SqlBulkCopyOptions currentOptions, IWarewolfListIterator parametersIteratorCollection, IWarewolfIterator batchItr, IWarewolfIterator timeoutItr, IDSFDataObject dataObject, ErrorResultTO errorResultTo, ErrorResultTO allErrors, ref bool addExceptionToErrorList, int update)
        {
            MySqlBulkLoader sqlBulkCopy = new MySqlBulkLoader(new MySqlConnection(runtimeDatabase.ConnectionString));
            TableName = TableName.Replace("[", "").Replace("]", "");
            if (TableName.Contains("."))
                TableName = TableName.Substring(TableName.IndexOf(".", StringComparison.Ordinal)+1);
            if (string.IsNullOrEmpty(BatchSize) && string.IsNullOrEmpty(Timeout))
            {
                sqlBulkCopy = new MySqlBulkLoader(new MySqlConnection(runtimeDatabase.ConnectionString)) { TableName = TableName, FieldTerminator = ",", LineTerminator = "\n" };
            }
            else
            {
                while (parametersIteratorCollection.HasMoreData())
                {
                    sqlBulkCopy = SetupMySqlBulkCopy(batchItr, parametersIteratorCollection, timeoutItr, runtimeDatabase, currentOptions);
                }
            }
            if (sqlBulkCopy != null)
            {
                var dataTableToInsert = BuildDataTableToInsertMySql();

                if (InputMappings != null && InputMappings.Count > 0)
                {
                    var iteratorCollection = new WarewolfListIterator();
                    var listOfIterators = GetIteratorsFromInputMappings(dataObject, iteratorCollection, out errorResultTo, update);
                    allErrors.MergeErrors(errorResultTo);

                    // oh no, we have an issue, bubble it out ;)
                    if (allErrors.HasErrors())
                    {
                        addExceptionToErrorList = false;
                        throw new Exception(string.Format(ErrorResource.ProblemsWithIterators, "SQLBulkInsert"));
                    }

                    // emit options to debug as per acceptance test ;)
                    if (dataObject.IsDebugMode())
                    {
                        AddBatchSizeAndTimeOutToDebug(dataObject.Environment, update);
                        AddOptionsDebugItems();
                    }

                    FillDataTableWithDataFromDataList(iteratorCollection, dataTableToInsert, listOfIterators);

                    foreach (var dataColumnMapping in InputMappings)
                    {
                        if (!string.IsNullOrEmpty(dataColumnMapping.InputColumn))
                        {
                            sqlBulkCopy.Columns.Add(  dataColumnMapping.OutputColumn.ColumnName);
                        }
                    }
                }

                // Pass in wrapper now ;)
                var wrapper = new MySqlBulkCopyWrapper(sqlBulkCopy);
                var inserted = SqlBulkInserter.Insert(wrapper, dataTableToInsert);
                var resultString = inserted ? "Success" : "Failure";
                
                dataObject.Environment.Assign(Result,resultString, update);
                if (dataObject.IsDebugMode())
                {
                    AddDebugOutputItem(new DebugItemWarewolfAtomResult(resultString, Result, ""));
                }
                allErrors.MergeErrors(errorResultTo);
                dataTableToInsert?.Dispose();
            }
        }

        void AddOptionsDebugItems()
        {
            var debugItem = new DebugItem();
            AddDebugItem(new DebugItemStaticDataParams(CheckConstraints ? "YES" : "NO", "Check Constraints"), debugItem);
            AddDebugItem(new DebugItemStaticDataParams(KeepTableLock ? "YES" : "NO", "Keep Table Lock"), debugItem);
            _debugInputs.Add(debugItem);

            debugItem = new DebugItem();
            AddDebugItem(new DebugItemStaticDataParams(FireTriggers ? "YES" : "NO", "Fire Triggers"), debugItem);
            AddDebugItem(new DebugItemStaticDataParams(KeepIdentity ? "YES" : "NO", "Keep Identity"), debugItem);
            _debugInputs.Add(debugItem);

            debugItem = new DebugItem();
            AddDebugItem(new DebugItemStaticDataParams(UseInternalTransaction ? "YES" : "NO", "Use Internal Transaction"), debugItem);
            AddDebugItem(new DebugItemStaticDataParams(IgnoreBlankRows ? "YES" : "NO", "Skip Blank Rows"), debugItem);
            _debugInputs.Add(debugItem);
        }

        void AddBatchSizeAndTimeOutToDebug(IExecutionEnvironment executionEnvironment, int update)
        {
            DebugItem debugItem = new DebugItem();
            if(!string.IsNullOrEmpty(BatchSize))
            {
                AddDebugInputItemFromEntry(BatchSize, "Batch Size ", executionEnvironment, debugItem, update);
            }
            if(!string.IsNullOrEmpty(Timeout))
            {
                AddDebugInputItemFromEntry(Timeout, "Timeout  ", executionEnvironment, debugItem, update);
            }
            _debugInputs.Add(debugItem);
        }

        void AddDebugInputItemFromEntry(string expression, string parameterName, IExecutionEnvironment environment, DebugItem debugItem, int update)
        {
            AddDebugItem(new DebugEvalResult(expression, parameterName, environment, update), debugItem);
        }

        public SqlBulkCopy SetupSqlBulkCopy(IWarewolfIterator batchItr, IWarewolfListIterator parametersIteratorCollection, IWarewolfIterator timeoutItr, DbSource runtimeDatabase, SqlBulkCopyOptions copyOptions)
        {
            var batchSize = -1;
            var timeout = -1;
            GetParameterValuesForBatchSizeAndTimeOut(batchItr, parametersIteratorCollection, timeoutItr, ref batchSize, ref timeout);
            var sqlBulkCopy = new SqlBulkCopy(runtimeDatabase.ConnectionString, copyOptions) { DestinationTableName = TableName };
            if(batchSize != -1)
            {
                sqlBulkCopy.BatchSize = batchSize;
            }
            if(timeout != -1)
            {
                sqlBulkCopy.BulkCopyTimeout = timeout;
            }
            return sqlBulkCopy;
        }
        public MySqlBulkLoader SetupMySqlBulkCopy(IWarewolfIterator batchItr, IWarewolfListIterator parametersIteratorCollection, IWarewolfIterator timeoutItr, DbSource runtimeDatabase, SqlBulkCopyOptions copyOptions)
        {
            var batchSize = -1;
            var timeout = -1;
            GetParameterValuesForBatchSizeAndTimeOut(batchItr, parametersIteratorCollection, timeoutItr, ref batchSize, ref timeout);
            var sqlBulkCopy = new MySqlBulkLoader(new MySqlConnection(runtimeDatabase.ConnectionString)) { TableName = TableName, FieldTerminator = ",", LineTerminator = "\n" };
            return sqlBulkCopy;
        }

        void GetParameterValuesForBatchSizeAndTimeOut(IWarewolfIterator batchItr, IWarewolfListIterator parametersIteratorCollection, IWarewolfIterator timeoutItr, ref int batchSize, ref int timeout)
        {
            GetBatchSize(batchItr, parametersIteratorCollection, ref batchSize);
            GetTimeOut(parametersIteratorCollection, timeoutItr, ref timeout);
        }

        void GetTimeOut(IWarewolfListIterator parametersIteratorCollection, IWarewolfIterator timeoutItr, ref int timeout)
        {
            if(timeoutItr != null)
            {
                var timeoutString = parametersIteratorCollection.FetchNextValue(timeoutItr);
                if(!string.IsNullOrEmpty(timeoutString))
                {
                    int parsedValue;
                    if(int.TryParse(timeoutString, out parsedValue))
                    {
                        timeout = parsedValue;
                    }
                }
            }
            else
            {
                int.TryParse(Timeout, out timeout);
            }
        }

        void GetBatchSize(IWarewolfIterator batchItr, IWarewolfListIterator parametersIteratorCollection, ref int batchSize)
        {
            if(batchItr != null)
            {
                var batchSizeString = parametersIteratorCollection.FetchNextValue(batchItr);
                if(!string.IsNullOrEmpty(batchSizeString))
                {
                    int parsedValue;
                    if(int.TryParse(batchSizeString, out parsedValue))
                    {
                        batchSize = parsedValue;
                    }
                }
            }
            else
            {
                int.TryParse(BatchSize, out batchSize);
            }
        }

        IWarewolfListIterator BuildParametersIteratorCollection(IExecutionEnvironment executionEnvironment, out IWarewolfIterator batchIterator, out IWarewolfIterator timeOutIterator, int update)
        {
            var parametersIteratorCollection = new WarewolfListIterator();
            batchIterator = null;
            timeOutIterator = null;
            if(!string.IsNullOrEmpty(BatchSize))
            {
                var batchItr = new WarewolfIterator(executionEnvironment.Eval(BatchSize, update));
                parametersIteratorCollection.AddVariableToIterateOn(batchItr);
                batchIterator = batchItr;
            }
            if(!string.IsNullOrEmpty(Timeout))
            {
                var timeoutItr = new WarewolfIterator(executionEnvironment.Eval(Timeout, update));
                parametersIteratorCollection.AddVariableToIterateOn(timeoutItr);
                timeOutIterator = timeoutItr;
            }
            return parametersIteratorCollection;
        }

        void FillDataTableWithDataFromDataList(WarewolfListIterator iteratorCollection, DataTable dataTableToInsert, List<IWarewolfIterator> listOfIterators)
        {
            while(iteratorCollection.HasMoreData())
            {

                // ReSharper disable LoopCanBeConvertedToQuery
                var values = listOfIterators.Select(iteratorCollection.FetchNextValue).Where(val => val != null).Select(val =>
                {
                    try
                    {
                        return val;
                    }
                    catch(NullValueInVariableException)
                    {
                        return "";
                    }
                });
                IEnumerable<string> enumerable = values as string[] ?? values.ToArray();
  

                if (IgnoreBlankRows && enumerable.All(string.IsNullOrEmpty))
                {
                    continue;
                }

                // ReSharper disable once CoVariantArrayConversion
                dataTableToInsert.Rows.Add(enumerable.ToArray());                
            }
        }

        List<IWarewolfIterator> GetIteratorsFromInputMappings(IDSFDataObject dataObject, IWarewolfListIterator iteratorCollection, out ErrorResultTO errorsResultTo, int update)
        {
            errorsResultTo = new ErrorResultTO();
            var listOfIterators = new List<IWarewolfIterator>();
            var indexCounter = 1;
            
            foreach(var row in InputMappings)
            {
                try
                {
                    ExecutionEnvironment.IsValidRecordSetIndex(row.InputColumn);
                }
                catch(Exception)
                {
                    errorsResultTo.AddError(ErrorResource.InvalidRecordset + row.InputColumn);
                }
                if(string.IsNullOrEmpty(row.InputColumn)) continue;
                if(dataObject.IsDebugMode())
                {
                    AddDebugInputItem(row.InputColumn, row.OutputColumn.ColumnName, dataObject.Environment, row.OutputColumn.DataTypeName, indexCounter, update);
                    indexCounter++;
                }
                
                    try
                    {
                        var itr = new WarewolfIterator(dataObject.Environment.Eval(row.InputColumn, update));
                        iteratorCollection.AddVariableToIterateOn(itr);
                        listOfIterators.Add(itr);
                    }
                    catch (Exception e)
                    {
                        errorsResultTo.AddError(e.Message);
                    }
                

            }
            
            return listOfIterators;
        }

        void AddDebugInputItem(string inputColumn, string outputColumnName, IExecutionEnvironment executionEnvironment, string outputColumnDataType, int indexCounter, int update)
        {
            var itemToAdd = new DebugItem();
            AddDebugItem(new DebugItemStaticDataParams("", indexCounter.ToString(CultureInfo.InvariantCulture)), itemToAdd);
            AddDebugItem(new DebugEvalResult(inputColumn, "", executionEnvironment, update), itemToAdd);
            AddDebugItem(new DebugItemStaticDataParams(outputColumnName, "To Field"), itemToAdd);
            AddDebugItem(new DebugItemStaticDataParams(outputColumnDataType, "Type"), itemToAdd);
            _debugInputs.Add(itemToAdd);
        }

        public SqlBulkCopyOptions BuildSqlBulkCopyOptions()
        {
            var sqlBulkOptions = SqlBulkCopyOptions.Default;
            if(CheckConstraints)
            {
                sqlBulkOptions = sqlBulkOptions | SqlBulkCopyOptions.CheckConstraints;
            }
            if(FireTriggers)
            {
                sqlBulkOptions = sqlBulkOptions | SqlBulkCopyOptions.FireTriggers;
            }
            if(KeepIdentity)
            {
                sqlBulkOptions = sqlBulkOptions | SqlBulkCopyOptions.KeepIdentity;
            }
            if(UseInternalTransaction)
            {
                sqlBulkOptions = sqlBulkOptions | SqlBulkCopyOptions.UseInternalTransaction;
            }
            if(KeepTableLock)
            {
                sqlBulkOptions = sqlBulkOptions | SqlBulkCopyOptions.TableLock;
            }
            return sqlBulkOptions;
        }

        DataTable BuildDataTableToInsert()
        {
            if(InputMappings == null) return null;
            var dataTableToInsert = new DataTable();
   
            foreach(var dataColumnMapping in InputMappings)
            {
                if(string.IsNullOrEmpty(dataColumnMapping.InputColumn))
                {
                    // Nulls are ok ;)
                    if(dataColumnMapping.OutputColumn.IsNullable)
                    {
                        continue;
                    }

                    // Check identity flag ;)
                    if(dataColumnMapping.OutputColumn.IsAutoIncrement)
                    {
                        // check keep identity value ;)
                        if(KeepIdentity)
                        {
                            // no mapping, identity and keep on, this is an issue ;)
                            throw new Exception(string.Format(ErrorResource.ColumnSetAsIdentityKeepIdentityIsTrue, dataColumnMapping.OutputColumn.ColumnName));
                        }

                        // null, identity and no keep flag active ;)
                        continue;
                    }

                    // Nulls are not ok ;)
                    throw new Exception(string.Format(ErrorResource.ColumnDoesNotAlloNull, dataColumnMapping.OutputColumn.ColumnName));
                }

                // more identity checks - this time it has data ;)
                if(dataColumnMapping.OutputColumn.IsAutoIncrement)
                {
                    if(!KeepIdentity)
                    {
                        // we have data in an identity column and the keep identity option is disabled - oh no!
                        throw new Exception(string.Format(ErrorResource.ColumnSetAsIdentityKeepIdentityIsFalse, dataColumnMapping.OutputColumn.ColumnName));
                    }
                }

                var dataColumn = new DataColumn { ColumnName = dataColumnMapping.OutputColumn.ColumnName, DataType = dataColumnMapping.OutputColumn.DataType };
                if(dataColumn.DataType == typeof(string))
                {
                    dataColumn.MaxLength = dataColumnMapping.OutputColumn.MaxLength;
                }
                dataTableToInsert.Columns.Add(dataColumn);
            }
            return dataTableToInsert;
        }

        List<Type> GETTypesFromMappingTypes()
        {
            return InputMappings?.Select(dataColumnMapping => dataColumnMapping.OutputColumn.DataType).ToList();
        }

        List<string> GetNamesFromMappings()
        {
            return InputMappings?.Select(dataColumnMapping => dataColumnMapping.OutputColumn.ColumnName).ToList();
        }
        DataTable BuildDataTableToInsertMySql()
        {
            if (InputMappings == null) return null;
            var dataTableToInsert = new DataTable();
            foreach (var dataColumnMapping in InputMappings)
            {
                if (string.IsNullOrEmpty(dataColumnMapping.InputColumn))
                {
                    // Nulls are ok ;)
                    if (dataColumnMapping.OutputColumn.IsNullable)
                    {
                        continue;
                    }

                    // Check identity flag ;)
                    if (dataColumnMapping.OutputColumn.IsAutoIncrement)
                    {
                        // check keep identity value ;)
                        if (KeepIdentity)
                        {
                            // no mapping, identity and keep on, this is an issue ;)
                            throw new Exception(string.Format(ErrorResource.ColumnSetAsIdentityKeepIdentityIsTrue, dataColumnMapping.OutputColumn.ColumnName));
                        }

                        // null, identity and no keep flag active ;)
                        continue;
                    }

                    // Nulls are not ok ;)
                    throw new Exception(string.Format(ErrorResource.ColumnDoesNotAlloNull, dataColumnMapping.OutputColumn.ColumnName));
                }

                // more identity checks - this time it has data ;)
                if (dataColumnMapping.OutputColumn.IsAutoIncrement)
                {
                    if (!KeepIdentity)
                    {
                        // we have data in an identity column and the keep identity option is disabled - oh no!
                        throw new Exception(string.Format(ErrorResource.ColumnSetAsIdentityKeepIdentityIsFalse, dataColumnMapping.OutputColumn.ColumnName));
                    }
                }

                var dataColumn = new DataColumn { ColumnName = dataColumnMapping.OutputColumn.ColumnName, DataType = typeof(string) };
                if (dataColumn.DataType == typeof(string))
                {
                    dataColumn.MaxLength = dataColumnMapping.OutputColumn.MaxLength;
                }
                dataTableToInsert.Columns.Add(dataColumn);
            }
            return dataTableToInsert;
        }

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates)
        {
            if(updates != null)
            {
                foreach(Tuple<string, string> t in updates)
                {
                    // locate all updates for this tuple
                    Tuple<string, string> t1 = t;
                    var items = InputMappings.Where(c => !string.IsNullOrEmpty(c.InputColumn) && c.InputColumn.Equals(t1.Item1));

                    // issues updates
                    foreach(var a in items)
                    {
                        a.InputColumn = t.Item2;
                    }

                    if(TableName == t.Item1)
                    {
                        TableName = t.Item2;
                    }
                    if(BatchSize == t.Item1)
                    {
                        BatchSize = t.Item2;
                    }
                    if(Timeout == t.Item1)
                    {
                        Timeout = t.Item2;
                    }
                }
            }
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates)
        {
            var itemUpdate = updates?.FirstOrDefault(tuple => tuple.Item1 == Result);
            if(itemUpdate != null)
            {
                Result = itemUpdate.Item2;
            }
        }

        public override IList<DsfForEachItem> GetForEachInputs()
        {
            var items = new[] { BatchSize, Timeout, TableName }.Union(InputMappings.Where(c => !string.IsNullOrEmpty(c.InputColumn)).Select(c => c.InputColumn)).ToArray();
            return GetForEachItems(items);
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            return GetForEachItems(Result);
        }
        #endregion

        #region GetDebugInputs

        public override List<DebugItem> GetDebugInputs(IExecutionEnvironment dataList, int update)
        {
            foreach(IDebugItem debugInput in _debugInputs)
            {
                debugInput.FlushStringBuilder();
            }
            return _debugInputs;
        }

        #endregion

        #region GetDebugOutputs

        public override List<DebugItem> GetDebugOutputs(IExecutionEnvironment dataList, int update)
        {
            foreach(IDebugItem debugOutput in _debugOutputs)
            {
                debugOutput.FlushStringBuilder();
            }
            return _debugOutputs;
        }

        #endregion
    }
}
