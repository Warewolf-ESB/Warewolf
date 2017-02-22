/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Data.Decision;
using Dev2.Data.SystemTemplates.Models;
using Dev2.Data.TO;
using Dev2.Data.Util;
using Dev2.Diagnostics;
using Dev2.Diagnostics.Debug;
using Dev2.Interfaces;
using Microsoft.CSharp.Activities;
using Newtonsoft.Json;
using Warewolf.Storage;
// ReSharper disable NonReadonlyMemberInGetHashCode

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
            _dataListId = _dataObject.DataListID;
            InitializeDebug(_dataObject);

            if(_dataObject.IsDebugMode())
            {
                DispatchDebugState(_dataObject, StateType.Before, 0);
            }

       
            Dev2DataListDecisionHandler.Instance.AddEnvironment(_dataListId, _dataObject.Environment);
            context.ScheduleActivity(_expression, OnCompleted, OnFaulted);
        }

        #endregion

        #region OnCompleted

        void OnCompleted(NativeActivityContext context, ActivityInstance completedInstance, TResult result)
        {
            try
            {



                IDSFDataObject dataObject = context.GetExtension<IDSFDataObject>();
                Result.Set(context, result);
                _theResult = result;

                if (dataObject != null && dataObject.IsDebugMode())
                {
                    DispatchDebugState(dataObject, StateType.After, 0);
                }

                OnExecutedCompleted(context);
            }
            finally
            {

                Dev2DataListDecisionHandler.Instance.RemoveEnvironment(_dataListId);
            }
        }

        #endregion

        #region OnFaulted

        void OnFaulted(NativeActivityFaultContext faultContext, Exception propagatedException, ActivityInstance propagatedFrom)
        {
            IDSFDataObject dataObject = faultContext.GetExtension<IDSFDataObject>();
            dataObject.Environment.AddError(propagatedException.Message);
            if(dataObject.IsDebugMode())
            {
                DispatchDebugState(dataObject, StateType.After, 0);
            }
            OnExecutedCompleted(faultContext);
        }

        #endregion

        #region Get Debug Inputs/Outputs

        // Travis.Frisinger - 28.01.2013 : Amended for Debug
        public override List<DebugItem> GetDebugInputs(IExecutionEnvironment env, int update)
        {
            if (_debugInputs != null && _debugInputs.Count > 0)
            {
                return _debugInputs;
            }
            List<IDebugItem> result = new List<IDebugItem>();
            var allErrors = new ErrorResultTO();

            var val = new StringBuilder(Dev2DecisionStack.ExtractModelFromWorkflowPersistedData(ExpressionText));

            try
            {
                Dev2DecisionStack dds = DataListUtil.ConvertFromJsonToModel<Dev2DecisionStack>(val);
                ErrorResultTO error;
                string userModel = dds.GenerateUserFriendlyModel(env, dds.Mode, out error);
                allErrors.MergeErrors(error);

                foreach (Dev2Decision dev2Decision in dds.TheStack)
                {
                    AddInputDebugItemResultsAfterEvaluate(result, ref userModel, env, dev2Decision.Col1, out  error);
                    allErrors.MergeErrors(error);
                    AddInputDebugItemResultsAfterEvaluate(result, ref userModel, env, dev2Decision.Col2, out error);
                    allErrors.MergeErrors(error);
                    AddInputDebugItemResultsAfterEvaluate(result, ref userModel, env, dev2Decision.Col3, out error);
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
                AddDebugItem(new DebugItemStaticDataParams(dds.Mode == Dev2DecisionMode.AND ? "YES" : "NO", "Require all decisions to be true"), itemToAdd);
                result.Add(itemToAdd);

            }
            catch (JsonSerializationException)
            {
                Dev2Switch ds = new Dev2Switch { SwitchVariable = val.ToString() };
                DebugItem itemToAdd = new DebugItem();

                var a = env.Eval(ds.SwitchVariable, 0);
                var debugResult = new DebugItemWarewolfAtomResult(ExecutionEnvironment.WarewolfEvalResultToString(a), "", ds.SwitchVariable, "", "Switch on", "", "=");
                itemToAdd.AddRange(debugResult.GetDebugItemResult());
                result.Add(itemToAdd);
            }
            catch (Exception e)
            {
                allErrors.AddError(e.Message);
            }
            finally
            {
                if (allErrors.HasErrors())
                {
                    var serviceName = GetType().Name;
                    DisplayAndWriteError(serviceName, allErrors);
                }
            }

            return result.Select(a => a as DebugItem).ToList();
        }

        void AddInputDebugItemResultsAfterEvaluate(List<IDebugItem> result, ref string userModel, IExecutionEnvironment env, string expression, out ErrorResultTO error, DebugItem parent = null)
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
                    var expressiomToStringValue = ExecutionEnvironment.WarewolfEvalResultToString(env.Eval(expression, 0));// EvaluateExpressiomToStringValue(expression, decisionMode, dataList);
                    userModel = userModel.Replace(expression, expressiomToStringValue);
                    debugResult = new DebugItemWarewolfAtomResult(expressiomToStringValue, expression, "");
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
        public override List<DebugItem> GetDebugOutputs(IExecutionEnvironment dataList, int update)
        {
            if (_debugOutputs != null && _debugOutputs.Count > 0)
            {
                return _debugOutputs;
            }
            var result = new List<DebugItem>();
            string resultString = _theResult.ToString();
            DebugItem itemToAdd = new DebugItem();
            var val = new StringBuilder(Dev2DecisionStack.ExtractModelFromWorkflowPersistedData(ExpressionText));

            try
            {
                Dev2DecisionStack dds = DataListUtil.ConvertFromJsonToModel<Dev2DecisionStack>(val);

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
                // ReSharper disable EmptyGeneralCatchClause
            catch(Exception)
                // ReSharper restore EmptyGeneralCatchClause
            {

                    itemToAdd.AddRange(new DebugItemStaticDataParams(resultString, "").GetDebugItemResult());
                    result.Add(itemToAdd);
                
            }

            return result;
        }

        #endregion

        #region Private Debug Methods

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

        #region Overrides of DsfNativeActivity<TResult>

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// true if the specified object  is equal to the current object; otherwise, false.
        /// </returns>
        /// <param name="obj">The object to compare with the current object. </param>
        public override bool Equals(object obj)
        {
            var act = obj as IDev2Activity;
            if (obj is IFlowNodeActivity)
            {
                var flowNodeAct = this as IFlowNodeActivity;
                var other = act as IFlowNodeActivity;
                if (other != null)
                {
                    return UniqueID == act.UniqueID && flowNodeAct.ExpressionText.Equals(other.ExpressionText);
                }
            }
            return base.Equals(obj);
        }

        #region Overrides of Object

        /// <summary>
        /// Serves as a hash function for a particular type. 
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        public override int GetHashCode()
        {
   


                return UniqueID.GetHashCode();



        }

        #endregion

        #endregion
    }
}
