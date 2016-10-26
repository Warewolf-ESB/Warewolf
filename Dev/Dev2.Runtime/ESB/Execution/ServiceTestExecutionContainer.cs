using System;
using System.Activities;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using Dev2.Activities;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Communication;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.Diagnostics.Debug;
using Dev2.DynamicServices.Objects;
using Dev2.Interfaces;
using Dev2.Runtime.ESB.WF;
using Dev2.Runtime.Execution;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Security;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Services.Security;
using Dev2.Workspaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Security.Encryption;
using Warewolf.Storage;
// ReSharper disable ParameterTypeCanBeEnumerable.Local

// ReSharper disable CyclomaticComplexity

namespace Dev2.Runtime.ESB.Execution
{
    public class ServiceTestExecutionContainer : EsbExecutionContainer
    {
        private readonly EsbExecuteRequest _request;

        public ServiceTestExecutionContainer(ServiceAction sa, IDSFDataObject dataObj, IWorkspace theWorkspace, IEsbChannel esbChannel, EsbExecuteRequest request)
            : base(sa, dataObj, theWorkspace, esbChannel)
        {
            _request = request;
        }

        /// <summary>
        /// Executes the specified errors.
        /// </summary>
        /// <param name="errors">The errors.</param>
        /// <param name="update"></param>
        /// <returns></returns>
        public override Guid Execute(out ErrorResultTO errors, int update)
        {

            errors = new ErrorResultTO();
            // WorkflowApplicationFactory wfFactor = new WorkflowApplicationFactory();
            Guid result = GlobalConstants.NullDataListID;


            Dev2Logger.Debug("Entered Wf Container");

            // Set Service Name
            DataObject.ServiceName = ServiceAction.ServiceName;

            // Set server ID, only if not set yet - original server;
            if (DataObject.ServerID == Guid.Empty)
                DataObject.ServerID = HostSecurityProvider.Instance.ServerID;

            // Set resource ID, only if not set yet - original resource;
            if (DataObject.ResourceID == Guid.Empty && ServiceAction?.Service != null)
                DataObject.ResourceID = ServiceAction.Service.ID;

            // Travis : Now set Data List
            DataObject.DataList = ServiceAction.DataListSpecification;
            // Set original instance ID, only if not set yet - original resource;
            if (DataObject.OriginalInstanceID == Guid.Empty)
                DataObject.OriginalInstanceID = DataObject.DataListID;
            Dev2Logger.Info($"Started Execution for Service Name:{DataObject.ServiceName} Resource Id:{DataObject.ResourceID} Mode:{(DataObject.IsDebug ? "Debug" : "Execute")}");
            //Set execution origin
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
           
            
            ErrorResultTO to = errors;
            var serviceTestModelTO = TestCatalog.Instance.FetchTest(DataObject.ResourceID, DataObject.TestName);
            if (serviceTestModelTO == null)
            {
                Dev2JsonSerializer serializer = new Dev2JsonSerializer();
                var testRunResult = new TestRunResult
                {
                    TestName = DataObject.TestName,
                    Result = RunResult.TestInvalid
                };
                Dev2Logger.Error($"Test {DataObject.TestName} for Resource {DataObject.ServiceName} ID {DataObject.ResourceID}");
                testRunResult.Message = $"Test {DataObject.TestName} for Resource {DataObject.ServiceName} ID {DataObject.ResourceID}";                
                _request.ExecuteResult = serializer.SerializeToBuilder(testRunResult);
                return Guid.NewGuid();
            }
            if (serviceTestModelTO.Enabled)
            {
                if (serviceTestModelTO.AuthenticationType == AuthenticationType.User)
                {
                    Impersonator impersonator = new Impersonator();
                    var userName = serviceTestModelTO.UserName;
                    var domain = "";
                    if (userName.Contains("\\"))
                    {
                        var slashIndex = userName.IndexOf("\\", StringComparison.InvariantCultureIgnoreCase);
                        domain = userName.Substring(0, slashIndex);
                        userName = userName.Substring(slashIndex + 1);
                    }
                    else if (userName.Contains("@"))
                    {
                        var atIndex = userName.IndexOf("@", StringComparison.InvariantCultureIgnoreCase);
                        userName = userName.Substring(0, atIndex);
                        domain = userName.Substring(atIndex + 1);
                    }
                    var hasImpersonated = impersonator.Impersonate(userName, domain, DpapiWrapper.DecryptIfEncrypted(serviceTestModelTO.Password));
                    if (!hasImpersonated)
                    {
                        DataObject.Environment.AllErrors.Add("Unauthorized to execute this resource.");
                        DataObject.StopExecution = true;
                    }
                }
                else if (serviceTestModelTO.AuthenticationType == AuthenticationType.Public)
                {
                    Thread.CurrentPrincipal = GlobalConstants.GenericPrincipal;
                }
                var userPrinciple = Thread.CurrentPrincipal;                
                Common.Utilities.PerformActionInsideImpersonatedContext(userPrinciple, () =>
                {
                    result = ExecuteWf(to, serviceTestModelTO);
                });
                foreach (var err in DataObject.Environment.Errors)
                {
                    errors.AddError(err, true);
                }
                foreach (var err in DataObject.Environment.AllErrors)
                {
                    errors.AddError(err, true);
                }

                Dev2Logger.Info($"Completed Execution for Service Name:{DataObject.ServiceName} Resource Id: {DataObject.ResourceID} Mode:{(DataObject.IsDebug ? "Debug" : "Execute")}");
            }
            return result;
        }

