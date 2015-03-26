
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Data;
using Dev2.Data.Factories;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Diagnostics;
using Dev2.Enums;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.TO;
using Dev2.Util;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;
using Warewolf.Storage;

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
            _debugInputs = new List<DebugItem>();
            _debugOutputs = new List<DebugItem>();
            var dataObject = context.GetExtension<IDSFDataObject>();
            var compiler = DataListFactory.CreateDataListCompiler();
            var toUpsert = Dev2DataListBuilderFactory.CreateStringDataListUpsertBuilder();
            toUpsert.IsDebug = dataObject.IsDebugMode();
            toUpsert.ResourceID = dataObject.ResourceID;
            var errorResultTo = new ErrorResultTO();
            var allErrors = new ErrorResultTO();
            var executionId = DataListExecutionID.Get(context);
            DataTable dataTableToInsert = null;
            bool addExceptionToErrorList = true;
            InitializeDebug(dataObject);
            try
            {
                IWarewolfIterator batchItr;
                IWarewolfIterator timeoutItr;
                var parametersIteratorCollection = BuildParametersIteratorCollection(dataObject.Environment, out batchItr, out timeoutItr);
                SqlBulkCopy sqlBulkCopy = null;
                var currentOptions = BuildSqlBulkCopyOptions();
                var runtimeDatabase = ResourceCatalog.Instance.GetResource<DbSource>(dataObject.WorkspaceID, Database.ResourceID);
                if(String.IsNullOrEmpty(BatchSize) && String.IsNullOrEmpty(Timeout))
                {
                    sqlBulkCopy = new SqlBulkCopy(runtimeDatabase.ConnectionString, currentOptions) { DestinationTableName = TableName };
                }
                else
                {
                    while(parametersIteratorCollection.HasMoreData())
                    {
                        sqlBulkCopy = SetupSqlBulkCopy(batchItr, parametersIteratorCollection, timeoutItr, runtimeDatabase, currentOptions);
                    }
                }
                if(sqlBulkCopy != null)
                {
                    // BuiltUsingSingleRecset was very poorly put together it assumes a 1-1 mapping between target and destination columns ?! ;(
                    // And it forced a need for duplicate logic?!
                    dataTableToInsert = BuildDataTableToInsert();

                    if(InputMappings != null && InputMappings.Count > 0)
                    {
                        var iteratorCollection = new WarewolfListIterator();
                        var listOfIterators = GetIteratorsFromInputMappings(dataObject, iteratorCollection, out errorResultTo);
                        allErrors.MergeErrors(errorResultTo);

                        // oh no, we have an issue, bubble it out ;)
                        if(allErrors.HasErrors())
                        {
                            addExceptionToErrorList = false;
                            throw new Exception("Problems with Iterators for SQLBulkInsert");
                        }

                        // emit options to debug as per acceptance test ;)
                        if(dataObject.IsDebugMode())
                        {
                            AddBatchSizeAndTimeOutToDebug(dataObject.Environment);
                            AddOptionsDebugItems();
                        }

                        FillDataTableWithDataFromDataList(iteratorCollection, dataTableToInsert, listOfIterators);

                        foreach(var dataColumnMapping in InputMappings)
                        {
                            if(!String.IsNullOrEmpty(dataColumnMapping.InputColumn))
                            {
                                sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping(dataColumnMapping.OutputColumn.ColumnName, dataColumnMapping.OutputColumn.ColumnName));
                            }
                        }
                    }

                    // Pass in wrapper now ;)
                    var wrapper = new SqlBulkCopyWrapper(sqlBulkCopy);
                    SqlBulkInserter.Insert(wrapper, dataTableToInsert);

                    dataObject.Environment.Assign(Result, "Success");
                    allErrors.MergeErrors(errorResultTo);
                    if(toUpsert.IsDebug)
                    {
                       AddDebugOutputItem(new DebugItemWarewolfAtomResult("Success",Result,""));
                    }
                }
            }
            catch(Exception e)
            {
                if(addExceptionToErrorList)
                {
                    allErrors.AddError(e.Message);
                }
                // ReSharper disable InvokeAsExtensionMethod
                Dev2Logger.Log.Error(this, e);
                // ReSharper restore InvokeAsExtensionMethod
            }
            finally
            {
                // Handle Errors
                if(allErrors.HasErrors())
                {
                    if(toUpsert.IsDebug)
                    {
//                        foreach(var debugOutputTo in toUpsert.DebugOutputs)
//                        {
//                            AddDebugOutputItem(new DebugItemVariableParams(debugOutputTo));
//                        }
                    }

                    DisplayAndWriteError("DsfSqlBulkInsertActivity", allErrors);
                    compiler.UpsertSystemTag(dataObject.DataListID, enSystemTag.Dev2Error, allErrors.MakeDataListReady(), out errorsTo);
                    compiler.Upsert(executionId, Result, (string)null, out errorResultTo);
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

        void AddBatchSizeAndTimeOutToDebug(IExecutionEnvironment executionEnvironment)
        {
            DebugItem debugItem = new DebugItem();
            if(!string.IsNullOrEmpty(BatchSize))
            {
                AddDebugInputItemFromEntry(BatchSize, "Batch Size ", executionEnvironment, debugItem);
            }
            if(!String.IsNullOrEmpty(Timeout))
            {
                AddDebugInputItemFromEntry(Timeout, "Timeout  ", executionEnvironment, debugItem);
            }
            _debugInputs.Add(debugItem);
        }

        void AddDebugInputItemFromEntry(string expression, string parameterName, IExecutionEnvironment environment, DebugItem debugItem)
        {
            AddDebugItem(new DebugEvalResult(expression, parameterName, environment), debugItem);
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
                if(!String.IsNullOrEmpty(timeoutString))
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
                Int32.TryParse(Timeout, out timeout);
            }
        }

        void GetBatchSize(IWarewolfIterator batchItr, IWarewolfListIterator parametersIteratorCollection, ref int batchSize)
        {
            if(batchItr != null)
            {
                var batchSizeString = parametersIteratorCollection.FetchNextValue(batchItr);
                if(!String.IsNullOrEmpty(batchSizeString))
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
                Int32.TryParse(BatchSize, out batchSize);
            }
        }

        IWarewolfListIterator BuildParametersIteratorCollection(IExecutionEnvironment executionEnvironment, out IWarewolfIterator batchIterator, out IWarewolfIterator timeOutIterator)
        {
            var parametersIteratorCollection = new WarewolfListIterator();
            batchIterator = null;
            timeOutIterator = null;
            if(!String.IsNullOrEmpty(BatchSize))
            {
                var batchItr = new WarewolfIterator(executionEnvironment.Eval(BatchSize));
                parametersIteratorCollection.AddVariableToIterateOn(batchItr);
                batchIterator = batchItr;
            }
            if(!String.IsNullOrEmpty(Timeout))
            {
                var timeoutItr = new WarewolfIterator(executionEnvironment.Eval(Timeout));
                parametersIteratorCollection.AddVariableToIterateOn(timeoutItr);
                timeOutIterator = timeoutItr;
            }
            return parametersIteratorCollection;
        }

        void FillDataTableWithDataFromDataList(WarewolfListIterator iteratorCollection, DataTable dataTableToInsert, List<IWarewolfIterator> listOfIterators)
        {
            while(iteratorCollection.HasMoreData())
            {
                // If the type is numeric blank cannot be added here - causes exception to be thrown even before its added to table ;)
                // Instead build a list, then check it, then if all good add it ;)

                var tmpData = new List<string>();
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
                foreach(var value in values)
                // ReSharper restore LoopCanBeConvertedToQuery
                {
                    tmpData.Add(value);
                }

                if(IgnoreBlankRows && tmpData.All(o => o == null || (String.IsNullOrEmpty(o))))
                {
                    continue;
                }

                // now we can create the row and add data ;)
                var dataRow = dataTableToInsert.NewRow();
                for(int pos = 0; pos < tmpData.Count; pos++)
                {
                    dataRow[pos] = tmpData[pos];
                }
                dataTableToInsert.Rows.Add(dataRow);
            }
        }

        List<IWarewolfIterator> GetIteratorsFromInputMappings(IDSFDataObject dataObject, WarewolfListIterator iteratorCollection, out ErrorResultTO errorsResultTo)
        {
            errorsResultTo = new ErrorResultTO();
            var listOfIterators = new List<IWarewolfIterator>();
            var indexCounter = 1;
            foreach(var row in InputMappings)
            {
                if(String.IsNullOrEmpty(row.InputColumn)) continue;
                if(dataObject.IsDebugMode())
                {
                    AddDebugInputItem(row.InputColumn, row.OutputColumn.ColumnName, dataObject.Environment, row.OutputColumn.DataTypeName, indexCounter);
                    indexCounter++;
                }
                var itr =new WarewolfIterator(dataObject.Environment.Eval(row.InputColumn));
                iteratorCollection.AddVariableToIterateOn(itr);
                listOfIterators.Add(itr);
            }
            return listOfIterators;
        }

        void AddDebugInputItem(string inputColumn, string outputColumnName, IExecutionEnvironment executionEnvironment, string outputColumnDataType, int indexCounter)
        {
            var itemToAdd = new DebugItem();
            AddDebugItem(new DebugItemStaticDataParams("", indexCounter.ToString(CultureInfo.InvariantCulture)), itemToAdd);
            AddDebugItem(new DebugEvalResult(inputColumn, "", executionEnvironment), itemToAdd);
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
                if(String.IsNullOrEmpty(dataColumnMapping.InputColumn))
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
                            throw new Exception("The column " + dataColumnMapping.OutputColumn.ColumnName + " is an IDENTITY and you have the Keep Identity option enabled. Either disable this option or map data.");
                        }

                        // null, identity and no keep flag active ;)
                        continue;
                    }

                    // Nulls are not ok ;)
                    throw new Exception("The column " + dataColumnMapping.OutputColumn.ColumnName + " does not allow NULL. Please check your mappings to ensure you have mapped data into it.");
                }

                // more identity checks - this time it has data ;)
                if(dataColumnMapping.OutputColumn.IsAutoIncrement)
                {
                    if(!KeepIdentity)
                    {
                        // we have data in an identity column and the keep identity option is disabled - oh no!
                        throw new Exception("The column " + dataColumnMapping.OutputColumn.ColumnName + " is an IDENTITY and you have the Keep Identity option disabled. Either enable it or remove the mapping.");
                    }
                }

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
