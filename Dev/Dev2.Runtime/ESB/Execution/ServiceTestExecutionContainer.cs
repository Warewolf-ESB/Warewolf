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
using System.Threading;
using Dev2.Activities;
using Dev2.Activities.Debug;
using Dev2.Activities.SelectAndApply;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Enums;
using Dev2.Communication;
using Dev2.Data;
using Dev2.Data.Decisions.Operations;
using Dev2.Data.TO;
using Dev2.Data.Util;
using Dev2.DataList;
using Dev2.DataList.Contract;
using Dev2.Diagnostics;
using Dev2.Diagnostics.Debug;
using Dev2.DynamicServices.Objects;
using Dev2.Interfaces;
using Dev2.Runtime.ESB.WF;
using Dev2.Runtime.Execution;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.Security;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Workspaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Storage;
using Warewolf.Storage.Interfaces;

namespace Dev2.Runtime.ESB.Execution
{
    public class ServiceTestExecutionContainer : EsbExecutionContainer
    {
        readonly EsbExecuteRequest _request;

        public ServiceTestExecutionContainer(ServiceAction sa, IDSFDataObject dataObj, IWorkspace theWorkspace, IEsbChannel esbChannel, EsbExecuteRequest request)
            : base(sa, dataObj, theWorkspace, esbChannel)
        {
            _request = request;
            TstCatalog = TestCatalog.Instance;
            ResourceCat = ResourceCatalog.Instance;
        }

        protected ITestCatalog TstCatalog { get; set; }
        protected IResourceCatalog ResourceCat { get; set; }

        public override Guid Execute(out ErrorResultTO errors, int update)
        {
            errors = new ErrorResultTO();
            var testCatalog = TstCatalog ?? TestCatalog.Instance;

            var result = GlobalConstants.NullDataListID;

            Dev2Logger.Debug("Entered Wf Container", DataObject.ExecutionID.ToString());

            DataObject.ServiceName = ServiceAction.ServiceName;

            if (DataObject.ServerID == Guid.Empty)
            {
                DataObject.ServerID = HostSecurityProvider.Instance.ServerID;
            }

            if (DataObject.ResourceID == Guid.Empty && ServiceAction?.Service != null)
            {
                DataObject.ResourceID = ServiceAction.Service.ID;
            }
            DataObject.DataList = ServiceAction.DataListSpecification;
            if (DataObject.OriginalInstanceID == Guid.Empty)
            {
                DataObject.OriginalInstanceID = DataObject.DataListID;
            }

            Dev2Logger.Info($"Started Execution for Service Name:{DataObject.ServiceName} Resource Id:{DataObject.ResourceID} Mode:{(DataObject.IsDebug ? "Debug" : "Execute")}", DataObject.ExecutionID.ToString());
            if (!string.IsNullOrWhiteSpace(DataObject.ParentServiceName))
            {
                DataObject.ExecutionOrigin = ExecutionOrigin.Workflow;
                DataObject.ExecutionOriginDescription = DataObject.ParentServiceName;
            }
            else if (DataObject.IsDebug)
            {
                DataObject.ExecutionOrigin = ExecutionOrigin.Debug;
            }
            else
            {
                DataObject.ExecutionOrigin = ExecutionOrigin.External;
            }


            var to = errors;
            var serviceTestModelTo = testCatalog.FetchTest(DataObject.ResourceID, DataObject.TestName);
            if (serviceTestModelTo == null)
            {
                testCatalog.Load();
                serviceTestModelTo = testCatalog.FetchTest(DataObject.ResourceID, DataObject.TestName);
            }
            if (serviceTestModelTo == null)
            {

                var serializer = new Dev2JsonSerializer();
                var testRunResult = new ServiceTestModelTO
                {
                    Result = new TestRunResult
                    {
                        TestName = DataObject.TestName,
                        RunTestResult = RunResult.TestInvalid,
                        Message = $"Test {DataObject.TestName} for Resource {DataObject.ServiceName} ID {DataObject.ResourceID}, has been deleted."
                    }
                };
                Dev2Logger.Error($"Test {DataObject.TestName} for Resource {DataObject.ServiceName} ID {DataObject.ResourceID}, has been deleted.", DataObject.ExecutionID.ToString());
                _request.ExecuteResult = serializer.SerializeToBuilder(testRunResult);
                return Guid.NewGuid();
            }

            if (serviceTestModelTo.AuthenticationType == AuthenticationType.User)
            {
                var userName = serviceTestModelTo.UserName;
                var domain = "";
                if (userName.Contains("\\"))
                {
                    var slashIndex = userName.IndexOf("\\", StringComparison.InvariantCultureIgnoreCase);
                    domain = userName.Substring(0, slashIndex);
                    userName = userName.Substring(slashIndex + 1);
                }
                else
                {
                    if (userName.Contains("@"))
                    {
                        var atIndex = userName.IndexOf("@", StringComparison.InvariantCultureIgnoreCase);
                        userName = userName.Substring(0, atIndex);
                        domain = userName.Substring(atIndex + 1);
                    }
                }
                var resource = ResourceCat.GetResource(GlobalConstants.ServerWorkspaceID, DataObject.ResourceID);
                var testNotauthorizedmsg = string.Format(Warewolf.Resource.Messages.Messages.Test_NotAuthorizedMsg, resource?.ResourceName);
                DataObject.Environment.AllErrors.Add(testNotauthorizedmsg);
                DataObject.StopExecution = true;
            }
            else
            {
                if (serviceTestModelTo.AuthenticationType == AuthenticationType.Public)
                {
                    Thread.CurrentPrincipal = GlobalConstants.GenericPrincipal;
                }
            }
            var userPrinciple = Thread.CurrentPrincipal;
            Common.Utilities.PerformActionInsideImpersonatedContext(userPrinciple, () =>
            {
                result = ExecuteWf(to, serviceTestModelTo);
            });
            if (DataObject.Environment.Errors != null)
            {
                foreach (var err in DataObject.Environment.Errors)
                {
                    errors.AddError(err, true);
                }
            }

            if (DataObject.Environment.AllErrors != null)
            {
                foreach (var err in DataObject.Environment.AllErrors)
                {
                    errors.AddError(err, true);
                }
            }

            Dev2Logger.Info($"Completed Execution for Service Name:{DataObject.ServiceName} Resource Id: {DataObject.ResourceID} Mode:{(DataObject.IsDebug ? "Debug" : "Execute")}", DataObject.ExecutionID.ToString());

            return result;
        }


