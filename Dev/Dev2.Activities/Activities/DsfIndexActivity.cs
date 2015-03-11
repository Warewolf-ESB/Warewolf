
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Common.Interfaces.Toolbox;
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
using Warewolf.Core;

// ReSharper disable CheckNamespace
namespace Unlimited.Applications.BusinessDesignStudio.Activities
// ReSharper restore CheckNamespace
{
     [ToolDescriptorInfo("Data-FindIndex", "Find Index", ToolType.Native, "8999E59A-38A3-43BB-A98F-6090C5C9EA1E", "Bob", "1.0.0.0", "c:\\", "Data", "/Warewolf.Studio.Themes.Luna;component/Images.xaml")]
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



                IDev2IteratorCollection outerIteratorCollection = Dev2ValueObjectFactory.CreateIteratorCollection();
                IDev2IteratorCollection innerIteratorCollection = Dev2ValueObjectFactory.CreateIteratorCollection();

                allErrors.MergeErrors(errors);


                IBinaryDataListEntry expressionsEntry = compiler.Evaluate(executionId, enActionType.User, Characters, false, out errors);
                allErrors.MergeErrors(errors);
                IDev2DataListEvaluateIterator itrChar = Dev2ValueObjectFactory.CreateEvaluateIterator(expressionsEntry);

                outerIteratorCollection.AddIterator(itrChar);

                #region Iterate and Find Index

                expressionsEntry = compiler.Evaluate(executionId, enActionType.User, InField, false, out errors);

                if(dataObject.IsDebugMode())
                {
                    AddDebugInputItem(new DebugItemVariableParams(InField, "In Field", expressionsEntry, executionId));
                    AddDebugInputItem(new DebugItemStaticDataParams(Index, "Index"));
                    AddDebugInputItem(new DebugItemVariableParams(Characters, "Characters", itrChar.FetchEntry(), executionId));
                    AddDebugInputItem(new DebugItemStaticDataParams(Direction, "Direction"));
                }

                var completeResultList = new List<string>();

                while(outerIteratorCollection.HasMoreData())
                {
                    allErrors.MergeErrors(errors);
                    errors.ClearErrors();
                    IDev2DataListEvaluateIterator itrInField = Dev2ValueObjectFactory.CreateEvaluateIterator(expressionsEntry);
                    innerIteratorCollection.AddIterator(itrInField);

                    string chars = outerIteratorCollection.FetchNextRow(itrChar).TheValue;
                    while(innerIteratorCollection.HasMoreData())
                    {
                        if(!string.IsNullOrEmpty(InField) && !string.IsNullOrEmpty(Characters))
                        {
                            var val = innerIteratorCollection.FetchNextRow(itrInField);
                            if(val != null)
                            {
                                IEnumerable<int> returedData = indexFinder.FindIndex(val.TheValue, Index, chars, Direction, MatchCase, StartIndex);
                                completeResultList.AddRange(returedData.Select(value => value.ToString(CultureInfo.InvariantCulture)).ToList());
                                //2013.06.03: Ashley Lewis for bug 9498 - handle multiple regions in result

                            }
                        }
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


                    var rsType = DataListUtil.GetRecordsetIndexType(Result);
                    if(rsType == enRecordsetIndexType.Numeric)
                    {

                        toUpsertScalar.Add(Result, string.Join(",", completeResultList));
                        compiler.Upsert(executionId, toUpsertScalar, out errors);
                        allErrors.MergeErrors(errors);
                        if(!allErrors.HasErrors() && dataObject.IsDebugMode())
                        {
                            foreach(var debugOutputTo in toUpsertScalar.DebugOutputs)
                            {
                                AddDebugOutputItem(new DebugItemVariableParams(debugOutputTo));
                            }
                            toUpsert.DebugOutputs.Clear();
                        }
                    }
                    else
                    {
                        toUpsert.Add(Result, completeResultList);
                        compiler.Upsert(executionId, toUpsert, out errors);
                        allErrors.MergeErrors(errors);
                        if(!allErrors.HasErrors() && dataObject.IsDebugMode())
                        {
                            foreach(var debugOutputTo in toUpsert.DebugOutputs)
                            {
                                AddDebugOutputItem(new DebugItemVariableParams(debugOutputTo));
                            }
                            toUpsert.DebugOutputs.Clear();
                        }
                    }
                }
                #endregion


            }
            catch(Exception e)
            {
                Dev2Logger.Log.Error("DSFFindActivity", e);
                allErrors.AddError(e.Message);
            }
            finally
            {
                #region Handle Errors
                var hasErrors = allErrors.HasErrors();
                if(hasErrors)
                {
                    DisplayAndWriteError("DsfIndexActivity", allErrors);
                    compiler.UpsertSystemTag(dataObject.DataListID, enSystemTag.Dev2Error, allErrors.MakeDataListReady(), out errors);
                    compiler.Upsert(executionId, Result, (string)null, out errors);
                }

                #endregion

                if(dataObject.IsDebugMode())
                {
                    if(hasErrors)
                    {
                        foreach(var debugOutputTo in toUpsert.DebugOutputs)
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

        public override List<DebugItem> GetDebugInputs(IBinaryDataList dataList)
        {
            foreach(DebugItem debugInput in _debugInputs)
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
