﻿/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Activities;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Dev2.Activities.Debug;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Data.Decisions.Operations;
using Dev2.Data.SystemTemplates.Models;
using Dev2.Data.TO;
using Dev2.Data.Util;
using Dev2.Diagnostics;
using Dev2.Interfaces;
using Newtonsoft.Json;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Storage;
using Warewolf.Storage.Interfaces;
using System.Activities.Statements;

namespace Dev2.Activities
{
    public class DsfDecision : DsfActivityAbstract<string>, IEquatable<DsfDecision>, IAdapterActivity
    {
        public IEnumerable<IDev2Activity> TrueArm { get; set; }

        public IEnumerable<IDev2Activity> FalseArm { get; set; }
        public Dev2DecisionStack Conditions { get; set; }

        readonly DsfFlowDecisionActivity _inner;

        public DsfDecision(DsfFlowDecisionActivity inner) : this()
        {
            _inner = inner;
            UniqueID = _inner.UniqueID;
        }
        public override string GetDisplayName() => Conditions.DisplayText;

        public override IEnumerable<IDev2Activity> GetNextNodes()
        {
            var nextNodes = new List<IDev2Activity>();
            if (TrueArm != null)
            {
                nextNodes.Add(TrueArm?.FirstOrDefault());
            }
            if (FalseArm != null)
            {
                nextNodes.Add(FalseArm?.FirstOrDefault());
            }
            return nextNodes;
        }

        public override List<(string Description, string Key, string SourceUniqueId, string DestinationUniqueId)> ArmConnectors()
        {
            var armConnectors = new List<(string Description, string Key, string SourceUniqueId, string DestinationUniqueId)>();
            if (TrueArm != null)
            {
                foreach (var next in TrueArm)
                {
                    armConnectors.Add(($"{GetDisplayName()}: TRUE -> {next.GetDisplayName()}", "True", UniqueID, next.UniqueID));
                }
            }
            else
            {
                armConnectors.Add(($"{GetDisplayName()}: TRUE -> ", "True", UniqueID, Guid.Empty.ToString()));
            }
            if (FalseArm != null)
            {
                foreach (var next in FalseArm)
                {
                    armConnectors.Add(($"{GetDisplayName()}: FALSE -> {next.GetDisplayName()}", "False", UniqueID, next.UniqueID));
                }
            }
            else
            {
                armConnectors.Add(($"{GetDisplayName()}: FALSE -> ", "False", UniqueID, Guid.Empty.ToString()));
            }
            return armConnectors;
        }

        public DsfDecision()
        : base("Decision") { }
        /// <summary>
        /// When overridden runs the activity's execution logic
        /// </summary>
        /// <param name="context">The context to be used.</param>
        protected override void OnExecute(NativeActivityContext context)
        {
        }

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates)
        {
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates)
        {
        }

        public override IList<DsfForEachItem> GetForEachInputs() => null;

        public override IList<DsfForEachItem> GetForEachOutputs() => null;

        Dev2Decision ParseDecision(IExecutionEnvironment env, Dev2Decision decision, bool errorIfNull)
        {
            var col1 = env.EvalAsList(decision.Col1, 0, errorIfNull);
            var col2 = env.EvalAsList(decision.Col2 ?? "", 0, errorIfNull);
            var col3 = env.EvalAsList(decision.Col3 ?? "", 0, errorIfNull);
            return new Dev2Decision { Cols1 = col1, Cols2 = col2, Cols3 = col3, EvaluationFn = decision.EvaluationFn };
        }

