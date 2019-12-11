/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2;
using Dev2.Activities;
using Dev2.Common;
using Dev2.Data.Decisions.Operations;
using Dev2.Data.SystemTemplates.Models;
using Dev2.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using TechTalk.SpecFlow;
using Warewolf.Data;
using Warewolf.Data.Options;
using Warewolf.Data.Options.Enums;
using Warewolf.Tools.Specs.BaseTypes;

namespace Warewolf.Tools.Specs.Toolbox.ControlFlow.Gate
{
    [Binding]
    public class GateSteps : RecordSetBases
    {
        readonly ScenarioContext scenarioContext;
        readonly Guid RetryEntryPointId = Guid.NewGuid();
        readonly Guid ResumeEndpoint = Guid.NewGuid();

        public GateSteps(ScenarioContext scenarioContext)
            : base(scenarioContext)
        {
            this.scenarioContext = scenarioContext ?? throw new ArgumentNullException("scenarioContext");
            scenarioContext.Add("activity", CreateActivity());
            scenarioContext.Add("nextActivity", CreateActivity());
        }

        private void BuildActivity()
        {
            BuildDataList();

            scenarioContext.TryGetValue("activity", out GateActivity gateActivity);

            gateActivity.DisplayName = "Gate 1";
            gateActivity.UniqueID = RetryEntryPointId.ToString();

            TestStartNode = new FlowStep
            {
                Action = gateActivity
            };
        }

        private void BuildNextActivity()
        {
            BuildDataList();

            scenarioContext.TryGetValue("activity", out GateActivity gateActivity);
            scenarioContext.TryGetValue("nextActivity", out GateActivity nextGateActivity);

            gateActivity.DisplayName = "Gate 1";
            gateActivity.UniqueID = RetryEntryPointId.ToString();

            scenarioContext.TryGetValue("mode", out Dev2DecisionMode mode);

            var decisionModels = scenarioContext.Get<List<Tuple<string, enDecisionType, string, string>>>("decisionModels");
            var dds = new Dev2DecisionStack { TheStack = new List<Dev2Decision>(), Mode = mode, TrueArmText = "YES", FalseArmText = "NO" };

            foreach (var dm in decisionModels)
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

            scenarioContext.TryGetValue("modelData", out var modelData);

            nextGateActivity.UniqueID = Guid.NewGuid().ToString();
            nextGateActivity.DisplayName = "Gate 2";

            gateActivity.NextNodes = new List<IDev2Activity> { nextGateActivity };
            nextGateActivity.NextNodes = new List<IDev2Activity> { nextGateActivity };

            nextGateActivity.ExpressionText = string.Join("", GlobalConstants.InjectedDecisionHandler, "(\"", modelData,
                                                          "\",", GlobalConstants.InjectedDecisionDataListVariable, ")");

            var gateTwoFlowStep = new FlowStep
            {
                Action = nextGateActivity
            };

            TestStartNode = new FlowStep
            {
                Action = gateActivity,
                Next = gateTwoFlowStep
            };
        }
       
        protected override void BuildDataList()
        {
            BuildShapeAndTestData();

            scenarioContext.TryGetValue("decisionModels", out List<Tuple<string, enDecisionType, string, string>> decisionModels);

            if (decisionModels == null)
            {
                decisionModels = new List<Tuple<string, enDecisionType, string, string>>();
                scenarioContext.Add("decisionModels", decisionModels);
            }

            var variable1 = "[[a]]";
            var decision = enDecisionType.IsEqual.ToString();
            var variable2 = "bob";

            decisionModels.Add(
                new Tuple<string, enDecisionType, string, string>(
                    variable1, (enDecisionType)Enum.Parse(typeof(enDecisionType), decision), variable2, null
                    ));

            scenarioContext.TryGetValue("variableList", out List<Tuple<string, string>> variableList);

            if (variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                scenarioContext.Add("variableList", variableList);
            }

            variableList.Add(new Tuple<string, string>(ResultVariable, ""));
        }

        private GateActivity CreateActivity()
        {
            var noBackoffGateActivity = new GateActivity
            {
                GateFailure = GateFailureAction.StopOnError.ToString()
            };

            CreateGateOptions(noBackoffGateActivity);

            return noBackoffGateActivity;
        }

