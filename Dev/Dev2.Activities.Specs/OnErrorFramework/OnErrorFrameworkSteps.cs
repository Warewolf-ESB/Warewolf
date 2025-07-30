using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Activities.Specs.BaseTypes;
using Dev2.Interfaces;
using Dev2.Common.Interfaces;
using System.Activities;

namespace Dev2.Activities.Specs.OnErrorFramework
{
    [Binding]
    public class OnErrorFrameworkSteps : RecordSetBases
    {
        readonly ScenarioContext _scenarioContext;
        readonly CommonSteps _commonSteps;

        public OnErrorFrameworkSteps(ScenarioContext scenarioContext) : base(scenarioContext)
        {
            _scenarioContext = scenarioContext ?? throw new ArgumentNullException(nameof(scenarioContext));
            _commonSteps = new CommonSteps(_scenarioContext);
        }

        [Given(@"""(.*)"" has OnErrorVariable ""(.*)""")]
        public void GivenHasOnErrorVariable(string activityName, string onErrorVariable)
        {
            var activityList = _commonSteps.GetActivityList();
            var activity = activityList[activityName];
            
            if (activity is DsfNativeActivity<string> nativeActivity)
            {
                nativeActivity.OnErrorVariable = onErrorVariable;
                _commonSteps.AddVariableToVariableList(onErrorVariable);
            }
            else if (activity is DsfEnhancedDotNetDllActivity dllActivity)
            {
                dllActivity.OnErrorVariable = onErrorVariable;
                _commonSteps.AddVariableToVariableList(onErrorVariable);
            }
            else if (activity is DsfMultiAssignActivity assignActivity)
            {
                assignActivity.OnErrorVariable = onErrorVariable;
                _commonSteps.AddVariableToVariableList(onErrorVariable);
            }
        }

        [Given(@"""(.*)"" has OnErrorWorkflow ""(.*)""")]
        public void GivenHasOnErrorWorkflow(string activityName, string onErrorWorkflow)
        {
            var activityList = _commonSteps.GetActivityList();
            var activity = activityList[activityName];
            
            if (activity is DsfNativeActivity<string> nativeActivity)
            {
                nativeActivity.OnErrorWorkflow = onErrorWorkflow;
            }
            else if (activity is DsfEnhancedDotNetDllActivity dllActivity)
            {
                dllActivity.OnErrorWorkflow = onErrorWorkflow;
            }
            else if (activity is DsfMultiAssignActivity assignActivity)
            {
                assignActivity.OnErrorWorkflow = onErrorWorkflow;
            }
        }

        [Given(@"""(.*)"" has IsEndedOnError ""(.*)""")]
        public void GivenHasIsEndedOnError(string activityName, string isEndedOnError)
        {
            var activityList = _commonSteps.GetActivityList();
            var activity = activityList[activityName];
            
            bool isEndedOnErrorBool = bool.Parse(isEndedOnError);
            
            if (activity is DsfNativeActivity<string> nativeActivity)
            {
                nativeActivity.IsEndedOnError = isEndedOnErrorBool;
            }
            else if (activity is DsfEnhancedDotNetDllActivity dllActivity)
            {
                dllActivity.IsEndedOnError = isEndedOnErrorBool;
            }
            else if (activity is DsfMultiAssignActivity assignActivity)
            {
                assignActivity.IsEndedOnError = isEndedOnErrorBool;
            }
        }

        [Then(@"the execution has ""(.*)"" error")]
        public void ThenTheExecutionHasError(string hasError)
        {
            TryGetValue("activityList", out Dictionary<string, Activity> activityList);
            var debugStates = Get<List<IDebugState>>("debugStates").ToList();
            
            if (hasError == "AN")
            {
                // Check if there's at least one error in debug states
                var hasErrorState = debugStates.Any(state => state.HasError);
                Assert.IsTrue(hasErrorState, "Expected workflow to have errors but none were found");
            }
            else if (hasError == "NO")
            {
                // Check that there are no errors in debug states
                var hasErrorState = debugStates.Any(state => state.HasError);
                Assert.IsFalse(hasErrorState, "Expected workflow to have no errors but errors were found");
            }
        }

        [Then(@"""(.*)"" equals ""(.*)""")]
        public void ThenVariableEquals(string variableName, string expectedValue)
        {
            TryGetValue("environment", out IServer environment);
            var debugStates = Get<List<IDebugState>>("debugStates").ToList();
            
            // Get the last debug state that has outputs containing our variable
            var lastDebugState = debugStates
                .Where(ds => ds.Outputs != null && ds.Outputs.Any(output => 
                    output.ResultsList.Any(result => result.Variable == variableName)))
                .LastOrDefault();
                
            if (lastDebugState != null)
            {
                var outputResult = lastDebugState.Outputs
                    .SelectMany(output => output.ResultsList)
                    .FirstOrDefault(result => result.Variable == variableName);
                    
                if (outputResult != null)
                {
                    Assert.AreEqual(expectedValue, outputResult.Value, 
                        $"Expected variable {variableName} to equal '{expectedValue}' but was '{outputResult.Value}'");
                    return;
                }
            }
            
            // If not found in debug states, check if it's empty and expected to be empty
            if (string.IsNullOrEmpty(expectedValue) || expectedValue == "")
            {
                // Variable might not be set, which means it's empty - this is acceptable for error scenarios
                return;
            }
            
            Assert.Fail($"Variable {variableName} was not found in debug outputs or could not be verified");
        }

        void TryGetValue<T>(string keyName, out T value)
        {
            _scenarioContext.TryGetValue(keyName, out value);
        }

        T Get<T>(string keyName)
        {
            return _scenarioContext.Get<T>(keyName);
        }
    }
}