        IDev2Activity ExecuteDecision(IDSFDataObject dataObject)
        {
            InitializeDebug(dataObject);

            if (dataObject.IsDebugMode())
            {
                _debugInputs = CreateDebugInputs(dataObject.Environment);
                DispatchDebugState(dataObject, StateType.Before, 0);
            }

            var errorIfNull = !Conditions.TheStack.Any(decision => decision.EvaluationFn == enDecisionType.IsNull || decision.EvaluationFn == enDecisionType.IsNotNull);

            var stack = Conditions.TheStack.Select(a => ParseDecision(dataObject.Environment, a, errorIfNull));

            var factory = Dev2DecisionFactory.Instance();
            var res = stack.SelectMany(a =>
            {
                if (a.EvaluationFn == enDecisionType.IsError)
                {
                    return new[] { dataObject.Environment.AllErrors.Count > 0 };
                }
                if (a.EvaluationFn == enDecisionType.IsNotError)
                {
                    return new[] { dataObject.Environment.AllErrors.Count == 0 };
                }
                IList<bool> ret = new List<bool>();
                var iter = new WarewolfListIterator();
                var c1 = new WarewolfAtomIterator(a.Cols1);
                var c2 = new WarewolfAtomIterator(a.Cols2);
                var c3 = new WarewolfAtomIterator(a.Cols3);
                iter.AddVariableToIterateOn(c1);
                iter.AddVariableToIterateOn(c2);
                iter.AddVariableToIterateOn(c3);
                while (iter.HasMoreData())
                {
                    try
                    {
                        ret.Add(factory.FetchDecisionFunction(a.EvaluationFn).Invoke(new[] { iter.FetchNextValue(c1), iter.FetchNextValue(c2), iter.FetchNextValue(c3) }));
                    }
                    catch (Exception)
                    {
                        if (errorIfNull)
                        {
                            throw;
                        }
                        ret.Add(false);
                    }
                }
                return ret;
            });

            var results = res as IList<bool> ?? res.ToList();
            var resultval = true;
            if (results.Any())
            {
                if (And)
                {
                    if (results.Any(b => !b))
                    {
                        resultval = false;
                    }
                }
                else
                {
                    resultval = results.Any(b => b);
                }
            }

            Result = GetResultString(resultval.ToString(), Conditions);
            if (dataObject.IsDebugMode())
            {
                _debugOutputs = GetDebugOutputs(resultval.ToString());
            }
            if (resultval)
            {
                if (TrueArm != null)
                {
                    var activity = TrueArm.FirstOrDefault();
                    return activity;
                }
            }
            else
            {
                if (FalseArm != null)
                {
                    var activity = FalseArm.FirstOrDefault();
                    return activity;
                }
            }

            return null;
        }

        public override FlowNode GetFlowNode() => new FlowDecision(_inner);

        public IFlowNodeActivity GetInnerNode() => _inner;

        protected override void ExecuteTool(IDSFDataObject dataObject, int update)
        {
            var allErrors = new ErrorResultTO();
            try
            {
                var activity = ExecuteDecision(dataObject);
                NextNodes = new List<IDev2Activity> { activity };
            }
            catch (Exception e)
            {
                allErrors.AddError(e.Message);
            }
            finally
            {
                // Handle Errors
                var hasErrors = allErrors.HasErrors();
                if (hasErrors)
                {
                    DisplayAndWriteError("DsfDecision", allErrors);
                    var errorString = allErrors.MakeDisplayReady();
                    dataObject.Environment.AddError(errorString);
                }
                if (dataObject.IsDebugMode())
                {
                    DispatchDebugState(dataObject, StateType.After, update);
                    _debugOutputs = new List<DebugItem>();
                }
            }
        }

        public override List<DebugItem> GetDebugInputs(IExecutionEnvironment env, int update) => _debugInputs;

