using Dev2;
using Dev2.Activities;
using Dev2.Activities.Debug;
using Dev2.Data.SystemTemplates.Models;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.Value_Objects;
using Dev2.Diagnostics;
using Microsoft.CSharp.Activities;
using Newtonsoft.Json;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;

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
            IDSFDataObject dataObject = context.GetExtension<IDSFDataObject>();
            InitializeDebug(dataObject);
            if(dataObject != null && dataObject.IsDebugMode())
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
            var result = new List<DebugItem>();
            IDataListCompiler c = DataListFactory.CreateDataListCompiler();

            string val = Dev2DecisionStack.ExtractModelFromWorkflowPersistedData(ExpressionText);

            try
            {
                Dev2DecisionStack dds = c.ConvertFromJsonToModel<Dev2DecisionStack>(val);
                string userModel = dds.GenerateUserFriendlyModel(dataList.UID, dds.Mode);

                foreach(Dev2Decision dev2Decision in dds.TheStack)
                {
                    AddInputDebugItemResultsAfterEvaluate(result, ref userModel, dataList, dds.Mode, dev2Decision.Col1);
                    AddInputDebugItemResultsAfterEvaluate(result, ref userModel, dataList, dds.Mode, dev2Decision.Col2);
                    AddInputDebugItemResultsAfterEvaluate(result, ref userModel, dataList, dds.Mode, dev2Decision.Col3);
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
                Dev2Switch ds = new Dev2Switch { SwitchVariable = val };
                DebugItem itemToAdd = new DebugItem();
                ErrorResultTO errors;
                IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
                IBinaryDataListEntry expressionsEntry = compiler.Evaluate(dataList.UID, enActionType.User, ds.SwitchVariable, false, out errors);
                var debugResult = new DebugItemVariableParams(ds.SwitchVariable, "Switch on", expressionsEntry, dataList.UID);
                itemToAdd.AddRange(debugResult.GetDebugItemResult());
                result.Add(itemToAdd);
            }
            catch(Exception e)
            {
                DebugItem itemToAdd = new DebugItem();
                var debugItem = new DebugItemStaticDataParams(e.Message, "Error");
                itemToAdd.AddRange(debugItem.GetDebugItemResult());
                result.Add(itemToAdd);
            }

            return result;
        }

        void AddInputDebugItemResultsAfterEvaluate(List<DebugItem> result, ref string userModel, IBinaryDataList dataList, Dev2DecisionMode decisionMode, string expression, DebugItem parent = null)
        {
            if(expression != null && DataListUtil.IsEvaluated(expression))
            {
                var expressiomToStringValue = EvaluateExpressiomToStringValue(expression, decisionMode, dataList);

                userModel = userModel.Replace(expression, expressiomToStringValue);

                ErrorResultTO errors;
                IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
                IBinaryDataListEntry expressionsEntry = compiler.Evaluate(dataList.UID, enActionType.User, expression, false, out errors);

                var debugResult = new DebugItemVariableParams(expression, "", expressionsEntry, dataList.UID);
                var itemResults = debugResult.GetDebugItemResult();

                List<DebugItemResult> allReadyAdded = new List<DebugItemResult>();

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
            string val = Dev2DecisionStack.ExtractModelFromWorkflowPersistedData(ExpressionText);

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
            catch(Exception )
            {
                if (!dataList.HasErrors())
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
            if(dlEntry.IsRecordset)
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
                IBinaryDataListItem scalarItem = dlEntry.FetchScalar();
                result = scalarItem.TheValue;
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
