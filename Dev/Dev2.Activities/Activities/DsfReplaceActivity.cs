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
using System.Globalization;
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
        public string FieldsToSearch { get; set; }

        /// <summary>
        /// Property for holding a string the user selects in the "Find" drop down box
        /// </summary>
        [Inputs("Find")]
        public string Find { get; set; }

        /// <summary>
        /// Property for holding a string the user enters into the "Replace With" box
        /// </summary>
        [Inputs("ReplaceWith")]
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
            IDataListCompiler compiler = context.GetExtension<IDataListCompiler>();
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
                while (iteratorCollection.HasMoreData())
                {
                    // Fetch all fields to search....
                    IList<string> toSearch = FieldsToSearch.Split(',');
                    // now process each field for entire evaluated Where expression....                    

                    string findValue = iteratorCollection.FetchNextRow(itrFind).TheValue;
                    string replaceWithValue = iteratorCollection.FetchNextRow(itrReplace).TheValue;
                    foreach (string s in toSearch)
                    {

                        toUpsert = replaceOperation.Replace(executionId, s.Trim(), findValue, replaceWithValue, CaseMatch, toUpsert,
                                                            out errors, out replacementCount);
                        replacementTotal += replacementCount;

                        allErrors.MergeErrors(errors);
                    }

                }

                toUpsert.Add(Result, replacementTotal.ToString(CultureInfo.InvariantCulture));

                // now push the result to the server
                compiler.Upsert(executionId, toUpsert, out errors);
                allErrors.MergeErrors(errors);

                compiler.Shape(executionId, enDev2ArgumentType.Output, OutputMapping, out errors);
                allErrors.MergeErrors(errors);
            }
            finally
            {

                if (allErrors.HasErrors())
                {
                    string err = DisplayAndWriteError("DsfReplaceActivity", allErrors);
                    compiler.UpsertSystemTag(dataObject.DataListID, enSystemTag.Error, err, out errors);
                }
            }

        }

        #region Get Debug Inputs/Outputs

        public override IList<IDebugItem> GetDebugInputs(IBinaryDataList dataList)
        {
            IList<IDebugItem> results = new List<IDebugItem>();

            IList<string> fieldsList = FieldsToSearch.Split(',');
            if (fieldsList.Count > 0)
            {
                foreach (string s in fieldsList)
                {
                    foreach (IDebugItem debugItem in CreateDebugItems(s, dataList))
                    {
                        debugItem.Label = debugItem.Label + " Lookin ";
                        results.Add(debugItem);
                    }
                }
            }

            if (!string.IsNullOrEmpty(Find))
            {
                foreach (IDebugItem debugItem in CreateDebugItems(Find, dataList))
                {
                    debugItem.Label = debugItem.Label + " Find ";
                    results.Add(debugItem);
                }
            }

            if (!string.IsNullOrEmpty(ReplaceWith))
            {
                foreach (IDebugItem debugItem in CreateDebugItems(ReplaceWith, dataList))
                {
                    debugItem.Label = debugItem.Label + " Replace With ";
                    results.Add(debugItem);
                }
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

        public override IList<DsfForEachItem> GetForEachInputs(NativeActivityContext context)
        {
            return GetForEachItems(context, StateType.Before, FieldsToSearch, Find, ReplaceWith);
        }

        public override IList<DsfForEachItem> GetForEachOutputs(NativeActivityContext context)
        {
            return GetForEachItems(context, StateType.After, Result);
        }

        #endregion

    }
}
