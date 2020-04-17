/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Activities.Statements;
using System.Collections.Generic;
using Dev2.Common;
using Dev2.Data.Decisions.Operations;
using Dev2.Data.SystemTemplates.Models;
using Dev2.Interfaces;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Tools.Specs.BaseTypes;
using Warewolf.Storage.Interfaces;
using Dev2.Data.TO;
using Dev2.Common.Common;
using Warewolf.Storage;
using Dev2.Data.Util;
using Dev2.Data.Interfaces.Enums;
using System.Data;
using Dev2.Data.Decision;
using Warewolf.Resource.Errors;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Concurrent;
using Warewolf.Exceptions;

namespace Dev2.Activities.Specs.Toolbox.ControlFlow.Decision
{
    [Binding]
    public class DecisionSteps : RecordSetBases
    {
        readonly ScenarioContext scenarioContext;
        internal static readonly IDictionary<Guid, IExecutionEnvironment> _environments = new ConcurrentDictionary<Guid, IExecutionEnvironment>();

        public DecisionSteps(ScenarioContext scenarioContext)
            : base(scenarioContext)
        {
            this.scenarioContext = scenarioContext ?? throw new ArgumentNullException("scenarioContext");
        }

        protected override void BuildDataList()
        {
            BuildShapeAndTestData();
            var decisionActivity = new DsfFlowDecisionActivity();
            scenarioContext.TryGetValue("mode", out Dev2DecisionMode mode);

            var decisionModels =
                scenarioContext.Get<List<Tuple<string, enDecisionType, string, string>>>("decisionModels");
            var dds = new Dev2DecisionStack { TheStack = new List<Dev2Decision>(), Mode = mode, TrueArmText = "YES", FalseArmText = "NO" };

            foreach(var dm in decisionModels)
            {
                var dev2Decision = new Dev2Decision
                    {
                        Col1 = dm.Item1 ?? string.Empty,
                        EvaluationFn = dm.Item2,
                        Col2 = dm.Item3 ?? string.Empty,
                        Col3 = dm.Item4 ?? string.Empty
                    };

                dds.AddModelItem(dev2Decision);
            }

            var modelData = dds.ToVBPersistableModel();
            scenarioContext.Add("modelData", modelData);

            decisionActivity.ExpressionText = string.Join("", GlobalConstants.InjectedDecisionHandler, "(\"", modelData,
                                                          "\",", GlobalConstants.InjectedDecisionDataListVariable, ")");

            scenarioContext.TryGetValue("variableList", out List<Tuple<string, string>> variableList);

            if (variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                scenarioContext.Add("variableList", variableList);
            }


            var multiAssign = new DsfMultiAssignActivity();
            var row = 1;
            foreach (var variable in variableList)
            {
                multiAssign.FieldsCollection.Add(new ActivityDTO(variable.Item1, variable.Item2, row, true));
                row++;
            }
            var x = new FlowDecision();
            x.Condition=decisionActivity;
            TestStartNode = new FlowStep
                {
                    Action = multiAssign,
                    Next = x
                };
            scenarioContext.Add("activity", decisionActivity);
        }

        [Given(@"a decision variable ""(.*)"" value ""(.*)""")]
        public void GivenADecisionVariableValue(string variable, string value)
        {
            scenarioContext.TryGetValue("variableList", out List<Tuple<string, string>> variableList);

            if (variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                scenarioContext.Add("variableList", variableList);
            }

            variableList.Add(new Tuple<string, string>(variable, value));
        }

        [Given(@"Require all decisions to be true is ""(.*)""")]
        public void GivenRequireAllDecisionsToBeTrueIs(string p0)
        {
            if (p0 == "true")
            {
                scenarioContext.Add("mode", Dev2DecisionMode.AND);
            }
            else
            {
                scenarioContext.Add("mode", Dev2DecisionMode.OR);
            }
        }

        [Given(@"the decision mode is ""(.*)""")]
        public void GivenTheDecisionModeIs(string mode)
        {
            scenarioContext.Add("mode", (Dev2DecisionMode)Enum.Parse(typeof(Dev2DecisionMode), mode));
        }