        public override bool CanExecute(Guid resourceId, IDSFDataObject dataObject, AuthorizationContext authorizationContext)
        {
            return true;
        }

        static void AddRecordsetsInputs(IEnumerable<IServiceTestInput> recSets, IExecutionEnvironment environment)
        {
            if (recSets != null)
            {
                var groupedRecsets = recSets.GroupBy(item => DataListUtil.ExtractRecordsetNameFromValue(item.Variable));
                foreach (var groupedRecset in groupedRecsets)
                {
                    var dataListItems = groupedRecset.GroupBy(item => DataListUtil.ExtractIndexRegionFromRecordset(item.Variable));
                    foreach (var dataListItem in dataListItems)
                    {
                        var recSetsToAssign = new List<IServiceTestInput>();
                        var empty = true;
                        foreach (var listItem in dataListItem)
                        {
                            if (!string.IsNullOrEmpty(listItem.Value))
                            {
                                empty = false;
                            }
                            recSetsToAssign.Add(listItem);
                        }
                        if (!empty)
                        {
                            foreach (var serviceTestInput in recSetsToAssign)
                            {
                                if (!serviceTestInput.EmptyIsNull || !string.IsNullOrEmpty(serviceTestInput.Value))
                                {
                                    environment.Assign(DataListUtil.AddBracketsToValueIfNotExist(serviceTestInput.Variable), serviceTestInput.Value, 0);
                                }
                            }
                        }
                    }
                }
            }
        }