        private void CreateGateOptions(GateActivity noBackoffGateActivity)
        {
            var noBackoffAlgorithm = new NoBackoff();

            var expectedWorkflow = new WorkflowWithInputs
            {
                Name = "WorkflowName",
                Value = ResumeEndpoint,
                Inputs = new List<IServiceInputBase>()
            };

            var gateOptions = new GateOptions
            {
                Resume = Resumable.AllowResumption,
                //ResumeEndpoint = ResumeEndpoint,
                ResumeEndpoint = expectedWorkflow,
                Count = 3,
                Strategy = noBackoffAlgorithm
            };

            noBackoffGateActivity.GateOptions = gateOptions;
        }

        [Given(@"I have the following conditions")]
        public void GivenIHaveTheFollowingConditions(Table table)
        {
            FillConditions(table);
        }

        void FillConditions(Table table)
        {
            var tableRows = table.Rows.ToList();

            foreach (TableRow tableRow in tableRows)
            {
                scenarioContext.TryGetValue("variableList", out List<Tuple<string, string>> variableList);

                if (variableList == null)
                {
                    variableList = new List<Tuple<string, string>>();
                    scenarioContext.Add("variableList", variableList);
                }
                variableList.Add(new Tuple<string, string>(tableRow[0], tableRow[1]));
            }

            scenarioContext.TryGetValue("activity", out GateActivity gateActivity);
            scenarioContext.TryGetValue("nextActivity", out GateActivity nextGateActivity);
            scenarioContext.TryGetValue("mode", out Dev2DecisionMode mode);
            var dds = new Dev2DecisionStack { TheStack = new List<Dev2Decision>(), Mode = mode, TrueArmText = "YES", FalseArmText = "NO" };

            foreach (var dm in tableRows)
            {
                var evalFn = enDecisionType.IsEqual;
                if (Enum.TryParse<enDecisionType>(dm[1], out var tmpEvalFn))
                {
                    evalFn = tmpEvalFn;
                }

                var dev2Decision = new Dev2Decision
                {
                    Col1 = dm[0] ?? string.Empty,
                    EvaluationFn = evalFn,
                    Col2 = dm[2] ?? string.Empty,
                    Col3 = string.Empty
                };

                dds.AddModelItem(dev2Decision);
            }

            var modelData = dds.ToVBPersistableModel();
            scenarioContext.Add("modelData", modelData);

            gateActivity.ExpressionText = string.Join("", GlobalConstants.InjectedDecisionHandler, "(\"", modelData,
                                                          "\",", GlobalConstants.InjectedDecisionDataListVariable, ")");

            nextGateActivity.ExpressionText = string.Join("", GlobalConstants.InjectedDecisionHandler, "(\"", modelData,
                                                          "\",", GlobalConstants.InjectedDecisionDataListVariable, ")");
        }

        [Given(@"GateFailure has ""(.*)"" selected")]
        public void GivenGateFailureHasSelected(string gateFailure)
        {
            scenarioContext.TryGetValue("activity", out GateActivity gateActivity);
            gateActivity.GateFailure = gateFailure;
        }

        [Given(@"next gate GateFailure has ""(.*)"" selected")]
        public void GivenNextGateGateFailureHasSelected(string gateFailure)
        {
            scenarioContext.TryGetValue("nextActivity", out GateActivity gateActivity);
            gateActivity.GateFailure = gateFailure;
        }

        [Given(@"Gates has ""(.*)"" selected")]
        public void GivenGatesHasSelected(string retryEntryPointId)
        {
            scenarioContext.TryGetValue("activity", out GateActivity gateActivity);
            gateActivity.RetryEntryPointId = string.IsNullOrEmpty(retryEntryPointId) ? Guid.Empty : RetryEntryPointId;
        }

        [Given(@"next gate Gates has ""(.*)"" selected")]
        public void GivenNextGateGatesHasSelected(string retryEntryPointId)
        {
            scenarioContext.TryGetValue("nextActivity", out GateActivity gateActivity);
            gateActivity.RetryEntryPointId = string.IsNullOrEmpty(retryEntryPointId) ? Guid.Empty : RetryEntryPointId;
        }

        [Given(@"GateRetryStrategy has ""(.*)"" selected")]
        public void GivenGateRetryStrategyHasSelected(string retryStrategy)
        {
            scenarioContext.TryGetValue("activity", out GateActivity gateActivity);
            SetGateStrategy(retryStrategy, gateActivity);
        }

