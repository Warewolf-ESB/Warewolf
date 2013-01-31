using Dev2;
using Dev2.Activities;
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
            IDSFDataObject dataObject = context.GetExtension<IDSFDataObject>();
            IDataListCompiler compiler = context.GetExtension<IDataListCompiler>();
            IDev2IndexFinder indexFinder = new Dev2IndexFinder();
            ErrorResultTO allErrors = new ErrorResultTO();
            ErrorResultTO errors = new ErrorResultTO();
            Guid executionId = DataListExecutionID.Get(context);

            try
            {


                IDev2IteratorCollection outerIteratorCollection = Dev2ValueObjectFactory.CreateIteratorCollection();
                IDev2IteratorCollection innerIteratorCollection = Dev2ValueObjectFactory.CreateIteratorCollection();
                IDev2DataListUpsertPayloadBuilder<string> toUpsert = Dev2DataListBuilderFactory.CreateStringDataListUpsertBuilder(true);
                allErrors.MergeErrors(errors);


                IBinaryDataListEntry expressionsEntry = compiler.Evaluate(executionId, enActionType.User, Characters, false, out errors);
                allErrors.MergeErrors(errors);
                IDev2DataListEvaluateIterator itrChar = Dev2ValueObjectFactory.CreateEvaluateIterator(expressionsEntry);

                outerIteratorCollection.AddIterator(itrChar);

                #region Iterate and Find Index

                string result = string.Empty;
                while (outerIteratorCollection.HasMoreData())
                {
                    expressionsEntry = compiler.Evaluate(executionId, enActionType.User, InField, false, out errors);
                    allErrors.MergeErrors(errors);
                    IDev2DataListEvaluateIterator itrInField = Dev2ValueObjectFactory.CreateEvaluateIterator(expressionsEntry);
                    innerIteratorCollection.AddIterator(itrInField);

                    string chars = outerIteratorCollection.FetchNextRow(itrChar).TheValue;
                    while (innerIteratorCollection.HasMoreData())
                    {
                        if (!string.IsNullOrEmpty(InField) && !string.IsNullOrEmpty(Characters))
                        {
                            IEnumerable<int> returedData = indexFinder.FindIndex(innerIteratorCollection.FetchNextRow(itrInField).TheValue, Index,
                                                               chars,
                                                               Direction, MatchCase, StartIndex);

                            result = string.Join(",", returedData);

                            toUpsert.Add(Result, result);
                            toUpsert.FlushIterationFrame();
                        }
                    }
                }

                #endregion

                #region Add Result to DataList

                compiler.Upsert(executionId, toUpsert, out errors);
                allErrors.MergeErrors(errors);
                compiler.Shape(executionId, enDev2ArgumentType.Output, OutputMapping, out errors);
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
                    string err = DisplayAndWriteError("DsfIndexActivity", allErrors);
                    compiler.UpsertSystemTag(dataObject.DataListID, enSystemTag.Error, err, out errors);
                }

                #endregion
            }
        }

        #region Private Methods

        #endregion Private Methods

        public override IBinaryDataList GetWizardData()
        {
            return null;
        }

        #region Get Debug Inputs/Outputs

        public override IList<IDebugItem> GetDebugInputs(IBinaryDataList dataList)
        {
            IList<IDebugItem> results = new List<IDebugItem>();

            if (!string.IsNullOrEmpty(InField))
            {
                foreach (IDebugItem debugItem in CreateDebugItems(InField, dataList))
                {
                    // BUG 8104 : Refactor DebugItem
                    //debugItem.Label = debugItem.Label + " Field To Look In ";
                    results.Add(debugItem);
                }
            }

            if (!string.IsNullOrEmpty(Index))
            {
                results.Add(new DebugItem(" Index To Find", Index, null));
            }

            if (!string.IsNullOrEmpty(Characters))
            {
                foreach (IDebugItem debugItem in CreateDebugItems(Characters, dataList))
                {
                    // BUG 8104 : Refactor DebugItem
                    //debugItem.Label = debugItem.Label + " Charecters To Look For ";
                    results.Add(debugItem);
                }
            }

            if (!string.IsNullOrEmpty(Direction))
            {
                results.Add(new DebugItem(" Direction To Search In ", Direction, null));
            }

            return results;
        }

        public override IList<IDebugItem> GetDebugOutputs(IBinaryDataList dataList)
        {
            IList<IDebugItem> results = new List<IDebugItem>();
            if (!string.IsNullOrEmpty(Result))
            {
                foreach (IDebugItem debugItem in CreateDebugItems(Result, dataList))
                {
                    results.Add(debugItem);
                }
            }

            return results;
        }

        #endregion Get Inputs/Outputs

        #region Update ForEach Inputs/Outputs

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
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

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            if (updates.Count == 1)
            {
                Result = updates[0].Item2;
            }
        }

        #endregion

        #region GetForEachInputs/Outputs

        public override IList<DsfForEachItem> GetForEachInputs(NativeActivityContext context)
        {
            return GetForEachItems(context, StateType.Before, InField, Characters);
        }

        public override IList<DsfForEachItem> GetForEachOutputs(NativeActivityContext context)
        {
            return GetForEachItems(context, StateType.After, Result);
        }

        #endregion

    }
}