        Guid ExecuteWf(ErrorResultTO to, IServiceTestModelTO test)
        {
            var result = new Guid();
            var wfappUtils = new WfApplicationUtils();
            var invokeErrors = new ErrorResultTO();
            var resourceId = DataObject.ResourceID;
            if (test?.Inputs != null)
            {
                AddRecordsetsInputs(test.Inputs.Where(input => DataListUtil.IsValueRecordset(input.Variable) && !input.Variable.Contains("@")), DataObject.Environment);
                foreach (var input in test.Inputs)
                {
                    var variable = DataListUtil.AddBracketsToValueIfNotExist(input.Variable);
                    var value = input.Value;
                    if (variable.StartsWith("[[@"))
                    {
                        var jContainer = JsonConvert.DeserializeObject(value) as JObject;
                        DataObject.Environment.AddToJsonObjects(variable, jContainer);
                    }
                    else
                    {
                        if (!DataListUtil.IsValueRecordset(input.Variable))
                        {
                            if (ExecutionEnvironment.IsValidVariableExpression(input.Value, out string errorMessage, 0))
                            {
                                DataObject.Environment.AllErrors.Add("Cannot use variables as input value.");
                            }
                            else
                            {
                                if (!input.EmptyIsNull || !string.IsNullOrEmpty(value))
                                {
                                    DataObject.Environment.Assign(variable, value, 0);
                                }
                            }
                        }
                    }
                }
            }
            var serializer = new Dev2JsonSerializer();
            try
            {
                IExecutionToken exeToken = new ExecutionToken { IsUserCanceled = false };
                DataObject.ExecutionToken = exeToken;

                if (DataObject.IsDebugMode())
                {
                    var debugState = wfappUtils.GetDebugState(DataObject, StateType.Start, DataObject.Environment.HasErrors(), DataObject.Environment.FetchErrors(), invokeErrors, DateTime.Now, true, false, false);
                    wfappUtils.WriteDebug(DataObject, debugState);
                }

                var testRunResult = Eval(resourceId, DataObject, test);

                if (DataObject.IsDebugMode())
                {
                    if (!DataObject.StopExecution)
                    {
                        var debugState = wfappUtils.GetDebugState(DataObject, StateType.End, DataObject.Environment.HasErrors(), DataObject.Environment.FetchErrors(), invokeErrors, DataObject.StartTime, false, true, true);
                        var outputDebugItem = new DebugItem();
                        if (test != null)
                        {
                            var msg = test.FailureMessage;
                            if (test.TestPassed)
                            {
                                msg = Warewolf.Resource.Messages.Messages.Test_PassedResult;
                            }
                            outputDebugItem.AddRange(new DebugItemServiceTestStaticDataParams(msg, test.TestFailing).GetDebugItemResult());
                        }
                        debugState.AssertResultList.Add(outputDebugItem);
                        wfappUtils.WriteDebug(DataObject, debugState);
                    }
                    DebugState testAggregateDebugState;
                    if (DataObject.StopExecution && DataObject.Environment.HasErrors())
                    {
                        var existingErrors = DataObject.Environment.FetchErrors();
                        DataObject.Environment.AllErrors.Clear();
                        testAggregateDebugState = wfappUtils.GetDebugState(DataObject, StateType.TestAggregate, DataObject.Environment.HasErrors(), string.Empty, new ErrorResultTO(), DataObject.StartTime, false, false, false);

                        SetTestFailureBasedOnExpectedError(test, existingErrors);

                    }
                    else
                    {
                        testAggregateDebugState = wfappUtils.GetDebugState(DataObject, StateType.TestAggregate, false, string.Empty, new ErrorResultTO(), DataObject.StartTime, false, false, false);
                        AggregateTestResult(resourceId, test);
                    }


                    var itemToAdd = new DebugItem();
                    if (test != null)
                    {
                        var msg = test.FailureMessage;
                        if (test.TestPassed)
                        {
                            msg = Warewolf.Resource.Messages.Messages.Test_PassedResult;
                        }
                        itemToAdd.AddRange(new DebugItemServiceTestStaticDataParams(msg, test.TestFailing).GetDebugItemResult());
                    }
                    testAggregateDebugState.AssertResultList.Add(itemToAdd);
                    wfappUtils.WriteDebug(DataObject, testAggregateDebugState);

                    if (testRunResult != null)
                    {
                        if (test?.Result != null)
                        {
                            test.Result.DebugForTest = TestDebugMessageRepo.Instance.FetchDebugItems(resourceId, test.TestName);
                        }

                        _request.ExecuteResult = serializer.SerializeToBuilder(testRunResult);
                    }
                }
                else
                {
                    if (DataObject.StopExecution && DataObject.Environment.HasErrors())
                    {
                        var existingErrors = DataObject.Environment.FetchErrors();
                        DataObject.Environment.AllErrors.Clear();
                        SetTestFailureBasedOnExpectedError(test, existingErrors);
                        _request.ExecuteResult = serializer.SerializeToBuilder(test);

                    }
                    else
                    {
                        AggregateTestResult(resourceId, test);
                        if (test != null)
                        {
                            _request.ExecuteResult = serializer.SerializeToBuilder(test);
                        }
                    }

                }
                result = DataObject.DataListID;
            }
            catch (InvalidWorkflowException iwe)
            {
                Dev2Logger.Error(iwe, DataObject.ExecutionID.ToString());
                var msg = iwe.Message;

                var start = msg.IndexOf("Flowchart ", StringComparison.Ordinal);
                to?.AddError(start > 0 ? GlobalConstants.NoStartNodeError : iwe.Message);
                var failureMessage = DataObject.Environment.FetchErrors();
                wfappUtils.DispatchDebugState(DataObject, StateType.End, DataObject.Environment.HasErrors(), failureMessage, out invokeErrors, DataObject.StartTime, false, true);

                
                test.TestFailing = false;
                test.TestPassed = false;
                test.TestPending = false;
                test.TestInvalid = true;
                test.LastRunDate = DateTime.Now;


                Common.Utilities.PerformActionInsideImpersonatedContext(Common.Utilities.ServerUser, () => { TestCatalog.Instance.SaveTest(resourceId, test); });

                var testRunResult = new TestRunResult { TestName = test.TestName };
                if (test.TestInvalid)
                {
                    testRunResult.RunTestResult = RunResult.TestInvalid;
                    testRunResult.Message = failureMessage;
                    Dev2Logger.Error($"Test {DataObject.TestName} for Resource {DataObject.ServiceName} ID {DataObject.ResourceID} marked invalid in exception for no start node", DataObject.ExecutionID.ToString());
                }
                testRunResult.DebugForTest = TestDebugMessageRepo.Instance.FetchDebugItems(resourceId, test.TestName);
                if (_request != null)
                {
                    _request.ExecuteResult = serializer.SerializeToBuilder(testRunResult);
                }
            }
            catch (Exception ex)
            {
                Dev2Logger.Error(ex, GlobalConstants.WarewolfError);
                to.AddError(ex.Message);
                var failureMessage = DataObject.Environment.FetchErrors();
                wfappUtils.DispatchDebugState(DataObject, StateType.End, DataObject.Environment.HasErrors(), failureMessage, out invokeErrors, DataObject.StartTime, false, true);
                
                test.TestFailing = false;
                test.TestPassed = false;
                test.TestPending = false;
                test.TestInvalid = true;
                test.LastRunDate = DateTime.Now;


                Common.Utilities.PerformActionInsideImpersonatedContext(Common.Utilities.ServerUser, () => { TestCatalog.Instance.SaveTest(resourceId, test); });

                var testRunResult = new TestRunResult { TestName = test.TestName };
                if (test.TestInvalid)
                {
                    testRunResult.RunTestResult = RunResult.TestInvalid;
                    testRunResult.Message = ex.Message;
                    Dev2Logger.Error($"Test {DataObject.TestName} for Resource {DataObject.ServiceName} ID {DataObject.ResourceID} marked invalid in general exception", GlobalConstants.WarewolfError);
                }
                testRunResult.DebugForTest = TestDebugMessageRepo.Instance.FetchDebugItems(resourceId, test.TestName);
                _request.ExecuteResult = serializer.SerializeToBuilder(testRunResult);
            }
            return result;
        }

