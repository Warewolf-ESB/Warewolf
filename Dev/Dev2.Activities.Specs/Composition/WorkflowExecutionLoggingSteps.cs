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
using Dev2.Studio.Core;
using Dev2.Studio.Core.Models;
using Dev2.Studio.Interfaces;
using Dev2.Studio.Interfaces.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Security.Principal;
using TechTalk.SpecFlow;

namespace Dev2.Activities.Specs.Composition
{
    [Binding]
    public class WorkflowExecutionLoggingSteps
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

        static ResourceModel BuildResourceModel(string workflowName, IServer server)
        {
            var newGuid = Guid.NewGuid();
            var resourceModel = new ResourceModel(server)
            {
                ResourceName = workflowName,
                DisplayName = workflowName,
                DataList = "",
                ID = newGuid,
                Category = workflowName
            };
            return resourceModel;
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

        [Given(@"""(.*)"" workflow execution entry point detailed logs are created and logged")]
        public void GivenWorkflowExecutionEntryPointDetailedLogsAreCreatedAndLogged(string wfName)
        {
            //This step will call these steps: 
            //Given an existing workflow "Hello World"
            //When a "Hello World" workflow request is received
            ScenarioContext.Current.Pending();
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

            var esbServicesEndpoint = new EsbServicesEndpoint();
            NewSerciveEndPoint(workflow, out DsfDataObject dataObject, out EsbExecuteRequest request, out Guid workspaceId, out Mock<IExecutionManager> mockExecutionManager);

            var resultId = esbServicesEndpoint.ExecuteRequest(dataObject, request, workspaceId, out var errors);

            Assert.IsNotNull(resultId);
            //TODO: These should pass
            /*mockStateNotifier.Verify(o => o.LogExecutionInputs(), Times.Once); //TODO: Suggest we add LogExecutionInputs with parameters might be string or json 
            mockStateNotifier.Verify(o => o.LogPreExecuteState(It.IsAny<IDev2Activity>()), Times.Once);
            mockStateNotifier.Verify(o => o.LogAdditionalDetail(It.IsAny<object>(), It.IsAny<string>()), Times.Once);
            mockStateNotifier.Verify(o => o.LogPostExecuteState(It.IsAny<IDev2Activity>(), It.IsAny<IDev2Activity>()), Times.Once);*/

            mockExecutionManager.Verify(o => o.CompleteExecution(), Times.Once);
        }


        private static void NewSerciveEndPoint(IResourceModel workflow, out DsfDataObject dataObject, out EsbExecuteRequest request, out Guid workspaceId, out Mock<IExecutionManager> mockExecutionManager)
        {
            //Start: TODO: This to be moved to EsbServicesEndpointBuilder 
            var mockPrincipal = new Mock<IPrincipal>();
            mockPrincipal.Setup(o => o.Identity).Returns(WindowsIdentity.GetCurrent());

            var mockStateNotifier = new Mock<IStateNotifier>();
            //Start: TODO: This to be moved to DsfDataObjectBuilder
            dataObject = new DsfDataObject("", Guid.NewGuid())
            {
                ResourceID = workflow.ID,
                ExecutionID = Guid.NewGuid(), 
                ServiceName = workflow.DisplayName, 
                ExecutingUser = mockPrincipal.Object,
                IsDebug = false,
                RunWorkflowAsync = true,
                StateNotifier = mockStateNotifier.Object,
                Settings = new Dev2WorkflowSettingsTO
                {
                    EnableDetailedLogging = true,
                    LoggerType = LoggerType.JSON,
                    KeepLogsForDays = 2,
                    CompressOldLogFiles = true
                }
            };
            dataObject.Environment.Assign("[[Name]]", "somename", 0);
            //End

            request = new EsbExecuteRequest();
            workspaceId = Guid.NewGuid();
            mockExecutionManager = new Mock<IExecutionManager>();
            var executionManager = mockExecutionManager.Object;

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

            var performanceCounterLocater = new WarewolfPerformanceCounterManager(register.Counters, new List<IResourcePerformanceCounter>(), register, new Mock<IPerformanceCounterPersistence>().Object, _performanceCounterFactory);
            CustomContainer.Register<IWarewolfPerformanceCounterLocater>(performanceCounterLocater);
            CustomContainer.Register<IExecutionManager>(executionManager);

            //End
        }

        [Then(@"a detailed entry log is created")]
        public void ThenADetailedLogEntryIsCreated(Table table)
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"it has these input parameter values")]
        public void ThenItHasTheseInputParameterValues(Table table)
        {
            ScenarioContext.Current.Pending();
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
}