        List<DebugItem> CreateDebugInputs(IExecutionEnvironment env)
        {
            var result = new List<IDebugItem>();

            var allErrors = new ErrorResultTO();

            try
            {
                var dds = Conditions;
                var userModel = dds.GenerateUserFriendlyModel(env, dds.Mode, out ErrorResultTO error);
                allErrors.MergeErrors(error);

                foreach (Dev2Decision dev2Decision in dds.TheStack)
                {
                    AddInputDebugItemResultsAfterEvaluate(result, ref userModel, env, dev2Decision.Col1, out error);
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
            catch (JsonSerializationException e)
            {
                Dev2Logger.Warn(e.Message, "Warewolf Warn");
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

            var val = result.Select(a => a as DebugItem).ToList();
            _inner?.SetDebugInputs(val);
            return val;
        }

        public override List<DebugItem> GetDebugOutputs(IExecutionEnvironment env, int update) => _debugOutputs;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new string Result { get; set; }

        List<DebugItem> GetDebugOutputs(string theResult)
        {
            var result = new List<DebugItem>();
            var resultString = theResult;
            var itemToAdd = new DebugItem();
            var dds = Conditions;

            try
            {
                resultString = GetResultString(theResult, dds);

                itemToAdd.AddRange(new DebugItemStaticDataParams(resultString, "").GetDebugItemResult());
                result.Add(itemToAdd);
            }
            catch (Exception)
            {
                itemToAdd.AddRange(new DebugItemStaticDataParams(resultString, "").GetDebugItemResult());
                result.Add(itemToAdd);
            }

            _inner?.SetDebugOutputs(result);
            return result;
        }

        static string GetResultString(string theResult, Dev2DecisionStack dds)
        {
            var resultString = theResult;
            if (theResult == "True")
            {
                resultString = dds.TrueArmText;
            }
            else
            {
                if (theResult == "False")
                {
                    resultString = dds.FalseArmText;
                }
            }
            return resultString;
        }

        void AddInputDebugItemResultsAfterEvaluate(List<IDebugItem> result, ref string userModel, IExecutionEnvironment env, string expression, out ErrorResultTO error, DebugItem parent = null)
        {
            error = new ErrorResultTO();
            if (expression != null && DataListUtil.IsEvaluated(expression))
            {
                DebugOutputBase debugResult;
                if (error.HasErrors())
                {
                    debugResult = new DebugItemStaticDataParams("", expression, "");
                }
                else
                {
                    string expressiomToStringValue;
                    try
                    {
                        expressiomToStringValue = ExecutionEnvironment.WarewolfEvalResultToString(env.Eval(expression, 0));
                    }
                    catch (NullValueInVariableException)
                    {
                        expressiomToStringValue = "";
                    }
                    userModel = userModel.Replace(expression, expressiomToStringValue);
                    debugResult = new DebugItemWarewolfAtomResult(expressiomToStringValue, expression, "");
                }

                var itemResults = debugResult.GetDebugItemResult();

                var allReadyAdded = new List<IDebugItemResult>();

                itemResults.ForEach(a =>
                {
                    var found = result.SelectMany(r => r.FetchResultsList()).SingleOrDefault(r => r.Variable.Equals(a.Variable));
                    if (found != null)
                    {
                        allReadyAdded.Add(a);
                    }
                });

                allReadyAdded.ForEach(i => itemResults.Remove(i));

                if (parent == null)
                {
                    result.Add(new DebugItem(itemResults));
                }
                else
                {
                    parent.AddRange(itemResults);
                }
            }
        }

        public override List<string> GetOutputs() => new List<string>();

        public bool And { get; set; }

        public bool Equals(DsfDecision other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }
            var areConditionsEqual = CommonEqualityOps.AreObjectsEqual(Conditions, other.Conditions);
            if (!areConditionsEqual)
            {
                return false;
            }
            return string.Equals(DisplayName, other.DisplayName)
                && string.Equals(Result, other.Result)
                && And == other.And
                && Equals(UniqueID, other.UniqueID);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((DsfDecision)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (Conditions != null ? Conditions.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Result != null ? Result.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ And.GetHashCode();
                return hashCode;
            }
        }
    }

    public class TestMockDecisionStep : DsfActivityAbstract<string>
    {
        readonly DsfDecision _dsfDecision;

        public TestMockDecisionStep() : base("Mock Decision")
        {
        }

        public TestMockDecisionStep(DsfDecision dsfDecision)
            : base(dsfDecision.DisplayName)
        {
            _dsfDecision = dsfDecision;
            UniqueID = _dsfDecision.UniqueID;
        }

        public string NameOfArmToReturn { get; set; }

        protected override void OnExecute(NativeActivityContext context)
        {
        }

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates)
        {
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates)
        {
        }

        public override IList<DsfForEachItem> GetForEachInputs() => null;

        public override IList<DsfForEachItem> GetForEachOutputs() => null;

        protected override void ExecuteTool(IDSFDataObject dataObject, int update)
        {
            var trueArmText = _dsfDecision.Conditions.TrueArmText;
            var falseArmText = _dsfDecision.Conditions.FalseArmText;
            if (dataObject.IsDebugMode())
            {
                DispatchDebugState(dataObject, StateType.Before, 0, null, null, true);
            }
            var hasResult = false;
            if (NameOfArmToReturn == falseArmText)
            {
                NextNodes = _dsfDecision.FalseArm;
                if (dataObject.IsDebugMode())
                {
                    var debugItemStaticDataParams = new DebugItemStaticDataParams(falseArmText, "", true);
                    AddDebugOutputItem(debugItemStaticDataParams);
                    AddDebugAssertResultItem(debugItemStaticDataParams);
                }

                hasResult = true;
            }
            if (NameOfArmToReturn == trueArmText)
            {
                NextNodes = _dsfDecision.TrueArm;
                if (dataObject.IsDebugMode())
                {
                    var debugItemStaticDataParams = new DebugItemStaticDataParams(trueArmText, "", true);
                    AddDebugOutputItem(debugItemStaticDataParams);
                    AddDebugAssertResultItem(debugItemStaticDataParams);
                }
                hasResult = true;
            }
            if (dataObject.IsDebugMode() && hasResult)
            {
                DispatchDebugState(dataObject, StateType.After, update);
                DispatchDebugState(dataObject, StateType.Duration, update);
            }
            if (!hasResult)
            {
                throw new ArgumentException($"No matching arm for Decision Mock. Mock Arm value '{NameOfArmToReturn}'. Decision Arms True Arm: '{trueArmText}' False Arm: '{falseArmText}'");
            }
        }

        public override List<string> GetOutputs() => new List<string>();


        public bool Equals(TestMockDecisionStep other)
        {
            return ReferenceEquals(this, other);
        }
        public override bool Equals(object obj)
        {
            if (obj is TestMockDecisionStep instance)
            {
                return Equals(instance);
            }
            return false;
        }
    }
}