        static void SetTestFailureBasedOnExpectedError(IServiceTestModelTO test, string existingErrors)
        {
            if (test != null)
            {
                test.Result = new TestRunResult();
                test.FailureMessage = existingErrors;
                if (test.ErrorExpected)
                {
                    if (test.FailureMessage.Contains(test.ErrorContainsText) && !string.IsNullOrEmpty(test.ErrorContainsText))
                    {
                        test.TestPassed = true;
                        test.TestFailing = false;
                        test.Result.RunTestResult = RunResult.TestPassed;
                    }
                    else
                    {
                        test.TestFailing = true;
                        test.TestPassed = false;
                        test.Result.RunTestResult = RunResult.TestFailed;
                        var assertError = string.Format(Warewolf.Resource.Messages.Messages.Test_FailureMessage_Error,
                            test.ErrorContainsText, test.FailureMessage);
                        test.FailureMessage = assertError;
                    }
                }
                if (test.NoErrorExpected)
                {
                    if (string.IsNullOrEmpty(test.FailureMessage))
                    {
                        test.TestPassed = true;
                        test.TestFailing = false;
                        test.Result.RunTestResult = RunResult.TestPassed;
                    }
                    else
                    {
                        test.TestFailing = true;
                        test.TestPassed = false;
                        test.Result.RunTestResult = RunResult.TestFailed;
                    }
                }
            }
        }

        void UpdateToPending(IList<IServiceTestStep> testSteps)
        {
            if (testSteps != null)
            {
                foreach (var serviceTestStep in testSteps)
                {
                    if (serviceTestStep != null)
                    {
                        if (serviceTestStep.Result != null)
                        {
                            serviceTestStep.Result.RunTestResult = RunResult.TestPending;
                        }
                        else
                        {
                            serviceTestStep.Result = new TestRunResult { RunTestResult = RunResult.TestPending };
                        }
                        UpdateToPending(serviceTestStep.StepOutputs);
                        if (serviceTestStep.Children != null && serviceTestStep.Children.Count > 0)
                        {
                            UpdateToPending(serviceTestStep.Children);
                        }
                    }
                }
            }
        }

        void UpdateToPending(IEnumerable<IServiceTestOutput> stepOutputs)
        {
            var serviceTestOutputs = stepOutputs as IList<IServiceTestOutput> ?? stepOutputs.ToList();
            if (serviceTestOutputs.Count > 0)
            {
                foreach (var serviceTestOutput in serviceTestOutputs)
                {
                    if (serviceTestOutput?.Result != null)
                    {
                        serviceTestOutput.Result.RunTestResult = RunResult.TestPending;
                    }
                    else
                    {
                        if (serviceTestOutput != null)
                        {
                            serviceTestOutput.Result = new TestRunResult { RunTestResult = RunResult.TestPending };
                        }
                    }
                }
            }
        }



