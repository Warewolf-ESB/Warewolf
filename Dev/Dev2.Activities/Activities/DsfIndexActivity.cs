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
using Dev2.Util;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    public class DsfIndexActivity : DsfActivityAbstract<string>
    {

        #region Properties

        /// <summary>
        /// The property that holds the date time string the user enters into the "InField" box
        /// </summary>
        [Inputs("InField")]
        [FindMissing]
        public string InField { get; set; }

        /// <summary>
        /// The property that holds the input format string the user enters into the "Index" dropdownbox
        /// </summary>
        [Inputs("Index")]
        public string Index { get; set; }

        /// <summary>
        /// The property that holds the output format string the user enters into the "Characters" box
        /// </summary>
        [Inputs("Characters")]
        [FindMissing]
        public string Characters { get; set; }

        /// <summary>
        /// The property that holds the time modifier string the user selects in the "Direction" combobox
        /// </summary>
        [Inputs("Direction")]
        public string Direction { get; set; }

        /// <summary>
        /// The property that holds the result string the user enters into the "Result" box
        /// </summary>
        [Outputs("Result")]
        [FindMissing]
        public new string Result { get; set; }

        /// <summary>
        /// The property that holds the boolean for the match case checkbox 
        /// </summary>
        [Inputs("MatchCase")]
        public bool MatchCase { get; set; }

        /// <summary>
        /// The property that holds the start index that the user enters into the "StartIndex" textbox
        /// </summary>
        [Inputs("StartIndex")]
        [FindMissing]
        public string StartIndex { get; set; }


        #endregion Properties

        #region Ctor

        /// <summary>
        /// The consructor for the activity 
        /// </summary>
        public DsfIndexActivity()
            : base("Find Index")
        {
            InField = string.Empty;
            Index = "First Occurrence";
            Characters = string.Empty;
            Direction = "Left to Right";
            MatchCase = false;
            Result = string.Empty;
            StartIndex = "0";
        }

        #endregion Ctor

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);
        }

        /// <summary>
        /// The execute method that is called when the activity is executed at run time and will hold all the logic of the activity
        /// </summary>       
        protected override void OnExecute(NativeActivityContext context)
        {
            _debugInputs = new List<DebugItem>();
            _debugOutputs = new List<DebugItem>();
            IDSFDataObject dataObject = context.GetExtension<IDSFDataObject>();

            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
            IDev2IndexFinder indexFinder = new Dev2IndexFinder();
            ErrorResultTO allErrors = new ErrorResultTO();
            ErrorResultTO errors = new ErrorResultTO();
            Guid executionId = DataListExecutionID.Get(context);

            try
            {


                IDev2IteratorCollection outerIteratorCollection = Dev2ValueObjectFactory.CreateIteratorCollection();
                IDev2IteratorCollection innerIteratorCollection = Dev2ValueObjectFactory.CreateIteratorCollection();
                IDev2DataListUpsertPayloadBuilder<List<string>> toUpsert = Dev2DataListBuilderFactory.CreateStringListDataListUpsertBuilder();
                allErrors.MergeErrors(errors);


                IBinaryDataListEntry expressionsEntry = compiler.Evaluate(executionId, enActionType.User, Characters, false, out errors);
                allErrors.MergeErrors(errors);
                IDev2DataListEvaluateIterator itrChar = Dev2ValueObjectFactory.CreateEvaluateIterator(expressionsEntry);

                outerIteratorCollection.AddIterator(itrChar);

                #region Iterate and Find Index

                string result = string.Empty;
                expressionsEntry = compiler.Evaluate(executionId, enActionType.User, InField, false, out errors);
                if (dataObject.IsDebug || ServerLogger.ShouldLog(dataObject.ResourceID) || dataObject.RemoteInvoke)
                {
                    AddDebugInputItem(InField, "Look In Field", expressionsEntry, executionId);
                    AddDebugInputItem(Characters, string.Empty, itrChar.FetchEntry(), executionId);
                }
                int iterationCount = 0;
                var completeResultList = new List<string>();

                while (outerIteratorCollection.HasMoreData())
                {
                    allErrors.MergeErrors(errors);
                    errors.ClearErrors();
                    IDev2DataListEvaluateIterator itrInField = Dev2ValueObjectFactory.CreateEvaluateIterator(expressionsEntry);
                    innerIteratorCollection.AddIterator(itrInField);

                    string chars = outerIteratorCollection.FetchNextRow(itrChar).TheValue;
                    while (innerIteratorCollection.HasMoreData())
                    {
                        if (!string.IsNullOrEmpty(InField) && !string.IsNullOrEmpty(Characters))
                        {
                            var val = innerIteratorCollection.FetchNextRow(itrInField);
                            if (val != null)
                            {
                                IEnumerable<int> returedData = indexFinder.FindIndex(val.TheValue, Index, chars, Direction, MatchCase, StartIndex);
                                completeResultList.AddRange(returedData.Select(value => value.ToString()).ToList());
                                //2013.06.03: Ashley Lewis for bug 9498 - handle multiple regions in result

                            }
                        }
                    }
                   
                }
                foreach (var region in DataListCleaningUtils.SplitIntoRegions(Result))
                {
                    toUpsert.Add(region, completeResultList);
                    compiler.Upsert(executionId, toUpsert, out errors);
                    if (dataObject.IsDebug || ServerLogger.ShouldLog(dataObject.ResourceID) || dataObject.RemoteInvoke)
                    {
                        AddDebugOutputItem(region, result, executionId, iterationCount);
                    }
                    iterationCount++;
                }
                #endregion

                #region Add Result to DataList

                allErrors.MergeErrors(errors);

                #endregion Add Result to DataList

            }
            catch (Exception e)
            {
                allErrors.AddError(e.Message);
            }
            finally
            {
                #region Handle Errors

                if (allErrors.HasErrors())
                {
                    DisplayAndWriteError("DsfIndexActivity", allErrors);
                    compiler.UpsertSystemTag(dataObject.DataListID, enSystemTag.Dev2Error, allErrors.MakeDataListReady(), out errors);
                }

                #endregion

                if (dataObject.IsDebug || ServerLogger.ShouldLog(dataObject.ResourceID) || dataObject.RemoteInvoke)
                {
                    DispatchDebugState(context, StateType.Before);
                    DispatchDebugState(context, StateType.After);
                }
            }
        }

        #region Private Methods

        private void AddDebugInputItem(string expression, string labelText, IBinaryDataListEntry valueEntry, Guid executionId)
        {
            DebugItem itemToAdd = new DebugItem();

            if (string.IsNullOrWhiteSpace(labelText))
            {
                itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = "Find" });
                itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Value, Value = Index });
                itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = "Of" });
            }

            if (!string.IsNullOrWhiteSpace(labelText))
            {
                itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = labelText });
            }

            if (valueEntry != null)
            {
                itemToAdd.AddRange(CreateDebugItemsFromEntry(expression, valueEntry, executionId, enDev2ArgumentType.Input));
            }

            _debugInputs.Add(itemToAdd);

            if (string.IsNullOrWhiteSpace(labelText))
            {
                itemToAdd = new DebugItem();
                itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = "Direction" });
                itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Value, Value = Direction });
                _debugInputs.Add(itemToAdd);
            }
        }

        private void AddDebugOutputItem(string expression, string value, Guid dlId, int iterationCount)
        {
            DebugItem itemToAdd = new DebugItem();

            itemToAdd.AddRange(CreateDebugItemsFromString(expression, value, dlId, iterationCount, enDev2ArgumentType.Output));
            _debugOutputs.Add(itemToAdd);
        }

        #endregion Private Methods

        public override IBinaryDataList GetWizardData()
        {
            return null;
        }

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

        #region Update ForEach Inputs/Outputs

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            if (updates != null)
            {
                foreach (Tuple<string, string> t in updates)
                {

                    if (t.Item1 == InField)
                    {
                        InField = t.Item2;
                    }

                    if (t.Item1 == Characters)
                    {
                        Characters = t.Item2;
                    }
                }
            }
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            if (updates != null && updates.Count == 1)
            {
                Result = updates[0].Item2;
            }
        }

        #endregion

        #region GetForEachInputs/Outputs

        public override IList<DsfForEachItem> GetForEachInputs()
        {
            return GetForEachItems(InField, Characters);
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            return GetForEachItems(Result);
        }

        #endregion

    }
}
