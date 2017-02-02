/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Activities.Designers2.Rename;
using Dev2.Activities.Specs.BaseTypes;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.ESB.Execution;
using Dev2.Runtime.Execution;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Workspaces;
using Moq;
using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Data.TO;
using Dev2.Interfaces;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Tools.Specs.BaseTypes;

namespace Dev2.Activities.Specs.Toolbox.FileAndFolder.Rename
{
    [Binding]
    public class RenameSteps : FileToolsBase
    {
        private readonly ScenarioContext scenarioContext;

        public RenameSteps(ScenarioContext scenarioContext)
            : base(scenarioContext)
        {
            if (scenarioContext == null) throw new ArgumentNullException("scenarioContext");
            this.scenarioContext = scenarioContext;
        }

        [When(@"the rename file tool is executed")]
        public void WhenTheRenameFileToolIsExecuted()
        {
            BuildDataList();
            IDSFDataObject result = ExecuteProcess(isDebug: true, throwException: false);
            scenarioContext.Add("result", result);
        }

        protected new IDSFDataObject ExecuteProcess(IDSFDataObject dataObject = null, bool isDebug = false, IEsbChannel channel = null, bool isRemoteInvoke = false, bool throwException = true, bool isDebugMode = false, Guid currentEnvironmentId = default(Guid), bool overrideRemote = false)
        {
            var svc = new ServiceAction { Name = "TestAction", ServiceName = "UnitTestService" };
            svc.SetActivity(FlowchartProcess);
            Mock<IEsbChannel> mockChannel = new Mock<IEsbChannel>();

            if (CurrentDl == null)
            {
                CurrentDl = TestData;
            }

            var errors = new ErrorResultTO();
            if (ExecutionId == Guid.Empty)
            {
                if (dataObject != null)
                {
                    dataObject.ExecutingUser = User;
                    dataObject.DataList = new StringBuilder(CurrentDl);
                }
            }

            if (errors.HasErrors())
            {
                string errorString = errors.FetchErrors().Aggregate(string.Empty, (current, item) => current + item);

                if (throwException)
                {
                    throw new Exception(errorString);
                }
            }

            if (dataObject == null)
            {
                dataObject = new DsfDataObject(CurrentDl, ExecutionId)
                {
                    // NOTE: WorkflowApplicationFactory.InvokeWorkflowImpl() will use HostSecurityProvider.Instance.ServerID
                    //       if this is NOT provided which will cause the tests to fail!
                    ServerID = Guid.NewGuid(),
                    ExecutingUser = User,
                    IsDebug = isDebugMode,
                    EnvironmentID = currentEnvironmentId,
                    IsRemoteInvokeOverridden = overrideRemote,
                    DataList = new StringBuilder(CurrentDl)
                };
            }
            if (!string.IsNullOrEmpty(TestData))
            {
                ExecutionEnvironmentUtils.UpdateEnvironmentFromXmlPayload(DataObject, new StringBuilder(TestData), CurrentDl, 0);
            }
            dataObject.IsDebug = isDebug;

            // we now need to set a thread ID ;)
            dataObject.ParentThreadID = 1;

            if (isRemoteInvoke)
            {
                dataObject.RemoteInvoke = true;
                dataObject.RemoteInvokerID = Guid.NewGuid().ToString();
            }

            var esbChannel = mockChannel.Object;
            if (channel != null)
            {
                esbChannel = channel;
            }
            dataObject.ExecutionToken = new ExecutionToken();
            WfExecutionContainer wfec = new WfExecutionContainer(svc, dataObject, WorkspaceRepository.Instance.ServerWorkspace, esbChannel);

            errors.ClearErrors();
            CustomContainer.Register<IActivityParser>(new ActivityParser());
            if (dataObject.ResourceID == Guid.Empty)
            {
                dataObject.ResourceID = Guid.NewGuid();
            }
            dataObject.Environment = DataObject.Environment;
            wfec.Eval(FlowchartProcess, dataObject, 0);
            DebugItemResults = GetDebugOutputItemResults(FlowchartProcess);
            DataObject = dataObject;
            return dataObject;
        }

        public List<IDebugItemResult> DebugItemResults { get; set; }

        protected override void BuildDataList()
        {
            BuildShapeAndTestData();

            string privateKeyFile;
            string destPrivateKeyFile;
            scenarioContext.TryGetValue(CommonSteps.SourcePrivatePublicKeyFile, out privateKeyFile);
            scenarioContext.TryGetValue(CommonSteps.DestinationPrivateKeyFile, out destPrivateKeyFile);
            var rename = new DsfPathRename
            {
                InputPath = scenarioContext.Get<string>(CommonSteps.SourceHolder),
                Username = scenarioContext.Get<string>(CommonSteps.SourceUsernameHolder),
                Password = scenarioContext.Get<string>(CommonSteps.SourcePasswordHolder),
                OutputPath = scenarioContext.Get<string>(CommonSteps.DestinationHolder),
                DestinationUsername = scenarioContext.Get<string>(CommonSteps.DestinationUsernameHolder),
                DestinationPassword = scenarioContext.Get<string>(CommonSteps.DestinationPasswordHolder),
                Overwrite = scenarioContext.Get<bool>(CommonSteps.OverwriteHolder),
                Result = scenarioContext.Get<string>(CommonSteps.ResultVariableHolder),
                PrivateKeyFile = privateKeyFile,
                DestinationPrivateKeyFile = destPrivateKeyFile
            };

            TestStartNode = new FlowStep
            {
                Action = rename
            };
            if (!scenarioContext.ContainsKey("activity"))
                scenarioContext.Add("activity", rename);
        }

        [When(@"validating the rename tool")]
        public void WhenValidatingTheRenameTool()
        {
            string privateKeyFile;
            string destPrivateKeyFile;
            scenarioContext.TryGetValue(CommonSteps.SourcePrivatePublicKeyFile, out privateKeyFile);
            scenarioContext.TryGetValue(CommonSteps.DestinationPrivateKeyFile, out destPrivateKeyFile);

            DsfPathRename dsfRename = new DsfPathRename
            {
                InputPath = scenarioContext.Get<string>(CommonSteps.SourceHolder),
                Username = scenarioContext.Get<string>(CommonSteps.SourceUsernameHolder),
                Password = scenarioContext.Get<string>(CommonSteps.SourcePasswordHolder),
                OutputPath = scenarioContext.Get<string>(CommonSteps.DestinationHolder),
                DestinationUsername = scenarioContext.Get<string>(CommonSteps.DestinationUsernameHolder),
                DestinationPassword = scenarioContext.Get<string>(CommonSteps.DestinationPasswordHolder),
                Overwrite = scenarioContext.Get<bool>(CommonSteps.OverwriteHolder),
                Result = scenarioContext.Get<string>(CommonSteps.ResultVariableHolder),
                PrivateKeyFile = privateKeyFile,
                DestinationPrivateKeyFile = destPrivateKeyFile
            };
            if (!scenarioContext.ContainsKey("activity"))
                scenarioContext.Add("activity", dsfRename);
            dsfRename.PerformValidation();

            var viewModel = new RenameDesignerViewModel(ModelItemUtils.CreateModelItem(dsfRename));
            if (!scenarioContext.ContainsKey("viewModel"))
                scenarioContext.Add("viewModel", viewModel);
        }
    }
}