        IServiceTestModelTO Eval(Guid resourceId, IDSFDataObject dataObject, IServiceTestModelTO test)
        {
            Dev2Logger.Debug("Getting Resource to Execute", GlobalConstants.WarewolfDebug);
            var resourceCatalog = ResourceCat ?? ResourceCatalog.Instance;
            var resource = resourceCatalog.Parse(TheWorkspace.ID, resourceId);
            var serializer = new Dev2JsonSerializer();
            var execPlan = serializer.SerializeToBuilder(resource);
            var clonedExecPlan = serializer.Deserialize<IDev2Activity>(execPlan);
            Dev2Logger.Debug("Got Resource to Execute", GlobalConstants.WarewolfDebug);

            if (test != null)
            {
                var testPassed = true;
                var canExecute = true;
                var failureMessage = new StringBuilder();
                if (ServerAuthorizationService.Instance != null)
                {
                    var authorizationService = ServerAuthorizationService.Instance;
                    var hasView = authorizationService.IsAuthorized(AuthorizationContext.View, DataObject.ResourceID.ToString());
                    var hasExecute = authorizationService.IsAuthorized(AuthorizationContext.Execute, DataObject.ResourceID.ToString());
                    canExecute = hasExecute && hasView;
                }
                if (!canExecute)
                {
                    dataObject.Environment.AllErrors.Add("Unauthorized to execute this resource.");
                }
                else
                {
                    if (!dataObject.StopExecution)
                    {
                        dataObject.ServiceTest = test;
                        UpdateToPending(test.TestSteps);
                        EvalInner(dataObject, clonedExecPlan, dataObject.ForEachUpdateValue, test.TestSteps);
                        if (test.Outputs != null)
                        {
                            var dev2DecisionFactory = Dev2DecisionFactory.Instance();
                            var testRunResults = test.Outputs.SelectMany(output => GetTestRunResults(dataObject, output, dev2DecisionFactory)).ToList();
                            testPassed = testRunResults.All(result => result.RunTestResult == RunResult.TestPassed);
                            if (!testPassed)
                            {
                                failureMessage = failureMessage.Append(string.Join("", testRunResults.Select(result => result.Message).Where(s => !string.IsNullOrEmpty(s)).ToList()));
                            }
                        }
                    }


                }
                ValidateError(test, testPassed, failureMessage);
                test.FailureMessage = failureMessage.ToString();
                return test;
            }
            throw new Exception($"Test {dataObject.TestName} for Resource {dataObject.ServiceName} ID {resourceId}");
        }

        static void AggregateTestResult(Guid resourceId, IServiceTestModelTO test)
        {
            UpdateTestWithStepValues(test);
            UpdateTestWithFinalResult(resourceId, test);
        }

        static void UpdateTestWithStepValues(IServiceTestModelTO test)
        {
            var testPassed = test.TestPassed;

            var serviceTestSteps = GetStepValues(test, out IEnumerable<IServiceTestStep> pendingSteps, out IEnumerable<IServiceTestStep> invalidSteps, out IEnumerable<IServiceTestStep> failingSteps);
            var failingOutputs = GetOutputValues(test, out IEnumerable<IServiceTestOutput> pendingOutputs, out IEnumerable<IServiceTestOutput> invalidOutputs);
            var invalidTestSteps = GetSteps(invalidSteps, pendingSteps, failingSteps, out IList<IServiceTestStep> pendingTestSteps, out IList<IServiceTestStep> failingTestSteps);
            var pendingTestOutputs = GetOutputs(pendingOutputs, invalidOutputs, failingOutputs, out IList<IServiceTestOutput> invalidTestOutputs, out IList<IServiceTestOutput> failingTestOutputs);

            var hasInvalidSteps = invalidTestSteps?.Any() ?? false;
            var hasPendingSteps = pendingTestSteps?.Any() ?? false;
            var hasFailingSteps = failingTestSteps?.Any() ?? false;
            var hasFailingOutputs = failingTestOutputs?.Any() ?? false;
            var hasPendingOutputs = pendingTestOutputs?.Any() ?? false;
            var hasInvalidOutputs = invalidTestOutputs?.Any() ?? false;
            var testStepPassed = TestPassedBasedOnSteps(hasPendingSteps, hasInvalidSteps, hasFailingSteps) && TestPassedBasedOnOutputs(hasPendingOutputs, hasInvalidOutputs, hasFailingOutputs);

            testPassed = testPassed && testStepPassed;

            var failureMessage = UpdateFailureMessage(hasPendingSteps, pendingTestSteps, hasInvalidSteps, invalidTestSteps, hasFailingSteps, failingTestSteps, hasPendingOutputs, pendingTestOutputs, hasInvalidOutputs, invalidTestOutputs, hasFailingOutputs, failingTestOutputs, serviceTestSteps);
            test.FailureMessage = failureMessage.ToString();
            test.TestFailing = !testPassed;
            test.TestPassed = testPassed;
            test.TestPending = false;
            test.TestInvalid = hasInvalidSteps;
        }

