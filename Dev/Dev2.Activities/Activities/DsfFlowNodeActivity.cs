
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
using System.Linq;
using System.Text;
using Dev2;
using Dev2.Activities;
using Dev2.Activities.Debug;
using Dev2.Common.Interfaces.DataList.Contract;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Data.SystemTemplates.Models;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.Value_Objects;
using Dev2.Diagnostics;
using Dev2.Diagnostics.Debug;
using Microsoft.CSharp.Activities;
using Newtonsoft.Json;

// ReSharper disable CheckNamespace
namespace Unlimited.Applications.BusinessDesignStudio.Activities
// ReSharper restore CheckNamespace
{
    public abstract class DsfFlowNodeActivity<TResult> : DsfActivityAbstract<TResult>, IFlowNodeActivity
    {
        // Changing the ExpressionText property of a VisualBasicValue during runtime has no effect. 
        // The expression text is only evaluated and converted to an expression tree when CacheMetadata() is called.
        readonly CSharpValue<TResult> _expression; // BUG 9304 - 2013.05.08 - TWR - Changed type to CSharpValue
        TResult _theResult;
        Guid _dataListId;
        IDSFDataObject _dataObject;

        #region Ctor

        protected DsfFlowNodeActivity(string displayName)
            : this(displayName, DebugDispatcher.Instance, true)
        {
        }

        // BUG 9304 - 2013.05.08 - TWR - Added this constructor for testing purposes
        protected DsfFlowNodeActivity(string displayName, IDebugDispatcher debugDispatcher, bool isAsync = false)
            : base(displayName, debugDispatcher, isAsync)
        {
            _expression = new CSharpValue<TResult>();
        }

        #endregion

        #region ExpressionText

        public string ExpressionText
        {
            get
            {
                return _expression.ExpressionText;
            }
            set
            {
                _expression.ExpressionText = value;
            }
        }

        #endregion

        #region CacheMetadata

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);
            //
            // Must use AddChild (which adds children as 'public') otherwise you will get the following exception:
            //
            // The private implementation of activity Decision has the following validation error:
            // Compiler error(s) encountered processing expression t.Eq(d.Get("FirstName",AmbientDataList),"Trevor").
            // 't' is not declared. It may be inaccessible due to its protection level
            // 'd' is not declared. It may be inaccessible due to its protection level
            //
            metadata.AddChild(_expression);
        }

        #endregion

        #region OnExecute

        protected override void OnExecute(NativeActivityContext context)
        {
            _dataObject = context.GetExtension<IDSFDataObject>();
            DataListFactory.CreateDataListCompiler();
            _dataListId = _dataObject.DataListID;
            InitializeDebug(_dataObject);

            if(_dataObject.IsDebugMode())
            {
                DispatchDebugState(context, StateType.Before);
            }
            context.ScheduleActivity(_expression, OnCompleted, OnFaulted);
        }

        #endregion

        #region OnCompleted

        void OnCompleted(NativeActivityContext context, ActivityInstance completedInstance, TResult result)
        {
            IDSFDataObject dataObject = context.GetExtension<IDSFDataObject>();
            Result.Set(context, result);
            _theResult = result;

            if(dataObject != null && dataObject.IsDebugMode())
            {
                DispatchDebugState(context, StateType.After);
            }

            OnExecutedCompleted(context, false, false);
        }

        #endregion

        #region OnFaulted

        void OnFaulted(NativeActivityFaultContext faultContext, Exception propagatedException, ActivityInstance propagatedFrom)
        {
            IDSFDataObject dataObject = faultContext.GetExtension<IDSFDataObject>();
            if(dataObject != null && dataObject.IsDebugMode())
            {
                DispatchDebugState(faultContext, StateType.After);
            }
            OnExecutedCompleted(faultContext, true, false);
        }

        #endregion

        #region Get Debug Inputs/Outputs

        // Travis.Frisinger - 28.01.2013 : Amended for Debug
        public override List<DebugItem> GetDebugInputs(IBinaryDataList dataList)
        {
            List<IDebugItem> result = new List<IDebugItem>();
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
            var allErrors = new ErrorResultTO();

            var val = new StringBuilder(Dev2DecisionStack.ExtractModelFromWorkflowPersistedData(ExpressionText));

            try
            {
                Dev2DecisionStack dds = compiler.ConvertFromJsonToModel<Dev2DecisionStack>(val);
                ErrorResultTO error;
                string userModel = dds.GenerateUserFriendlyModel(dataList.UID, dds.Mode, out error);
                allErrors.MergeErrors(error);

                foreach(Dev2Decision dev2Decision in dds.TheStack)
                {
                    AddInputDebugItemResultsAfterEvaluate(result, ref userModel, dataList, dds.Mode, dev2Decision.Col1, out  error);
                    allErrors.MergeErrors(error);
                    AddInputDebugItemResultsAfterEvaluate(result, ref userModel, dataList, dds.Mode, dev2Decision.Col2, out error);
                    allErrors.MergeErrors(error);
                    AddInputDebugItemResultsAfterEvaluate(result, ref userModel, dataList, dds.Mode, dev2Decision.Col3, out error);
                    allErrors.MergeErrors(error);
                }

                var itemToAdd = new DebugItem();

                userModel = userModel.Replace("OR", " OR\r\n")
                                     .Replace("AND", " AND\r\n")
                                     .Replace("\r\n ", "\r\n")
                                     .Replace("\r\n\r\n", "\r\n")
                                     .Replace("  ", " ");

                AddDebugItem(new DebugItemStaticDataParams(userModel, "Statement"), itemToAdd);
                result.Add(itemToAdd);

                itemToAdd = new DebugItem();
                AddDebugItem(new DebugItemStaticDataParams(dds.Mode == Dev2DecisionMode.AND ? "YES" : "NO", "Require All decisions to be True"), itemToAdd);
                result.Add(itemToAdd);
            }
            catch(JsonSerializationException)
            {
                Dev2Switch ds = new Dev2Switch { SwitchVariable = val.ToString() };
                DebugItem itemToAdd = new DebugItem();
                ErrorResultTO errors;
                IBinaryDataListEntry expressionsEntry = compiler.Evaluate(dataList.UID, enActionType.User, ds.SwitchVariable, false, out errors);
                var debugResult = new DebugItemVariableParams(ds.SwitchVariable, "Switch on", expressionsEntry, dataList.UID);
                itemToAdd.AddRange(debugResult.GetDebugItemResult());
                result.Add(itemToAdd);
            }
            catch(Exception e)
            {
                allErrors.AddError(e.Message);
            }
            finally
            {
                if(allErrors.HasErrors())
                {
                    var serviceName = GetType().Name;
                    DisplayAndWriteError(serviceName, allErrors);
                    ErrorResultTO error;
                    compiler.UpsertSystemTag(_dataListId, enSystemTag.Dev2Error, allErrors.MakeDataListReady(), out error);
                }
            }

            return result.Select(a => a as DebugItem).ToList();
        }

        void AddInputDebugItemResultsAfterEvaluate(List<IDebugItem> result, ref string userModel, IBinaryDataList dataList, Dev2DecisionMode decisionMode, string expression, out ErrorResultTO error, DebugItem parent = null)
        {
            error = new ErrorResultTO();
            if(expression != null && DataListUtil.IsEvaluated(expression))
            {
                DebugOutputBase debugResult;
                if(error.HasErrors())
                {
                    debugResult = new DebugItemStaticDataParams("", expression, "");
                }
                else
                {
                    var expressiomToStringValue = EvaluateExpressiomToStringValue(expression, decisionMode, dataList);
                    userModel = userModel.Replace(expression, expressiomToStringValue);
                    ErrorResultTO errors;
                    IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
                    IBinaryDataListEntry expressionsEntry = compiler.Evaluate(dataList.UID, enActionType.User, expression, false, out errors);
                    debugResult = new DebugItemVariableParams(expression, "", expressionsEntry, dataList.UID);
                }

                var itemResults = debugResult.GetDebugItemResult();

                var allReadyAdded = new List<IDebugItemResult>();

                itemResults.ForEach(a =>
                    {
                        var found = result.SelectMany(r => r.FetchResultsList())
                                          .SingleOrDefault(r => r.Variable.Equals(a.Variable));
                        if(found != null)
                        {
                            allReadyAdded.Add(a);
                        }
                    });

                allReadyAdded.ForEach(i => itemResults.Remove(i));

                if(parent == null)
                {
                    result.Add(new DebugItem(itemResults));
                }
                else
                {
                    parent.AddRange(itemResults);
                }
            }
        }

        // Travis.Frisinger - 28.01.2013 : Amended for Debug
        public override List<DebugItem> GetDebugOutputs(IBinaryDataList dataList)
        {
            var result = new List<DebugItem>();
            string resultString = _theResult.ToString();
            DebugItem itemToAdd = new DebugItem();
            IDataListCompiler c = DataListFactory.CreateDataListCompiler();
            var val = new StringBuilder(Dev2DecisionStack.ExtractModelFromWorkflowPersistedData(ExpressionText));

            try
            {
                Dev2DecisionStack dds = c.ConvertFromJsonToModel<Dev2DecisionStack>(val);

                if(_theResult.ToString() == "True")
                {
                    resultString = dds.TrueArmText;
                }
                else if(_theResult.ToString() == "False")
                {
                    resultString = dds.FalseArmText;
                }

                itemToAdd.AddRange(new DebugItemStaticDataParams(resultString, "").GetDebugItemResult());
                result.Add(itemToAdd);
            }
            catch(Exception)
            {
                if(!dataList.HasErrors())
                {
                    itemToAdd.AddRange(new DebugItemStaticDataParams(resultString, "").GetDebugItemResult());
                    result.Add(itemToAdd);
                }
            }

            return result;
        }

        #endregion

        #region Private Debug Methods

        private string EvaluateExpressiomToStringValue(string expression, Dev2DecisionMode mode, IBinaryDataList dataList)
        {
            string result = string.Empty;
            IDataListCompiler c = DataListFactory.CreateDataListCompiler();

            ErrorResultTO errors;
            var dlEntry = c.Evaluate(dataList.UID, enActionType.User, expression, false, out errors);
            if(dlEntry != null && dlEntry.IsRecordset)
            {
                if(DataListUtil.GetRecordsetIndexType(expression) == enRecordsetIndexType.Numeric)
                {
                    int index;
                    if(int.TryParse(DataListUtil.ExtractIndexRegionFromRecordset(expression), out index))
                    {
                        string error;
                        IList<IBinaryDataListItem> listOfCols = dlEntry.FetchRecordAt(index, out error);
                        if(listOfCols != null)
                        {
                            foreach(IBinaryDataListItem binaryDataListItem in listOfCols)
                            {
                                result = binaryDataListItem.TheValue;
                            }
                        }
                    }
                }
                else
                {
                    if(DataListUtil.GetRecordsetIndexType(expression) == enRecordsetIndexType.Star)
                    {
                        IDev2IteratorCollection colItr = Dev2ValueObjectFactory.CreateIteratorCollection();
                        IBinaryDataListEntry entry = c.Evaluate(dataList.UID, enActionType.User, expression, false, out errors);
                        IDev2DataListEvaluateIterator col1Iterator = Dev2ValueObjectFactory.CreateEvaluateIterator(entry);
                        colItr.AddIterator(col1Iterator);

                        bool firstTime = true;
                        while(colItr.HasMoreData())
                        {
                            if(firstTime)
                            {
                                result = colItr.FetchNextRow(col1Iterator).TheValue;
                                firstTime = false;
                            }
                            else
                            {
                                result += " " + mode + " " + colItr.FetchNextRow(col1Iterator).TheValue;
                            }
                        }
                    }
                    else
                    {
                        result = string.Empty;
                    }
                }
            }
            else
            {
                if(dlEntry != null)
                {
                    var scalarItem = dlEntry.FetchScalar();
                    result = scalarItem.TheValue;
                }
            }


            return result;
        }

        #endregion

        #region GetForEachInputs/Outputs

        public override IList<DsfForEachItem> GetForEachInputs()
        {
            return GetForEachItems(ExpressionText);
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            return GetForEachItems(_theResult.ToString());
        }

        #endregion

        // BUG 9304 - 2013.05.08 - TWR - Added for testing purposes
        public CodeActivity<TResult> GetTheExpression()
        {
            return _expression;
        }

        // BUG 9304 - 2013.05.08 - TWR - Added for testing purposes
        public string GetTheResult()
        {
            return _theResult.ToString();
        }

    }
}