        [Given(@"is ""(.*)"" ""(.*)"" ""(.*)""")]
        public void GivenIs(string variable1, string decision, string variable2)
        {
            scenarioContext.TryGetValue("decisionModels", out List<Tuple<string, enDecisionType, string, string>> decisionModels);

            if (decisionModels == null)
            {
                decisionModels = new List<Tuple<string, enDecisionType, string, string>>();
                scenarioContext.Add("decisionModels", decisionModels);
            }

            decisionModels.Add(
                new Tuple<string, enDecisionType, string, string>(
                    variable1, (enDecisionType)Enum.Parse(typeof(enDecisionType), decision), variable2, null
                    ));
        }

        [Given(@"I want to check ""(.*)""")]
        public void GivenIWantToCheck(string decision)
        {
            scenarioContext.TryGetValue("decisionModels", out List<Tuple<string, enDecisionType, string, string>> decisionModels);

            if (decisionModels == null)
            {
                decisionModels = new List<Tuple<string, enDecisionType, string, string>>();
                scenarioContext.Add("decisionModels", decisionModels);
            }

            decisionModels.Add(
                new Tuple<string, enDecisionType, string, string>(
                    null, (enDecisionType)Enum.Parse(typeof(enDecisionType), decision), null, null
                    ));
        }

        [Given(@"decide if ""(.*)"" ""(.*)""")]
        public void GivenDecideIf(string variable1, string decision)
        {
            scenarioContext.TryGetValue("decisionModels", out List<Tuple<string, enDecisionType, string, string>> decisionModels);

            if (decisionModels == null)
            {
                decisionModels = new List<Tuple<string, enDecisionType, string, string>>();
                scenarioContext.Add("decisionModels", decisionModels);
            }

            decisionModels.Add(
                new Tuple<string, enDecisionType, string, string>(
                    variable1, (enDecisionType)Enum.Parse(typeof(enDecisionType), decision), null, null
                    ));
        }

        [Given(@"check if ""(.*)"" ""(.*)"" ""(.*)"" and ""(.*)""")]
        public void GivenCheckIfAnd(string variable1, string decision, string variable2, string variable3)
        {
            scenarioContext.TryGetValue("decisionModels", out List<Tuple<string, enDecisionType, string, string>> decisionModels);

            if (decisionModels == null)
            {
                decisionModels = new List<Tuple<string, enDecisionType, string, string>>();
                scenarioContext.Add("decisionModels", decisionModels);
            }

            decisionModels.Add(
                new Tuple<string, enDecisionType, string, string>(
                    variable1, (enDecisionType)Enum.Parse(typeof(enDecisionType), decision), variable2, variable3
                    ));
        }

        [When(@"the decision tool is executed")]
        public void WhenTheDecisionToolIsExecuted()
        {
            BuildDataList();
            var result = ExecuteProcess(isDebug: true, throwException: false);
            scenarioContext.Add("result", result);
        }

        [Then(@"the decision result should be ""(.*)""")]
        public void ThenTheDecisionResultShouldBe(string expectedRes)
        {
            var modelData = scenarioContext.Get<string>("modelData");
            var result = scenarioContext.Get<IDSFDataObject>("result");
            if (result.DataListID == Guid.Empty)
            {
                result.DataListID = Guid.NewGuid();
            }
            Dev2DataListDecisionHandler.Instance.AddEnvironment(result.DataListID, result.Environment);

            var actual = ExecuteDecisionStack(modelData, new List<string>
            {
                result.DataListID.ToString()
            }, 0);
            var expected = Boolean.Parse(expectedRes);
            Assert.AreEqual(expected, actual);
        }

