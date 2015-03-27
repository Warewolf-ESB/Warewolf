
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
using Dev2.Data;
using Dev2.Data.Factories;
using Dev2.Data.Interfaces;
using Dev2.Data.Operations;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Builders;
using Dev2.Diagnostics;
using Dev2.Util;
using Dev2.Validation;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;
using Warewolf.Storage;

// ReSharper disable CheckNamespace
namespace Unlimited.Applications.BusinessDesignStudio.Activities
// ReSharper restore CheckNamespace
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

        // ReSharper disable RedundantOverridenMember
        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);
        }
        // ReSharper restore RedundantOverridenMember

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
            InitializeDebug(dataObject);
            IDev2DataListUpsertPayloadBuilder<List<string>> toUpsert = Dev2DataListBuilderFactory.CreateStringListDataListUpsertBuilder();
            IDev2DataListUpsertPayloadBuilder<string> toUpsertScalar = Dev2DataListBuilderFactory.CreateStringDataListUpsertBuilder();
            toUpsert.IsDebug = dataObject.IsDebugMode();
            toUpsertScalar.IsDebug = dataObject.IsDebugMode();
            try
            {

                var outerIteratorCollection = new WarewolfListIterator();
                var innerIteratorCollection = new WarewolfListIterator();

                allErrors.MergeErrors(errors);


                var itrChar = new WarewolfIterator(dataObject.Environment.Eval(Characters));
                outerIteratorCollection.AddVariableToIterateOn(itrChar);

                #region Iterate and Find Index


                if (dataObject.IsDebugMode())
                {
                    AddDebugInputItem(new DebugEvalResult(InField, "In Field", dataObject.Environment));
                    AddDebugInputItem(new DebugItemStaticDataParams(Index, "Index"));
                    AddDebugInputItem(new DebugEvalResult(Characters, "Characters", dataObject.Environment));
                    AddDebugInputItem(new DebugItemStaticDataParams(Direction, "Direction"));
                }

                var completeResultList = new List<string>();

                while (outerIteratorCollection.HasMoreData())
                {
                    allErrors.MergeErrors(errors);
                    errors.ClearErrors();
                    var itrInField = new WarewolfIterator(dataObject.Environment.Eval(InField));
                    innerIteratorCollection.AddVariableToIterateOn(itrInField);

                    string chars = outerIteratorCollection.FetchNextValue(itrChar);
                    while (innerIteratorCollection.HasMoreData())
                    {
                        if (!string.IsNullOrEmpty(InField) && !string.IsNullOrEmpty(Characters))
                        {
                            var val = innerIteratorCollection.FetchNextValue(itrInField);
                            if (val != null)
                            {
                                IEnumerable<int> returedData = indexFinder.FindIndex(val, Index, chars, Direction, MatchCase, StartIndex);
                                completeResultList.AddRange(returedData.Select(value => value.ToString(CultureInfo.InvariantCulture)).ToList());
                                //2013.06.03: Ashley Lewis for bug 9498 - handle multiple regions in result

                            }
                        }
                    }

                }
                var rule = new IsSingleValueRule(() => Result);
                var single = rule.Check();
                if (single != null)
                {
                    allErrors.AddError(single.Message);
                }
                else
                {



                    if (DataListUtil.IsValueRecordset(Result))
                    {
                        var rsType = DataListUtil.GetRecordsetIndexType(Result);
                        if (rsType == enRecordsetIndexType.Numeric)
                        {

                            dataObject.Environment.Assign(Result, string.Join(",", completeResultList));
                            allErrors.MergeErrors(errors);
                            if (!allErrors.HasErrors() && dataObject.IsDebugMode())
                            {
                                AddDebugOutputItem(new DebugEvalResult(Result, "", dataObject.Environment));
                            }
                        }
                        else
                        {
                            var idx = 1;
                            foreach (var res in completeResultList)
                            {
                                if (rsType == enRecordsetIndexType.Blank)
                                {
                                    dataObject.Environment.Assign(Result, res);
                                }
                                if (rsType == enRecordsetIndexType.Star)
                                {
                                    var expression = DataListUtil.CreateRecordsetDisplayValue(DataListUtil.ExtractRecordsetNameFromValue(Result), DataListUtil.ExtractFieldNameFromValue(Result), idx.ToString());
                                    dataObject.Environment.Assign(DataListUtil.AddBracketsToValueIfNotExist(expression), res);
                                    idx++;
                                }
                            }
                        }
                    }
                    else
                    {
                        dataObject.Environment.Assign(Result, string.Join(",", completeResultList));
                    }
                    allErrors.MergeErrors(errors);
                    if (!allErrors.HasErrors() && dataObject.IsDebugMode())
                    {
                        AddDebugOutputItem(new DebugEvalResult(Result,"",dataObject.Environment));
                    }
                }

                #endregion


            }
            catch (Exception e)
            {
                Dev2Logger.Log.Error("DSFFindActivity", e);
                allErrors.AddError(e.Message);
            }
            finally
            {
                #region Handle Errors
                var hasErrors = allErrors.HasErrors();
                if (hasErrors)
                {
                    DisplayAndWriteError("DsfIndexActivity", allErrors);
                    compiler.UpsertSystemTag(dataObject.DataListID, enSystemTag.Dev2Error, allErrors.MakeDataListReady(), out errors);
                    compiler.Upsert(executionId, Result, (string)null, out errors);
                }

                #endregion

                if (dataObject.IsDebugMode())
                {
                    if (hasErrors)
                    {
                        foreach (var debugOutputTo in toUpsert.DebugOutputs)
                        {
                            AddDebugOutputItem(new DebugItemVariableParams(debugOutputTo));
                        }
                    }
                    DispatchDebugState(context, StateType.Before);
                    DispatchDebugState(context, StateType.After);
                }
            }
        }

        #region Private Methods



        #endregion Private Methods

        #region Get Debug Inputs/Outputs

        public override List<DebugItem> GetDebugInputs(IExecutionEnvironment dataList)
        {
            foreach(DebugItem debugInput in _debugInputs)
            {
                debugInput.FlushStringBuilder();
            }
            return _debugInputs;
        }

        public override List<DebugItem> GetDebugOutputs(IExecutionEnvironment dataList)
        {
            foreach(IDebugItem debugOutput in _debugOutputs)
            {
                debugOutput.FlushStringBuilder();
            }
            return _debugOutputs;
        }

        #endregion Get Inputs/Outputs

        #region Update ForEach Inputs/Outputs

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            if(updates != null)
            {
                foreach(Tuple<string, string> t in updates)
                {

                    if(t.Item1 == InField)
                    {
                        InField = t.Item2;
                    }

                    if(t.Item1 == Characters)
                    {
                        Characters = t.Item2;
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
            return GetForEachItems(InField, Characters);
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            return GetForEachItems(Result);
        }

        #endregion

    }
}
