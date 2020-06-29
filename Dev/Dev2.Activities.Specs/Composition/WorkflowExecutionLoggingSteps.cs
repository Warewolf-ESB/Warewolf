/*
*  Warewolf - Once bitten, there's no going bac
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.Enums;
using Dev2.Common.Interfaces.Monitoring;
using Dev2.Communication;
using Dev2.Data;
using Dev2.Data.Util;
using Dev2.DynamicServices;
using Dev2.Interfaces;
using Dev2.PerformanceCounters.Management;
using Dev2.Runtime;
using Dev2.Runtime.ESB.Control;
using Dev2.Runtime.Hosting;
using Dev2.Studio.Interfaces;
using Dev2.Data.Interfaces.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Activities;
using System.Linq;
using System.Security.Principal;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Warewolf.Auditing;
using Warewolf.Storage;
using Warewolf.Storage.Interfaces;

namespace Dev2.Activities.Specs.Composition
{
    [Binding]
    public class WorkflowExecutionLoggingSteps : Steps
    {
        private IServer _environmentModel;
        private ScenarioContext _scenarioContext;
        private readonly IPrincipal _principal;
        private IExecutionEnvironment _environment;
        private WarewolfPerformanceCounterManager _performanceCounterLocater;
        private bool _expectException;
        private readonly Exception _falseException = new Exception("False exception from WorkflowExecutionLoggingSteps");

        public WorkflowExecutionLoggingSteps(ScenarioContext scenarioContext, FeatureContext featureContext)
        {
            _scenarioContext = scenarioContext ?? throw new ArgumentNullException(nameof(scenarioContext));
            _environmentModel = featureContext.Get<IServer>("environmentModel") ?? throw new ArgumentNullException(nameof(featureContext));
            _performanceCounterLocater = featureContext.Get<WarewolfPerformanceCounterManager>("performanceCounterLocater");
            _principal = featureContext.Get<IPrincipal>("principal");
            _environment = BuildExecutionEnvironmet();
            _expectException = false;
        }

        [Given(@"an existing workflow ""(.*)""")]
        public void GivenAnExistingWorkflow(string wfName)
        {
            var workflow = GetWorkflow(wfName);
            _scenarioContext.Add(wfName, workflow);

            Assert.IsNotNull(workflow, workflow + " was not found.");
        }

        private IResourceModel GetWorkflow(string wfName)
        {
            return _environmentModel.ResourceRepository.FindSingle(o => o.ResourceName == wfName);
        }

        [Given(@"""(.*)"" stop on error is set to ""(.*)""")]
        public void GivenStopOnErrorIsSetTo(string wfName, bool stopOnError)
        {
            var workflow = GetWorkflow(wfName);
            var mockStateNotifier = new Mock<IStateNotifier>();

            var dataObject = BuildDataObject(workflow, _principal, mockStateNotifier.Object, _environment, stopOnError);
            _scenarioContext.Add("dataObject", dataObject);

            if (stopOnError)
            {
                dataObject.Environment.AddError("False error from spec");
            }
        }

        [Given(@"workflow execution entry point detailed logs are created and logged")]
        public void GivenWorkflowExecutionEntryPointDetailedLogsAreCreatedAndLogged()
        {
            Given(@"an existing workflow ""Hello World""");
            When(@"a ""Hello World"" workflow request is received");

            _scenarioContext.TryGetValue<bool>("expectException", out bool expectException);
            var table = new Table("key", "value");
            if (expectException)
            {
                table.AddRow("IDev2ActivityProxy", "SpecActivity");
            }
            else
            {
                table.AddRow("DsfDecision", "If [[Name]] <> (Not Equal)");
            }

            Then(@"a detailed entry log is created", table);
        }

        [Given(@"a workflow stops on error has no logs")]
        public void GivenAWorkflowStopsOnErrorHasNoLogs()
        {
            var mockStateNotifier = _scenarioContext.Get<Mock<IStateNotifier>>("mockStateNotifier");
            var resource = _scenarioContext.Get<IDev2Activity>("resource");

            mockStateNotifier.Verify(o => o.LogStopExecutionState(resource), Times.Never);
        }

        [When(@"a ""(.*)"" workflow request is received")]
        public void WhenAWorkflowRequestIsReceived(string wfName)
        {
            var workflow = _scenarioContext.Get<IResourceModel>(wfName);
            var dataObject = _scenarioContext.Get<DsfDataObject>("dataObject");
            var mockStateNotifier = SetupMockStateNotifier();

            var mockExecutionManager = new Mock<IExecutionManager>();
            var executionManager = mockExecutionManager.Object;

            var esbServicesEndpoint = new EsbServicesEndpoint();

            CustomContainer.Register<IWarewolfPerformanceCounterLocater>(_performanceCounterLocater);
            CustomContainer.Register<IExecutionManager>(executionManager);
            var mockLogManager = new Mock<IStateNotifierFactory>();
            mockLogManager.Setup(o => o.New(dataObject)).Returns(mockStateNotifier.Object);
            CustomContainer.Register<IStateNotifierFactory>(mockLogManager.Object);

            var workspaceId = Guid.NewGuid();
            var request = new EsbExecuteRequest();
            var resultId = esbServicesEndpoint.ExecuteRequest(dataObject, request, workspaceId, out var errors, null);

            Assert.IsNotNull(resultId);

            _scenarioContext.Add(nameof(mockExecutionManager), mockExecutionManager);
        }

        private Mock<IStateNotifier> SetupMockStateNotifier()
        {
            var mockStateNotifier = new Mock<IStateNotifier>();
            _scenarioContext.Add(nameof(mockStateNotifier), mockStateNotifier);
            return mockStateNotifier;
        }

        private static DsfDataObject BuildDataObject(IResourceModel workflow, IPrincipal principal, IStateNotifier stateNotifier, IExecutionEnvironment environment, bool stopOnError)
        {
            return new DsfDataObject("", Guid.NewGuid())
            {
                ResourceID = workflow.ID,
                ExecutionID = Guid.NewGuid(),
                ServiceName = workflow.DisplayName,
                ExecutingUser = principal,
                WorkspaceID = Guid.NewGuid(),
                IsDebug = false,
                StopExecution = stopOnError,
                RunWorkflowAsync = true,
                StateNotifier = stateNotifier,
                Environment = environment,
                Settings = new Dev2WorkflowSettingsTO
                {
                    ExecutionLogLevel = LogLevel.DEBUG.ToString(),
                    EnableDetailedLogging = true,
                    LoggerType = LoggerType.JSON,
                    KeepLogsForDays = 2,
                    CompressOldLogFiles = true
                }
            };
        }

        private static ExecutionEnvironment BuildExecutionEnvironmet()
        {
            var env = new ExecutionEnvironment();
            env.Assign("[[Name]]", "", 0);
            env.Assign("[[servicename]]", @"Hello World.json", 0);
            return env;
        }

        [Then(@"a detailed entry log is created")]
        public void ThenADetailedLogEntryIsCreated(Table table)
        {
            var nodeTable = table.CreateSet<NodeLogTable>().ToList();

            var dataObject = _scenarioContext.Get<IDSFDataObject>("dataObject");
            IDev2Activity resource = GetWFsFirstNode(dataObject);

            var displayName = resource.GetDisplayName();
            var nodeType = resource.GetType();
            _scenarioContext.Add(nameof(resource), resource);

            Assert.AreEqual(nodeTable[0].Key, nodeType.Name);
            Assert.AreEqual(nodeTable[0].Value, displayName.Trim());
        }

        [Then(@"it has these input parameter values")]
        public void ThenItHasTheseInputParameterValues(Table table)
        {
            var nodeTable = table.CreateSet<NodeLogTable>().ToList();

            var dataObject = _scenarioContext.Get<DsfDataObject>("dataObject");
            var acctualInput = Eval(dataObject, "[[Name]]");

            Assert.AreEqual(nodeTable[0].Key, "[[Name]]");
            Assert.AreEqual(nodeTable[0].Value, acctualInput);
        }

        private static IDev2Activity GetWFsFirstNode(IDSFDataObject dataObject)
        {
            return new ResourceCatalog().Parse(dataObject.WorkspaceID, dataObject.ResourceID, dataObject.ExecutionID.ToString());
        }

        [Then(@"a detailed execution completed log entry is created")]
        public void ThenADetailedExecutionCompletedLogEntryIsCreated(Table table)
        {
            var nodeTable = table.CreateSet<NodeLogTable>().ToList();

            var mockStateNotifier = _scenarioContext.Get<Mock<IStateNotifier>>("mockStateNotifier");
            var resource = _scenarioContext.Get<DsfDecision>("resource");
            var lastExecutedNode = resource.GetNextNodes().ToList()[0];
            var lastNodeType = lastExecutedNode.GetType();

            Assert.AreEqual(nodeTable[0].Key, lastNodeType.Name);
            Assert.AreEqual(nodeTable[0].Value, lastExecutedNode.GetDisplayName().Trim());

            mockStateNotifier.Verify(o => o.LogExecuteCompleteState(lastExecutedNode), Times.Once);
        }

        [Then(@"it has these output parameter values")]
        public void ThenItHasTheseOutputParameterValues(Table table)
        {
            var nodeTable = table.CreateSet<NodeLogTable>().ToList();

            var resource = _scenarioContext.Get<DsfDecision>("resource");
            var dataObject = _scenarioContext.Get<DsfDataObject>("dataObject");
            var lastNode = resource.GetNextNodes().ToList()[0].GetOutputs();
            var messageVariable = lastNode[0];
            string actual = Eval(dataObject, messageVariable);

            Assert.AreEqual(nodeTable[0].Key, messageVariable);
            Assert.AreEqual(nodeTable[0].Value, actual);
        }

        private static string Eval(DsfDataObject dataObject, string messageVariable)
        {
            var warewolfEvalResult = dataObject.Environment.Eval(DataListUtil.AddBracketsToValueIfNotExist(messageVariable), 0, true);

            var atomResult = (warewolfEvalResult as CommonFunctions.WarewolfEvalResult.WarewolfAtomResult).Item;
            var actual = (atomResult as DataStorage.WarewolfAtom.DataString).Item;
            return actual;
        }

        [When(@"a workflow stops on error")]
        public void WhenAWorkflowStopsOnError()
        {
            var mockStateNotifier = _scenarioContext.Get<Mock<IStateNotifier>>("mockStateNotifier");
            var resource = _scenarioContext.Get<DsfDecision>("resource");

            mockStateNotifier.Verify(o => o.LogStopExecutionState(resource), Times.Once);
        }

        [Then(@"execution is complete")]
        public void ThenExecutionIsComplete()
        {
            var mockExecutionManager = _scenarioContext.Get<Mock<IExecutionManager>>("mockExecutionManager");
            mockExecutionManager.Verify(o => o.CompleteExecution());
        }

        [Given(@"the workflow is expected to throw exception")]
        public void GivenTheWorkflowIsExpectedToThrowException()
        {
            var dataObject = _scenarioContext.Get<DsfDataObject>("dataObject");

            var activityParserMock = new Mock<IActivityParser>();
            var activityMock = new Mock<IDev2Activity>();

            activityMock.Setup(o => o.GetDisplayName()).Returns("SpecActivity");
            activityMock.Setup(o => o.Execute(dataObject, 0)).Throws(_falseException);
            activityParserMock.Setup(o => o.Parse(It.IsAny<DynamicActivity>())).Returns(activityMock.Object);
            CustomContainer.Register<IActivityParser>(activityParserMock.Object);
            _scenarioContext.Add("activityMock", activityMock.Object);

            var expectException = _expectException = true;
            _scenarioContext.Add("expectException", expectException);
        }

        [When(@"a workflow execution has an exception")]
        public void WhenAWorkflowExecutionHasAnException()
        {
            var mockStateNotifier = _scenarioContext.Get<Mock<IStateNotifier>>("mockStateNotifier");
            var activityMock = _scenarioContext.Get<IDev2Activity>("activityMock");

            mockStateNotifier.Verify(o => o.LogExecuteException(_falseException, activityMock));
        }

        [Then(@"a detailed execution exception log entry is created")]
        public void ThenADetailedExecutionExceptionLogEntryIsCreated(Table table)
        {
            var nodeTable = table.CreateSet<NodeLogTable>().ToList();

            Assert.AreEqual(nodeTable[0].Key, _falseException.GetType().Name);
            Assert.AreEqual(nodeTable[0].Value, _falseException.Message);
        }

        [Then(@"a detailed execution completed log entry will have no logs")]
        public void ThenADetailedExecutionCompletedLogEntryWillHaveNoLogs()
        {
            var mockStateNotifier = _scenarioContext.Get<Mock<IStateNotifier>>("mockStateNotifier");

            mockStateNotifier.Verify(o => o.LogExecuteCompleteState(It.IsAny<IDev2Activity>()), Times.Never);
        }
    }

    internal class NodeLogTable
    {
        public string Value { get; set; }

        public string Key { get; set; }
    }
}