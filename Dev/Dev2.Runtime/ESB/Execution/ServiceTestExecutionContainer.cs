//#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
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
            var vvv = new Implementation(DataObject, TheWorkspace, ResourceCat, TstCatalog, ServiceAction, _request);
            return vvv.Execute(out errors, update);
        }
        

        internal class Implementation
        {
            readonly IDSFDataObject _dataObject;
            readonly IWorkspace _theWorkspace;
            readonly IResourceCatalog _resourceCat;
            readonly ITestCatalog _testCatalog;
            readonly ServiceAction _serviceAction;
            readonly EsbExecuteRequest _request;


            public Implementation(IDSFDataObject dataObject, IWorkspace theWorkspace, IResourceCatalog resourceCat, ITestCatalog testCatalog, ServiceAction serviceAction, EsbExecuteRequest esbExecuteRequest)
            {
                _dataObject = dataObject;
                _theWorkspace = theWorkspace;
                _resourceCat = resourceCat;
                _testCatalog = testCatalog;
                _serviceAction = serviceAction;
                _request = esbExecuteRequest;
            }

            public Guid Execute(out ErrorResultTO errors, int update)
            {
                errors = new ErrorResultTO();
                var testCatalog = _testCatalog ?? TestCatalog.Instance;

                var result = GlobalConstants.NullDataListID;

                Dev2Logger.Debug("Entered Wf Container", _dataObject.ExecutionID.ToString());
                SetDataObjectProperties();

                Dev2Logger.Info($"Started Execution for Service Name:{_dataObject.ServiceName} Resource Id:{_dataObject.ResourceID} Mode:{(_dataObject.IsDebug ? "Debug" : "Execute")}", _dataObject.ExecutionID.ToString());
                SetExecutionOrigin();

                var to = errors;
                var serviceTestModelTo = testCatalog.FetchTest(_dataObject.ResourceID, _dataObject.TestName);
                if (serviceTestModelTo == null)
                {
                    testCatalog.Load();
                    serviceTestModelTo = testCatalog.FetchTest(_dataObject.ResourceID, _dataObject.TestName);
                }
                if (serviceTestModelTo == null)
                {
                    var serializer = new Dev2JsonSerializer();
                    var testRunResult = new ServiceTestModelTO
                    {
                        Result = new TestRunResult
                        {
                            TestName = _dataObject.TestName,
                            RunTestResult = RunResult.TestInvalid,
                            Message = $"Test {_dataObject.TestName} for Resource {_dataObject.ServiceName} ID {_dataObject.ResourceID}, has been deleted."
                        }
                    };
                    Dev2Logger.Error($"Test {_dataObject.TestName} for Resource {_dataObject.ServiceName} ID {_dataObject.ResourceID}, has been deleted.", _dataObject.ExecutionID.ToString());
                    _request.ExecuteResult = serializer.SerializeToBuilder(testRunResult);
                    return Guid.NewGuid();
                }

                SetExecutingUserForPublicAuthenticationType(serviceTestModelTo);

                result = GetUserPrincipalGuid(result, to, serviceTestModelTo);

                SetDataObjectEnvironmentErrors(errors);

                Dev2Logger.Info($"Completed Execution for Service Name:{_dataObject.ServiceName} Resource Id: {_dataObject.ResourceID} Mode:{(_dataObject.IsDebug ? "Debug" : "Execute")}", _dataObject.ExecutionID.ToString());

                return result;
            }

            private Guid GetUserPrincipalGuid(Guid result, ErrorResultTO to, IServiceTestModelTO serviceTestModelTo)
            {
                var userPrinciple = Thread.CurrentPrincipal;
                Common.Utilities.PerformActionInsideImpersonatedContext(userPrinciple, () =>
                {
                     result = TryExecuteWf(to, serviceTestModelTo);
                });
                return result;
            }

            private void SetDataObjectProperties()
            {
                _dataObject.ServiceName = _serviceAction.ServiceName;

                if (_dataObject.ServerID == Guid.Empty)
                {
                    _dataObject.ServerID = HostSecurityProvider.Instance.ServerID;
                }

                if (_dataObject.ResourceID == Guid.Empty && _serviceAction?.Service != null)
                {
                    _dataObject.ResourceID = _serviceAction.Service.ID;
                }

                _dataObject.DataList = _serviceAction.DataListSpecification;

                if (_dataObject.OriginalInstanceID == Guid.Empty)
                {
                    _dataObject.OriginalInstanceID = _dataObject.DataListID;
                }
            }

            private void SetExecutionOrigin()
            {
                if (!string.IsNullOrWhiteSpace(_dataObject.ParentServiceName))
                {
                    _dataObject.ExecutionOrigin = ExecutionOrigin.Workflow;
                    _dataObject.ExecutionOriginDescription = _dataObject.ParentServiceName;
                }
                else if (_dataObject.IsDebug)
                {
                    _dataObject.ExecutionOrigin = ExecutionOrigin.Debug;
                }
                else
                {
                    _dataObject.ExecutionOrigin = ExecutionOrigin.External;
                }
            }

            private void SetExecutingUserForPublicAuthenticationType(IServiceTestModelTO serviceTestModelTo)
            {
                if (serviceTestModelTo.AuthenticationType == AuthenticationType.User)
                {
                    var resource = _resourceCat.GetResource(GlobalConstants.ServerWorkspaceID, _dataObject.ResourceID);
                    var testNotauthorizedmsg = string.Format(Warewolf.Resource.Messages.Messages.Test_NotAuthorizedMsg, resource?.ResourceName);
                    _dataObject.Environment.AllErrors.Add(testNotauthorizedmsg);
                    _dataObject.StopExecution = true;
                }
                else
                {
                    if (serviceTestModelTo.AuthenticationType == AuthenticationType.Public)
                    {
                        Thread.CurrentPrincipal = GlobalConstants.GenericPrincipal;
                        _dataObject.ExecutingUser = GlobalConstants.GenericPrincipal;
                    }
                }
            }

            private void SetDataObjectEnvironmentErrors(ErrorResultTO errors)
            {
                if (_dataObject.Environment.Errors != null)
                {
                    foreach (var err in _dataObject.Environment.Errors)
                    {
                        errors.AddError(err, true);
                    }
                }

                if (_dataObject.Environment.AllErrors != null)
                {
                    foreach (var err in _dataObject.Environment.AllErrors)
                    {
                        errors.AddError(err, true);
                    }
                }
            }

            Guid TryExecuteWf(ErrorResultTO to, IServiceTestModelTO test)
            {
                var result = new Guid();
                var wfappUtils = new WfApplicationUtils();
                var invokeErrors = new ErrorResultTO();
                var resourceId = _dataObject.ResourceID;

                AddTestInputsToJosonOrRecordset(test);

                var serializer = new Dev2JsonSerializer();
                try
                {
                    var executeWorkflowArgs = new ExecuteWorkflowArgs
                    {
                        DataObject = _dataObject,
                        EsbExecuteRequest = _request,
                        ResourceCatalog = _resourceCat,
                        Workspace = _theWorkspace,
                        Test = test,
                        WfappUtils = wfappUtils,
                        InvokeErrors = invokeErrors,
                        ResourceId = resourceId,
                        Serializer = serializer
                    };

                    result = new ExecuteWorkflowImplementation(executeWorkflowArgs).ExecuteWf(test, wfappUtils, invokeErrors, resourceId, serializer);
                }
                catch (InvalidWorkflowException iwe)
                {
                    Dev2Logger.Error(iwe, _dataObject.ExecutionID.ToString());
                    var msg = iwe.Message;

                    var start = msg.IndexOf("Flowchart ", StringComparison.Ordinal);
                    to?.AddError(start > 0 ? GlobalConstants.NoStartNodeError : iwe.Message);
                    var failureMessage = _dataObject.Environment.FetchErrors();
                    wfappUtils.DispatchDebugState(_dataObject, StateType.End, out invokeErrors);

                    SetTestRunResult(test, resourceId, serializer, failureMessage);
                }
                catch (Exception ex)
                {
                    Dev2Logger.Error(ex, GlobalConstants.WarewolfError);
                    to.AddError(ex.Message);
                    wfappUtils.DispatchDebugState(_dataObject, StateType.End, out invokeErrors);

                    SetTestRunResult(test, resourceId, serializer, ex);
                }
                return result;
            }

            private void AddTestInputsToJosonOrRecordset(IServiceTestModelTO test)
            {
                if (test?.Inputs != null)
                {
                    AddRecordsetsInputs(test.Inputs.Where(input => DataListUtil.IsValueRecordset(input.Variable) && !input.Variable.Contains("@")), _dataObject.Environment);
                    foreach (var input in test.Inputs)
                    {
                        var variable = DataListUtil.AddBracketsToValueIfNotExist(input.Variable);
                        var value = input.Value;
                        if (variable.StartsWith("[[@"))
                        {
                            var jContainer = JsonConvert.DeserializeObject(value) as JObject;
                            _dataObject.Environment.AddToJsonObjects(variable, jContainer);
                        }
                        else
                        {
                            AddToRecordsetObjects(input, variable, value);
                        }
                    }
                }
            }

            private void AddToRecordsetObjects(IServiceTestInput input, string variable, string value)
            {
                if (!DataListUtil.IsValueRecordset(input.Variable))
                {
                    if (ExecutionEnvironment.IsValidVariableExpression(input.Value, out string errorMessage, 0))
                    {
                        _dataObject.Environment.AllErrors.Add("Cannot use variables as input value.");
                    }
                    else
                    {
                        if (!input.EmptyIsNull || !string.IsNullOrEmpty(value))
                        {
                            _dataObject.Environment.Assign(variable, value, 0);
                        }
                    }
                }
            }

            private void SetTestRunResult(IServiceTestModelTO test, Guid resourceId, Dev2JsonSerializer serializer, string failureMessage)
            {
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
                    Dev2Logger.Error($"Test {_dataObject.TestName} for Resource {_dataObject.ServiceName} ID {_dataObject.ResourceID} marked invalid in exception for no start node", _dataObject.ExecutionID.ToString());
                }
                testRunResult.DebugForTest = TestDebugMessageRepo.Instance.FetchDebugItems(resourceId, test.TestName);
                if (_request != null)
                {
                    _request.ExecuteResult = serializer.SerializeToBuilder(testRunResult);
                }
            }

            private void SetTestRunResult(IServiceTestModelTO test, Guid resourceId, Dev2JsonSerializer serializer, Exception ex)
            {
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
                    Dev2Logger.Error($"Test {_dataObject.TestName} for Resource {_dataObject.ServiceName} ID {_dataObject.ResourceID} marked invalid in general exception", GlobalConstants.WarewolfError);
                }
                testRunResult.DebugForTest = TestDebugMessageRepo.Instance.FetchDebugItems(resourceId, test.TestName);
                _request.ExecuteResult = serializer.SerializeToBuilder(testRunResult);
            }

        }

        public struct ExecuteWorkflowArgs
        {
            public IServiceTestModelTO Test { get; set; }
            public WfApplicationUtils WfappUtils { get; set; }
            public ErrorResultTO InvokeErrors { get; set; }
            public Guid ResourceId { get; set; }
            public Dev2JsonSerializer Serializer { get; set; }
            public IDSFDataObject DataObject { get; set; }
            public EsbExecuteRequest EsbExecuteRequest { get; set; }
            public IResourceCatalog ResourceCatalog { get; set; }
            public IWorkspace Workspace { get; set; }
        }

        internal class ExecuteWorkflowImplementation
        {
            readonly IDSFDataObject _dataObject;
            readonly EsbExecuteRequest _request;
            readonly IResourceCatalog _resourceCat;
            readonly IWorkspace _theWorkspace;

            private IServiceTestModelTO Test { get; set; }
            private WfApplicationUtils WfappUtils { get; set; }
            private ErrorResultTO InvokeErrors { get; set; }
            private Guid ResourceId { get; set; }
            private Dev2JsonSerializer Serializer { get; set; }

            public ExecuteWorkflowImplementation(ExecuteWorkflowArgs executeWorkflowArgs)
            {
                Test = executeWorkflowArgs.Test;
                WfappUtils = executeWorkflowArgs.WfappUtils;
                InvokeErrors = executeWorkflowArgs.InvokeErrors;
                ResourceId = executeWorkflowArgs.ResourceId;
                Serializer = executeWorkflowArgs.Serializer;
                _dataObject = executeWorkflowArgs.DataObject;
                _request = executeWorkflowArgs.EsbExecuteRequest;
                _resourceCat = executeWorkflowArgs.ResourceCatalog;
                _theWorkspace = executeWorkflowArgs.Workspace;
            }

            internal Guid ExecuteWf(IServiceTestModelTO test, WfApplicationUtils wfappUtils, ErrorResultTO invokeErrors, Guid resourceId, Dev2JsonSerializer serializer)
            {
                Guid result;
                IExecutionToken exeToken = new ExecutionToken { IsUserCanceled = false };
                _dataObject.ExecutionToken = exeToken;

                if (_dataObject.IsDebugMode())
                {
                    var debugState = wfappUtils.GetDebugState(_dataObject, StateType.Start, invokeErrors, interrogateInputs: true);
                    wfappUtils.TryWriteDebug(_dataObject, debugState);
                }

                var testRunResult = Eval(resourceId, _dataObject, test);

                if (_dataObject.IsDebugMode())
                {
                    if (!_dataObject.StopExecution)
                    {
                        AddAssertResultList(test, wfappUtils, invokeErrors);
                    }

                    DebugState testAggregateDebugState;
                    if (_dataObject.StopExecution && _dataObject.Environment.HasErrors())
                    {
                        var existingErrors = _dataObject.Environment.FetchErrors();
                        _dataObject.Environment.AllErrors.Clear();
                        testAggregateDebugState = wfappUtils.GetDebugState(_dataObject, StateType.TestAggregate, new ErrorResultTO());
                        SetTestFailureBasedOnExpectedError(test, existingErrors);
                    }
                    else
                    {
                        testAggregateDebugState = wfappUtils.GetDebugState(_dataObject, StateType.TestAggregate, new ErrorResultTO());
                        AggregateTestResult(resourceId, test);
                    }

                    var itemToAdd = new DebugItem();
                    if (test != null)
                    {
                        AddDebugItems(test, itemToAdd);
                    }
                    testAggregateDebugState.AssertResultList.Add(itemToAdd);
                    wfappUtils.TryWriteDebug(_dataObject, testAggregateDebugState);

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
                    SetExecuteResult(test, resourceId, serializer);
                }

                result = _dataObject.DataListID;
                return result;
            }

            private void SetExecuteResult(IServiceTestModelTO test, Guid resourceId, Dev2JsonSerializer serializer)
            {
                if (_dataObject.StopExecution && _dataObject.Environment.HasErrors())
                {
                    var existingErrors = _dataObject.Environment.FetchErrors();
                    _dataObject.Environment.AllErrors.Clear();
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

            private static void AddDebugItems(IServiceTestModelTO test, DebugItem itemToAdd)
            {
                var msg = test.FailureMessage;
                if (test.TestPassed)
                {
                    msg = Warewolf.Resource.Messages.Messages.Test_PassedResult;
                }
                itemToAdd.AddRange(new DebugItemServiceTestStaticDataParams(msg, test.TestFailing).GetDebugItemResult());
            }

            private void AddAssertResultList(IServiceTestModelTO test, WfApplicationUtils wfappUtils, ErrorResultTO invokeErrors)
            {
                var debugState = wfappUtils.GetDebugState(_dataObject, StateType.End, invokeErrors, interrogateOutputs: true, durationVisible: true);
                var outputDebugItem = new DebugItem();
                if (test != null)
                {
                    var msg = test.TestPassed ? Warewolf.Resource.Messages.Messages.Test_PassedResult : test.FailureMessage;
                    outputDebugItem.AddRange(new DebugItemServiceTestStaticDataParams(msg, test.TestFailing).GetDebugItemResult());
                }
                debugState.AssertResultList.Add(outputDebugItem);
                wfappUtils.TryWriteDebug(_dataObject, debugState);
            }

            IServiceTestModelTO Eval(Guid resourceId, IDSFDataObject dataObject, IServiceTestModelTO test)
            {
                Dev2Logger.Debug("Getting Resource to Execute", GlobalConstants.WarewolfDebug);
                var resourceCatalog = _resourceCat ?? ResourceCatalog.Instance;
                var resource = resourceCatalog.Parse(_theWorkspace.ID, resourceId);
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
                        var hasView = authorizationService.IsAuthorized(_dataObject.ExecutingUser, AuthorizationContext.View, _dataObject.ResourceID.ToString());
                        var hasExecute = authorizationService.IsAuthorized(_dataObject.ExecutingUser, AuthorizationContext.Execute, _dataObject.ResourceID.ToString());
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
                            GetTestResults(dataObject, test, ref testPassed, ref failureMessage);
                        }


                    }
                    ValidateError(test, testPassed, failureMessage);
                    test.FailureMessage = failureMessage.ToString();
                    return test;
                }
                throw new Exception($"Test {dataObject.TestName} for Resource {dataObject.ServiceName} ID {resourceId}");
            }


            void UpdateToPending(IList<IServiceTestStep> testSteps)
            {
                if (testSteps != null)
                {
                    foreach (var serviceTestStep in testSteps)
                    {
                        UpdateToPending(serviceTestStep);
                    }
                }
            }

            void UpdateToPending(IServiceTestStep serviceTestStep)
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

            void UpdateToPending(IEnumerable<IServiceTestOutput> stepOutputs)
            {
                var serviceTestOutputs = stepOutputs as IList<IServiceTestOutput> ?? stepOutputs.ToList();
                if (serviceTestOutputs.Count > 0)
                {
                    foreach (var serviceTestOutput in serviceTestOutputs)
                    {
                        UpdateToPending(serviceTestOutput);
                    }
                }
            }

            private static void UpdateToPending(IServiceTestOutput serviceTestOutput)
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

            void GetTestResults(IDSFDataObject dataObject, IServiceTestModelTO test, ref bool testPassed, ref StringBuilder failureMessage)
            {
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

            internal IEnumerable<TestRunResult> GetTestRunResults(IDSFDataObject dataObject, IServiceTestOutput output, Dev2DecisionFactory factory)
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

            void ValidateError(IServiceTestModelTO test, bool testPassed, StringBuilder failureMessage)
            {
                var fetchErrors = _dataObject.Environment.FetchErrors();
                var hasErrors = _dataObject.Environment.HasErrors();
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
        }


        public override bool CanExecute(Guid resourceId, IDSFDataObject dataObject, AuthorizationContext authorizationContext) => true;

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
                        AddRecordsetInput(environment, dataListItem);
                    }
                }
            }
        }

        static void AddRecordsetInput(IExecutionEnvironment environment, IGrouping<string, IServiceTestInput> dataListItem)
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
        
        static void AggregateTestResult(Guid resourceId, IServiceTestModelTO test)
        {
            UpdateTestWithStepValuesImplimentation.UpdateTestWithStepValues(test);
            UpdateTestWithFinalResult(resourceId, test);
        }

        internal class UpdateTestWithStepValuesImplimentation
        {
            protected UpdateTestWithStepValuesImplimentation()
            {

            }

            public static void UpdateTestWithStepValues(IServiceTestModelTO test)
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

                var messageArgs = new UpdateFailureMessageArgs
                {
                    HasPendingSteps = hasPendingSteps,
                    PendingTestSteps = pendingTestSteps,
                    HasInvalidSteps = hasInvalidSteps,
                    InvalidTestSteps = invalidTestSteps,
                    HasFailingSteps = hasFailingSteps,
                    FailingTestSteps = failingTestSteps,
                    HasPendingOutputs = hasPendingOutputs,
                    PendingTestOutputs = pendingTestOutputs,
                    HasInvalidOutputs = hasInvalidOutputs,
                    InvalidTestOutputs = invalidTestOutputs,
                    HasFailingOutputs = hasFailingOutputs,
                    FailingTestOutputs = failingTestOutputs,
                    ServiceTestSteps = serviceTestSteps,
                };
                var failureMessage = UpdateFailureMessage(messageArgs);

                test.FailureMessage = failureMessage.ToString();
                test.TestFailing = !testPassed;
                test.TestPassed = testPassed;
                test.TestPending = false;
                test.TestInvalid = hasInvalidSteps;
            }

            static List<IServiceTestStep> GetStepValues(IServiceTestModelTO test, out IEnumerable<IServiceTestStep> pendingSteps, out IEnumerable<IServiceTestStep> invalidSteps, out IEnumerable<IServiceTestStep> failingSteps)
            {
                var serviceTestSteps = test.TestSteps;
                pendingSteps = serviceTestSteps?.Where(step => step.Type != StepType.Mock && step.Result?.RunTestResult == RunResult.TestPending);
                invalidSteps = serviceTestSteps?.Where(step => step.Type != StepType.Mock && step.Result?.RunTestResult == RunResult.TestInvalid);
                failingSteps = serviceTestSteps?.Where(step => step.Type != StepType.Mock && step.Result?.RunTestResult == RunResult.TestFailed);
                return serviceTestSteps;
            }

            static IEnumerable<IServiceTestOutput> GetOutputValues(IServiceTestModelTO test, out IEnumerable<IServiceTestOutput> pendingOutputs, out IEnumerable<IServiceTestOutput> invalidOutputs)
            {
                var failingOutputs = test.Outputs?.Where(output => output.Result?.RunTestResult == RunResult.TestFailed);
                pendingOutputs = test.Outputs?.Where(output => output.Result?.RunTestResult == RunResult.TestPending);
                invalidOutputs = test.Outputs?.Where(output => output.Result?.RunTestResult == RunResult.TestInvalid);

                return SetServiceTestOutputs(failingOutputs);
            }

            private static IEnumerable<IServiceTestOutput> SetServiceTestOutputs(IEnumerable<IServiceTestOutput> failingOutputs)
            {
                var serviceTestOutputs = failingOutputs as IServiceTestOutput[] ?? failingOutputs?.ToArray();
                if (serviceTestOutputs?.Any() ?? false)
                {
                    SetServiceTestOutputResultMessage(serviceTestOutputs);
                }
                return serviceTestOutputs;
            }
            
            static IList<IServiceTestStep> GetSteps(IEnumerable<IServiceTestStep> invalidSteps, IEnumerable<IServiceTestStep> pendingSteps, IEnumerable<IServiceTestStep> failingSteps, out IList<IServiceTestStep> pendingTestSteps, out IList<IServiceTestStep> failingTestSteps)
            {
                var invalidTestSteps = invalidSteps as IList<IServiceTestStep> ?? invalidSteps?.ToList();
                pendingTestSteps = pendingSteps as IList<IServiceTestStep> ?? pendingSteps?.ToList();
                failingTestSteps = failingSteps as IList<IServiceTestStep> ?? failingSteps?.ToList();
                return invalidTestSteps;
            }

            static IList<IServiceTestOutput> GetOutputs(IEnumerable<IServiceTestOutput> pendingOutputs, IEnumerable<IServiceTestOutput> invalidOutputs, IEnumerable<IServiceTestOutput> failingOutputs, out IList<IServiceTestOutput> invalidTestOutputs, out IList<IServiceTestOutput> failingTestOutputs)
            {
                var pendingTestOutputs = pendingOutputs as IList<IServiceTestOutput> ?? pendingOutputs?.ToList();
                invalidTestOutputs = invalidOutputs as IList<IServiceTestOutput> ?? invalidOutputs?.ToList();
                failingTestOutputs = failingOutputs as IList<IServiceTestOutput> ?? failingOutputs?.ToList();
                return pendingTestOutputs;
            }
            
            static bool TestPassedBasedOnSteps(bool hasPendingSteps, bool hasInvalidSteps, bool hasFailingSteps) => !hasPendingSteps && !hasInvalidSteps && !hasFailingSteps;

            static bool TestPassedBasedOnOutputs(bool pending, bool invalid, bool failing) => !pending && !invalid && !failing;
            
        }

        static void UpdateTestWithStepValues(IServiceTestModelTO test)
        {
            UpdateTestWithStepValuesImplimentation.UpdateTestWithStepValues(test);
        }

        public struct UpdateFailureMessageArgs
        {
            public bool HasPendingSteps { get; set; }
            public IList<IServiceTestStep> PendingTestSteps {get; set;}
            public bool HasInvalidSteps {get; set;}
            public IList<IServiceTestStep> InvalidTestSteps {get; set;}
            public bool HasFailingSteps {get; set;}
            public IList<IServiceTestStep> FailingTestSteps {get; set;}
            public bool HasPendingOutputs {get; set;}
            public IList<IServiceTestOutput> PendingTestOutputs {get; set;}
            public bool HasInvalidOutputs {get; set;}
            public IList<IServiceTestOutput> InvalidTestOutputs {get; set;}
            public bool HasFailingOutputs {get; set;}
            public IList<IServiceTestOutput> FailingTestOutputs {get; set;}
            public List<IServiceTestStep> ServiceTestSteps {get; set;}
        }

        static StringBuilder UpdateFailureMessage(UpdateFailureMessageArgs messageArgs)
        {
            var failureMessage = new StringBuilder();
            if (messageArgs.HasFailingSteps)
            {
                AppendServiceTestFailedSteps(messageArgs.FailingTestSteps, failureMessage);
            }
            if (messageArgs.HasInvalidSteps)
            {
                AppendServiceTestInvalidStep(messageArgs.InvalidTestSteps, failureMessage);
            }
            if (messageArgs.HasPendingSteps)
            {
                AppendServiceTestPendingSteps(messageArgs.PendingTestSteps, failureMessage);
            }

            if (messageArgs.HasFailingOutputs)
            {
                AppendServiceTestFailedOutput(messageArgs.FailingTestOutputs, failureMessage);
            }
            if (messageArgs.HasInvalidOutputs)
            {
                AppendServiceTestInvalidOutputVariables(messageArgs.InvalidTestOutputs, failureMessage);
            }
            if (messageArgs.HasPendingOutputs)
            {
                AppendServiceTestPendingOutput(messageArgs.PendingTestOutputs, failureMessage);
            }
            
            if (messageArgs.ServiceTestSteps != null)
            {
                failureMessage.AppendLine(string.Join("", messageArgs.ServiceTestSteps.Where(step => !string.IsNullOrEmpty(step.Result?.Message)).Select(step => step.Result?.Message)));
            }
            return failureMessage;
        }

        private static void AppendServiceTestPendingOutput(IList<IServiceTestOutput> pendingTestOutputs, StringBuilder failureMessage)
        {
            foreach (var serviceTestStep in pendingTestOutputs)
            {
                failureMessage.AppendLine("Pending Output for Variable: " + serviceTestStep.Variable);
            }
        }

        private static void AppendServiceTestInvalidOutputVariables(IList<IServiceTestOutput> invalidTestOutputs, StringBuilder failureMessage)
        {
            foreach (var serviceTestStep in invalidTestOutputs)
            {
                failureMessage.AppendLine("Invalid Output for Variable: " + serviceTestStep.Variable);
                failureMessage.AppendLine("Message: " + serviceTestStep.Result?.Message);
            }
        }

        private static void AppendServiceTestFailedOutput(IList<IServiceTestOutput> failingTestOutputs, StringBuilder failureMessage)
        {
            foreach (var serviceTestStep in failingTestOutputs)
            {
                failureMessage.AppendLine("Failed Output For Variable: " + serviceTestStep.Variable + " ");
                failureMessage.AppendLine("Message: " + serviceTestStep.Result?.Message);
            }
        }

        private static void AppendServiceTestPendingSteps(IList<IServiceTestStep> pendingTestSteps, StringBuilder failureMessage)
        {
            foreach (var serviceTestStep in pendingTestSteps)
            {
                failureMessage.AppendLine("Pending Step: " + serviceTestStep.StepDescription);
            }
        }

        private static void AppendServiceTestInvalidStep(IList<IServiceTestStep> invalidTestSteps, StringBuilder failureMessage)
        {
            foreach (var serviceTestStep in invalidTestSteps)
            {
                failureMessage.AppendLine("Invalid Step: " + serviceTestStep.StepDescription + " ");
                failureMessage.AppendLine("Message: " + serviceTestStep.Result?.Message);
            }
        }

        private static void AppendServiceTestFailedSteps(IList<IServiceTestStep> failingTestSteps, StringBuilder failureMessage)
        {
            foreach (var serviceTestStep in failingTestSteps)
            {
                failureMessage.AppendLine("Failed Step: " + serviceTestStep.StepDescription + " ");
                failureMessage.AppendLine("Message: " + serviceTestStep.Result?.Message);
            }
        }

        private static void SetServiceTestOutputResultMessage(IServiceTestOutput[] serviceTestOutputs)
        {
            foreach (var serviceTestOutput in serviceTestOutputs)
            {
                serviceTestOutput.Result.Message = DataListUtil.StripBracketsFromValue(serviceTestOutput.Result.Message);
            }
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


        public override IDSFDataObject Execute(IDSFDataObject inputs, IDev2Activity activity) => null;

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
                    foreach (var error in dsfDataObject.Environment.Errors)
                    {
                        dsfDataObject.Environment.AllErrors.Add(error);
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
                        NextInSequence(foundTestStep, acts);
                    }
                }
                else if (foundTestStep.ActivityType == typeof(DsfForEachActivity).Name)
                {
                    if (resource is DsfForEachActivity forEach && foundTestStep.Children != null)
                    {
                        var replacement = NextActivity(forEach.DataFunc.Handler as IDev2Activity, foundTestStep.Children.ToList()) as Activity;
                        forEach.DataFunc.Handler = replacement;
                    }

                }
                else if (foundTestStep.ActivityType == typeof(DsfSelectAndApplyActivity).Name)
                {
                    if (resource is DsfSelectAndApplyActivity forEach && foundTestStep.Children != null)
                    {
                        var replacement = NextActivity(forEach.ApplyActivityFunc.Handler as IDev2Activity, foundTestStep.Children.ToList()) as Activity;
                        forEach.ApplyActivityFunc.Handler = replacement;
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

        private static void NextInSequence(IServiceTestStep foundTestStep, System.Collections.ObjectModel.Collection<Activity> acts)
        {
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
}