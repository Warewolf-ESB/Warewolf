
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
using System.Globalization;
using System.Linq;
using Dev2;
using Dev2.Activities;
using Dev2.Activities.Debug;
using Dev2.Common;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Data.Factories;
using Dev2.Data.Interfaces;
using Dev2.Data.Operations;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.Builders;
using Dev2.DataList.Contract.Value_Objects;
using Dev2.Diagnostics;
using Dev2.Util;
using Dev2.Validation;
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
            IDev2DataListUpsertPayloadBuilder<string> toUpsert = Dev2DataListBuilderFactory.CreateStringDataListUpsertBuilder(false);
            toUpsert.IsDebug = dataObject.IsDebugMode();
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

            InitializeDebug(dataObject);
            try
            {

                IList<string> toSearch = FieldsToSearch.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

                foreach(var s in toSearch)
                {
                    if(dataObject.IsDebugMode())
                    {
                        IBinaryDataListEntry inFieldsEntry = compiler.Evaluate(executionId, enActionType.User, s, false, out errors);
                        AddDebugInputItem(new DebugItemVariableParams(s, "In Field(s)", inFieldsEntry, executionId));
                    }
                }
                var rule = new IsSingleValueRule(() => Result);
                var single = rule.Check();
                if(single != null)
                {
                    allErrors.AddError(single.Message);
                }
                else
                {
                    while(iteratorCollection.HasMoreData())
                    {
                        // now process each field for entire evaluated Where expression....                    
                        var findValue = iteratorCollection.FetchNextRow(itrFind).TheValue;
                        var replaceWithValue = iteratorCollection.FetchNextRow(itrReplace).TheValue;
                        foreach(string s in toSearch)
                        {
                            if(!DataListUtil.IsEvaluated(s))
                            {
                                allErrors.AddError("Please insert only variables into Fields To Search");
                                return;
                            }
                            if(!string.IsNullOrEmpty(findValue))
                            {
                                IBinaryDataListEntry entryToReplaceIn;
                                toUpsert = replaceOperation.Replace(executionId, s.Trim(), findValue, replaceWithValue, CaseMatch, toUpsert, out errors, out replacementCount, out entryToReplaceIn);
                            }

                            replacementTotal += replacementCount;

                            allErrors.MergeErrors(errors);
                        }
                    }
                }
                if(dataObject.IsDebugMode())
                {
                    AddDebugInputItem(new DebugItemVariableParams(Find, "Find", expressionsEntryFind, executionId));
                    AddDebugInputItem(new DebugItemVariableParams(ReplaceWith, "Replace With", expressionsEntryReplaceWith, executionId));
                }

                toUpsert.Add(Result, replacementTotal.ToString(CultureInfo.InvariantCulture));


                // now push the result to the server
                compiler.Upsert(executionId, toUpsert, out errors);
                allErrors.MergeErrors(errors);
                if(dataObject.IsDebugMode() && !allErrors.HasErrors())
                {
                    foreach(var debugOutputTo in toUpsert.DebugOutputs)
                    {
                        AddDebugOutputItem(new DebugItemVariableParams(debugOutputTo));
                    }
                }
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch(Exception ex)
            {
                Dev2Logger.Log.Error("DSFReplace", ex);
                allErrors.AddError(ex.Message);
            }
            finally
            {
                if(allErrors.HasErrors())
                {
                    if(dataObject.IsDebugMode())
                    {
                        AddDebugOutputItem(new DebugItemStaticDataParams("", Result, ""));
                    }
                    DisplayAndWriteError("DsfReplaceActivity", allErrors);
                    compiler.UpsertSystemTag(dataObject.DataListID, enSystemTag.Dev2Error, allErrors.MakeDataListReady(), out errors);
                    compiler.Upsert(executionId, Result, (string)null, out errors);
                }

                if(dataObject.IsDebugMode())
                {
                    DispatchDebugState(context, StateType.Before);
                    DispatchDebugState(context, StateType.After);
                }
            }

        }

        #region Private Methods

        #endregion

        #region Get Debug Inputs/Outputs

        public override List<DebugItem> GetDebugInputs(IBinaryDataList dataList)
        {
            foreach(IDebugItem debugInput in _debugInputs)
            {
                debugInput.FlushStringBuilder();
            }
            return _debugInputs;
        }

        public override List<DebugItem> GetDebugOutputs(IBinaryDataList dataList)
        {
            foreach(IDebugItem debugOutput in _debugOutputs)
            {
                debugOutput.FlushStringBuilder();
            }
            return _debugOutputs;
        }

        #endregion Get Inputs/Outputs

        #region Get ForEach Inputs/Outputs Updates

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            if(updates != null)
            {
                foreach(Tuple<string, string> t in updates)
                {

                    if(t.Item1 == FieldsToSearch)
                    {
                        FieldsToSearch = t.Item2;
                    }

                    if(t.Item1 == Find)
                    {
                        Find = t.Item2;
                    }

                    if(t.Item1 == ReplaceWith)
                    {
                        ReplaceWith = t.Item2;
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
