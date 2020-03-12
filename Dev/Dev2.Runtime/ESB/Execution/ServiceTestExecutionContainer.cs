#pragma warning disable
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
using System.Threading;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Enums;
using Dev2.Communication;
using Dev2.Data;
using Dev2.Data.TO;
using Dev2.Data.Util;
using Dev2.Diagnostics.Debug;
using Dev2.DynamicServices.Objects;
using Dev2.Interfaces;
using Dev2.Runtime.ESB.Management;
using Dev2.Runtime.ESB.WF;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.Security;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Workspaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Warewolf.Execution;
using Warewolf.Resource.Messages;
using Warewolf.Storage;
using Warewolf.Storage.Interfaces;

namespace Dev2.Runtime.ESB.Execution
{
    public class ServiceTestExecutionContainer : EsbExecutionContainer
    {
        readonly EsbExecuteRequest _request;
        readonly IExecutingEvaluator _inner;

        public ServiceTestExecutionContainer(ServiceAction sa, IDSFDataObject dataObj, IWorkspace theWorkspace, IEsbChannel esbChannel, EsbExecuteRequest request)
            : base(sa, dataObj, theWorkspace, esbChannel)
        {
            _request = request;
            TstCatalog = TestCatalog.Instance;
            ResourceCat = ResourceCatalog.Instance;
            _inner = new ExecutingEvaluator(dataObj, ResourceCat, theWorkspace, _request);
        }

        protected ITestCatalog TstCatalog { get; set; }
        protected IResourceCatalog ResourceCat { get; set; }

        public override Guid Execute(out ErrorResultTO errors, int update)
        {
            errors = new ErrorResultTO();
            var testCatalog = TstCatalog ?? TestCatalog.Instance;

            var result = GlobalConstants.NullDataListID;

            Dev2Logger.Debug("Entered Wf Container", DataObject.ExecutionID.ToString());

            PrepareDataObjectForExecution();

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

            CheckAuthentication(serviceTestModelTo);
            var userPrinciple = Thread.CurrentPrincipal;
            Common.Utilities.PerformActionInsideImpersonatedContext(userPrinciple, () =>
            {
                result = TryExecuteWf(to, serviceTestModelTo);
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

        private void PrepareDataObjectForExecution()
        {
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
        }

        private void CheckAuthentication(IServiceTestModelTO serviceTestModelTo)
        {
            if (serviceTestModelTo.AuthenticationType == AuthenticationType.User)
            {
                var resource = ResourceCat.GetResource(GlobalConstants.ServerWorkspaceID, DataObject.ResourceID);
                var testNotauthorizedmsg = string.Format(Messages.Test_NotAuthorizedMsg, resource?.ResourceName);
                DataObject.Environment.AllErrors.Add(testNotauthorizedmsg);
                DataObject.StopExecution = true;
            }
            else
            {
                if (serviceTestModelTo.AuthenticationType == AuthenticationType.Public)
                {
                    Thread.CurrentPrincipal = GlobalConstants.GenericPrincipal;
                    DataObject.ExecutingUser = GlobalConstants.GenericPrincipal;
                }
            }
        }

        public override bool CanExecute(Guid resourceId, IDSFDataObject dataObject, AuthorizationContext authorizationContext) => true;
        public override bool CanExecute(IEsbManagementEndpoint eme, IDSFDataObject dataObject)
        {  var resourceId = eme.GetResourceID(Request.Args);
            var authorizationContext = eme.GetAuthorizationContextForService();
            var isFollower = string.IsNullOrWhiteSpace(Config.Cluster.LeaderServerKey);
            if (isFollower && eme.CanExecute(new CanExecuteArg{ IsFollower = isFollower }))
            {
                return false;
            }
            return CanExecute(resourceId, dataObject, authorizationContext);
        }

        static void AddRecordsetsInputs(IEnumerable<IServiceTestInput> recSets, IExecutionEnvironment environment)
        {
            if (recSets is null)
            {
                return;
            }
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

        Guid TryExecuteWf(ErrorResultTO to, IServiceTestModelTO test)
        {
            var result = new Guid();
            var wfappUtils = new WfApplicationUtils();
            var invokeErrors = new ErrorResultTO();
            var resourceId = DataObject.ResourceID;

            if (test?.Inputs != null)
            {
                AddTestInputsToJsonOrRecordset(test);
            }

            var serializer = new Dev2JsonSerializer();
            try
            {
                result = _inner.ExecuteWf(new TestExecutionContext {
                    _test = test,
                    _wfappUtils = wfappUtils,
                    _invokeErrors = invokeErrors,
                    _serializer = serializer,
                });
            }
            catch (InvalidWorkflowException iwe)
            {
                Dev2Logger.Error(iwe, DataObject.ExecutionID.ToString());
                var msg = iwe.Message;

                var start = msg.IndexOf("Flowchart ", StringComparison.Ordinal);
                to?.AddError(start > 0 ? GlobalConstants.NoStartNodeError : iwe.Message);
                var failureMessage = DataObject.Environment.FetchErrors();
                wfappUtils.DispatchDebugState(DataObject, StateType.End, out invokeErrors);

                SetTestRunResultAfterInvalidWorkflowException(test, resourceId, serializer, failureMessage);
            }
            catch (Exception ex)
            {
                Dev2Logger.Error(ex, GlobalConstants.WarewolfError);
                to.AddError(ex.Message);
                wfappUtils.DispatchDebugState(DataObject, StateType.End, out invokeErrors);

                SetTestRunResultAfterException(test, resourceId, serializer, ex);
            }
            return result;
        }

        private void SetTestRunResultAfterException(IServiceTestModelTO test, Guid resourceId, Dev2JsonSerializer serializer, Exception ex)
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
                Dev2Logger.Error($"Test {DataObject.TestName} for Resource {DataObject.ServiceName} ID {DataObject.ResourceID} marked invalid in general exception", GlobalConstants.WarewolfError);
            }
            testRunResult.DebugForTest = TestDebugMessageRepo.Instance.FetchDebugItems(resourceId, test.TestName);
            _request.ExecuteResult = serializer.SerializeToBuilder(testRunResult);
        }

        private void SetTestRunResultAfterInvalidWorkflowException(IServiceTestModelTO test, Guid resourceId, Dev2JsonSerializer serializer, string failureMessage)
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
                Dev2Logger.Error($"Test {DataObject.TestName} for Resource {DataObject.ServiceName} ID {DataObject.ResourceID} marked invalid in exception for no start node", DataObject.ExecutionID.ToString());
            }
            testRunResult.DebugForTest = TestDebugMessageRepo.Instance.FetchDebugItems(resourceId, test.TestName);
            if (_request != null)
            {
                _request.ExecuteResult = serializer.SerializeToBuilder(testRunResult);
            }
        }

        private void AddTestInputsToJsonOrRecordset(IServiceTestModelTO test)
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
                    AddToRecordsetObjects(input, variable, value);
                }
            }
        }

        private void AddToRecordsetObjects(IServiceTestInput input, string variable, string value)
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

        public override IDSFDataObject Execute(IDSFDataObject inputs, IDev2Activity activity) => null;
    }
}