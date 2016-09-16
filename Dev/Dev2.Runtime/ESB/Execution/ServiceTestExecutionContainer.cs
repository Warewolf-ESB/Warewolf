using System;
using System.Activities;
using System.Diagnostics.CodeAnalysis;
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
using Warewolf.Security.Encryption;

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

        Guid ExecuteWf(ErrorResultTO to, IServiceTestModelTO test)
        {
            Guid result = new Guid();
            var wfappUtils = new WfApplicationUtils();
            ErrorResultTO invokeErrors;
            var resourceID = DataObject.ResourceID;
            if (test?.Inputs != null)
            {
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
                        if (!input.EmptyIsNull || !string.IsNullOrEmpty(value))
                        {
                            DataObject.Environment.Assign(variable, value, 0);
                        }
                    }
                }
            }
            try
            {
                IExecutionToken exeToken = new ExecutionToken { IsUserCanceled = false };
                DataObject.ExecutionToken = exeToken;

                if (DataObject.IsDebugMode())
                {
                    wfappUtils.DispatchDebugState(DataObject, StateType.Start, DataObject.Environment.HasErrors(), DataObject.Environment.FetchErrors(), out invokeErrors, DateTime.Now, true, false, false);
                }
                Dev2JsonSerializer serializer = new Dev2JsonSerializer();
                var testRunResult = Eval(resourceID, DataObject, test);

                if (DataObject.IsDebugMode())
                {
                    wfappUtils.DispatchDebugState(DataObject, StateType.End, DataObject.Environment.HasErrors(), DataObject.Environment.FetchErrors(), out invokeErrors, DataObject.StartTime, false, true);
                }
                testRunResult.DebugForTest = TestDebugMessageRepo.Instance.FetchDebugItems(resourceID, test.TestName);
                _request.ExecuteResult = serializer.SerializeToBuilder(testRunResult);
                result = DataObject.DataListID;
            }
            catch (InvalidWorkflowException iwe)
            {
                Dev2Logger.Error(iwe);
                var msg = iwe.Message;

                int start = msg.IndexOf("Flowchart ", StringComparison.Ordinal);
                to.AddError(start > 0 ? GlobalConstants.NoStartNodeError : iwe.Message);
            }
            catch (Exception ex)
            {
                Dev2Logger.Error(ex);
                to.AddError(ex.Message);
                wfappUtils.DispatchDebugState(DataObject, StateType.End, DataObject.Environment.HasErrors(), DataObject.Environment.FetchErrors(), out invokeErrors, DataObject.StartTime, false, true);
            }
            return result;
        }

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public void Eval(DynamicActivity flowchartProcess, IDSFDataObject dsfDataObject, int update)
        {
            IDev2Activity resource = new ActivityParser().Parse(flowchartProcess);

            EvalInner(dsfDataObject, resource, update);
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
                        EvalInner(dataObject, clonedExecPlan, dataObject.ForEachUpdateValue);
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

        static void EvalInner(IDSFDataObject dsfDataObject, IDev2Activity resource, int update)
        {
            if (resource == null)
            {
                throw new InvalidOperationException(GlobalConstants.NoStartNodeError);
            }
            WorkflowExecutionWatcher.HasAWorkflowBeenExecuted = true;
            var next = resource.Execute(dsfDataObject, update);
            while (next != null)
            {
                if (!dsfDataObject.StopExecution)
                {
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
    }
}