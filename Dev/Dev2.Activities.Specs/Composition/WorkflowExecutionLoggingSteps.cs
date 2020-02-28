/*
*  Warewolf - Once bitten, there's no going bac
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.DynamicServices;
using Dev2.Interfaces;
using Dev2.PerformanceCounters.Counters;
using Dev2.PerformanceCounters.Management;
using Dev2.Runtime;
using Dev2.Runtime.ESB.Control;
using Dev2.Runtime.Hosting;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Models;
using Dev2.Studio.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;

namespace Dev2.Activities.Specs.Composition
{
    [Binding]
    public class WorkflowExecutionLoggingSteps : Steps
    {
        //Start: TODO: Move this to WorkflowExecutionLoggingHooks
        private static IServer _environmentModel;
        private const int EXPECTED_NUMBER_OF_RESOURCES = 108;
        private static IResourceModel _resourceModel;
        private ScenarioContext _scenarioContext;

        [BeforeFeature()]
        private static void Setup()
        {
            ConnectAndLoadServer();
            Assert.IsTrue(_environmentModel.ResourceRepository.All().Count >= EXPECTED_NUMBER_OF_RESOURCES, $"This test expects {EXPECTED_NUMBER_OF_RESOURCES} resources on localhost but there are only {_environmentModel.ResourceRepository.All().Count}.");
        }

        public WorkflowExecutionLoggingSteps(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext ?? throw new ArgumentNullException(nameof(scenarioContext));
        }

        private static void ConnectAndLoadServer()
        {
            _environmentModel = ServerRepository.Instance.Source;
            _environmentModel.ConnectAsync().Wait(60000);
            if (_environmentModel.IsConnected)
            {
                _environmentModel.ResourceRepository.Load(true);
            }
            else
            {
                throw new Exception("Failed to connect to localhost Warewolf server.");
            }
        }

        [AfterFeature]
        private static void Cleanup()
        {
            //TODO: release all resource used by this feature 
        }
        //End of beforeFeature hook


        [Given(@"an existing workflow ""(.*)""")]
        public void GivenAnExistingWorkflow(string wfName)
        {
            var workflow = _environmentModel.ResourceRepository.FindSingle(o => o.ResourceName == wfName);
            _scenarioContext.Add(wfName, workflow);
            
            Assert.IsNotNull(workflow, workflow +" was not found.");
        }

        [Given(@"""(.*)"" stop on error is set to ""(.*)""")]
        public void GivenStopOnErrorIsSetTo(string wfName, bool stopOnError)
        {
            /*var workflow = _scenarioContext.Get<IResourceModel>(wfName);
            var mockStateNotifier = new Mock<IStateNotifier>();

            var principal = BuildPrincipal();
            var dataObject = BuildDataObject(workflow, principal, mockStateNotifier.Object, stopOnError);
            if(stopOnError)
            {
                dataObject.Environment.AddError("False error from spec");
            }
            _scenarioContext.Add(nameof(dataObject), dataObject);*/

        }

        [Given(@"""(.*)"" workflow execution entry point detailed logs are created and logged")]
        public void GivenWorkflowExecutionEntryPointDetailedLogsAreCreatedAndLogged(string p0)
        {
            ScenarioContext.Current.Pending();
        }

        [Given(@"workflow execution entry point detailed logs are created and logged")]
        public void GivenWorkflowExecutionEntryPointDetailedLogsAreCreatedAndLogged()
        {
            Given(@"an existing workflow ""Hello World""");
            When(@"a ""Hello World"" workflow request is received");

            var table = new Table("nodeType", "displayName");
            table.AddRow("DsfDecision", "If [[Name]] <> (Not Equal)");

            Then(@"a detailed entry log is created", table);
        }

        [Given(@"a workflow stops on error has no logs")]
        public void GivenAWorkflowStopsOnErrorHasNoLogs()
        {
            ScenarioContext.Current.Pending();
        }

        [When(@"a ""(.*)"" workflow request is received")]
        public void WhenAWorkflowRequestIsReceived(string wfName)
        {
            var workflow = _scenarioContext.Get<IResourceModel>(wfName);
            var mockStateNotifier = new Mock<IStateNotifier>();
            _scenarioContext.Add(nameof(mockStateNotifier), mockStateNotifier);

            var principal = BuildPrincipal();
            var dataObject = BuildDataObject(workflow, principal, mockStateNotifier.Object, false);
            _scenarioContext.Add("dataObject", dataObject);

            var mockExecutionManager = new Mock<IExecutionManager>();
            var executionManager = mockExecutionManager.Object;

            var esbServicesEndpoint = new EsbServicesEndpoint();
            var performanceCounterLocater = BuildPerfomanceCounter();

            CustomContainer.Register<IWarewolfPerformanceCounterLocater>(performanceCounterLocater);
            CustomContainer.Register<IExecutionManager>(executionManager);

            var workspaceId = Guid.NewGuid();
            var request = new EsbExecuteRequest();
            var resultId = esbServicesEndpoint.ExecuteRequest(dataObject, request, workspaceId, out var errors);

            Assert.IsNotNull(resultId);

            _scenarioContext.Add(nameof(mockExecutionManager), mockExecutionManager);
           
            mockExecutionManager.Verify(o => o.CompleteExecution(), Times.Once);
        }

        private WarewolfPerformanceCounterManager BuildPerfomanceCounter()
        {
            var _mockPerformanceCounterFactory = new Mock<IRealPerformanceCounterFactory>();
            var _performanceCounterFactory = _mockPerformanceCounterFactory.Object;

            var register = new WarewolfPerformanceCounterRegister(new List<IPerformanceCounter>
                                                        {
                                                            new WarewolfCurrentExecutionsPerformanceCounter(_performanceCounterFactory),
                                                            new WarewolfNumberOfErrors(_performanceCounterFactory),
                                                            new WarewolfRequestsPerSecondPerformanceCounter(_performanceCounterFactory),
                                                            new WarewolfAverageExecutionTimePerformanceCounter(_performanceCounterFactory),
                                                            new WarewolfNumberOfAuthErrors(_performanceCounterFactory),
                                                            new WarewolfServicesNotFoundCounter(_performanceCounterFactory),
                                                        }, new List<IResourcePerformanceCounter>());

            return new WarewolfPerformanceCounterManager(register.Counters, new List<IResourcePerformanceCounter>(), register, new Mock<IPerformanceCounterPersistence>().Object, _performanceCounterFactory);
        }

        private static IPrincipal BuildPrincipal()
        { 
            var mockPrincipal = new Mock<IPrincipal>();
            mockPrincipal.Setup(o => o.Identity).Returns(WindowsIdentity.GetCurrent());
            return mockPrincipal.Object;
        }

        private static DsfDataObject BuildDataObject(IResourceModel workflow, IPrincipal principal, IStateNotifier stateNotifier, bool stopOnError)
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
                Settings = new Dev2WorkflowSettingsTO
                {
                    EnableDetailedLogging = true,
                    LoggerType = LoggerType.JSON,
                    KeepLogsForDays = 2,
                    CompressOldLogFiles = true
                }
            };
        }

        [Then(@"a detailed entry log is created")]
        public void ThenADetailedLogEntryIsCreated(Table table)
        {
            var nodeTable = table.CreateSet<NodeLogTable>().ToList();

            var mockStateNotifier = _scenarioContext.Get<Mock<IStateNotifier>>("mockStateNotifier");
            var mockExecutionManager = _scenarioContext.Get<Mock<IExecutionManager>>("mockExecutionManager");
            var dataObject = _scenarioContext.Get<IDSFDataObject>("dataObject");

            var resource = new ResourceCatalog().Parse(dataObject.WorkspaceID, dataObject.ResourceID, dataObject.ExecutionID.ToString());
            var displayName = resource.GetDisplayName();
            var nextNode = resource.GetNextNodes().ToList()[0];
            var nodeType = resource.GetType();
            _scenarioContext.Add(nameof(resource), resource);

            Assert.AreEqual(nodeTable[0].NodeType, nodeType.Name);
            Assert.AreEqual(nodeTable[0].DisplayName, displayName.Trim());
            
            //TODO: These should pass
            //mockStateNotifier.Verify(o => o.LogExecutionInputs(), Times.Once); //TODO: Suggest we add LogExecutionInputs with parameters might be string or json 
            //mockStateNotifier.Verify(o => o.LogPreExecuteState(resource), Times.Once); //TODO: this should pass
            //mockStateNotifier.Verify(o => o.LogAdditionalDetail(It.IsAny<object>(), dataObject.ExecutionID.ToString()), Times.Once);
            //mockStateNotifier.Verify(o => o.LogExecutionOutputs(), Times.Once); //TODO: Suggest we add LogExecutionOutputs with parameters might be string or json 
            //mockStateNotifier.Verify(o => o.LogPostExecuteState(resource, nextNode), Times.Once);

            mockExecutionManager.Verify(o => o.CompleteExecution(), Times.Once);
        }

        [Then(@"it has these input parameter values")]
        public void ThenItHasTheseInputParameterValues(Table table)
        {
            var inputTable = table.CreateSet<InputTable>().ToList();

            var dataObject = _scenarioContext.Get<IDSFDataObject>("dataObject");

            var dataListTO = new DataListTO(dataObject.DataList.ToString());

            //alteration of this list to be assert to the incoming table
            Assert.IsNotNull(dataListTO.Inputs[0]);
        }

        [Then(@"a detailed execution completed log entry is created")]
        public void ThenADetailedExecutionCompletedLogEntryIsCreated(Table table)
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"it has these output parameter values")]
        public void ThenItHasTheseOutputParameterValues(Table table)
        {
            ScenarioContext.Current.Pending();
        }

        [When(@"a workflow stops on error")]
        public void WhenAWorkflowStopsOnError()
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"a detailed on error log entry is created")]
        public void ThenADetailedOnErrorLogEntryIsCreated(Table table)
        {
            ScenarioContext.Current.Pending();
        }

        [When(@"a workflow execution has an exception")]
        public void WhenAWorkflowExecutionHasAnException()
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"a detailed execution exception log entry is created")]
        public void ThenADetailedExecutionExceptionLogEntryIsCreated(Table table)
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"a detailed execution completed log entry is has no logs")]
        public void ThenADetailedExecutionCompletedLogEntryIsHasNoLogs()
        {
            ScenarioContext.Current.Pending();
        }

    }

    internal class InputTable
    {
        public string Variable { get; internal set; }
    }

    internal class NodeLogTable
    {
        public string DisplayName { get; set; }

        public string NodeType { get; set; }
    }
}