        private static void AddRecordsetsInputs(IEnumerable<IServiceTestInput> recSets, IExecutionEnvironment environment)
        {
            if(recSets != null)
            {
                var groupedRecsets = recSets.GroupBy(item => DataListUtil.ExtractRecordsetNameFromValue(item.Variable));
                foreach (var groupedRecset in groupedRecsets)
                {
                    var dataListItems = groupedRecset.GroupBy(item => DataListUtil.ExtractIndexRegionFromRecordset(item.Variable));
                    foreach (var dataListItem in dataListItems)
                    {
                        List<IServiceTestInput> recSetsToAssign = new List<IServiceTestInput>();
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
                            foreach(var serviceTestInput in recSetsToAssign)
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
            Guid result = new Guid();
            var wfappUtils = new WfApplicationUtils();
            ErrorResultTO invokeErrors;
            var resourceID = DataObject.ResourceID;
            if (test?.Inputs != null)
            {
                AddRecordsetsInputs(test.Inputs.Where(input => DataListUtil.IsValueRecordset(input.Variable) && !input.Variable.Contains("@")),DataObject.Environment);
                foreach (var input in test.Inputs)
                {
                    var variable = DataListUtil.AddBracketsToValueIfNotExist(input.Variable);
                    var value = input.Value;
                    if (variable.StartsWith("[[@"))
                    {
                        var jContainer = JsonConvert.DeserializeObject(value) as JObject;
                        DataObject.Environment.AddToJsonObjects(variable, jContainer);
                    }
                    else if(!DataListUtil.IsValueRecordset(input.Variable))
                    {
                        if (!input.EmptyIsNull || !string.IsNullOrEmpty(value))
                        {
                            DataObject.Environment.Assign(variable, value, 0);
                        }
                    }
                }
            }
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            try
            {
                IExecutionToken exeToken = new ExecutionToken { IsUserCanceled = false };
                DataObject.ExecutionToken = exeToken;

                if (DataObject.IsDebugMode())
                {
                    wfappUtils.DispatchDebugState(DataObject, StateType.Start, DataObject.Environment.HasErrors(), DataObject.Environment.FetchErrors(), out invokeErrors, DateTime.Now, true, false, false);
                }
                
                var testRunResult = Eval(resourceID, DataObject, test);

                if (DataObject.IsDebugMode())
                {
                    wfappUtils.DispatchDebugState(DataObject, StateType.End, DataObject.Environment.HasErrors(), DataObject.Environment.FetchErrors(), out invokeErrors, DataObject.StartTime, false, true);
                }
                if(testRunResult != null)
                {
                    if(test != null)
                        testRunResult.DebugForTest = TestDebugMessageRepo.Instance.FetchDebugItems(resourceID, test.TestName);
                    _request.ExecuteResult = serializer.SerializeToBuilder(testRunResult);
                }
                result = DataObject.DataListID;
            }
            catch (InvalidWorkflowException iwe)
            {
                Dev2Logger.Error(iwe);
                var msg = iwe.Message;

                int start = msg.IndexOf("Flowchart ", StringComparison.Ordinal);
                to?.AddError(start > 0 ? GlobalConstants.NoStartNodeError : iwe.Message);
                var failureMessage = DataObject.Environment.FetchErrors();
                wfappUtils.DispatchDebugState(DataObject, StateType.End, DataObject.Environment.HasErrors(), failureMessage, out invokeErrors, DataObject.StartTime, false, true);

                // ReSharper disable once PossibleNullReferenceException
                test.TestFailing = false;
                test.TestPassed = false;
                test.TestPending = false;
                test.TestInvalid = true;
                test.LastRunDate = DateTime.Now;


                Common.Utilities.PerformActionInsideImpersonatedContext(Common.Utilities.ServerUser, () => { TestCatalog.Instance.SaveTest(resourceID, test); });

                var testRunResult = new TestRunResult { TestName = test.TestName };
                if (test.TestInvalid)
                {
                    testRunResult.Result = RunResult.TestInvalid;
                    testRunResult.Message = failureMessage;
                    Dev2Logger.Error($"Test {DataObject.TestName} for Resource {DataObject.ServiceName} ID {DataObject.ResourceID} marked invalid in exception for no start node");
                }
                testRunResult.DebugForTest = TestDebugMessageRepo.Instance.FetchDebugItems(resourceID, test.TestName);
                if(_request != null)
                    _request.ExecuteResult = serializer.SerializeToBuilder(testRunResult);
            }
            catch (Exception ex)
            {
                Dev2Logger.Error(ex);
                to.AddError(ex.Message);
                var failureMessage = DataObject.Environment.FetchErrors();
                wfappUtils.DispatchDebugState(DataObject, StateType.End, DataObject.Environment.HasErrors(), failureMessage, out invokeErrors, DataObject.StartTime, false, true);
                // ReSharper disable once PossibleNullReferenceException
                test.TestFailing = false;
                test.TestPassed = false;
                test.TestPending = false;
                test.TestInvalid = true;
                test.LastRunDate = DateTime.Now;


                Common.Utilities.PerformActionInsideImpersonatedContext(Common.Utilities.ServerUser, () => { TestCatalog.Instance.SaveTest(resourceID, test); });

                var testRunResult = new TestRunResult { TestName = test.TestName };
                if (test.TestInvalid)
                {
                    testRunResult.Result = RunResult.TestInvalid;
                    testRunResult.Message = ex.Message;
                    Dev2Logger.Error($"Test {DataObject.TestName} for Resource {DataObject.ServiceName} ID {DataObject.ResourceID} marked invalid in general exception");
                }
                testRunResult.DebugForTest = TestDebugMessageRepo.Instance.FetchDebugItems(resourceID, test.TestName);
                _request.ExecuteResult = serializer.SerializeToBuilder(testRunResult);
            }
            return result;
        }
        
        private TestRunResult Eval(Guid resourceID, IDSFDataObject dataObject,IServiceTestModelTO test)
        {
            Dev2Logger.Debug("Getting Resource to Execute");
            IDev2Activity resource = ResourceCatalog.Instance.Parse(TheWorkspace.ID, resourceID);
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            var execPlan = serializer.SerializeToBuilder(resource);
            var clonedExecPlan = serializer.Deserialize<IDev2Activity>(execPlan);
            Dev2Logger.Debug("Got Resource to Execute");
            
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
                        EvalInner(dataObject, clonedExecPlan, dataObject.ForEachUpdateValue,test.TestSteps);
                        if (test.Outputs != null)
                        {
                            foreach (var output in test.Outputs)
                            {
                                var variable = DataListUtil.AddBracketsToValueIfNotExist(output.Variable);
                                var value = output.Value;

                                var result = dataObject.Environment.Eval(variable, 0);
                                if (result.IsWarewolfAtomResult)
                                {
                                    var x = (result as CommonFunctions.WarewolfEvalResult.WarewolfAtomResult)?.Item;
                                    // ReSharper disable once PossibleNullReferenceException
                                    var actualValue = x.ToString();
                                    if (!string.Equals(actualValue,value))
                                    {
                                        testPassed = false;
                                        failureMessage.AppendLine(string.Format(Warewolf.Resource.Messages.Messages.Test_FailureMessage_Equals, value, variable, actualValue));
                                    }
                                }
                            }
                        }
                    }
                }
                var hasErrors = DataObject.Environment.HasErrors();
                if (test.ErrorExpected)
                {
                    testPassed = hasErrors && testPassed;
                }
                else if (test.NoErrorExpected)
                {
                    testPassed = !hasErrors && testPassed;
                    if (hasErrors)
                    {
                        failureMessage.AppendLine(DataObject.Environment.FetchErrors());
                    }
                }

                test.TestFailing = !testPassed;
                test.TestPassed = testPassed;
                test.TestPending = false;
                test.TestInvalid = false;
                test.LastRunDate = DateTime.Now;


                Common.Utilities.PerformActionInsideImpersonatedContext(Common.Utilities.ServerUser, () => { TestCatalog.Instance.SaveTest(resourceID, test); });

                var testRunResult = new TestRunResult { TestName = test.TestName };
                if (test.TestFailing)
                {
                    testRunResult.Result = RunResult.TestFailed;
                    testRunResult.Message = failureMessage.ToString();
                }
                if (test.TestPassed)
                {
                    testRunResult.Result = RunResult.TestPassed;
                }
                return testRunResult;
            }
            throw new Exception($"Test {dataObject.TestName} for Resource {dataObject.ServiceName} ID {resourceID}");
        }


