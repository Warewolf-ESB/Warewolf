using Dev2.Activities.Debug;
using Dev2.Common;
using Dev2.Common.Enums;
using Dev2.Data.Factories;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.Value_Objects;
using Dev2.Diagnostics;
using Dev2.Enums;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.TO;
using Dev2.Util;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;

namespace Dev2.Activities
{
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
            var compiler = DataListFactory.CreateDataListCompiler();
            var toUpsert = Dev2DataListBuilderFactory.CreateStringDataListUpsertBuilder();
            toUpsert.IsDebug = dataObject.IsDebugMode();
            toUpsert.ResourceID = dataObject.ResourceID;
            var errorResultTO = new ErrorResultTO();
            var allErrors = new ErrorResultTO();
            var executionID = DataListExecutionID.Get(context);
            DataTable dataTableToInsert = null;

            InitializeDebug(dataObject);
            try
            {
                IDev2DataListEvaluateIterator batchItr;
                IDev2DataListEvaluateIterator timeoutItr;
                var parametersIteratorCollection = BuildParametersIteratorCollection(compiler, executionID, out batchItr, out timeoutItr);
                SqlBulkCopy sqlBulkCopy = null;
                SqlBulkInserter.CurrentOptions = BuildSqlBulkCopyOptions();
                var runtimeDatabase = ResourceCatalog.Instance.GetResource<DbSource>(dataObject.WorkspaceID, Database.ResourceID);
                if(String.IsNullOrEmpty(BatchSize) && String.IsNullOrEmpty(Timeout))
                {
                    sqlBulkCopy = new SqlBulkCopy(runtimeDatabase.ConnectionString, SqlBulkInserter.CurrentOptions) { DestinationTableName = TableName };
                }
                else
                {
                    while(parametersIteratorCollection.HasMoreData())
                    {
                        sqlBulkCopy = SetupSqlBulkCopy(batchItr, parametersIteratorCollection, timeoutItr, runtimeDatabase);
                    }
                }
                if(sqlBulkCopy != null)
                {

                    if(!BuiltUsingSingleRecset(sqlBulkCopy, compiler, executionID, dataObject, out dataTableToInsert))
                    {
                        dataTableToInsert = BuildDataTableToInsert();

                        if(InputMappings != null && InputMappings.Count > 0)
                        {
                            var iteratorCollection = Dev2ValueObjectFactory.CreateIteratorCollection();
                            allErrors.MergeErrors(errorResultTO);
                            var listOfIterators = GetIteratorsFromInputMappings(compiler, executionID, allErrors, dataObject, iteratorCollection, out errorResultTO);

                            FillDataTableWithDataFromDataList(iteratorCollection, dataTableToInsert, listOfIterators);
                            foreach(var dataColumnMapping in InputMappings)
                            {
                                if(!String.IsNullOrEmpty(dataColumnMapping.InputColumn))
                                {
                                    sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping(dataColumnMapping.OutputColumn.ColumnName, dataColumnMapping.OutputColumn.ColumnName));
                                }
                            }
                        }
                    }

                    SqlBulkInserter.Insert(sqlBulkCopy, dataTableToInsert);

                    toUpsert.Add(Result, "Success");
                    compiler.Upsert(executionID, toUpsert, out errorsTo);
                    allErrors.MergeErrors(errorResultTO);
                    if(toUpsert.IsDebug)
                    {
                        foreach(var debugOutputTO in toUpsert.DebugOutputs)
                        {
                            AddDebugOutputItem(new DebugItemVariableParams(debugOutputTO));
                        }
                    }
                }
            }
            catch(Exception e)
            {
                toUpsert.Add(Result, string.Format("Failure"));
                compiler.Upsert(executionID, toUpsert, out errorsTo);
                allErrors.AddError(e.Message);
                this.LogError(e);
            }
            finally
            {
                // Handle Errors
                if(allErrors.HasErrors())
                {
                    if(toUpsert.IsDebug)
                    {
                        foreach(var debugOutputTO in toUpsert.DebugOutputs)
                        {
                            AddDebugOutputItem(new DebugItemVariableParams(debugOutputTO));
                        }
                    }

                    DisplayAndWriteError("DsfSqlBulkInsertActivity", allErrors);
                    compiler.UpsertSystemTag(dataObject.DataListID, enSystemTag.Dev2Error, allErrors.MakeDataListReady(), out errorsTo);
                }
                if(toUpsert.IsDebug)
                {
                    DispatchDebugState(context, StateType.Before);
                    DispatchDebugState(context, StateType.After);
                }
                if(dataTableToInsert != null)
                {
                    dataTableToInsert.Dispose();
                }
            }
        }

        bool BuiltUsingSingleRecset(SqlBulkCopy sqlBulkCopy, IDataListCompiler compiler, Guid executionID, IDSFDataObject dataObject, out DataTable dataTableToInsert)
        {
            var actualDataTable = new DataTable();
            if(InputMappings != null && InputMappings.All(mapping => DataListUtil.IsValueRecordset(mapping.InputColumn) || String.IsNullOrEmpty(mapping.InputColumn)))
            {
                var currentRecSetName = "";
                var hasMultiple = false;
                foreach(var dataColumnMapping in InputMappings)
                {
                    var inputColumn = dataColumnMapping.InputColumn;
                    if(!String.IsNullOrEmpty(inputColumn))
                    {
                        if(string.IsNullOrEmpty(currentRecSetName))
                        {
                            currentRecSetName = DataListUtil.ExtractRecordsetNameFromValue(inputColumn);
                        }
                        var currentRecSetType = DataListUtil.GetRecordsetIndexType(inputColumn);
                        if(DataListUtil.ExtractRecordsetNameFromValue(inputColumn) != currentRecSetName || currentRecSetType == enRecordsetIndexType.Blank || currentRecSetType == enRecordsetIndexType.Numeric || currentRecSetType == enRecordsetIndexType.Error)
                        {
                            hasMultiple = true;
                            break;
                        }
                        var inputFieldName = DataListUtil.ExtractFieldNameFromValue(dataColumnMapping.InputColumn);
                        if(!actualDataTable.Columns.Contains(inputFieldName))
                        {
                            actualDataTable.Columns.Add(inputFieldName, dataColumnMapping.OutputColumn.DataType);
                        }
                    }
                }
                if(!hasMultiple)
                {
                    var binaryDataList = compiler.FetchBinaryDataList(executionID, out errorsTo);
                    var populateOptions = PopulateOptions.IgnoreBlankRows;
                    if(!IgnoreBlankRows)
                    {
                        populateOptions = PopulateOptions.PopulateBlankRows;
                    }

                    var indexCounter = 1;
                    foreach(var dataColumnMapping in InputMappings)
                    {
                        var inputFieldName = DataListUtil.ExtractFieldNameFromValue(dataColumnMapping.InputColumn);
                        if(!String.IsNullOrEmpty(dataColumnMapping.InputColumn))
                        {
                            if(dataObject.IsDebugMode())
                            {
                                var expressionsEntry = compiler.Evaluate(executionID, enActionType.User, dataColumnMapping.InputColumn, false, out errorsTo);
                                AddDebugInputItem(dataColumnMapping.InputColumn, dataColumnMapping.OutputColumn.ColumnName, expressionsEntry, dataColumnMapping.OutputColumn.DataTypeName, executionID, indexCounter);
                                indexCounter++;
                            }
                            sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping(inputFieldName, dataColumnMapping.OutputColumn.ColumnName));
                        }
                    }


                    if(dataObject.IsDebugMode())
                    {
                        AddBatchSizeAndTimeOutToDebug(compiler, executionID);
                        AddOptionsDebugItems();
                    }

                    dataTableToInsert = compiler.ConvertToDataTable(binaryDataList, currentRecSetName, out errorsTo, populateOptions);
                    foreach(DataRow dataRow in dataTableToInsert.Rows)
                    {
                        actualDataTable.ImportRow(dataRow);
                    }
                    dataTableToInsert = actualDataTable;

                    return true;
                }
            }
            dataTableToInsert = null;
            return false;
        }

        SqlBulkCopy SetupSqlBulkCopy(IDev2DataListEvaluateIterator batchItr, IDev2IteratorCollection parametersIteratorCollection, IDev2DataListEvaluateIterator timeoutItr, DbSource runtimeDatabase)
        {
            var batchSize = -1;
            var timeout = -1;
            GetParameterValuesForBatchSizeAndTimeOut(batchItr, parametersIteratorCollection, timeoutItr, ref batchSize, ref timeout);
            var sqlBulkCopy = new SqlBulkCopy(runtimeDatabase.ConnectionString, SqlBulkInserter.CurrentOptions) { DestinationTableName = TableName };
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

        void AddBatchSizeAndTimeOutToDebug(IDataListCompiler compiler, Guid executionID)
        {
            DebugItem debugItem = new DebugItem();
            if(!string.IsNullOrEmpty(BatchSize))
            {
                AddDebugInputItemFromEntry(BatchSize, "Batch Size ", compiler, executionID, debugItem);
            }
            if(!String.IsNullOrEmpty(Timeout))
            {
                AddDebugInputItemFromEntry(Timeout, "Timeout  ", compiler, executionID, debugItem);
            }
            _debugInputs.Add(debugItem);
        }

        static void GetParameterValuesForBatchSizeAndTimeOut(IDev2DataListEvaluateIterator batchItr, IDev2IteratorCollection parametersIteratorCollection, IDev2DataListEvaluateIterator timeoutItr, ref int batchSize, ref int timeout)
        {
            GetBatchSize(batchItr, parametersIteratorCollection, ref batchSize);
            GetTimeOut(parametersIteratorCollection, timeoutItr, ref timeout);
        }

        static void GetTimeOut(IDev2IteratorCollection parametersIteratorCollection, IDev2DataListEvaluateIterator timeoutItr, ref int timeout)
        {
            if(timeoutItr != null)
            {
                var timeoutString = parametersIteratorCollection.FetchNextRow(timeoutItr).TheValue;
                if(!String.IsNullOrEmpty(timeoutString))
                {
                    int parsedValue;
                    if(int.TryParse(timeoutString, out parsedValue))
                    {
                        timeout = parsedValue;
                    }
                }
            }
        }

        static void GetBatchSize(IDev2DataListEvaluateIterator batchItr, IDev2IteratorCollection parametersIteratorCollection, ref int batchSize)
        {
            if(batchItr != null)
            {
                var batchSizeString = parametersIteratorCollection.FetchNextRow(batchItr).TheValue;
                if(!String.IsNullOrEmpty(batchSizeString))
                {
                    int parsedValue;
                    if(int.TryParse(batchSizeString, out parsedValue))
                    {
                        batchSize = parsedValue;
                    }
                }
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

        IDev2IteratorCollection BuildParametersIteratorCollection(IDataListCompiler compiler, Guid executionID, out IDev2DataListEvaluateIterator batchIterator, out IDev2DataListEvaluateIterator timeOutIterator)
        {
            ErrorResultTO errorResultTO;
            var parametersIteratorCollection = Dev2ValueObjectFactory.CreateIteratorCollection();
            batchIterator = null;
            timeOutIterator = null;
            if(!String.IsNullOrEmpty(BatchSize))
            {
                var batchSizeEntry = compiler.Evaluate(executionID, enActionType.User, BatchSize, false, out errorResultTO);
                var batchItr = Dev2ValueObjectFactory.CreateEvaluateIterator(batchSizeEntry);
                parametersIteratorCollection.AddIterator(batchItr);
                batchIterator = batchItr;
            }
            if(!String.IsNullOrEmpty(Timeout))
            {
                var timeoutEntry = compiler.Evaluate(executionID, enActionType.User, Timeout, false, out errorResultTO);
                var timeoutItr = Dev2ValueObjectFactory.CreateEvaluateIterator(timeoutEntry);
                parametersIteratorCollection.AddIterator(timeoutItr);
                timeOutIterator = timeoutItr;
            }
            return parametersIteratorCollection;
        }

        void FillDataTableWithDataFromDataList(IDev2IteratorCollection iteratorCollection, DataTable dataTableToInsert, List<IDev2DataListEvaluateIterator> listOfIterators)
        {
            while(iteratorCollection.HasMoreData())
            {
                var dataRow = dataTableToInsert.NewRow();
                var pos = 0;
                foreach(var value in from iterator in listOfIterators select iteratorCollection.FetchNextRow(iterator) into val where val != null select val.TheValue)
                {
                    dataRow[pos] = value;
                    pos++;
                }
                if(IgnoreBlankRows && dataRow.ItemArray.All(o => o == null || (String.IsNullOrEmpty(o as string))))
                {
                    continue;
                }
                dataTableToInsert.Rows.Add(dataRow);
            }
        }

        List<IDev2DataListEvaluateIterator> GetIteratorsFromInputMappings(IDataListCompiler compiler, Guid executionID, ErrorResultTO allErrors, IDSFDataObject dataObject, IDev2IteratorCollection iteratorCollection, out ErrorResultTO errorsResultTO)
        {
            errorsResultTO = new ErrorResultTO();
            var listOfIterators = new List<IDev2DataListEvaluateIterator>();
            var indexCounter = 1;
            foreach(var row in InputMappings)
            {
                if(String.IsNullOrEmpty(row.InputColumn)) continue;
                var expressionsEntry = compiler.Evaluate(executionID, enActionType.User, row.InputColumn, false, out errorsResultTO);
                allErrors.MergeErrors(errorsResultTO);
                if(dataObject.IsDebugMode())
                {
                    AddDebugInputItem(row.InputColumn, row.OutputColumn.ColumnName, expressionsEntry, row.OutputColumn.DataTypeName, executionID, indexCounter);
                    indexCounter++;
                }
                var itr = Dev2ValueObjectFactory.CreateEvaluateIterator(expressionsEntry);
                iteratorCollection.AddIterator(itr);
                listOfIterators.Add(itr);
            }
            return listOfIterators;
        }

        void AddDebugInputItem(string inputColumn, string outputColumnName, IBinaryDataListEntry expressionsEntry, string outputColumnDataType, Guid executionID, int indexCounter)
        {
            var itemToAdd = new DebugItem();
            AddDebugItem(new DebugItemStaticDataParams("", indexCounter.ToString(CultureInfo.InvariantCulture)), itemToAdd);
            AddDebugItem(new DebugItemVariableParams(inputColumn, "", expressionsEntry, executionID), itemToAdd);
            AddDebugItem(new DebugItemStaticDataParams(outputColumnName, "To Field"), itemToAdd);
            AddDebugItem(new DebugItemStaticDataParams(outputColumnDataType, "Type"), itemToAdd);
            _debugInputs.Add(itemToAdd);
        }

        void AddDebugInputItemFromEntry(string expression, string parameterName, IDataListCompiler compiler, Guid executionID, DebugItem debugItem)
        {
            ErrorResultTO errorsResultTO;
            var expressionsEntry = compiler.Evaluate(executionID, enActionType.User, expression, false, out errorsResultTO);
            AddDebugItem(new DebugItemVariableParams(expression, parameterName, expressionsEntry, executionID), debugItem);
        }

        SqlBulkCopyOptions BuildSqlBulkCopyOptions()
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
                if(String.IsNullOrEmpty(dataColumnMapping.InputColumn)) continue;
                var dataColumn = new DataColumn { ColumnName = dataColumnMapping.OutputColumn.ColumnName, DataType = dataColumnMapping.OutputColumn.DataType };
                if(dataColumn.DataType == typeof(String))
                {
                    dataColumn.MaxLength = dataColumnMapping.OutputColumn.MaxLength;
                }
                dataTableToInsert.Columns.Add(dataColumn);
            }
            return dataTableToInsert;
        }

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
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

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            if(updates != null)
            {
                var itemUpdate = updates.FirstOrDefault(tuple => tuple.Item1 == Result);
                if(itemUpdate != null)
                {
                    Result = itemUpdate.Item2;
                }
            }
        }

        public override IList<DsfForEachItem> GetForEachInputs()
        {
            var items = (new[] { BatchSize, Timeout, TableName }).Union(InputMappings.Where(c => !string.IsNullOrEmpty(c.InputColumn)).Select(c => c.InputColumn)).ToArray();
            return GetForEachItems(items);
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            return GetForEachItems(Result);
        }
        #endregion

        #region GetDebugInputs

        public override List<DebugItem> GetDebugInputs(IBinaryDataList dataList)
        {
            foreach(IDebugItem debugInput in _debugInputs)
            {
                debugInput.FlushStringBuilder();
            }
            return _debugInputs;
        }

        #endregion

        #region GetDebugOutputs

        public override List<DebugItem> GetDebugOutputs(IBinaryDataList dataList)
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
