using System;
using System.Activities;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using Dev2.Common;
using Dev2.Data.Factories;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.Value_Objects;
using Dev2.Diagnostics;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.TO;
using Dev2.Util;
using Microsoft.JScript;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;

namespace Dev2.Activities
{
    public class DsfSqlBulkInsertActivity : DsfActivityAbstract<string>
    {
        [NonSerialized]
        ISqlBulkInserter _sqlBulkInserter;

        int _timeout;

        public DsfSqlBulkInsertActivity()
            : base("SQL Bulk Insert")
        {
            InputMappings = new List<DataColumnMapping>();
            _timeout = 30;
        }

        public IList<DataColumnMapping> InputMappings{ get; set; }

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

        public int Timeout
        {
            get
            {
                return _timeout;
            }
            set
            {
                _timeout = value;
            }
        }

        public int BatchSize { get; set; }

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
            toUpsert.IsDebug = (dataObject.IsDebug || ServerLogger.ShouldLog(dataObject.ResourceID) || dataObject.RemoteInvoke);
            toUpsert.ResourceID = dataObject.ResourceID;
            var errorResultTO = new ErrorResultTO();
            var allErrors = new ErrorResultTO();
            var executionID = DataListExecutionID.Get(context);

            try
            {
                if(toUpsert.IsDebug)
                {

                }
                var dataTableToInsert = BuildDataTableToInsert();
                SqlBulkInserter.CurrentOptions = BuildSqlBulkCopyOptions();

                if(InputMappings != null && InputMappings.Count > 0)
                {
                    var iteratorCollection = Dev2ValueObjectFactory.CreateIteratorCollection();
                    allErrors.MergeErrors(errorResultTO);
                    var listOfIterators = GetIteratorsFromInputMappings(compiler, executionID, allErrors, dataObject, iteratorCollection, out errorResultTO);
                    FillDataTableWithDataFromDataList(iteratorCollection, dataTableToInsert, listOfIterators);
                }
                var sqlBulkCopy = new SqlBulkCopy(Database.ConnectionString, SqlBulkInserter.CurrentOptions) { BulkCopyTimeout = Timeout, BatchSize = BatchSize,DestinationTableName = TableName};
                SqlBulkInserter.Insert(sqlBulkCopy, dataTableToInsert);
                toUpsert.Add(Result, "Success");
                compiler.Upsert(executionID, toUpsert, out errors);
                AddDebugOutputItemFromEntry(Result, compiler,executionID);
            }
            catch(Exception e)
            {
                toUpsert.Add(Result, "Failure");
                compiler.Upsert(executionID, toUpsert, out errors);
                AddDebugOutputItemFromEntry(Result, compiler, executionID);
                allErrors.AddError(e.Message);
            }
            finally
            {
                // Handle Errors
                if(allErrors.HasErrors())
                {
                    DisplayAndWriteError("DsfSqlBulkInsertActivity", allErrors);
                    compiler.UpsertSystemTag(dataObject.DataListID, enSystemTag.Dev2Error, allErrors.MakeDataListReady(), out errors);
                }
                if(toUpsert.IsDebug)
                {
                    DispatchDebugState(context, StateType.Before);
                    DispatchDebugState(context, StateType.After);
                }
            }
        }

        

        static void FillDataTableWithDataFromDataList(IDev2IteratorCollection iteratorCollection, DataTable dataTableToInsert, List<IDev2DataListEvaluateIterator> listOfIterators)
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
                var expressionsEntry = compiler.Evaluate(executionID, enActionType.User, row.InputColumn, false, out errorsResultTO);
                allErrors.MergeErrors(errorsResultTO);
                if(dataObject.IsDebugMode())
                {
                    AddDebugInputItem(row.InputColumn, row.OutputColumn.ColumnName, expressionsEntry, row.OutputColumn.DataType,row.OutputColumn.MaxLength, executionID, indexCounter);
                    indexCounter++;
                }
                var itr = Dev2ValueObjectFactory.CreateEvaluateIterator(expressionsEntry);

                iteratorCollection.AddIterator(itr);
                listOfIterators.Add(itr);
            }
            return listOfIterators;
        }

        void AddDebugInputItem(string inputColumn, string outputColumnName, IBinaryDataListEntry expressionsEntry, Type outputColumnDataType,int maxLength, Guid executionID,int indexCounter)
        {
            DebugItem itemToAdd = new DebugItem();

            itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = indexCounter.ToString(CultureInfo.InvariantCulture) });
            itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = "Insert Into" });
            itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Variable, Value = outputColumnName+" "+outputColumnDataType+"("+maxLength+")" });
            
            itemToAdd.AddRange(CreateDebugItemsFromEntry(inputColumn, expressionsEntry, executionID, enDev2ArgumentType.Input));

            _debugInputs.Add(itemToAdd);
        }

        void AddDebugOutputItemFromEntry(string expression, IDataListCompiler compiler,Guid executionID)
        {
            ErrorResultTO errorsResultTO;
            var expressionsEntry = compiler.Evaluate(executionID, enActionType.User, expression, false, out errorsResultTO);
            var itemToAdd = new DebugItem();

            itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = "1"});

            itemToAdd.AddRange(CreateDebugItemsFromEntry(expression, expressionsEntry, executionID, enDev2ArgumentType.Output));
            _debugOutputs.Add(itemToAdd);
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
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
        }

        public override IList<DsfForEachItem> GetForEachInputs()
        {
            return null;
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            return null;
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