        public override IDSFDataObject Execute(IDSFDataObject inputs, IDev2Activity activity)
        {
            return null;
        }

        static void EvalInner(IDSFDataObject dsfDataObject, IDev2Activity resource, int update,List<IServiceTestStep> testSteps)
        {
            if (resource == null)
            {
                throw new InvalidOperationException(GlobalConstants.NoStartNodeError);
            }
            WorkflowExecutionWatcher.HasAWorkflowBeenExecuted = true;
            resource = MockActivity(resource, testSteps);
            var next = resource.Execute(dsfDataObject, update);            
            while (next != null)
            {
                if (!dsfDataObject.StopExecution)
                {
                    next = MockActivity(next, testSteps);
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

        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        private static IDev2Activity MockActivity(IDev2Activity resource, List<IServiceTestStep> testSteps)
        {
            var foundTestStep = testSteps?.FirstOrDefault(step => step.UniqueId.ToString() == resource.UniqueID);
            if(foundTestStep != null && foundTestStep.Type == StepType.Mock)
            {
                if(foundTestStep.ActivityType == typeof(DsfDecision).Name)
                {
                    var serviceTestOutput = foundTestStep.StepOutputs.FirstOrDefault(output => output.Variable == "Condition Result");
                    if(serviceTestOutput != null)
                    {
                        resource = new TestMockDecisionStep(resource as DsfDecision) { NameOfArmToReturn = serviceTestOutput.Value };
                    }
                }
                else if(foundTestStep.ActivityType == typeof(DsfSwitch).Name)
                {
                    var serviceTestOutput = foundTestStep.StepOutputs.FirstOrDefault(output => output.Variable == "Condition Result");
                    if(serviceTestOutput != null)
                    {
                        resource = new TestMockSwitchStep(resource as DsfSwitch) { ConditionToUse = serviceTestOutput.Value };
                    }
                }
                else
                {
                    resource = new TestMockStep(resource, foundTestStep.StepOutputs);
                }
            }
            return resource;
        }
    }

    public class TestMockStep : DsfActivityAbstract<string>
    {
        private readonly IDev2Activity _originalActivity;
        private readonly List<IServiceTestOutput> _outputs;

        public TestMockStep(IDev2Activity originalActivity , List<IServiceTestOutput> outputs)
        {
            _originalActivity = originalActivity;
            _outputs = outputs;
            var act = originalActivity as DsfBaseActivity;
            if(act != null)
                DisplayName = act.DisplayName;
        }

        #region Overrides of DsfNativeActivity<string>

        protected override void OnExecute(NativeActivityContext context)
        {
        }

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates)
        {
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates)
        {
        }

        public override IList<DsfForEachItem> GetForEachInputs()
        {
            return null;
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            return null;
        }

        protected override void ExecuteTool(IDSFDataObject dataObject, int update)
        {
            AddRecordsetsInputs(_outputs.Where(output => DataListUtil.IsValueRecordset(output.Variable) && !output.Variable.Contains("@")), dataObject.Environment);
            foreach (var output in _outputs)
            {
                var variable = DataListUtil.AddBracketsToValueIfNotExist(output.Variable);
                var value = output.Value;
                if (variable.StartsWith("[[@"))
                {
                    var jContainer = JsonConvert.DeserializeObject(value) as JObject;
                    dataObject.Environment.AddToJsonObjects(variable, jContainer);
                }
                else if (!DataListUtil.IsValueRecordset(output.Variable))
                {
                    dataObject.Environment.Assign(variable, value, 0);
                }
            }
            NextNodes = _originalActivity.NextNodes;
        }


        private static void AddRecordsetsInputs(IEnumerable<IServiceTestOutput> recSets, IExecutionEnvironment environment)
        {
            if(recSets != null)
            {
                var groupedRecsets = recSets.GroupBy(item => DataListUtil.ExtractRecordsetNameFromValue(item.Variable));
                foreach (var groupedRecset in groupedRecsets)
                {
                    var dataListItems = groupedRecset.GroupBy(item => DataListUtil.ExtractIndexRegionFromRecordset(item.Variable));
                    foreach (var dataListItem in dataListItems)
                    {
                        List<IServiceTestOutput> recSetsToAssign = new List<IServiceTestOutput>();
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
                                environment.Assign(DataListUtil.AddBracketsToValueIfNotExist(serviceTestInput.Variable), serviceTestInput.Value, 0);
                            }
                        }
                    }
                }
            }
        }

        #endregion
    }
}