        [Given(@"next gate GateRetryStrategy has ""(.*)"" selected")]
        public void GivenNextGateGateRetryStrategyHasSelected(string retryStrategy)
        {
            scenarioContext.TryGetValue("nextActivity", out GateActivity gateActivity);
            SetGateStrategy(retryStrategy, gateActivity);
        }

        private static void SetGateStrategy(string retryStrategy, GateActivity gateActivity)
        {
            switch (retryStrategy)
            {
                case "NoBackoff":
                    gateActivity.GateOptions.Strategy = new NoBackoff();
                    break;
                case "ConstantBackoff":
                    gateActivity.GateOptions.Strategy = new ConstantBackoff();
                    break;
                case "LinearBackoff":
                    gateActivity.GateOptions.Strategy = new LinearBackoff();
                    break;
                case "FibonacciBackoff":
                    gateActivity.GateOptions.Strategy = new FibonacciBackoff();
                    break;
                case "QuadraticBackoff":
                    gateActivity.GateOptions.Strategy = new QuadraticBackoff();
                    break;
            }
        }

        [Given(@"Increment is set to ""(.*)""")]
        public void GivenIncrementIsSetTo(int increment)
        {
            scenarioContext.TryGetValue("activity", out GateActivity gateActivity);
            SetIncrement(increment, gateActivity);
        }

        [Given(@"Linear Increment is set to ""(.*)""")]
        public void GivenLinearIncrementIsSetTo(int increment)
        {
            scenarioContext.TryGetValue("activity", out GateActivity gateActivity);
            SetIncrement(increment, gateActivity);
        }

        [Given(@"next gate Linear Increment is set to ""(.*)""")]
        public void GivenNextGateLinearIncrementIsSetTo(int increment)
        {
            scenarioContext.TryGetValue("nextActivity", out GateActivity gateActivity);
            SetIncrement(increment, gateActivity);
        }

        private static void SetIncrement(int increment, GateActivity gateActivity)
        {
            if (gateActivity.GateOptions.Strategy is LinearBackoff linearBackoff)
            {
                linearBackoff.Increment = increment;
            }
            if (gateActivity.GateOptions.Strategy is ConstantBackoff constantBackoff)
            {
                constantBackoff.Increment = increment;
            }
        }

        [Given(@"Linear Timeout is set to ""(.*)""")]
        public void GivenLinearTimeoutIsSetTo(int timeOut)
        {
            scenarioContext.TryGetValue("activity", out GateActivity gateActivity);
            SetTimeout(timeOut, gateActivity);
        }

        [Given(@"next gate Linear Timeout is set to ""(.*)""")]
        public void GivenNextGateLinearTimeoutIsSetTo(int timeOut)
        {
            scenarioContext.TryGetValue("nextActivity", out GateActivity gateActivity);
            SetTimeout(timeOut, gateActivity);
        }

        private static void SetTimeout(int timeOut, GateActivity gateActivity)
        {
            if (gateActivity.GateOptions.Strategy is LinearBackoff linearBackoff)
            {
                linearBackoff.TimeOut = timeOut;
            }
        }

        [Given(@"Linear Max Retries is set to ""(.*)""")]
        public void GivenLinearMaxRetriesIsSetTo(int retries)
        {
            scenarioContext.TryGetValue("activity", out GateActivity gateActivity);
            SetMaxRetries(retries, gateActivity);
        }

        [Given(@"next gate Linear Max Retries is set to ""(.*)""")]
        public void GivenNextGateLinearMaxRetriesIsSetTo(int retries)
        {
            scenarioContext.TryGetValue("nextActivity", out GateActivity gateActivity);
            SetMaxRetries(retries, gateActivity);
        }

        private static void SetMaxRetries(int retries, GateActivity gateActivity)
        {
            if (gateActivity.GateOptions.Strategy is LinearBackoff linearBackoff)
            {
                linearBackoff.MaxRetries = retries;
            }
        }