        static StringBuilder UpdateFailureMessage(bool hasPendingSteps, IList<IServiceTestStep> pendingTestSteps, bool hasInvalidSteps, IList<IServiceTestStep> invalidTestSteps, bool hasFailingSteps, IList<IServiceTestStep> failingTestSteps, bool hasPendingOutputs, IList<IServiceTestOutput> pendingTestOutputs, bool hasInvalidOutputs, IList<IServiceTestOutput> invalidTestOutputs, bool hasFailingOutputs, IList<IServiceTestOutput> failingTestOutputs, List<IServiceTestStep> serviceTestSteps)
        {
            var failureMessage = new StringBuilder();
            if (hasFailingSteps)
            {
                foreach (var serviceTestStep in failingTestSteps)
                {
                    failureMessage.AppendLine("Failed Step: " + serviceTestStep.StepDescription + " ");
                    failureMessage.AppendLine("Message: " + serviceTestStep.Result?.Message);
                }
            }
            if (hasInvalidSteps)
            {
                foreach (var serviceTestStep in invalidTestSteps)
                {
                    failureMessage.AppendLine("Invalid Step: " + serviceTestStep.StepDescription + " ");
                    failureMessage.AppendLine("Message: " + serviceTestStep.Result?.Message);
                }
            }
            if (hasPendingSteps)
            {
                foreach (var serviceTestStep in pendingTestSteps)
                {
                    failureMessage.AppendLine("Pending Step: " + serviceTestStep.StepDescription);
                }
            }

            if (hasFailingOutputs)
            {
                foreach (var serviceTestStep in failingTestOutputs)
                {
                    failureMessage.AppendLine("Failed Output For Variable: " + serviceTestStep.Variable + " ");
                    failureMessage.AppendLine("Message: " + serviceTestStep.Result?.Message);
                }
            }
            if (hasInvalidOutputs)
            {
                foreach (var serviceTestStep in invalidTestOutputs)
                {
                    failureMessage.AppendLine("Invalid Output for Variable: " + serviceTestStep.Variable);
                    failureMessage.AppendLine("Message: " + serviceTestStep.Result?.Message);
                }
            }
            if (hasPendingOutputs)
            {
                foreach (var serviceTestStep in pendingTestOutputs)
                {
                    failureMessage.AppendLine("Pending Output for Variable: " + serviceTestStep.Variable);
                }
            }


            if (serviceTestSteps != null)
            {
                failureMessage.AppendLine(string.Join("", serviceTestSteps.Where(step => !string.IsNullOrEmpty(step.Result?.Message)).Select(step => step.Result?.Message)));
            }
            return failureMessage;
        }

        static bool TestPassedBasedOnSteps(bool hasPendingSteps, bool hasInvalidSteps, bool hasFailingSteps)
        {
            return !hasPendingSteps && !hasInvalidSteps && !hasFailingSteps;
        }

        static bool TestPassedBasedOnOutputs(bool pending, bool invalid, bool failing)
        {
            return !pending && !invalid && !failing;
        }

        static IList<IServiceTestOutput> GetOutputs(IEnumerable<IServiceTestOutput> pendingOutputs, IEnumerable<IServiceTestOutput> invalidOutputs, IEnumerable<IServiceTestOutput> failingOutputs, out IList<IServiceTestOutput> invalidTestOutputs, out IList<IServiceTestOutput> failingTestOutputs)
        {
            var pendingTestOutputs = pendingOutputs as IList<IServiceTestOutput> ?? pendingOutputs?.ToList();
            invalidTestOutputs = invalidOutputs as IList<IServiceTestOutput> ?? invalidOutputs?.ToList();
            failingTestOutputs = failingOutputs as IList<IServiceTestOutput> ?? failingOutputs?.ToList();
            return pendingTestOutputs;
        }

        static IList<IServiceTestStep> GetSteps(IEnumerable<IServiceTestStep> invalidSteps, IEnumerable<IServiceTestStep> pendingSteps, IEnumerable<IServiceTestStep> failingSteps, out IList<IServiceTestStep> pendingTestSteps, out IList<IServiceTestStep> failingTestSteps)
        {
            var invalidTestSteps = invalidSteps as IList<IServiceTestStep> ?? invalidSteps?.ToList();
            pendingTestSteps = pendingSteps as IList<IServiceTestStep> ?? pendingSteps?.ToList();
            failingTestSteps = failingSteps as IList<IServiceTestStep> ?? failingSteps?.ToList();
            return invalidTestSteps;
        }

        static IEnumerable<IServiceTestOutput> GetOutputValues(IServiceTestModelTO test, out IEnumerable<IServiceTestOutput> pendingOutputs, out IEnumerable<IServiceTestOutput> invalidOutputs)
        {
            var failingOutputs = test.Outputs?.Where(output => output.Result?.RunTestResult == RunResult.TestFailed);
            pendingOutputs = test.Outputs?.Where(output => output.Result?.RunTestResult == RunResult.TestPending);
            invalidOutputs = test.Outputs?.Where(output => output.Result?.RunTestResult == RunResult.TestInvalid);
            var serviceTestOutputs = failingOutputs as IServiceTestOutput[] ?? failingOutputs?.ToArray();
            if (serviceTestOutputs?.Any() ?? false)
            {
                foreach (var serviceTestOutput in serviceTestOutputs)
                {
                    serviceTestOutput.Result.Message = DataListUtil.StripBracketsFromValue(serviceTestOutput.Result.Message);
                }
            }
            return serviceTestOutputs;
        }