        public bool ExecuteDecisionStack(string decisionDataPayload, IList<string> oldAmbientData, int update)
        {
            var dlId = FetchDataListID(oldAmbientData);
            var newDecisionData = Dev2DecisionStack.FromVBPersitableModelToJSON(decisionDataPayload);
            var dds = EvaluateRegion(newDecisionData, dlId, update);

            var env = Dev2DataListDecisionHandler._environments[dlId];
            if (dds != null)
            {
                if (dlId != GlobalConstants.NullDataListID)
                {
                    try
                    {
                        if (dds.TheStack != null)
                        {

                            for (int i = 0; i < dds.TotalDecisions; i++)
                            {
                                var dd = dds.GetModelItem(i);
                                var typeOf = dd.EvaluationFn;

                                // Treat Errors special
                                if (typeOf == enDecisionType.IsError || typeOf == enDecisionType.IsNotError)
                                {
                                    dd.Col1 = env.FetchErrors();
                                }

                                var op = Dev2DecisionFactory.Instance().FetchDecisionFunction(typeOf);
                                if (op != null)
                                {
                                    try
                                    {
                                        var result = op.Invoke(dds.GetModelItem(i).FetchColsAsArray());

                                        if (!result && dds.Mode == Dev2DecisionMode.AND)
                                        {
                                            // Naughty stuff, we have a false in AND mode... break
                                            return false;
                                        }

                                        if (result && dds.Mode == Dev2DecisionMode.OR)
                                        {
                                            return true;
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        // An error, push into the DL
                                        env.AddError(e.Message);

                                        return false;
                                    }
                                }
                                else
                                {
                                    throw new InvalidExpressionException(string.Format(ErrorResource.CouldNotEvaluateDecisionData, typeOf));
                                }
                            }

                            // else we are in AND mode and all have passed ;)
                            if (dds.Mode == Dev2DecisionMode.AND)
                            {
                                return true;
                            }

                            //finally, it must be OR mode with no matches ;(
                            return false;
                        }

                        throw new InvalidExpressionException(ErrorResource.InvalidModelDataSent);
                    }
                    catch
                    {
                        // all hell has broken loose... ;)
                        throw new InvalidExpressionException(ErrorResource.NoModelDataSent);
                    }
                }

                throw new InvalidExpressionException(ErrorResource.NoDataListIDsent);
            }

            throw new InvalidExpressionException(ErrorResource.DataListErrors);
        }

        Dev2DecisionStack EvaluateRegion(string payload, Guid dlId, int update)
        {

            var env = Dev2DataListDecisionHandler._environments[dlId];

            if (payload.StartsWith("{\"TheStack\":[{") || payload.StartsWith("{'TheStack':[{"))
            {
                var dds = DataListUtil.ConvertFromJsonToModel<Dev2DecisionStack>(new StringBuilder(payload));

                if (dds.TheStack != null)
                {
                    var effectedCols = new[] { false, false, false };
                    //Find decisions that mention record sets with starred indexes
                    var invalidDecisions = new List<Dev2Decision>();
                    for (int i = 0; i < dds.TotalDecisions; i++)
                    {
                        var dd = dds.GetModelItem(i);

                        if (dd.Col1 != null && DataListUtil.GetRecordsetIndexType(dd.Col1) == enRecordsetIndexType.Star)
                        {
                            invalidDecisions.Add(dd);
                            effectedCols[0] = true;
                        }
                        else
                        {
                            var warewolfEvalResult = GetWarewolfEvalResult(env, dd.Col1, update);
                            dd.Col1 = ExecutionEnvironment.WarewolfEvalResultToString(warewolfEvalResult);
                        }

                        if (dd.Col2 != null && DataListUtil.GetRecordsetIndexType(dd.Col2) == enRecordsetIndexType.Star)
                        {
                            if (!effectedCols[0])
                            {
                                invalidDecisions.Add(dd);
                            }
                            effectedCols[1] = true;
                        }
                        else
                        {
                            var warewolfEvalResult = GetWarewolfEvalResult(env, dd.Col2, update);
                            dd.Col2 = ExecutionEnvironment.WarewolfEvalResultToString(warewolfEvalResult);
                        }

                        if (dd.Col3 != null && DataListUtil.GetRecordsetIndexType(dd.Col3) == enRecordsetIndexType.Star)
                        {
                            if (!effectedCols[0] && !effectedCols[1])
                            {
                                invalidDecisions.Add(dd);
                            }
                            effectedCols[2] = true;
                        }
                        else
                        {
                            var warewolfEvalResult = GetWarewolfEvalResult(env, dd.Col3, update);
                            dd.Col3 = ExecutionEnvironment.WarewolfEvalResultToString(warewolfEvalResult);
                        }
                    }
                    //Remove those record sets and replace them with a new decision for each resolved value
                    foreach (Dev2Decision decision in invalidDecisions)
                    {
                        dds = ResolveAllRecords(env, dds, decision, effectedCols, out ErrorResultTO errors, update);
                    }
                }

                return dds;
            }
            return null;
        }

        static CommonFunctions.WarewolfEvalResult GetWarewolfEvalResult(IExecutionEnvironment env, string col, int update)
        {
            var warewolfEvalResult = CommonFunctions.WarewolfEvalResult.NewWarewolfAtomResult(DataStorage.WarewolfAtom.Nothing);
            try
            {
                warewolfEvalResult = env.Eval(col, update);
            }
            catch (NullValueInVariableException)
            {
                //This is allow for decisions.
            }
            catch (Exception err)
            {
                env.Errors.Add(err.Message);
            }

            return warewolfEvalResult;
        }

        string FetchStackValue(Dev2DecisionStack stack, int stackIndex, int columnIndex)
        {
            // if out of bounds return an empty string ;)
            if (stackIndex >= stack.TheStack.Count)
            {
                return string.Empty;
            }

            if (columnIndex == 1)
            {
                return stack.TheStack[stackIndex].Col1;
            }
            if (columnIndex == 2)
            {
                return stack.TheStack[stackIndex].Col2;
            }

            return string.Empty;
        }

        Dev2DecisionStack ResolveAllRecords(IExecutionEnvironment env, Dev2DecisionStack stack, Dev2Decision decision, bool[] effectedCols, out ErrorResultTO errors, int update)
        {
            if (effectedCols == null)
            {
                throw new ArgumentNullException("effectedCols");
            }
            var stackIndex = stack.TheStack.IndexOf(decision);
            stack.TheStack.Remove(decision);
            errors = new ErrorResultTO();
            if (effectedCols[0])
            {
                var data = env.EvalAsListOfStrings(decision.Col1, update);

                var reStackIndex = stackIndex;

                foreach (var item in data)
                {


                    var newDecision = new Dev2Decision { Col1 = item, Col2 = decision.Col2, Col3 = decision.Col3, EvaluationFn = decision.EvaluationFn };
                    stack.TheStack.Insert(reStackIndex++, newDecision);
                }
            }
            if (effectedCols[1])
            {
                var data = env.EvalAsListOfStrings(decision.Col2, update);
                var reStackIndex = stackIndex;

                foreach (var item in data)
                {
                    var newDecision = new Dev2Decision { Col1 = FetchStackValue(stack, reStackIndex, 1), Col2 = item, Col3 = decision.Col3, EvaluationFn = decision.EvaluationFn };
                    if (effectedCols[0])
                    {
                        // ensure we have the correct indexing ;)
                        if (reStackIndex < stack.TheStack.Count)
                        {
                            stack.TheStack[reStackIndex++] = newDecision;
                        }
                        else
                        {
                            stack.TheStack.Insert(reStackIndex++, newDecision);
                        }
                    }
                    else
                    {
                        stack.TheStack.Insert(reStackIndex++, newDecision);
                    }
                }
            }
            if (effectedCols[2])
            {
                var data = env.EvalAsListOfStrings(decision.Col3, update);
                var reStackIndex = stackIndex;

                foreach (var item in data)
                {
                    var newDecision = new Dev2Decision { Col1 = FetchStackValue(stack, reStackIndex, 1), Col2 = FetchStackValue(stack, reStackIndex, 2), Col3 = item, EvaluationFn = decision.EvaluationFn };
                    if (effectedCols[0] || effectedCols[1])
                    {
                        // ensure we have the correct indexing ;)
                        if (reStackIndex < stack.TheStack.Count)
                        {
                            stack.TheStack[reStackIndex++] = newDecision;
                        }
                        else
                        {
                            stack.TheStack.Insert(reStackIndex++, newDecision);
                        }
                    }
                    else
                    {
                        stack.TheStack.Insert(reStackIndex++, newDecision);
                    }
                }
            }
            return stack;
        }

        public Guid FetchDataListID(IList<string> oldAmbientData)
        {
            var dlID = GlobalConstants.NullDataListID;
            if (oldAmbientData != null && oldAmbientData.Count == 1)
            {
                Guid.TryParse(oldAmbientData[0], out dlID);
            }

            return dlID;
        }
    }
}