        [Given(@"Fibonacci Timeout is set to ""(.*)""")]
        public void GivenFibonacciTimeoutIsSetTo(int timeOut)
        {
            scenarioContext.TryGetValue("activity", out GateActivity gateActivity);
            if (gateActivity.GateOptions.Strategy is FibonacciBackoff fibonacciBackoff)
            {
                fibonacciBackoff.TimeOut = timeOut;
            }
        }
        [Given(@"Fibonacci Max Retries is set to ""(.*)""")]
        public void GivenFibonacciMaxRetriesIsSetTo(int retries)
        {
            scenarioContext.TryGetValue("activity", out GateActivity gateActivity);
            if (gateActivity.GateOptions.Strategy is FibonacciBackoff fibonacciBackoff)
            {
                fibonacciBackoff.MaxRetries = retries;
            }
        }
        [Given(@"Quadratic Timeout is set to ""(.*)""")]
        public void GivenQuadraticTimeoutIsSetTo(int timeOut)
        {
            scenarioContext.TryGetValue("activity", out GateActivity gateActivity);
            if (gateActivity.GateOptions.Strategy is QuadraticBackoff quadraticBackoff)
            {
                quadraticBackoff.TimeOut = timeOut;
            }
        }
        [Given(@"Quadratic Max Retries is set to ""(.*)""")]
        public void GivenQuadraticMaxRetriesIsSetTo(int retries)
        {
            scenarioContext.TryGetValue("activity", out GateActivity gateActivity);
            if (gateActivity.GateOptions.Strategy is QuadraticBackoff quadraticBackoff)
            {
                quadraticBackoff.MaxRetries = retries;
            }
        }

        [Given(@"Resume is set to ""(.*)""")]
        public void GivenResumeIsSetTo(string resume)
        {
            scenarioContext.TryGetValue("activity", out GateActivity gateActivity);
            gateActivity.GateOptions.Resume = resume == "Yes" ? Resumable.AllowResumption : Resumable.ResumptionDisabled;
        }

        [Given(@"next gate Resume is set to ""(.*)""")]
        public void GivenNextGateResumeIsSetTo(string resume)
        {
            scenarioContext.TryGetValue("nextActivity", out GateActivity gateActivity);
            gateActivity.GateOptions.Resume = resume == "Yes" ? Resumable.AllowResumption : Resumable.ResumptionDisabled;
        }


        [Given(@"ResumeEndpoint is set to ""(.*)""")]
        public void GivenResumeEndpointIsSetTo(string resumptionWorkflow)
        {
            scenarioContext.TryGetValue("activity", out GateActivity gateActivity);
            var expectedWorkflow = new WorkflowWithInputs
            {
                Name = "WorkflowName",
                Value = Guid.Parse(resumptionWorkflow),
                Inputs = new List<IServiceInputBase>()
            };

            gateActivity.GateOptions.ResumeEndpoint = expectedWorkflow;
        }

        [Given(@"the Gate tool is executed")]
        public void GivenTheGateToolIsExecuted()
        {
            BuildActivity();
            var result = ExecuteProcess(isDebug: true, throwException: false);
            scenarioContext.Add("result", result);
        }

        [Given(@"the Gate tool is executed with next gate")]
        public void GivenTheGateToolIsExecutedWithNextGate()
        {
            BuildNextActivity();
            var result = ExecuteProcess(isDebug: true, throwException: false);
            scenarioContext.Add("result", result);
        }

        [Given(@"the Gate tool is executed with resumption endpoint")]
        public void GivenTheGateToolIsExecutedWithResumptionEndpoint()
        {
            BuildDataList();
            var result = ExecuteProcess(isDebug: true, throwException: false);
            scenarioContext.Add("result", result);
        }

        [Then(@"the execution has no errors")]
        public void ThenTheExecutionHasNoErrors()
        {
            var result = scenarioContext.Get<IDSFDataObject>("result");

            Assert.AreEqual(0, result.Environment.Errors.Count);
            Assert.AreEqual(0, result.Environment.AllErrors.Count);
        }

        [Then(@"the execution has errors")]
        public void ThenTheExecutionHasErrors(Table table)
        {
            var result = scenarioContext.Get<IDSFDataObject>("result");
            var fetchErrors = result.Environment.FetchErrors();
            var tableRows = table.Rows.ToList();

            Assert.AreEqual(tableRows.Count, result.Environment.Errors.Count);

            foreach (TableRow tableRow in tableRows)
            {
                var expectedError = tableRow["error"];
                var error = fetchErrors;

                Assert.AreEqual(expectedError, error);
            }
        }
    }
}
