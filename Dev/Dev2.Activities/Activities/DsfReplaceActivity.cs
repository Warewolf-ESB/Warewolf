using Dev2;
using Dev2.Activities;
using Dev2.Common;
using Dev2.Data.Factories;
using Dev2.Data.Interfaces;
using Dev2.Data.Operations;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.Builders;
using Dev2.DataList.Contract.Value_Objects;
using Dev2.Diagnostics;
using Dev2.Enums;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Globalization;
using Dev2.Util;
using Dev2.Utilities;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;

// ReSharper disable CheckNamespace
namespace Unlimited.Applications.BusinessDesignStudio.Activities
// ReSharper restore CheckNamespace
{
    /// <New>
    /// Activity for replacing a certain character set in a number of field with a new character set 
    /// </New>
    public class DsfReplaceActivity : DsfActivityAbstract<string>
    {

        #region Properties

        /// <summary>
        /// Property for holding a string the user enters into the "In Fields" box
        /// </summary>
        [Inputs("FieldsToSearch")]
        [FindMissing]
        public string FieldsToSearch { get; set; }

        /// <summary>
        /// Property for holding a string the user selects in the "Find" box
        /// </summary>
        [Inputs("Find")]
        [FindMissing]
        public string Find { get; set; }

        /// <summary>
        /// Property for holding a string the user enters into the "Replace With" box
        /// </summary>
        [Inputs("ReplaceWith")]
        [FindMissing]
        public string ReplaceWith { get; set; }

        /// <summary>
        /// Property for holding a boolean the user selects in the wizard checkbox
        /// </summary>
        [Inputs("CaseMatch")]
        public bool CaseMatch { get; set; }

        /// <summary>
        /// Property for holding a string the user enters into the "Result" box
        /// </summary>
        [Outputs("Result")]
        [FindMissing]
        public new string Result { get; set; }

        #endregion Properties

        #region Ctor

        public DsfReplaceActivity()
            : base("Replace")
        {
            // Initialise all the properties here
            FieldsToSearch = string.Empty;
            Find = string.Empty;
            ReplaceWith = string.Empty;
            Result = string.Empty;
        }

        #endregion Ctor

        /// <summary>
        /// Executes the logic of the activity and calls the backend code to do the work
        /// Also responsible for adding the results to the data list
        /// </summary>
        /// <param name="context"></param>
        protected override void OnExecute(NativeActivityContext context)
        {
            _debugInputs = new List<DebugItem>();
            _debugOutputs = new List<DebugItem>(); 
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
            IDSFDataObject dataObject = context.GetExtension<IDSFDataObject>();
            IDev2ReplaceOperation replaceOperation = Dev2OperationsFactory.CreateReplaceOperation();
            IDev2DataListUpsertPayloadBuilder<string> toUpsert = Dev2DataListBuilderFactory.CreateStringDataListUpsertBuilder(true);
            ErrorResultTO errors;
            ErrorResultTO allErrors = new ErrorResultTO();
            Guid executionId = DataListExecutionID.Get(context);

            IDev2IteratorCollection iteratorCollection = Dev2ValueObjectFactory.CreateIteratorCollection();

            IBinaryDataListEntry expressionsEntryFind = compiler.Evaluate(executionId, enActionType.User, Find, false, out errors);
            allErrors.MergeErrors(errors);
            IDev2DataListEvaluateIterator itrFind = Dev2ValueObjectFactory.CreateEvaluateIterator(expressionsEntryFind);

            iteratorCollection.AddIterator(itrFind);

            IBinaryDataListEntry expressionsEntryReplaceWith = compiler.Evaluate(executionId, enActionType.User, ReplaceWith, false, out errors);
            allErrors.MergeErrors(errors);
            IDev2DataListEvaluateIterator itrReplace = Dev2ValueObjectFactory.CreateEvaluateIterator(expressionsEntryReplaceWith);

            iteratorCollection.AddIterator(itrReplace);
            int replacementCount = 0;
            int replacementTotal = 0;
            try
            {
                if (dataObject.IsDebug || ServerLogger.ShouldLog(dataObject.ResourceID) || dataObject.RemoteInvoke)
                {
                    var labelItem = new DebugItem();
                    labelItem.Add(new DebugItemResult{Type = DebugItemResultType.Label,Value = "Fields To Search"});
                    _debugInputs.Add(labelItem);
                }
                // Fetch all fields to search....
                IList<string> toSearch = FieldsToSearch.Split(new[] { ',' , ' '}, StringSplitOptions.RemoveEmptyEntries);
                while (iteratorCollection.HasMoreData())
                {
                    
                    
                    // now process each field for entire evaluated Where expression....                    

                    string findValue = iteratorCollection.FetchNextRow(itrFind).TheValue;
                    string replaceWithValue = iteratorCollection.FetchNextRow(itrReplace).TheValue;
                    foreach (string s in toSearch)
                    {
                        if(!DataListUtil.IsEvaluated(s))
                        {
                            allErrors.AddError("Please insert only variables into Fields To Search");
                            return;
                        }
                        IBinaryDataListEntry entryToReplaceIn;

                        toUpsert = replaceOperation.Replace(executionId, s.Trim(), findValue, replaceWithValue, CaseMatch, toUpsert,
                                                            out errors, out replacementCount, out entryToReplaceIn);
                        if (dataObject.IsDebug || ServerLogger.ShouldLog(dataObject.ResourceID) || dataObject.RemoteInvoke)
                        {
                            AddDebugInputItem(s.Trim(), string.Empty, entryToReplaceIn, executionId);
                        }

                        replacementTotal += replacementCount;

                        allErrors.MergeErrors(errors);
                    }

                }

                if (dataObject.IsDebug || ServerLogger.ShouldLog(dataObject.ResourceID) || dataObject.RemoteInvoke)
                {
                    AddDebugInputItem(Find, "Find", expressionsEntryFind,executionId);
                    AddDebugInputItem(ReplaceWith, "Replace With", expressionsEntryReplaceWith, executionId);
                }

                //2013.06.03: Ashley Lewis for bug 9498 - handle multiple regions in result
                foreach (var region in DataListCleaningUtils.SplitIntoRegions(Result))
                {
                    toUpsert.Add(region, replacementTotal.ToString(CultureInfo.InvariantCulture));

                    if (dataObject.IsDebug || ServerLogger.ShouldLog(dataObject.ResourceID) || dataObject.RemoteInvoke)
                    {
                        AddDebugOutputItem(region, replacementTotal.ToString(CultureInfo.InvariantCulture), executionId);
                    }

                }

                // now push the result to the server
                compiler.Upsert(executionId, toUpsert, out errors);
                allErrors.MergeErrors(errors);
            }
            finally
            {

                if (allErrors.HasErrors())
                {
                    DisplayAndWriteError("DsfReplaceActivity", allErrors);
                    compiler.UpsertSystemTag(dataObject.DataListID, enSystemTag.Dev2Error, allErrors.MakeDataListReady(), out errors);
                }

                if (dataObject.IsDebug || ServerLogger.ShouldLog(dataObject.ResourceID) || dataObject.RemoteInvoke)
                {
                    DispatchDebugState(context,StateType.Before);
                    DispatchDebugState(context, StateType.After);
                }
            }

        }

        #region Private Methods

        private void AddDebugInputItem(string expression, string labelText, IBinaryDataListEntry valueEntry, Guid executionId)
        {
            DebugItem itemToAdd = new DebugItem();
           
            if (!string.IsNullOrWhiteSpace(labelText))
            {
                itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = labelText });
            }

            if (valueEntry != null)
            {
                itemToAdd.AddRange(CreateDebugItemsFromEntry(expression, valueEntry, executionId, enDev2ArgumentType.Input));
            }

            _debugInputs.Add(itemToAdd);            
        }

        private void AddDebugOutputItem(string expression, string value, Guid dlId)
        {
            DebugItem itemToAdd = new DebugItem();

            itemToAdd.AddRange(CreateDebugItemsFromString(expression, value, dlId,0, enDev2ArgumentType.Output));
            _debugOutputs.Add(itemToAdd);
        }

        #endregion
 
        #region Get Debug Inputs/Outputs

        public override List<DebugItem> GetDebugInputs(IBinaryDataList dataList)
        {
            foreach (IDebugItem debugInput in _debugInputs)
            {
                debugInput.FlushStringBuilder();
            }
            return _debugInputs;
        }

        public override List<DebugItem> GetDebugOutputs(IBinaryDataList dataList)
        {
            foreach (IDebugItem debugOutput in _debugOutputs)
            {
                debugOutput.FlushStringBuilder();
            }
            return _debugOutputs;
        }

        #endregion Get Inputs/Outputs

        #region Get ForEach Inputs/Outputs Updates

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            foreach (Tuple<string, string> t in updates)
            {

                if (t.Item1 == FieldsToSearch)
                {
                    FieldsToSearch = t.Item2;
                }

                if (t.Item1 == Find)
                {
                    Find = t.Item2;
                }

                if (t.Item1 == ReplaceWith)
                {
                    ReplaceWith = t.Item2;
                }
            }
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            if (updates.Count == 1)
            {
                Result = updates[0].Item2;
            }
        }

        #endregion

        #region GetForEachInputs/Outputs

        public override IList<DsfForEachItem> GetForEachInputs()
        {
            return GetForEachItems(FieldsToSearch, Find, ReplaceWith);
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            return GetForEachItems(Result);
        }

        #endregion

    }
}