        static List<IServiceTestStep> GetStepValues(IServiceTestModelTO test, out IEnumerable<IServiceTestStep> pendingSteps, out IEnumerable<IServiceTestStep> invalidSteps, out IEnumerable<IServiceTestStep> failingSteps)
        {
            var serviceTestSteps = test.TestSteps;
            pendingSteps = serviceTestSteps?.Where(step => step.Type != StepType.Mock && step.Result?.RunTestResult == RunResult.TestPending);
            invalidSteps = serviceTestSteps?.Where(step => step.Type != StepType.Mock && step.Result?.RunTestResult == RunResult.TestInvalid);
            failingSteps = serviceTestSteps?.Where(step => step.Type != StepType.Mock && step.Result?.RunTestResult == RunResult.TestFailed);
            return serviceTestSteps;
        }

        static void UpdateTestWithFinalResult(Guid resourceId, IServiceTestModelTO test)
        {
            test.LastRunDate = DateTime.Now;

            var testRunResult = new TestRunResult { TestName = test.TestName };
            if (test.TestFailing)
            {
                testRunResult.RunTestResult = RunResult.TestFailed;
                testRunResult.Message = test.FailureMessage;
            }
            if (test.TestPassed)
            {
                testRunResult.RunTestResult = RunResult.TestPassed;
            }
            if (test.TestInvalid)
            {
                testRunResult.RunTestResult = RunResult.TestInvalid;
            }
            test.Result = testRunResult;
            Common.Utilities.PerformActionInsideImpersonatedContext(Common.Utilities.ServerUser, () => { TestCatalog.Instance.SaveTest(resourceId, test); });
        }

        void ValidateError(IServiceTestModelTO test, bool testPassed, StringBuilder failureMessage)
        {
            var fetchErrors = DataObject.Environment.FetchErrors();
            var hasErrors = DataObject.Environment.HasErrors();
            if (test.ErrorExpected)
            {
                var testErrorContainsText = test.ErrorContainsText ?? "";
                testPassed = hasErrors && testPassed && fetchErrors.ToLower().Contains(testErrorContainsText.ToLower());
                if (!testPassed)
                {
                    failureMessage.Append(string.Format(Warewolf.Resource.Messages.Messages.Test_FailureMessage_Error, testErrorContainsText, fetchErrors));
                }
            }
            else
            {
                if (test.NoErrorExpected)
                {
                    testPassed = !hasErrors && testPassed;
                    if (hasErrors)
                    {
                        failureMessage.AppendLine(fetchErrors);
                    }
                }
            }
            test.TestPassed = testPassed;
            test.TestFailing = !testPassed;
        }

        IEnumerable<TestRunResult> GetTestRunResults(IDSFDataObject dataObject, IServiceTestOutput output, Dev2DecisionFactory factory)
        {
            var expressionType = output.AssertOp ?? string.Empty;
            var opt = FindRecsetOptions.FindMatch(expressionType);
            var decisionType = DecisionDisplayHelper.GetValue(expressionType);

            if (decisionType == enDecisionType.IsError)
            {
                var hasErrors = dataObject.Environment.AllErrors.Count > 0;
                var testResult = new TestRunResult();
                if (hasErrors)
                {
                    testResult.RunTestResult = RunResult.TestPassed;
                }
                else
                {
                    testResult.RunTestResult = RunResult.TestFailed;
                    var msg = DecisionDisplayHelper.GetFailureMessage(decisionType);
                    var actMsg = string.Format(msg);
                    testResult.Message = new StringBuilder(testResult.Message).AppendLine(actMsg).ToString();
                }
                return new[] { testResult };
            }
            if (decisionType == enDecisionType.IsNotError)
            {
                var noErrors = dataObject.Environment.AllErrors.Count == 0;
                var testResult = new TestRunResult();
                if (noErrors)
                {
                    testResult.RunTestResult = RunResult.TestPassed;
                }
                else
                {
                    testResult.RunTestResult = RunResult.TestFailed;
                    var msg = DecisionDisplayHelper.GetFailureMessage(decisionType);
                    var actMsg = string.Format(msg);
                    testResult.Message = new StringBuilder(testResult.Message).AppendLine(actMsg).ToString();
                }
                return new[] { testResult };
            }
            var value = new List<DataStorage.WarewolfAtom> { DataStorage.WarewolfAtom.NewDataString(output.Value) };
            var from = new List<DataStorage.WarewolfAtom> { DataStorage.WarewolfAtom.NewDataString(output.From) };
            var to = new List<DataStorage.WarewolfAtom> { DataStorage.WarewolfAtom.NewDataString(output.To) };

            IList<TestRunResult> ret = new List<TestRunResult>();
            var iter = new WarewolfListIterator();
            var variable = DataListUtil.AddBracketsToValueIfNotExist(output.Variable);
            var cols1 = dataObject.Environment.EvalAsList(variable, 0);
            var c1 = new WarewolfAtomIterator(cols1);
            var c2 = new WarewolfAtomIterator(value);
            var c3 = new WarewolfAtomIterator(to);
            if (opt.ArgumentCount > 2)
            {
                c2 = new WarewolfAtomIterator(from);
            }
            iter.AddVariableToIterateOn(c1);
            iter.AddVariableToIterateOn(c2);
            iter.AddVariableToIterateOn(c3);
            while (iter.HasMoreData())
            {
                var val1 = iter.FetchNextValue(c1);
                var val2 = iter.FetchNextValue(c2);
                var val3 = iter.FetchNextValue(c3);
                var assertResult = factory.FetchDecisionFunction(decisionType).Invoke(new[] { val1, val2, val3 });
                var testResult = new TestRunResult();
                if (assertResult)
                {
                    testResult.RunTestResult = RunResult.TestPassed;
                }
                else
                {
                    testResult.RunTestResult = RunResult.TestFailed;
                    var msg = DecisionDisplayHelper.GetFailureMessage(decisionType);
                    var actMsg = string.Format(msg, val2, variable, val1, val3);
                    testResult.Message = new StringBuilder(testResult.Message).AppendLine(actMsg).ToString();
                }
                output.Result = testResult;
                ret.Add(testResult);
            }
            return ret;
        }

        public override IDSFDataObject Execute(IDSFDataObject inputs, IDev2Activity activity)
        {
            return null;
        }

        static void EvalInner(IDSFDataObject dsfDataObject, IDev2Activity resource, int update, List<IServiceTestStep> testSteps)
        {
            if (resource == null)
            {
                throw new InvalidOperationException(GlobalConstants.NoStartNodeError);
            }
            WorkflowExecutionWatcher.HasAWorkflowBeenExecuted = true;
            resource = NextActivity(resource, testSteps);
            var next = resource.Execute(dsfDataObject, update);
            while (next != null)
            {
                if (!dsfDataObject.StopExecution)
                {
                    next = NextActivity(next, testSteps);
                    next = next.Execute(dsfDataObject, update);
                    if (dsfDataObject.Environment.Errors.Count > 0)
                    {
                        foreach (var e in dsfDataObject.Environment.Errors)
                        {
                            dsfDataObject.Environment.AllErrors.Add(e);
                        }

                    }
                }
                else
                {
                    break;
                }
            }
        }

        static IDev2Activity NextActivity(IDev2Activity resource, List<IServiceTestStep> testSteps)
        {
            var foundTestStep = testSteps?.FirstOrDefault(step => resource != null && step.UniqueId.ToString() == resource.UniqueID);
            if (foundTestStep != null)
            {
                if (foundTestStep.ActivityType == typeof(DsfDecision).Name && foundTestStep.Type == StepType.Mock)
                {
                    var serviceTestOutput = foundTestStep.StepOutputs.FirstOrDefault(output => output.Variable == GlobalConstants.ArmResultText);
                    if (serviceTestOutput != null)
                    {
                        resource = new TestMockDecisionStep(resource as DsfDecision) { NameOfArmToReturn = serviceTestOutput.Value };
                    }
                }
                else if (foundTestStep.ActivityType == typeof(DsfSwitch).Name && foundTestStep.Type == StepType.Mock)
                {
                    var serviceTestOutput = foundTestStep.StepOutputs.FirstOrDefault(output => output.Variable == GlobalConstants.ArmResultText);
                    if (serviceTestOutput != null)
                    {
                        resource = new TestMockSwitchStep(resource as DsfSwitch) { ConditionToUse = serviceTestOutput.Value };
                    }
                }
                else if (foundTestStep.ActivityType == typeof(DsfSequenceActivity).Name)
                {
                    if (resource is DsfSequenceActivity sequenceActivity)
                    {
                        var acts = sequenceActivity.Activities;
                        for (int index = 0; index < acts.Count; index++)
                        {
                            var activity = acts[index];
                            if (foundTestStep.Children != null)
                            {
                                var replacement = NextActivity(activity as IDev2Activity, foundTestStep.Children.ToList()) as Activity;
                                acts[index] = replacement;
                            }
                        }
                    }
                }
                else if (foundTestStep.ActivityType == typeof(DsfForEachActivity).Name)
                {
                    if (resource is DsfForEachActivity forEach)
                    {
                        if (foundTestStep.Children != null)
                        {
                            var replacement = NextActivity(forEach.DataFunc.Handler as IDev2Activity, foundTestStep.Children.ToList()) as Activity;
                            forEach.DataFunc.Handler = replacement;
                        }
                    }
                }
                else if (foundTestStep.ActivityType == typeof(DsfSelectAndApplyActivity).Name)
                {
                    if (resource is DsfSelectAndApplyActivity forEach)
                    {
                        if (foundTestStep.Children != null)
                        {
                            var replacement = NextActivity(forEach.ApplyActivityFunc.Handler as IDev2Activity, foundTestStep.Children.ToList()) as Activity;
                            forEach.ApplyActivityFunc.Handler = replacement;
                        }
                    }
                }
                else
                {
                    if (foundTestStep.Type == StepType.Mock)
                    {
                        resource = new TestMockStep(resource, foundTestStep.StepOutputs.ToList());
                    }
                }
            }
            return resource;
        }

    }
}