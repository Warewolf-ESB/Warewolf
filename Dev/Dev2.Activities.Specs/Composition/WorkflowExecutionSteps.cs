#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going bac
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Activities;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using Dev2.Activities.DropBox2016.DeleteActivity;
using Dev2.Activities.DropBox2016.DownloadActivity;
using Dev2.Activities.DropBox2016.DropboxFileActivity;
using Dev2.Activities.DropBox2016.UploadActivity;
using Dev2.Activities.RabbitMQ.Consume;
using Dev2.Activities.Scripting;
using Dev2.Activities.RabbitMQ.Publish;
using Dev2.Activities.SelectAndApply;
using Dev2.Activities.Sharepoint;
using Dev2.Activities.Specs.BaseTypes;
using Dev2.Common.Common;
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Enums.Enums;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Communication;
using Dev2.Controller;
using Dev2.Data.ServiceModel;
using Dev2.Data.Util;
using Dev2.Messages;
using Dev2.Network;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Services;
using Dev2.Services.Security;
using Dev2.Session;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.Repositories;
using Dev2.Studio.Core.Models;
using Dev2.Studio.Core.Network;
using Dev2.Studio.ViewModels.DataList;
using Dev2.Threading;
using Dev2.TO;
using Dev2.Util;
using Dev2.Utilities;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Moq;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.Enums;
using Dev2.Common.Interfaces.Infrastructure.SharedModels;
using Dev2.Common.Interfaces.Monitoring;
using Dev2.PerformanceCounters.Counters;
using Dev2.PerformanceCounters.Management;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Interfaces;
using Dev2.Studio.Interfaces.Enums;
using Warewolf.Core;
using Warewolf.Studio.ViewModels;
using Warewolf.Tools.Specs.BaseTypes;
using Dev2.Data.Interfaces.Enums;
using TestingDotnetDllCascading;
using Warewolf.Sharepoint;
using Caliburn.Micro;
using Dev2.Studio.Core.Helpers;
using SecPermissions = Dev2.Common.Interfaces.Security.Permissions;
using Dev2.Common;
using Dev2.Data.Decisions.Operations;
using Dev2.Data.SystemTemplates.Models;
using Dev2.Common.Wrappers;
using Dev2.Common.Interfaces.Wrappers;
using System.Reflection;
using Warewolf.Storage;
using WarewolfParserInterop;
using Dev2.Runtime.Hosting;
using Dev2.Infrastructure.Tests;
using Warewolf.UnitTestAttributes;
using Activity = System.Activities.Activity;

namespace Dev2.Activities.Specs.Composition
{
    [Binding]
    public class WorkflowExecutionSteps : RecordSetBases
    {
        readonly ScenarioContext _scenarioContext;
        IDirectory _dirHelper;
        public static Depends _containerOps;

        public WorkflowExecutionSteps(ScenarioContext scenarioContext)
            : base(scenarioContext)
        {
            _scenarioContext = scenarioContext ?? throw new ArgumentNullException(nameof(scenarioContext));
            _commonSteps = new CommonSteps(_scenarioContext);
            AppUsageStats.LocalHost = "http://localhost:3142";
            _dirHelper = new DirectoryWrapper();
        }

        const int EnvironmentConnectionTimeout = 15;

        SubscriptionService<DebugWriterWriteMessage> _debugWriterSubscriptionService;
        SpecExternalProcessExecutor _externalProcessExecutor;
        readonly AutoResetEvent _resetEvt = new AutoResetEvent(false);
        readonly CommonSteps _commonSteps;

        protected override void BuildDataList()
        {
            BuildShapeAndTestData();
        }

        [BeforeScenario]
        public void Setup()
        {
            if (_debugWriterSubscriptionService != null)
            {
                _debugWriterSubscriptionService.Unsubscribe();
                _debugWriterSubscriptionService.Dispose();
            }

            var mockServer = new Mock<IServer>();
            var mockshell = new Mock<IShellViewModel>();
            mockshell.Setup(a => a.ActiveServer).Returns(mockServer.Object);
            mockServer.Setup(a => a.GetServerVersion()).Returns("1.0.0.0");
            CustomContainer.Register(mockServer.Object);
            CustomContainer.Register(mockshell.Object);
            _externalProcessExecutor = new SpecExternalProcessExecutor();
        }

        public void WorkflowIsDeletedAsCleanup()
        {
            TryGetValue("resourcemodel", out IContextualResourceModel resourceModel);
            if (resourceModel != null)
            {
                TryGetValue("environment", out IServer server);
                TryGetValue("resourceRepo", out IResourceRepository repository);
                repository.DeleteResourceFromWorkspace(resourceModel);
                repository.DeleteResource(resourceModel);
            }
        }

        [AfterScenario]
        public void CleanUp()
        {
            _resetEvt?.Close();
            _containerOps?.Dispose();
        }

        [AfterScenario("ResumeWorkflowExecution")]
        public void CleanUpResources()
        {
            WorkflowIsDeletedAsCleanup();
        }
        public void CleanUp_DetailedLogFile()
        {
            WorkflowIsDeletedAsCleanup();
            if (_debugWriterSubscriptionService != null)
            {
                var files = Directory.GetFiles(EnvironmentVariables.DetailLogPath, "*", SearchOption.AllDirectories);

                foreach (var item in files)
                {
                    File.Delete(item);
                }
                _dirHelper.Delete(EnvironmentVariables.DetailLogPath, true);
            }
            _scenarioContext?.Clear();
        }

        [Given(@"Debug states are cleared")]
        public void GivenDebugStatesAreCleared()
        {
            TryGetValue("debugStates", out List<IDebugState> debugStates);
            debugStates?.Clear();
        }

        [Given(@"Debug events are reset")]
        public void GivenDebugEventsAreReset()
        {
            if (_debugWriterSubscriptionService != null)
            {
                _debugWriterSubscriptionService.Unsubscribe();
                _debugWriterSubscriptionService.Dispose();
            }
        }

        [Then(@"the workflow execution has ""(.*)"" error")]
        public void ThenTheWorkflowExecutionHasError(string hasError)
        {
            TryGetValue("activityList", out Dictionary<string, Activity> activityList);
            TryGetValue("parentWorkflowName", out string parentWorkflowName);
            var debugStates = Get<List<IDebugState>>("debugStates").ToList();
            if (hasError == "AN")
            {
                var hasErrorState = debugStates.FirstOrDefault(state => state.HasError);
                Assert.IsNotNull(hasErrorState);
            }
        }

        [Then(@"the ""(.*)"" workflow execution has ""(.*)"" error")]
        public void ThenTheWorkflowExecutionHasError(string workflowName, string hasError)
        {
            TryGetValue("activityList", out Dictionary<string, Activity> activityList);
            TryGetValue("parentWorkflowName", out string parentWorkflowName);
            var debugStates = Get<List<IDebugState>>("debugStates").ToList();

            if (hasError == "AN")
            {
                var innerWfHasErrorState = debugStates.FirstOrDefault(state => state.HasError && state.DisplayName.Equals(workflowName));
                var parentWfhasErrorState = debugStates.FirstOrDefault(state => state.HasError && state.DisplayName.Equals(parentWorkflowName));
                Assert.IsNotNull(innerWfHasErrorState);
                Assert.IsNotNull(parentWfhasErrorState);
            }
        }

        [Given(@"I have a localhost server")]
        public void GivenIHaveAServerAt()
        {
            var environmentModel = ServerRepository.Instance.Source;
            environmentModel.Connect();
            for (int count = 0; !environmentModel.IsConnected; count++)
            {
                if (count > 20)
                {
                    throw new Exception("connection to localhost timeout");
                }
                Thread.Sleep(500);
            }
        }


        [Given(@"I have a server at ""(.*)"" with workflow ""(.*)""")]
        public void GivenIHaveAWorkflowOnServer(string serverName, string workflow)
        {
            AppUsageStats.LocalHost = "http://localhost:3142";
            var environmentModel = ServerRepository.Instance.Source;
            environmentModel.Connect();
            environmentModel.ResourceRepository.Load(true);

            // connect to the remote environment now
            var remoteServerList = environmentModel.ResourceRepository.FindSourcesByType<Connection>(environmentModel, enSourceType.Dev2Server);

            if (remoteServerList != null && remoteServerList.Count > 0)
            {
                var remoteServer = remoteServerList.FirstOrDefault(r => r.ResourceName == serverName);

                if (remoteServer != null)
                {
                    ServerProxy connection;
                    if (remoteServer.AuthenticationType == AuthenticationType.Windows || remoteServer.AuthenticationType == AuthenticationType.Anonymous)
                    {
                        connection = new ServerProxy(new Uri(remoteServer.WebAddress));
                    }
                    else
                    {
                        //
                        // NOTE: Public needs to drop through to User for the rest of the framework to pick up properly behind the scenes ;)
                        //
                        connection = new ServerProxy(remoteServer.WebAddress, remoteServer.UserName, remoteServer.Password);
                    }

                    var newEnvironment = new Server(remoteServer.ResourceID, connection) { Name = remoteServer.ResourceName };
                    EnsureEnvironmentConnected(newEnvironment, EnvironmentConnectionTimeout);
                    LoadResourcesAndSubscribeToDebugOutput(workflow, newEnvironment);
                }
                else
                {
                    LoadResourcesAndSubscribeToDebugOutput(workflow, environmentModel);
                }
            }
            else
            {
                Assert.Fail($"Server \"{serverName}\" not found");
            }
        }

        void LoadResourcesAndSubscribeToDebugOutput(string workflow, IServer newEnvironment)
        {
            newEnvironment.ForceLoadResources();

            var resourceModel = newEnvironment.ResourceRepository.FindSingle(r => r.ResourceName == workflow);

            _debugWriterSubscriptionService = new SubscriptionService<DebugWriterWriteMessage>(newEnvironment.Connection.ServerEvents);

            _debugWriterSubscriptionService.Subscribe(msg => Append(msg.DebugState));

            Add(workflow, resourceModel);
            Add("parentWorkflowName", workflow);
            Add("environment", newEnvironment);
            Add("resourceRepo", newEnvironment.ResourceRepository);
            Add("debugStates", new List<IDebugState>());
        }

        [BeforeFeature()]
        static void SetUpLocalHost()
        {
            LocalEnvModel = ServerRepository.Instance.Source;
            LocalEnvModel.ConnectAsync().Wait(60000);
            LocalEnvModel.ForceLoadResources();
        }

        public static IServer LocalEnvModel { get; set; }

        [Given("I depend on a valid RabbitMQ server")]
        public void GivenIGetaValidRabbitMQServer() => _containerOps = new Depends(Depends.ContainerType.RabbitMQ);

        [Given("I depend on a valid PostgreSQL server")]
        public void GivenIGetaValidPostgreSQLServer() => _containerOps = new Depends(Depends.ContainerType.PostGreSQL);

        [Given("I depend on a valid MySQL server")]
        public void GivenIGetaValidMySQLServer() => _containerOps = new Depends(Depends.ContainerType.MySQL);

        [Given("I depend on a valid MSSQL server")]
        public void GivenIGetaValidMSSQLServer() => _containerOps = new Depends(Depends.ContainerType.MSSQL);

        [Given(@"I have a workflow ""(.*)""")]
        public void GivenIHaveAWorkflow(string workflowName)
        {
            var resourceId = Guid.NewGuid();
            var environmentModel = LocalEnvModel;
            EnsureEnvironmentConnected(environmentModel, EnvironmentConnectionTimeout);
            // TODO: move this to a spec command something like "I get a valid MySQL host called 'mysqlhost1'"
            if (workflowName == "TestMySqlWFWithMySqlCountries" ||
                workflowName == "TestMySqlWFWithMySqlLastIndex" ||
                workflowName == "TestMySqlWFWithMySqlScalar" ||
                workflowName == "TestMySqlWFWithMySqlStarIndex" ||
                workflowName == "TestMySqlWFWithMySqlIntIndex")
            {
                _containerOps = new Depends(Depends.ContainerType.MySQL);
            }
            var resourceModel = new ResourceModel(environmentModel)
            {
                ID = resourceId,
                ResourceName = workflowName,
                Category = "Acceptance Tests\\" + workflowName,
                ResourceType = ResourceType.WorkflowService
            };

            environmentModel.ResourceRepository.Add(resourceModel);
            _debugWriterSubscriptionService = new SubscriptionService<DebugWriterWriteMessage>(environmentModel.Connection.ServerEvents);

            _debugWriterSubscriptionService.Subscribe(msg => Append(msg.DebugState));
            Add("resourcemodel", resourceModel);
            Add(workflowName, resourceModel);
            Add("resourceId", resourceId);
            Add("parentWorkflowName", workflowName);
            Add("environment", environmentModel);
            Add("resourceRepo", environmentModel.ResourceRepository);
            Add("debugStates", new List<IDebugState>());
        }

        [Given(@"I have reset local performance Counters")]
        public void GivenIHaveResetLocalPerformanceCounters()
        {
            try
            {
                try
                {
                    PerformanceCounterCategory.Delete("Warewolf");
                }

                catch (Exception e)
                {
                    Assert.IsNotNull(e);
                }

                var performanceCounterFactory = new PerformanceCounterFactory();

                var register = new WarewolfPerformanceCounterRegister(new List<IPerformanceCounter>
                                                            {   new WarewolfCurrentExecutionsPerformanceCounter(performanceCounterFactory),
                                                                new WarewolfNumberOfErrors(performanceCounterFactory),
                                                                new WarewolfRequestsPerSecondPerformanceCounter(performanceCounterFactory),
                                                                new WarewolfAverageExecutionTimePerformanceCounter(performanceCounterFactory),
                                                                new WarewolfNumberOfAuthErrors(performanceCounterFactory),
                                                                new WarewolfServicesNotFoundCounter(performanceCounterFactory)
                                                            }, new List<IResourcePerformanceCounter>());
                CustomContainer.Register<IWarewolfPerformanceCounterLocater>(new WarewolfPerformanceCounterManager(register.Counters, new List<IResourcePerformanceCounter>(), register, new Mock<IPerformanceCounterPersistence>().Object, performanceCounterFactory));
            }
            catch (Exception ex)
            {
                Assert.Fail("failed to delete existing counters");
            }
        }

        [Then(@"the perfcounter raw values are")]
        public void ThenThePerfcounterRawValuesAre(Table table)
        {
            var performanceCounterCategory = new PerformanceCounterCategory("Warewolf");
            var instanceNames = performanceCounterCategory.GetInstanceNames();
            foreach (var instanceName in instanceNames)
            {
                var counters = performanceCounterCategory.GetCounters(instanceName);
                foreach (var tableRow in table.Rows)
                {
                    foreach (var counter in instanceNames)
                    {
                        var performanceCounter = counters.First(a => a.CounterName == tableRow[0]);
                        if (performanceCounter != null)
                        {
                            using (var cnt = new PerformanceCounter("Warewolf", tableRow[0], counter, true))
                            {
                                if (tableRow[1] == "x")
                                {
                                    Assert.AreNotEqual(cnt.RawValue, 0);
                                }
                                else
                                {
                                    Assert.AreEqual(cnt.RawValue, int.Parse(tableRow[1]));
                                }
                            }
                        }
                    }
                }
            }
        }

        void EnsureEnvironmentConnected(IServer server, int timeout)
        {
            var startTime = DateTime.UtcNow;
            server.ConnectAsync().Wait(60000);

            while (!server.IsConnected && !server.Connection.IsConnected)
            {
                Assert.AreEqual(server.IsConnected, server.Connection.IsConnected);

                var now = DateTime.UtcNow;
                if (now.Subtract(startTime).TotalSeconds > timeout)
                {
                    break;
                }
                Thread.Sleep(GlobalConstants.NetworkTimeOut);
            }

            if (!server.IsConnected && !server.Connection.IsConnected)
            {
                _scenarioContext.Add("ConnectTimeoutCountdown", EnvironmentConnectionTimeout);
                throw new TimeoutException("Connection to Warewolf server \"" + server.Name + "\" timed out.");
            }
        }

        void Add(string key, object value) => _scenarioContext.Add(key, value);

        void Append(IDebugState debugState)
        {
            TryGetValue("debugStates", out List<IDebugState> debugStates);
            TryGetValue("debugStatesDuration", out List<IDebugState> debugStatesDuration);
            TryGetValue("parentWorkflowName", out string workflowName);
            TryGetValue("environment", out IServer server);
            if (debugStatesDuration == null)
            {
                debugStatesDuration = new List<IDebugState>();
                Add("debugStatesDuration", debugStatesDuration);
            }
            if (debugState.WorkspaceID == server.Connection.WorkspaceID || debugState.WorkspaceID == GlobalConstants.ServerWorkspaceID)
            {
                if (debugState.StateType != StateType.Duration)
                {
                    debugStates.Add(debugState);
                }
                else
                {
                    debugStatesDuration.Add(debugState);
                }
            }
            if (debugState.IsFinalStep() && debugState.DisplayName.Equals(workflowName))
            {
                _resetEvt.Set();
            }

        }

        [Then(@"the ""(.*)"" in step (.*) for ""(.*)"" debug inputs as")]
        public void ThenTheInStepForDebugInputsAs(string toolName, int stepNumber, string forEachName, Table table)
        {
            TryGetValue("activityList", out Dictionary<string, Activity> activityList);
            TryGetValue("parentWorkflowName", out string parentWorkflowName);

            var debugStates = Get<List<IDebugState>>("debugStates").ToList();
            var workflowId = debugStates.First(wf => wf.DisplayName.Equals(forEachName)).ID;

            if (parentWorkflowName == forEachName)
            {
                workflowId = Guid.Empty;
            }

            var toolSpecificDebug =
                debugStates.Where(ds => ds.ParentID.GetValueOrDefault() == workflowId && ds.DisplayName.Equals(toolName)).ToList();

            Assert.IsTrue(toolSpecificDebug.Count >= stepNumber);
            var debugToUse = DebugToUse(stepNumber, toolSpecificDebug);

            _commonSteps.ThenTheDebugInputsAs(table, debugToUse.Inputs
                                                    .SelectMany(item => item.ResultsList).ToList());
        }

        [Then(@"the ""(.*)"" in '(.*)' in step (.*) for ""(.*)"" debug inputs as")]
        public void ThenTheInInStepForDebugInputsAs(string toolName, string sequenceName, int stepNumber, string forEachName, Table table)
        {
            TryGetValue("activityList", out Dictionary<string, Activity> activityList);
            TryGetValue("parentWorkflowName", out string parentWorkflowName);

            var debugStates = Get<List<IDebugState>>("debugStates").ToList();
            var workflowId = debugStates.First(wf => wf.DisplayName.Equals(forEachName)).ID;

            if (parentWorkflowName == forEachName)
            {
                workflowId = Guid.Empty;
            }

            var sequenceDebug = debugStates.Where(ds => ds.ParentID.GetValueOrDefault() == workflowId).ToList();
            Assert.IsTrue(sequenceDebug.Count >= stepNumber);

            var sequenceId = sequenceDebug[stepNumber - 1].ID;
            var sequenceIsInForEach = sequenceDebug.Any(state => state.ID == sequenceId);
            Assert.IsTrue(sequenceIsInForEach);

            var toolSpecificDebug =
                debugStates.Where(ds => ds.ParentID.GetValueOrDefault() == sequenceId && ds.DisplayName.Equals(toolName)).ToList();

            _commonSteps.ThenTheDebugInputsAs(table, toolSpecificDebug
                                                    .SelectMany(item => item.Inputs)
                                                    .SelectMany(item => item.ResultsList).ToList());

        }

        [Then(@"the dotnetdll ""(.*)"" in '(.*)' in step (.*) for ""(.*)"" debug inputs as")]
        public void ThenTheInInStepForDotNetDebugInputsAs(string toolName, string sequenceName, int stepNumber, string forEachName, Table table)
        {
            TryGetValue("activityList", out Dictionary<string, Activity> activityList);
            TryGetValue("parentWorkflowName", out string parentWorkflowName);

            var debugStates = Get<List<IDebugState>>("debugStates").ToList();
            var workflowId = debugStates.First(wf => wf.DisplayName.Equals(forEachName)).ID;

            if (parentWorkflowName == forEachName)
            {
                workflowId = Guid.Empty;
            }

            var sequenceDebug = debugStates.Where(ds => ds.ParentID.GetValueOrDefault() == workflowId).ToList();
            Assert.IsTrue(sequenceDebug.Count >= stepNumber);

            var sequenceId = sequenceDebug[stepNumber - 1].ID;
            var sequenceIsInForEach = sequenceDebug.Any(state => state.ID == sequenceId);
            Assert.IsTrue(sequenceIsInForEach);

            var toolSpecificDebug =
                debugStates.Where(ds => ds.ParentID.GetValueOrDefault() == sequenceId && ds.DisplayName.Equals(toolName)).ToList();
            Assert.IsNotNull(toolSpecificDebug);
            var debugState = toolSpecificDebug.FirstOrDefault();
            if (debugState != null)
            {
                for (int index = 0; index < debugState.Inputs.Count; index++)
                {
                    var debugItem = debugState.Inputs[index];
                    var tableRow = table.Rows[index];
                    var variable = tableRow["Variable"];
                    var value = tableRow["value"];
                    var label = tableRow["label"];
                    var operater = tableRow["operater"];
                    var debugItemResult = debugItem.ResultsList.FirstOrDefault();
                    if (debugItemResult != null)
                    {
                        Assert.AreEqual((object)value, debugItemResult.Value);
                        Assert.AreEqual(variable, debugItemResult.Variable);
                        Assert.AreEqual(label, debugItemResult.Label);
                        Assert.AreEqual(operater, debugItemResult.Operator);
                    }
                }
            }
        }

        [Then(@"the dotnetdll ""(.*)"" in ""(.*)"" in step (.*) for ""(.*)"" debug output as")]
        public void ThenTheDotnetdllInInStepForDebugOutputAs(string toolName, string sequenceName, int stepNumber, string forEachName, Table table)
        {
            TryGetValue("activityList", out Dictionary<string, Activity> activityList);
            TryGetValue("parentWorkflowName", out string parentWorkflowName);

            var debugStates = Get<List<IDebugState>>("debugStates").ToList();
            var workflowId = debugStates.First(wf => wf.DisplayName.Equals(forEachName)).ID;

            if (parentWorkflowName == forEachName)
            {
                workflowId = Guid.Empty;
            }

            var sequenceDebug = debugStates.Where(ds => ds.ParentID.GetValueOrDefault() == workflowId).ToList();
            Assert.IsTrue(sequenceDebug.Count >= stepNumber);

            var sequenceId = sequenceDebug[stepNumber - 1].ID;
            var sequenceIsInForEach = sequenceDebug.Any(state => state.ID == sequenceId);
            Assert.IsTrue(sequenceIsInForEach);

            var toolSpecificDebug =
                debugStates.Where(ds => ds.ParentID.GetValueOrDefault() == sequenceId && ds.DisplayName.Equals(toolName)).ToList();
            Assert.IsNotNull(toolSpecificDebug);
            var debugState = toolSpecificDebug.FirstOrDefault();
            if (debugState != null)
            {
                for (int index = 0; index < debugState.Outputs.Count; index++)
                {
                    var debugItem = debugState.Outputs[index];
                    var tableRow = table.Rows[index];
                    var variable = tableRow["Variable"];
                    var value = tableRow["value"];
                    var operater = tableRow["operater"];
                    var debugItemResult = debugItem.ResultsList.FirstOrDefault();
                    if (debugItemResult != null)
                    {
                        Assert.AreEqual((object)value, debugItemResult.Value);
                        Assert.AreEqual(variable, debugItemResult.Variable);
                        Assert.AreEqual(operater, debugItemResult.Operator);
                    }
                }
            }
        }


        [Then(@"the ""(.*)"" in '(.*)' in step (.*) for ""(.*)"" debug outputs as")]
        public void ThenTheInInStepForDebugOutputsAs(string toolName, string sequenceName, int stepNumber, string forEachName, Table table)
        {
            TryGetValue("activityList", out Dictionary<string, Activity> activityList);
            TryGetValue("parentWorkflowName", out string parentWorkflowName);

            var debugStates = Get<List<IDebugState>>("debugStates").ToList();
            var workflowId = debugStates.First(wf => wf.DisplayName.Equals(forEachName)).ID;

            if (parentWorkflowName == forEachName)
            {
                workflowId = Guid.Empty;
            }

            var sequenceDebug = debugStates.Where(ds => ds.ParentID.GetValueOrDefault() == workflowId).ToList();
            Assert.IsTrue(sequenceDebug.Count >= stepNumber);

            var sequenceId = sequenceDebug[stepNumber - 1].ID;
            var sequenceIsInForEach = sequenceDebug.Any(state => state.ID == sequenceId);
            Assert.IsTrue(sequenceIsInForEach);

            var toolSpecificDebug =
                debugStates.Where(ds => ds.ParentID.GetValueOrDefault() == sequenceId && ds.DisplayName.Equals(toolName)).ToList();

            _commonSteps.ThenTheDebugInputsAs(table, toolSpecificDebug
                                                    .SelectMany(item => item.Outputs)
                                                    .SelectMany(item => item.ResultsList).ToList());
        }


        IDebugState DebugToUse(int stepNumber, List<IDebugState> toolSpecificDebug)
        {
            var debugToUse = toolSpecificDebug[stepNumber - 1];
            return debugToUse;
        }




        [Then(@"Workflow ""(.*)"" has errors")]
        public void ThenWorkflowHasErrors(string workFlowName, Table table)
        {
            TryGetValue("activityList", out Dictionary<string, Activity> activityList);
            TryGetValue("parentWorkflowName", out string parentWorkflowName);

            var debugStates = Get<List<IDebugState>>("debugStates").ToList();
            var toolSpecificDebug = debugStates.Last(wf => wf.DisplayName.Equals(workFlowName));
            foreach (var tableRow in table.Rows)
            {
                var strings = toolSpecificDebug.ErrorMessage.Replace("\n", ",").Replace("\r", "").Split(',');
                var predicate = tableRow["Error"];
                var first = strings.First(p => p == predicate);
                Assert.IsFalse(string.IsNullOrEmpty(first));
            }
        }


        [Then(@"the ""(.*)"" in step (.*) for ""(.*)"" debug outputs as")]
        public void ThenTheInStepForDebugOutputsAs(string toolName, int stepNumber, string forEachName, Table table)
        {
            TryGetValue("activityList", out Dictionary<string, Activity> activityList);
            TryGetValue("parentWorkflowName", out string parentWorkflowName);

            var debugStates = Get<List<IDebugState>>("debugStates").ToList();
            var workflowId = debugStates.First(wf => wf.DisplayName.Equals(forEachName)).ID;

            if (parentWorkflowName == forEachName)
            {
                workflowId = Guid.Empty;
            }

            var toolSpecificDebug =
                debugStates.Where(ds => ds.ParentID.GetValueOrDefault() == workflowId && ds.DisplayName.Equals(toolName)).ToList();
            Assert.IsTrue(toolSpecificDebug.Count >= stepNumber);
            var debugToUse = DebugToUse(stepNumber, toolSpecificDebug);


            var outputDebugItems = debugToUse.Outputs
                .SelectMany(s => s.ResultsList).ToList();
            _commonSteps.ThenTheDebugOutputAs(table, outputDebugItems);
        }

        [Given(@"""(.*)"" contains a ""(.*)"" service ""(.*)"" with mappings")]
        public void GivenContainsADatabaseServiceWithMappings(string wf, string serviceType, string serviceName, Table table)
        {
            var environmentModel = ServerRepository.Instance.Source;
            var repository = new ResourceRepository(environmentModel);
            repository.Load(false);
            var resource = repository.FindSingle(r => r.ResourceName.Equals(serviceName), true, true);
            if (resource == null)
            {
                throw new Exception("Local Warewolf service " + serviceName + " not found.");
            }
            var activity = GetServiceActivity(serviceType);
            if (activity != null)
            {
                var outputSb = GetOutputMapping(table, resource);
                var inputSb = GetInputMapping(table, resource);
                var outputMapping = outputSb.ToString();
                var inputMapping = inputSb.ToString();
                resource.Outputs = outputMapping;
                resource.Inputs = inputMapping;
                resource.ResourceType = ResourceType.Service;
                activity.ResourceID = resource.ID;
                activity.ServiceName = resource.ResourceName;
                activity.DisplayName = serviceName;
                activity.OutputMapping = outputMapping;
                activity.InputMapping = inputMapping;

                if (resource.ServerResourceType == "DbService")
                {
                    var xml = resource.ToServiceDefinition(true).ToXElement();
                    var service = new DbService(xml);
                    var source = service.Source as DbSource;
                    Activity updatedActivity = null;
                    switch (serviceType)
                    {
                        case "mysql database":
                            updatedActivity = ActivityUtils.GetDsfMySqlDatabaseActivity((DsfDatabaseActivity)activity, source, service);
                            break;
                        case "sqlserver database":
                            updatedActivity = ActivityUtils.GetDsfSqlServerDatabaseActivity((DsfDatabaseActivity)activity, service, source);
                            break;
                        case "postgresql database":
                            updatedActivity = ActivityUtils.GetDsfPostgreSqlActivity((DsfDatabaseActivity)activity, service, source);
                            break;
                        default:
                            break;
                    }
                    _commonSteps.AddActivityToActivityList(wf, serviceName, updatedActivity);
                }
                else if (resource.ServerResourceType == "WebService")
                {
                    var updatedActivity = new WebGetActivity();
                    var xml = resource.ToServiceDefinition(true).ToXElement();
                    var service = new WebService(xml);
                    var source = service.Source as WebSource;
                    updatedActivity.Headers = new List<INameValue>();
                    service.Headers?.AddRange(service.Headers);
                    updatedActivity.OutputDescription = service.OutputDescription;
                    updatedActivity.QueryString = service.RequestUrl;
                    updatedActivity.Inputs = ActivityUtils.TranslateInputMappingToInputs(inputMapping);
                    updatedActivity.Outputs = ActivityUtils.TranslateOutputMappingToOutputs(outputMapping);
                    updatedActivity.DisplayName = serviceName;
                    if (source != null)
                    {
                        updatedActivity.SourceId = source.ResourceID;
                    }
                    _commonSteps.AddActivityToActivityList(wf, serviceName, updatedActivity);
                }
                else
                {
                    _commonSteps.AddActivityToActivityList(wf, serviceName, activity);
                }
            }
        }

        [Given(@"""(.*)"" contains ""(.*)"" from server ""(.*)"" with mapping as")]
        public void GivenContainsFromServerWithMappingAs(string wf, string remoteWf, string server, Table mappings)
        {
            if (server == "Remote Connection Integration")
            {
                _containerOps = new Depends(Depends.ContainerType.CIRemote);
            }
            else if (remoteWf == "TestSqlReturningXml" || remoteWf == "TestSqlExecutesOnce")
            {
                _containerOps = new Depends(Depends.ContainerType.MSSQL);
            }
            else if (remoteWf == "RabbitMQTest")
            {
                _containerOps = new Depends(Depends.ContainerType.RabbitMQ);
            }

            var localHostEnv = LocalEnvModel;

            EnsureEnvironmentConnected(localHostEnv, EnvironmentConnectionTimeout);

            if (server == "localhost" && remoteWf == "TestmySqlReturningXml")
            {
                _containerOps = new Depends(Depends.ContainerType.MySQL);
            }

            var remoteEnvironment = ServerRepository.Instance.FindSingle(model => model.Name == server);
            if (remoteEnvironment == null)
            {
                var environments = ServerRepository.Instance.LookupEnvironments(localHostEnv);
                remoteEnvironment = environments.FirstOrDefault(model => model.Name == server);
            }
            if (remoteEnvironment != null)
            {
                if (server == "Remote Connection Integration")
                {
                    remoteEnvironment.Connection = new ServerProxy(
                        $"http://{_containerOps.Container.IP}:{_containerOps.Container.Port}", "WarewolfAdmin",
                        "W@rEw0lf@dm1n");
                }

                EnsureEnvironmentConnected(remoteEnvironment, EnvironmentConnectionTimeout);
                if (!remoteEnvironment.HasLoadedResources)
                {
                    remoteEnvironment.ForceLoadResources();
                }
                var splitNameAndCat = remoteWf.Split('/');
                var resName = splitNameAndCat[splitNameAndCat.Length - 1];
                var remoteResourceModel = remoteEnvironment.ResourceRepository.FindSingle(model => model.ResourceName == resName
                                                                         || model.Category == remoteWf.Replace('/', '\\'), true);

                if (remoteResourceModel == null)
                {
                    remoteEnvironment.LoadResources();
                    remoteResourceModel = remoteEnvironment.ResourceRepository.FindSingle(model => model.ResourceName == resName
                                                                         || model.Category == remoteWf.Replace('/', '\\'), true);
                }
                if (remoteResourceModel != null)
                {
                    var dataMappingViewModel = GetDataMappingViewModel(remoteResourceModel, mappings);

                    var inputMapping = dataMappingViewModel.GetInputString(dataMappingViewModel.Inputs);
                    var outputMapping = dataMappingViewModel.GetOutputString(dataMappingViewModel.Outputs);

                    var activity = new DsfWorkflowActivity();

                    remoteResourceModel.Outputs = outputMapping;
                    remoteResourceModel.Inputs = inputMapping;
                    var remoteServerId = remoteEnvironment.EnvironmentID;
                    activity.ServiceServer = remoteServerId;
                    activity.EnvironmentID = remoteServerId;

                    activity.IsWorkflow = true;
                    if (remoteServerId != Guid.Empty)
                    {
                        activity.ServiceUri = remoteEnvironment.Connection.AppServerUri.ToString();
                        activity.IsWorkflow = false;
                    }

                    activity.ResourceID = remoteResourceModel.ID;
                    activity.ServiceName = remoteResourceModel.Category;
                    activity.DisplayName = remoteWf;
                    activity.OutputMapping = outputMapping;
                    activity.InputMapping = inputMapping;
                    _commonSteps.AddActivityToActivityList(wf, remoteWf, activity);
                }
                else
                {
                    throw new Exception($"Remote Warewolf service {remoteWf} not found on server {server}.");
                }
            }
            else
            {
                throw new Exception($"Remote server {server} not found");
            }
        }

        DataMappingViewModel GetDataMappingViewModel(IResourceModel remoteResourceModel, Table mappings)
        {
            var webActivity = new WebActivity { ResourceModel = remoteResourceModel as ResourceModel };
            var dataMappingViewModel = new DataMappingViewModel(webActivity);
            foreach (var inputOutputViewModel in dataMappingViewModel.Inputs)
            {
                inputOutputViewModel.Value = "";
                inputOutputViewModel.RecordSetName = "";
                inputOutputViewModel.Name = "";
                inputOutputViewModel.MapsTo = "";
            }

            foreach (var inputOutputViewModel in dataMappingViewModel.Outputs)
            {
                inputOutputViewModel.Value = "";
                inputOutputViewModel.RecordSetName = "";
                inputOutputViewModel.Name = "";
            }
            foreach (var tableRow in mappings.Rows)
            {
                tableRow.TryGetValue("Output from Service", out string output);
                tableRow.TryGetValue("To Variable", out string toVariable);
                if (!string.IsNullOrEmpty(output) && !string.IsNullOrEmpty(toVariable))
                {
                    var inputOutputViewModel = dataMappingViewModel.Outputs.FirstOrDefault(model => model.DisplayName == output);
                    if (inputOutputViewModel != null)
                    {
                        inputOutputViewModel.Value = toVariable;
                        if (DataListUtil.IsValueRecordset(output))
                        {
                            inputOutputViewModel.RecordSetName = DataListUtil.ExtractRecordsetNameFromValue(output);
                            inputOutputViewModel.Name = DataListUtil.ExtractFieldNameFromValue(output);
                        }
                        else
                        {
                            inputOutputViewModel.Name = output;
                        }
                        inputOutputViewModel.RecordSetName = DataListUtil.ExtractRecordsetNameFromValue(output);
                        _commonSteps.AddVariableToVariableList(toVariable);
                    }
                }

                tableRow.TryGetValue("Input to Service", out string input);
                tableRow.TryGetValue("From Variable", out string fromVariable);

                if (!string.IsNullOrEmpty(input) && !string.IsNullOrEmpty(fromVariable))
                {
                    var inputOutputViewModel = dataMappingViewModel.Inputs.FirstOrDefault(model => model.DisplayName == input);
                    if (inputOutputViewModel != null)
                    {
                        inputOutputViewModel.MapsTo = fromVariable;

                        if (DataListUtil.IsValueRecordset(input))
                        {
                            inputOutputViewModel.RecordSetName = DataListUtil.ExtractRecordsetNameFromValue(input);
                            inputOutputViewModel.Name = DataListUtil.ExtractFieldNameFromValue(input);
                        }
                        else
                        {
                            inputOutputViewModel.Name = input;
                        }
                        inputOutputViewModel.Value = input;
                        _commonSteps.AddVariableToVariableList(fromVariable);
                    }
                }

            }
            return dataMappingViewModel;
        }

        DsfActivity GetServiceActivity(string serviceType)
        {
            DsfActivity activity = null;
            switch (serviceType)
            {
                case "mysql database":
                    activity = new DsfDatabaseActivity();
                    break;
                case "sqlserver database":
                    activity = new DsfDatabaseActivity();
                    break;
                case "oracle database":
                    activity = new DsfDatabaseActivity();
                    break;
                case "odbc database":
                    activity = new DsfDatabaseActivity();
                    break;
                case "postgresql database":
                    activity = new DsfDatabaseActivity();
                    break;
                case "plugin":
                    activity = new DsfPluginActivity();
                    break;
                case "workflow":
                    activity = new DsfWorkflowActivity();
                    break;
                default:
                    break;
            }
            return activity;
        }

        StringBuilder GetOutputMapping(Table table, IResourceModel resource)
        {
            var outputSb = new StringBuilder();
            outputSb.Append("<Outputs>");

            foreach (var tableRow in table.Rows)
            {
                var output = tableRow["Output from Service"];
                var toVariable = tableRow["To Variable"];

                _commonSteps.AddVariableToVariableList(toVariable);

                if (resource != null)
                {

                    var outputs = XDocument.Parse(resource.Outputs);

                    string recordsetName;
                    string fieldName;

                    if (DataListUtil.IsValueRecordset(output))
                    {
                        recordsetName = DataListUtil.ExtractRecordsetNameFromValue(output);
                        fieldName = DataListUtil.ExtractFieldNameFromValue(output);
                    }
                    else
                    {
                        recordsetName = fieldName = output;
                    }

                    var element = (from elements in outputs.Descendants("Output")
                                   where string.Equals((string)elements.Attribute("RecordsetAlias"), recordsetName, StringComparison.InvariantCultureIgnoreCase) &&
                                         string.Equals((string)elements.Attribute("OriginalName"), fieldName, StringComparison.InvariantCultureIgnoreCase)
                                   select elements).SingleOrDefault();

                    element?.SetAttributeValue("Value", toVariable);

                    outputSb.Append(element);
                }
            }

            outputSb.Append("</Outputs>");
            return outputSb;
        }

        [Given(@"'(.*)' in ""(.*)"" contains Data Merge ""(.*)"" into ""(.*)"" as")]
        public void GivenInContainsDataMergeIntoAs(string sequenceName, string forEachName, string activityName, string resultVariable, Table table)
        {
            var activity = new DsfDataMergeActivity { Result = resultVariable, DisplayName = activityName };
            foreach (var tableRow in table.Rows)
            {
                var variable = tableRow["Variable"];
                var type = tableRow["Type"];
                var at = tableRow["Using"];
                var padding = tableRow["Padding"];
                var alignment = tableRow["Alignment"];

                activity.MergeCollection.Add(new DataMergeDTO(variable, type, at, 1, padding, alignment, true));
            }
            _commonSteps.AddVariableToVariableList(resultVariable);
            AddActivityToSequenceInsideForEach(sequenceName, forEachName, activity);
        }

        void AddActivityToSequenceInsideForEach(string sequenceName, string forEachName, Activity activity)
        {

            var activityList = _commonSteps.GetActivityList();
            if (activityList[forEachName] is DsfForEachActivity forEachActivity)
            {
                if (forEachActivity.DataFunc.Handler is DsfSequenceActivity sequenceActivity && sequenceActivity.DisplayName == sequenceName)
                {
                    sequenceActivity.Activities.Add(activity);
                }
            }
        }

        [Given(@"'(.*)' in ""(.*)"" contains Gather System Info ""(.*)"" as")]
        public void GivenInContainsGatherSystemInfoAs(string sequenceName, string forEachName, string activityName, Table table)
        {
            var activity = new DsfGatherSystemInformationActivity { DisplayName = activityName };
            foreach (var tableRow in table.Rows)
            {
                var variable = tableRow["Variable"];

                _commonSteps.AddVariableToVariableList(variable);

                var systemInfo = (enTypeOfSystemInformationToGather)Dev2EnumConverter.GetEnumFromStringDiscription(tableRow["Selected"], typeof(enTypeOfSystemInformationToGather));
                activity.SystemInformationCollection.Add(new GatherSystemInformationTO(systemInfo, variable, 1));
            }

            AddActivityToSequenceInsideForEach(sequenceName, forEachName, activity);
        }

        StringBuilder GetInputMapping(Table table, IResourceModel resource)
        {
            var inputSb = new StringBuilder();
            inputSb.Append("<Inputs>");

            foreach (var tableRow in table.Rows)
            {
                var input = tableRow["Input to Service"];
                var fromVariable = tableRow["From Variable"];

                _commonSteps.AddVariableToVariableList(fromVariable);

                if (resource != null)
                {
                    var inputs = XDocument.Parse(resource.Inputs);

                    string recordsetName;
                    XElement element;
                    if (DataListUtil.IsValueRecordset(input))
                    {
                        recordsetName = DataListUtil.ExtractRecordsetNameFromValue(input);
                        var fieldName = DataListUtil.ExtractFieldNameFromValue(input);

                        element = (from elements in inputs.Descendants("Input")
                                   where string.Equals((string)elements.Attribute("Recordset"), recordsetName, StringComparison.InvariantCultureIgnoreCase) &&
                                         string.Equals((string)elements.Attribute("OriginalName"), fieldName, StringComparison.InvariantCultureIgnoreCase)
                                   select elements).SingleOrDefault();

                        element?.SetAttributeValue("Value", fromVariable);

                        inputSb.Append(element);
                    }
                    else
                    {
                        recordsetName = input;

                        element = (from elements in inputs.Descendants("Input")
                                   where string.Equals((string)elements.Attribute("Name"), recordsetName, StringComparison.InvariantCultureIgnoreCase)
                                   select elements).SingleOrDefault();

                        element?.SetAttributeValue("Source", fromVariable);
                    }

                    if (element != null)
                    {
                        inputSb.Append(element);
                    }
                }
            }

            inputSb.Append("</Inputs>");
            return inputSb;
        }

        [When(@"""(.*)"" is the active environment used to execute ""(.*)""")]
        public void WhenIsTheActiveEnvironmentUsedToExecute(string connectionName, string workflowName)
        {
            TryGetValue(workflowName, out IContextualResourceModel resourceModel);
            TryGetValue("environment", out IServer server);
            TryGetValue("resourceRepo", out IResourceRepository repository);

            ExecuteWorkflow(resourceModel);
        }

        [When(@"the workflow is Saved")]
        private IContextualResourceModel SaveTheWorkflow() => SaveAWorkflow(null);

        [When(@"""(.*)"" is Saved")]
        private IContextualResourceModel SaveAWorkflow(string parentName)
        {
            TryGetValue("parentWorkflowName", out string parentWorkflowName);
            var workflowName = string.IsNullOrEmpty(parentWorkflowName) ? parentName : parentWorkflowName;

            Get<List<IDebugState>>("debugStates").Clear();
            BuildDataList();

            var activityList = _commonSteps.GetActivityList();

            var flowSteps = new List<FlowStep>();

            TestStartNode = new FlowStep();
            flowSteps.Add(TestStartNode);
            if (activityList != null)
            {
                foreach (var activity in activityList)
                {
                    if (activity.Value is DsfDecision dec)
                    {
                        var decConfig = Get<(string TrueArm, string FalseArm)>(dec.DisplayName);
                        var trueArmToolName = decConfig.TrueArm;
                        var falseArmToolName = decConfig.FalseArm;
                        dec.TrueArm = new List<IDev2Activity> { activityList.FirstOrDefault(act => act.Key == trueArmToolName).Value as IDev2Activity };
                        dec.FalseArm = new List<IDev2Activity> { activityList.FirstOrDefault(act => act.Key == falseArmToolName).Value as IDev2Activity };
                    }
                    if (TestStartNode.Action == null)
                    {
                        TestStartNode.Action = activity.Value;
                    }
                    else
                    {
                        var flowStep = new FlowStep { Action = activity.Value };
                        flowSteps.Last().Next = flowStep;
                        flowSteps.Add(flowStep);
                    }
                }
            }
            TryGetValue(workflowName, out IContextualResourceModel resourceModel);
            TryGetValue("environment", out IServer server);
            TryGetValue("resourceRepo", out IResourceRepository repository);

            var currentDl = CurrentDl;
            resourceModel.DataList = currentDl.Replace("root", "DataList");
            var helper = new WorkflowHelper();
            var xamlDefinition = helper.GetXamlDefinition(FlowchartActivityBuilder);
            resourceModel.WorkflowXaml = xamlDefinition;

            repository.Save(resourceModel);
            repository.SaveToServer(resourceModel);

            return resourceModel;
        }


        [Given(@"the ""(.*)"" in WorkFlow ""(.*)"" debug inputs as")]
        [When(@"the ""(.*)"" in WorkFlow ""(.*)"" debug inputs as")]
        [Then(@"the ""(.*)"" in WorkFlow ""(.*)"" debug inputs as")]
        public void ThenTheInWorkFlowDebugInputsAs(string toolName, string workflowName, Table table)
        {
            TryGetValue("activityList", out Dictionary<string, Activity> activityList);
            TryGetValue("parentWorkflowName", out string parentWorkflowName);
            var debugStates = Get<List<IDebugState>>("debugStates").ToList();
            var workflowId = Guid.Empty;

            if (parentWorkflowName != workflowName)
            {
                if (toolName != null && workflowName != null)
                {
                    workflowId = debugStates.First(wf => wf.DisplayName.Equals(workflowName)).ID;
                }
                else
                {
                    throw new InvalidOperationException("SpecFlow broke.");
                }
            }

            var toolSpecificDebug =
                debugStates.Where(ds => ds.ParentID.GetValueOrDefault() == workflowId && ds.DisplayName.Equals(toolName)).ToList();

            _commonSteps.ThenTheDebugInputsAs(table, toolSpecificDebug.Distinct()
                                                    .SelectMany(s => s.Inputs)
                                                    .SelectMany(s => s.ResultsList).ToList());
        }

        [Given(@"the tool ""(.*)"" with Guid of ""(.*)"" in WorkFlow ""(.*)"" debug inputs as")]
        [When(@"the tool ""(.*)"" with Guid of ""(.*)"" in WorkFlow ""(.*)"" debug inputs as")]
        [Then(@"the tool ""(.*)"" with Guid of ""(.*)"" in WorkFlow ""(.*)"" debug inputs as")]
        public void ThenTheToolWithGuidOfInWorkFlowDebugInputsAs(string toolName, string toolGuid, string workflowName, Table table)
        {
            TryGetValue("activityList", out Dictionary<string, Activity> activityList);
            TryGetValue("parentWorkflowName", out string parentWorkflowName);
            var debugStates = Get<List<IDebugState>>("debugStates").ToList();
            var workflowId = Guid.Empty;

            if (parentWorkflowName != workflowName)
            {
                if (workflowName != null)
                {
                    workflowId = debugStates.First(wf => wf.DisplayName.Equals(workflowName)).ID;
                }
                else
                {
                    throw new InvalidOperationException("Could not find workflow.");
                }
            }

            var toolSpecificDebug = debugStates.Where(ds => ds.ParentID.GetValueOrDefault() == workflowId &&
                                                            GetMatchingNames(toolName, ds.DisplayName) &&
                                                            ds.ID.Equals(Guid.Parse(toolGuid))).ToList();

            _commonSteps.ThenTheDebugInputsAs(table, toolSpecificDebug.Distinct()
                                                    .SelectMany(s => s.Inputs)
                                                    .SelectMany(s => s.ResultsList).ToList());
        }

        [Then(@"the ""(.*)"" has a start and end duration")]
        public void ThenTheHasAStartAndEndDuration(string workflowName)
        {
            TryGetValue("activityList", out Dictionary<string, Activity> activityList);
            TryGetValue("parentWorkflowName", out string parentWorkflowName);
            var debugStates = Get<List<IDebugState>>("debugStates").ToList();

            var end = debugStates.First(wf => wf.Name.Equals("End"));
            Assert.IsTrue(end.Duration.Ticks > 0, "Workflow debug output end step duration of " + end.Duration.Ticks + " ticks is less than or equal to 0 ticks. All workflows no matter how simple do take some time to execute.");
        }

        [Then(@"""(.*)"" Duration is less or equal to (.*) seconds")]
        [Given(@"""(.*)"" Duration is less or equal to (.*) seconds")]
        [When(@"""(.*)"" Duration is less or equal to (.*) seconds")]
        public void ThenDurationIsLessOrEqualToSeconds(string workflowName, int duration)
        {
            TryGetValue("activityList", out Dictionary<string, Activity> activityList);
            TryGetValue("parentWorkflowName", out string parentWorkflowName);
            var debugStates = Get<List<IDebugState>>("debugStates").ToList();

            var end = debugStates.First(wf => wf.Name.Equals("End"));
            Assert.IsTrue(end.Duration.Ticks > 0);
            var condition = end.Duration.Seconds <= duration;
            Assert.IsTrue(condition);
        }

        [Then(@"""(.*)"" Duration is greater or equal to (.*) seconds")]
        [When(@"""(.*)"" Duration is greater or equal to (.*) seconds")]
        [Given(@"""(.*)"" Duration is greater or equal to (.*) seconds")]
        public void ThenDurationIsGreaterOrEqualToSeconds(string workflowName, int duration)
        {
            TryGetValue("activityList", out Dictionary<string, Activity> activityList);
            TryGetValue("parentWorkflowName", out string parentWorkflowName);
            var debugStates = Get<List<IDebugState>>("debugStates").ToList();
            var end = debugStates.First(wf => wf.Name.Equals("End"));
            Assert.IsTrue(end.Duration.Ticks > 0);
            var condition = end.Duration.Seconds >= duration;
            Assert.IsTrue(condition);
        }




        [Then(@"the nested ""(.*)"" in WorkFlow ""(.*)"" debug inputs as")]
        public void ThenTheNestedInWorkFlowDebugInputsAs(string toolName, string workflowName, Table table)
        {
            TryGetValue("activityList", out Dictionary<string, Activity> activityList);
            TryGetValue("parentWorkflowName", out string parentWorkflowName);
            var debugStates = Get<List<IDebugState>>("debugStates").ToList();

            var toolSpecificDebug =
                debugStates.Where(ds => ds.DisplayName.Equals(toolName)).ToList();

            _commonSteps.ThenTheDebugInputsAs(table, toolSpecificDebug
                                                    .SelectMany(s => s.Inputs)
                                                    .SelectMany(s => s.ResultsList).ToList());
        }

        [Then(@"the ""(.*)"" in WorkFlow ""(.*)"" has  ""(.*)"" nested children")]
        public void ThenTheInWorkFlowHasNestedChildren(string toolName, string workflowName, int count)
        {
            TryGetValue("activityList", out Dictionary<string, Activity> activityList);
            TryGetValue("parentWorkflowName", out string parentWorkflowName);
            var debugStates = Get<List<IDebugState>>("debugStates").ToList();

            var id =
                debugStates.Where(ds => ds.DisplayName.Equals(toolName)).ToList().Select(a => a.ID).First();
            var children = debugStates.Count(a => a.ParentID.GetValueOrDefault() == id);
            Assert.AreEqual(count, children, String.Join(", ", debugStates.Select(val => val.DisplayName)));
        }

        [Then(@"the ""(.*)"" in WorkFlow ""(.*)"" has at least ""(.*)"" nested children")]
        public void ThenTheInWorkFlowHasAtLeastNestedChildren(string toolName, string workflowName, int count)
        {
            TryGetValue("activityList", out Dictionary<string, Activity> activityList);
            TryGetValue("parentWorkflowName", out string parentWorkflowName);
            var debugStates = Get<List<IDebugState>>("debugStates").ToList();

            var id =
                debugStates.Where(ds => ds.DisplayName.Equals(toolName)).ToList().Select(a => a.ID).First();
            var children = debugStates.Count(a => a.ParentID.GetValueOrDefault() == id);
            Assert.IsTrue(children >= count, $"Not enough children nested inside {toolName} in {workflowName}'s debug output");
        }

        [Then(@"each ""(.*)"" contains debug outputs for ""(.*)"" as")]
        public void ThenEachContainsDebugOutputsForAs(string toolName, string nestedToolName, Table table)
        {
            TryGetValue("activityList", out Dictionary<string, Activity> activityList);
            TryGetValue("parentWorkflowName", out string parentWorkflowName);
            var debugStates = Get<List<IDebugState>>("debugStates").ToList();

            var id = debugStates.Where(ds => ds.DisplayName.Equals("DsfActivity")).ToList();
            id.ForEach(x => Assert.AreEqual(1, debugStates.Count(a => a.ParentID.GetValueOrDefault() == x.ID && a.DisplayName == nestedToolName)));
        }

        T Get<T>(string keyName)
        {
            return _scenarioContext.Get<T>(keyName);
        }

        void TryGetValue<T>(string keyName, out T value)
        {
            _scenarioContext.TryGetValue(keyName, out value);
        }

        [Then(@"the ""(.*)"" in Workflow ""(.*)"" has a debug Server Name of """"(.*)""""")]
        public void ThenTheInWorkflowHasADebugServerNameOf(string toolName, string workflowName, string remoteName)
        {
            TryGetValue("activityList", out Dictionary<string, Activity> activityList);
            TryGetValue("parentWorkflowName", out string parentWorkflowName);

            var debugStates = Get<List<IDebugState>>("debugStates").ToList();
            var workflowId = debugStates.First(wf => wf.DisplayName.Equals(workflowName)).ID;

            if (parentWorkflowName == workflowName)
            {
                workflowId = Guid.Empty;
            }

            var toolSpecificDebug =
                debugStates.Where(ds => ds.ParentID.GetValueOrDefault() == workflowId && ds.DisplayName.Equals(toolName)).ToList();

            Assert.IsTrue(toolSpecificDebug.All(a => a.Server == remoteName));
            Assert.IsTrue(debugStates.Where(ds => ds.ParentID.GetValueOrDefault() == workflowId && !ds.DisplayName.Equals(toolName)).All(a => a.Server == "localhost"));
        }

        [Then(@"the ""(.*)"" in Workflow ""(.*)"" debug outputs is")]
        public void ThenTheInWorkflowDebugOutputsIs(string p0, string p1, Table table)
        {
            ThenTheInWorkflowDebugOutputsAs(p0, p1, table);
        }

        [Given(@"the ""(.*)"" in Workflow ""(.*)"" debug outputs as")]
        [When(@"the ""(.*)"" in Workflow ""(.*)"" debug outputs as")]
        [Then(@"the ""(.*)"" in Workflow ""(.*)"" debug outputs as")]
        public void ThenTheInWorkflowDebugOutputsAs(string toolName, string workflowName, Table table)
        {
            TryGetValue("activityList", out Dictionary<string, Activity> activityList);
            TryGetValue("parentWorkflowName", out string parentWorkflowName);

            var debugStates = Get<List<IDebugState>>("debugStates");
            var workflowId = Guid.Empty;

            if (parentWorkflowName != workflowName)
            {
                if (toolName != null && workflowName != null)
                {
                    IDebugState debugState = debugStates.FirstOrDefault(wf => wf.DisplayName.Equals(workflowName));
                    if (debugState != null)
                    {
                        workflowId = debugState.ID;
                    }
                    else
                    {
                        var errors = debugStates.Where(wf => wf.ErrorMessage != "");
                        var errorsMessage = "";
                        if (errors != null)
                        {
                            errorsMessage = " There were one or more errors found in other tools on the same workflow though: " + string.Join(", ", errors.Select(wf => wf.ErrorMessage).Distinct().ToArray());
                        }
                        Assert.Fail($"Debug output for {toolName} not found in {workflowName}.{errorsMessage}");
                    }
                }
                else
                {
                    throw new InvalidOperationException("SpecFlow broke.");
                }
            }

            var toolSpecificDebug =
                debugStates.Where(ds => ds.ParentID.GetValueOrDefault() == workflowId && ds.DisplayName.Equals(toolName)).ToList();
            if (!toolSpecificDebug.Any())
            {
                toolSpecificDebug =
                debugStates.Where(ds => ds.DisplayName.Equals(toolName)).ToList();
            }
            // Data Merge breaks our debug scheme, it only ever has 1 value, not the expected 2 ;)
            var isDataMergeDebug = toolSpecificDebug.Count == 1 && toolSpecificDebug.Any(t => t.Name == "Data Merge");
            IDebugState outputState;
            if (toolSpecificDebug.Count > 1 && toolSpecificDebug.Any(state => state.StateType == StateType.End))
            {
                outputState = toolSpecificDebug.FirstOrDefault(state => state.StateType == StateType.End);
            }
            else
            {
                outputState = toolSpecificDebug.FirstOrDefault();
            }

            if (outputState != null && outputState.Outputs != null)
            {
                var SelectResults = outputState.Outputs.SelectMany(s => s.ResultsList);
                if (SelectResults != null && SelectResults.ToList() != null)
                {
                    _commonSteps.ThenTheDebugOutputAs(table, SelectResults.ToList(), isDataMergeDebug);
                    return;
                }
                Assert.Fail(outputState.Outputs.ToList() + " debug outputs found on " + workflowName + " does not include " + toolName + ".");
            }
            Assert.Fail("No debug output found for " + workflowName + ".");
        }


        [Given(@"the ""(.*)"" in Workflow ""(.*)"" unsorted debug outputs as")]
        [When(@"the ""(.*)"" in Workflow ""(.*)"" unsorted debug outputs as")]
        [Then(@"the ""(.*)"" in Workflow ""(.*)"" unsorted debug outputs as")]
        public void ThenTheInWorkflowUnsortedDebugOutputsAs(string toolName, string workflowName, Table table)
        {
            TryGetValue("activityList", out Dictionary<string, Activity> activityList);
            TryGetValue("parentWorkflowName", out string parentWorkflowName);

            var debugStates = Get<List<IDebugState>>("debugStates");
            var workflowId = Guid.Empty;

            if (parentWorkflowName != workflowName)
            {
                if (toolName != null && workflowName != null)
                {
                    IDebugState debugState = debugStates.FirstOrDefault(wf => wf.DisplayName.Equals(workflowName));
                    if (debugState != null)
                    {
                        workflowId = debugState.ID;
                    }
                    else
                    {
                        var errors = debugStates.Where(wf => wf.ErrorMessage != "");
                        var errorsMessage = "";
                        if (errors != null)
                        {
                            errorsMessage = " There were one or more errors found in other tools on the same workflow though: " + string.Join(", ", errors.Select(wf => wf.ErrorMessage).Distinct().ToArray());
                        }
                        Assert.Fail($"Debug output for {toolName} not found in {workflowName}.{errorsMessage}");
                    }
                }
                else
                {
                    throw new InvalidOperationException("SpecFlow broke.");
                }
            }

            var toolSpecificDebug =
                debugStates.Where(ds => ds.ParentID.GetValueOrDefault() == workflowId && ds.DisplayName.Equals(toolName)).ToList();
            if (!toolSpecificDebug.Any())
            {
                toolSpecificDebug =
                debugStates.Where(ds => ds.DisplayName.Equals(toolName)).ToList();
            }
            // Data Merge breaks our debug scheme, it only ever has 1 value, not the expected 2 ;)
            var isDataMergeDebug = toolSpecificDebug.Count == 1 && toolSpecificDebug.Any(t => t.Name == "Data Merge");
            IDebugState outputState;
            if (toolSpecificDebug.Count > 1 && toolSpecificDebug.Any(state => state.StateType == StateType.End))
            {
                outputState = toolSpecificDebug.FirstOrDefault(state => state.StateType == StateType.End);
            }
            else
            {
                outputState = toolSpecificDebug.FirstOrDefault();
            }

            if (outputState != null && outputState.Outputs != null)
            {
                var SelectResults = outputState.Outputs.SelectMany(s => s.ResultsList);
                if (SelectResults != null && SelectResults.ToList() != null)
                {
                    _commonSteps.ThenTheDebugOutputAs(table, SelectResults.ToList(), isDataMergeDebug, true);
                    return;
                }
                Assert.Fail(outputState.Outputs.ToList() + " debug outputs found on " + workflowName + " does not include " + toolName + ".");
            }
            Assert.Fail("No debug output found for " + workflowName + ".");
        }


        [Then(@"the tool ""(.*)"" with Guid of ""(.*)"" in Workflow ""(.*)"" debug outputs as")]
        public void ThenTheToolWithGuidOfInWorkflowDebugOutputsAs(string toolName, string toolGuid, string workflowName, Table table)
        {
            TryGetValue("activityList", out Dictionary<string, Activity> activityList);
            TryGetValue("parentWorkflowName", out string parentWorkflowName);

            var debugStates = Get<List<IDebugState>>("debugStates");
            var workflowId = Guid.Empty;

            if (parentWorkflowName != workflowName)
            {
                if (workflowName != null)
                {
                    var debugState = debugStates.FirstOrDefault(wf => wf.DisplayName.Equals(workflowName));
                    if (debugState != null)
                    {
                        workflowId = debugState.ID;
                    }
                    else
                    {
                        var errors = debugStates.Where(wf => wf.ErrorMessage != "");
                        var errorsMessage = "";
                        if (errors != null)
                        {
                            errorsMessage = " There were one or more errors found in other tools on the same workflow though: " + string.Join(", ", errors.Select(wf => wf.ErrorMessage).Distinct().ToArray());
                        }
                        Assert.Fail($"Debug output for {toolName} not found in {workflowName}.{errorsMessage}");
                    }
                }
                else
                {
                    throw new InvalidOperationException("Could not find workflow.");
                }
            }

            var toolSpecificDebug = debugStates.Where(ds => ds.ParentID.GetValueOrDefault() == workflowId &&
                                                            GetMatchingNames(toolName, ds.DisplayName) &&
                                                            ds.ID.Equals(Guid.Parse(toolGuid))).ToList();
            if (!toolSpecificDebug.Any())
            {
                toolSpecificDebug = debugStates.Where(ds => ds.DisplayName.Equals(toolName)).ToList();
            }
            // Data Merge breaks our debug scheme, it only ever has 1 value, not the expected 2 ;)
            var isDataMergeDebug = toolSpecificDebug.Count == 1 && toolSpecificDebug.Any(t => t.Name == "Data Merge");
            IDebugState outputState;
            if (toolSpecificDebug.Count > 1 && toolSpecificDebug.Any(state => state.StateType == StateType.End))
            {
                outputState = toolSpecificDebug.FirstOrDefault(state => state.StateType == StateType.End);
            }
            else
            {
                outputState = toolSpecificDebug.FirstOrDefault();
            }

            if (outputState != null && outputState.Outputs != null)
            {
                var SelectResults = outputState.Outputs.SelectMany(s => s.ResultsList);
                if (SelectResults != null && SelectResults.ToList() != null)
                {
                    _commonSteps.ThenTheDebugOutputAs(table, SelectResults.ToList(), isDataMergeDebug);
                    return;
                }
                Assert.Fail(outputState.Outputs.ToList() + " debug outputs found on " + workflowName + " does not include " + toolName + ".");
            }
            Assert.Fail("No debug output found for " + workflowName + ".");
        }

        [Then(@"the ""(.*)"" in Workflow ""(.*)"" debug output contains as")]
        public void ThenTheInWorkflowDebugOutputContainsAs(string toolName, string workflowName, Table table)
        {
            TryGetValue("activityList", out Dictionary<string, Activity> activityList);
            TryGetValue("parentWorkflowName", out string parentWorkflowName);

            var debugStates = Get<List<IDebugState>>("debugStates");
            var workflowId = Guid.Empty;

            if (parentWorkflowName != workflowName)
            {
                if (toolName != null && workflowName != null)
                {
                    workflowId = debugStates.First(wf => wf.DisplayName.Equals(workflowName)).ID;
                }
                else
                {
                    throw new InvalidOperationException("SpecFlow broke.");
                }
            }

            var toolSpecificDebug =
                debugStates.Where(ds => ds.ParentID.GetValueOrDefault() == workflowId && ds.DisplayName.Equals(toolName)).ToList();
            if (!toolSpecificDebug.Any())
            {
                toolSpecificDebug =
                debugStates.Where(ds => ds.DisplayName.Equals(toolName)).ToList();
            }
            // Data Merge breaks our debug scheme, it only ever has 1 value, not the expected 2 ;)
            var isDataMergeDebug = toolSpecificDebug.Count == 1 && toolSpecificDebug.Any(t => t.Name == "Data Merge");
            var outputState = toolSpecificDebug.FirstOrDefault();
            if (toolSpecificDebug.Count > 1)
            {
                if (toolSpecificDebug.Any(state => state.StateType == StateType.End))
                {
                    outputState = toolSpecificDebug.FirstOrDefault(state => state.StateType == StateType.End);
                }
            }
            var debugItemResults = outputState?.Outputs.SelectMany(s => s.ResultsList).ToList();
            Assert.IsNotNull(debugItemResults);
            Assert.IsTrue(debugItemResults.Any(result => result.Value.Contains("{\r\n  \"Name\": \"Bob\"\r\n}")));
        }

        [Given(@"""(.*)"" contains an SQL Bulk Insert ""(.*)"" using database ""(.*)"" and table ""(.*)"" and KeepIdentity set ""(.*)"" and Result set ""(.*)"" for testing as")]
        [Given(@"""(.*)"" contains an SQL Bulk Insert ""(.*)"" using database ""(.*)"" and table ""(.*)"" and KeepIdentity set ""(.*)"" and Result set ""(.*)"" as")]
        public void GivenContainsAnSQLBulkInsertUsingDatabaseAndTableAndKeepIdentitySetAndResultSetForTestingAs(string workflowName, string activityName, string dbSrcName, string tableName, string keepIdentity, string result, Table table)
        {
            if (dbSrcName == "NewSqlServerSource" || dbSrcName == "NewSqlBulkInsertSource")
            {
                _containerOps = new Depends(Depends.ContainerType.MSSQL);
            }
            var environmentModel = ServerRepository.Instance.Source;
            environmentModel.Connect();
            var environmentConnection = environmentModel.Connection;
            var controllerFactory = new CommunicationControllerFactory();
            var _proxyLayer = new StudioServerProxy(controllerFactory, environmentConnection);
            var dbSources = _proxyLayer.QueryManagerProxy.FetchDbSources().ToList();
            var dbSource = dbSources.Single(source => source.Name == dbSrcName);

            // extract keepIdentity value ;)
            bool.TryParse(keepIdentity, out bool keepIdentityBool);

            // Configure activity ;)
            var dsfSqlBulkInsert = new DsfSqlBulkInsertActivity
            {
                Result = result
                ,
                DisplayName = activityName
                ,
                TableName = tableName
                ,
                Database = new DbSource
                {
                    ResourceID = dbSource.Id,
                    AuthenticationType = dbSource.AuthenticationType,
                    DatabaseName = dbSrcName,
                },
                KeepIdentity = keepIdentityBool,


            };
            // build input mapping
            var mappings = new List<DataColumnMapping>();

            var pos = 1;

            foreach (var row in table.Rows)

            {
                var outputColumn = row["Column"];
                var inputColumn = row["Mapping"];
                var isNullableStr = row["IsNullable"];
                var dataTypeName = row["DataTypeName"];
                var maxLengthStr = row["MaxLength"];
                var isAutoIncrementStr = row["IsAutoIncrement"];
                bool.TryParse(isNullableStr, out bool isNullable);
                bool.TryParse(isAutoIncrementStr, out bool isAutoIncrement);
                int.TryParse(maxLengthStr, out int maxLength);
                Enum.TryParse(dataTypeName, true, out SqlDbType dataType);

                var mapping = new DataColumnMapping { IndexNumber = pos, InputColumn = inputColumn, OutputColumn = new DbColumn { ColumnName = outputColumn, IsAutoIncrement = isAutoIncrement, IsNullable = isNullable, MaxLength = maxLength, SqlDataType = dataType } };
                mappings.Add(mapping);
                pos++;
            }

            dsfSqlBulkInsert.InputMappings = mappings;
            _commonSteps.AddVariableToVariableList(result);
            _commonSteps.AddActivityToActivityList(workflowName, activityName, dsfSqlBulkInsert);

        }

        [Given(@"""(.*)"" contains an Sort ""(.*)"" as")]
        public void GivenContainsAnSortAs(string parentName, string activityName, Table table)
        {
            var dsfSort = new DsfSortRecordsActivity { DisplayName = activityName, SortField = table.Rows[0][0], SelectedSort = table.Rows[0][1] };
            _commonSteps.AddActivityToActivityList(parentName, activityName, dsfSort);
        }

        [Given(@"""(.*)"" contains a Cmd Script ""(.*)"" ScriptToRun ""(.*)"" and result as ""(.*)""")]
        public void GivenContainsACmdScriptScriptToRunAndResultAs(string parentName, string activityName, string scriptToRun, string Result)
        {
            var commandLineActivity = new DsfExecuteCommandLineActivity
            {
                DisplayName = activityName,
                CommandFileName = scriptToRun,
                CommandResult = Result

            };
            _commonSteps.AddActivityToActivityList(parentName, activityName, commandLineActivity);
        }

        [Given(@"""(.*)"" contains a Java Script ""(.*)"" ScriptToRun ""(.*)"" and result as ""(.*)""")]
        public void GivenContainsAJavaScriptScriptToRunAndResultAs(string parentName, string activityName, string scriptToRun, string Result)
        {
            var dsfJavascriptActivity = new DsfJavascriptActivity
            {
                DisplayName = activityName,
                Result = Result,
                Script = scriptToRun
            };
            _commonSteps.AddActivityToActivityList(parentName, activityName, dsfJavascriptActivity);
        }

        [Given(@"""(.*)"" contains a Python ""(.*)"" ScriptToRun ""(.*)"" and result as ""(.*)""")]
        public void GivenContainsAPythonScriptToRunAndResultAs(string parentName, string activityName, string scriptToRun, string Result)
        {
            var dsfPythonActivity = new DsfPythonActivity
            {
                DisplayName = activityName
                ,
                Result = Result
                ,
                Script = scriptToRun
                ,
                EscapeScript = true
            };
            _commonSteps.AddActivityToActivityList(parentName, activityName, dsfPythonActivity);
        }

        [Given(@"""(.*)"" contains a Ruby ""(.*)"" ScriptToRun ""(.*)"" and result as ""(.*)""")]
        public void GivenContainsARubyScriptToRunAndResultAs(string parentName, string activityName, string scriptToRun, string Result)
        {
            var rubyActivity = new DsfRubyActivity { DisplayName = activityName, Result = Result, Script = scriptToRun };
            _commonSteps.AddActivityToActivityList(parentName, activityName, rubyActivity);
        }

        [Given(@"""(.*)"" contains SharepointDownloadFile ""(.*)"" as")]
        public void GivenContainsSharepointDownloadFileAs(string parentName, string activityName, Table table)
        {
            var server = table.Rows[0]["Server"];
            var result = table.Rows[0]["Result"];
            var serverPath = table.Rows[0]["ServerPathFrom"];
            var serverPathUniqueNameGuid = ScenarioContext.Current.Get<string>("serverPathToUniqueNameGuid");
            serverPath = CommonSteps.AddGuidToPath(serverPath, serverPathUniqueNameGuid);
            var localPath = table.Rows[0]["LocalPathTo"];
            var downLoadActivity = new SharepointFileDownLoadActivity
            {
                DisplayName = activityName
                ,
                SharepointServerResourceId = ConfigurationManager.AppSettings[server].ToGuid()
                ,
                LocalInputPath = localPath
                ,
                ServerInputPath = serverPath
                ,
                Result = result

            };
            _commonSteps.AddVariableToVariableList(result);
            _commonSteps.AddActivityToActivityList(parentName, activityName, downLoadActivity);
        }

        [Given(@"""(.*)"" contains SharepointMoveFile ""(.*)"" as")]
        public void GivenContainsSharepointMoveFileAs(string parentName, string activityName, Table table)
        {
            var environmentModel = ServerRepository.Instance.Source;
            environmentModel.Connect();

            var sources = environmentModel.ResourceRepository.FindSourcesByType<SharepointSource>(environmentModel, enSourceType.SharepointServerSource) ?? new List<SharepointSource>();
            var result = table.Rows[0]["Result"];
            var name = table.Rows[0]["Server"];
            var serverPathFrom = table.Rows[0]["ServerPathFrom"];
            var serverPathUniqueNameGuid = ScenarioContext.Current.Get<string>("serverPathToUniqueNameGuid");
            serverPathFrom = CommonSteps.AddGuidToPath(serverPathFrom, serverPathUniqueNameGuid);
            var serverPathTo = table.Rows[0]["ServerPathTo"];
            var sharepointServerResourceId = ConfigurationManager.AppSettings[name].ToGuid();
            var sharepointSource = sources.Single(source => source.ResourceID == sharepointServerResourceId);

            var readFolderItemActivity = new SharepointMoveFileActivity
            {
                DisplayName = activityName,
                SharepointServerResourceId = sharepointSource.ResourceID,
                Result = result,
                ServerInputPathFrom = serverPathFrom,
                ServerInputPathTo = serverPathTo,
                Overwrite = true


            };
            _commonSteps.AddVariableToVariableList(result);
            _commonSteps.AddActivityToActivityList(parentName, activityName, readFolderItemActivity);
        }

        [Given(@"""(.*)"" contains SharepointCopyFile ""(.*)"" as")]
        public void GivenContainsSharepointCopyFileAs(string parentName, string activityName, Table table)
        {
            var environmentModel = ServerRepository.Instance.Source;
            environmentModel.Connect();

            var sources = environmentModel.ResourceRepository.FindSourcesByType<SharepointSource>(environmentModel, enSourceType.SharepointServerSource) ?? new List<SharepointSource>();
            var result = table.Rows[0]["Result"];
            var name = table.Rows[0]["Server"];
            var overwrite = table.Rows[0]["Overwrite"];
            var serverPathFrom = table.Rows[0]["ServerPathFrom"];
            var serverPathUniqueNameGuid = ScenarioContext.Current.Get<string>("serverPathToUniqueNameGuid");
            serverPathFrom = CommonSteps.AddGuidToPath(serverPathFrom, serverPathUniqueNameGuid);
            var serverPathTo = table.Rows[0]["ServerPathTo"];
            var sharepointServerResourceId = ConfigurationManager.AppSettings[name].ToGuid();
            var sharepointSource = sources.Single(source => source.ResourceID == sharepointServerResourceId);

            var readFolderItemActivity = new SharepointCopyFileActivity
            {
                DisplayName = activityName,
                SharepointServerResourceId = sharepointSource.ResourceID,
                Result = result,
                ServerInputPathFrom = serverPathFrom,
                ServerInputPathTo = serverPathTo,
                Overwrite = bool.Parse(overwrite)

            };
            _commonSteps.AddVariableToVariableList(result);
            _commonSteps.AddActivityToActivityList(parentName, activityName, readFolderItemActivity);
        }


        [Given(@"""(.*)"" contains SharepointReadListItem ""(.*)"" as")]
        public void GivenContainsSharepointReadListItemAs(string parentName, string activityName, Table table)
        {
            //Load Source based on the name
            var environmentModel = ServerRepository.Instance.Source;
            environmentModel.Connect();

            var sharepointList = table.Rows[0]["List"];
            var readListActivity = new SharepointReadListActivity
            {
                DisplayName = activityName
                ,
                SharepointServerResourceId = ConfigurationManager.AppSettings[table.Rows[0]["Server"]].ToGuid()
                ,
                SharepointList = sharepointList

            };
            var sources = environmentModel.ResourceRepository.FindSourcesByType<SharepointSource>(environmentModel, enSourceType.SharepointServerSource) ?? new List<SharepointSource>();
            var sharepointSource = sources.Single(source => source.ResourceID == readListActivity.SharepointServerResourceId);
            var sharepointListTos = environmentModel.ResourceRepository.GetSharepointLists(sharepointSource);
            var sharepointListTo = sharepointListTos.Single(to => to.FullName == sharepointList);
            var sharepointFieldsToKeep = new List<ISharepointFieldTo>()
            {
                new SharepointFieldTo {InternalName = "Title"},
                new SharepointFieldTo {InternalName = "Name"},
                new SharepointFieldTo {InternalName = "IntField"},
                new SharepointFieldTo {InternalName = "CurrencyField"},
                new SharepointFieldTo {InternalName = "DateField"},
                new SharepointFieldTo {InternalName = "DateTimeField"},
                new SharepointFieldTo {InternalName = "BoolField"},
                new SharepointFieldTo {InternalName = "MultilineTextField"},
                new SharepointFieldTo {InternalName = "RequiredField"},
                new SharepointFieldTo {InternalName = "Loc"}
            };
            var asyncWorker = new SynchronousAsyncWorker();
            asyncWorker.Start(() => GetListFields(environmentModel, sharepointSource, sharepointListTo), columnList =>
            {
                if (columnList != null)
                {
                    var fieldMappings = columnList
                    .Where(to => sharepointFieldsToKeep.Any(fieldTo => fieldTo.InternalName == to.InternalName))
                    .Select(mapping =>
                    {

                        var recordsetDisplayValue = DataListUtil.CreateRecordsetDisplayValue(sharepointListTo.FullName.Replace(" ", "").Replace(".", ""), GetValidVariableName(mapping), "*");
                        var sharepointReadListTo = new SharepointReadListTo(DataListUtil.AddBracketsToValueIfNotExist(recordsetDisplayValue), mapping.Name, mapping.InternalName, mapping.Type.ToString()) { IsRequired = mapping.IsRequired };
                        return sharepointReadListTo;
                    }).ToList();
                    if (readListActivity.ReadListItems == null || readListActivity.ReadListItems.Count == 0)
                    {
                        readListActivity.ReadListItems = fieldMappings;
                    }
                    else
                    {
                        foreach (var sharepointReadListTo in fieldMappings)
                        {
                            var listTo = sharepointReadListTo;
                            var readListTo = readListActivity.ReadListItems.FirstOrDefault(to => to.FieldName == listTo.FieldName);
                            if (readListTo == null)
                            {
                                readListActivity.ReadListItems.Add(sharepointReadListTo);
                            }
                        }
                    }

                }
            });

            _commonSteps.AddVariableToVariableList("[[AccTesting().Title]]");
            _commonSteps.AddVariableToVariableList("[[AccTesting().Name]]");
            _commonSteps.AddVariableToVariableList("[[AccTesting().IntField]]");
            _commonSteps.AddVariableToVariableList("[[AccTesting().CurrencyField]]");
            _commonSteps.AddVariableToVariableList("[[AccTesting().DateField]]");
            _commonSteps.AddVariableToVariableList("[[AccTesting().DateTimeField]]");
            _commonSteps.AddVariableToVariableList("[[AccTesting().BoolField]]");
            _commonSteps.AddVariableToVariableList("[[AccTesting().MultilineTextField]]");
            _commonSteps.AddVariableToVariableList("[[AccTesting().Loc]] ");
            _commonSteps.AddVariableToVariableList("[[AccTesting().RequiredField]]");
            _commonSteps.AddActivityToActivityList(parentName, activityName, readListActivity);
        }

        [Given(@"""(.*)"" contains SharepointReadFolder ""(.*)"" as")]
        public void GivenContainsSharepointReadFolderAs(string parentName, string activityName, Table table)
        {
            var server = table.Rows[0]["Server"];
            var result = table.Rows[0]["Result"];
            var readFolderItemActivity = new SharepointReadFolderItemActivity
            {
                DisplayName = activityName
                ,
                SharepointServerResourceId = ConfigurationManager.AppSettings[server].ToGuid()
                ,
                Result = result
                ,
                IsFoldersSelected = true

            };
            _commonSteps.AddVariableToVariableList(result);
            _commonSteps.AddActivityToActivityList(parentName, activityName, readFolderItemActivity);
        }

        [Given(@"""(.*)"" contains SharepointDeleteSingle ""(.*)"" as")]
        public void GivenContainsSharepointDeleteListItemAs(string parentName, string activityName, Table table)
        {
            var environmentModel = ServerRepository.Instance.Source;
            environmentModel.Connect();

            var sources = environmentModel.ResourceRepository.FindSourcesByType<SharepointSource>(environmentModel, enSourceType.SharepointServerSource) ?? new List<SharepointSource>();
            var result = table.Rows[0]["Result"];
            var name = table.Rows[0]["Server"];
            var serverPath = table.Rows[0]["ServerPath"];
            if (ScenarioContext.Current.ContainsKey("serverPathToUniqueNameGuid"))
            {
                var serverPathUniqueNameGuid = ScenarioContext.Current.Get<string>("serverPathToUniqueNameGuid");
                serverPath = CommonSteps.AddGuidToPath(serverPath, serverPathUniqueNameGuid);
            }
            var sharepointServerResourceId = ConfigurationManager.AppSettings[name].ToGuid();
            var sharepointSource = sources.Single(source => source.ResourceID == sharepointServerResourceId);

            var deleteListItemActivity = new SharepointDeleteFileActivity
            {
                SharepointServerResourceId = sharepointSource.ResourceID
                ,
                DisplayName = activityName
                ,
                ServerInputPath = serverPath,
                Result = result
            };
            _commonSteps.AddVariableToVariableList(result);
            _commonSteps.AddActivityToActivityList(parentName, activityName, deleteListItemActivity);

        }
        [Given(@"""(.*)"" contains UpdateListItems ""(.*)"" as")]
        public void GivenContainsUpdateListItemsAs(string parentName, string activityName, Table table)
        {
            //Load Source based on the name
            var environmentModel = ServerRepository.Instance.Source;
            environmentModel.Connect();

            var sharepointList = table.Rows[0]["List"];
            sharepointList += "_" + ScenarioContext.Current.Get<int>("recordsetNameRandomizer").ToString();
            var result = table.Rows[0]["Result"];
            var createListItemActivity = new SharepointUpdateListItemActivity
            {
                DisplayName = activityName
                ,
                SharepointServerResourceId = ConfigurationManager.AppSettings[table.Rows[0]["Server"]].ToGuid()
                ,
                Result = result
                ,
                SharepointList = sharepointList,

            };
            var sources = environmentModel.ResourceRepository.FindSourcesByType<SharepointSource>(environmentModel, enSourceType.SharepointServerSource) ?? new List<SharepointSource>();
            var sharepointSource = sources.Single(source => source.ResourceID == createListItemActivity.SharepointServerResourceId);
            var sharepointListTos = environmentModel.ResourceRepository.GetSharepointLists(sharepointSource);
            var sharepointListTo = sharepointListTos.Single(to => to.FullName == sharepointList);
            var sharepointFieldsToKeep = new List<ISharepointFieldTo>()
            {
                new SharepointFieldTo() {InternalName = "Title"},
                new SharepointFieldTo() {InternalName = "Name"},
                new SharepointFieldTo() {InternalName = "IntField"},
                new SharepointFieldTo() {InternalName = "CurrencyField"},
                new SharepointFieldTo() {InternalName = "DateField"},
                new SharepointFieldTo() {InternalName = "DateTimeField"},
                new SharepointFieldTo() {InternalName = "BoolField"},
                new SharepointFieldTo() {InternalName = "MultilineTextField"},
                new SharepointFieldTo() {InternalName = "RequiredField"},
                new SharepointFieldTo() {InternalName = "Loc",},
            };
            var asyncWorker = new SynchronousAsyncWorker();
            asyncWorker.Start(() => GetListFields(environmentModel, sharepointSource, sharepointListTo), columnList =>
            {
                if (columnList != null)
                {
                    var fieldMappings = columnList
                    .Where(to => sharepointFieldsToKeep.Any(fieldTo => fieldTo.InternalName == to.InternalName))
                    .Select(mapping =>
                    {
                        var recordsetDisplayValue = DataListUtil.CreateRecordsetDisplayValue(sharepointListTo.FullName.Replace(" ", "").Replace(".", ""), GetValidVariableName(mapping), "*");
                        var sharepointReadListTo = new SharepointReadListTo(DataListUtil.AddBracketsToValueIfNotExist(recordsetDisplayValue), mapping.Name, mapping.InternalName, mapping.Type.ToString()) { IsRequired = mapping.IsRequired };
                        return sharepointReadListTo;
                    }).ToList();
                    if (createListItemActivity.ReadListItems == null || createListItemActivity.ReadListItems.Count == 0)
                    {
                        createListItemActivity.ReadListItems = fieldMappings;
                    }
                    else
                    {
                        foreach (var sharepointReadListTo in fieldMappings)
                        {
                            var listTo = sharepointReadListTo;
                            var readListTo = createListItemActivity.ReadListItems.FirstOrDefault(to => to.FieldName == listTo.FieldName);
                            if (readListTo == null)
                            {
                                createListItemActivity.ReadListItems.Add(sharepointReadListTo);
                            }
                        }
                    }

                }
            });

            _commonSteps.AddActivityToActivityList(parentName, activityName, createListItemActivity);

        }
        [Given(@"""(.*)"" contains CreateListItems ""(.*)"" as")]
        public void GivenContainsCreateListItemsAs(string parentName, string activityName, Table table)
        {
            //Load Source based on the name
            var environmentModel = ServerRepository.Instance.Source;
            environmentModel.Connect();

            var sharepointList = table.Rows[0]["List"];
            var result = table.Rows[0]["Result"];
            var createListItemActivity = new SharepointCreateListItemActivity
            {
                DisplayName = activityName
            ,
                SharepointServerResourceId = ConfigurationManager.AppSettings[table.Rows[0]["Server"]].ToGuid(),
                Result = result,
                SharepointList = sharepointList,

            };
            var sources = environmentModel.ResourceRepository.FindSourcesByType<SharepointSource>(environmentModel, enSourceType.SharepointServerSource) ?? new List<SharepointSource>();
            var sharepointSource = sources.Single(source => source.ResourceID == createListItemActivity.SharepointServerResourceId);
            var sharepointListTos = environmentModel.ResourceRepository.GetSharepointLists(sharepointSource);
            var sharepointListTo = sharepointListTos.Single(to => to.FullName == sharepointList);
            var sharepointFieldsToKeep = new List<ISharepointFieldTo>()
            {
                new SharepointFieldTo() {InternalName = "Title"},
                new SharepointFieldTo() {InternalName = "Name"},
                new SharepointFieldTo() {InternalName = "IntField"},
                new SharepointFieldTo() {InternalName = "CurrencyField"},
                new SharepointFieldTo() {InternalName = "DateField"},
                new SharepointFieldTo() {InternalName = "DateTimeField"},
                new SharepointFieldTo() {InternalName = "BoolField"},
                new SharepointFieldTo() {InternalName = "MultilineTextField"},
                new SharepointFieldTo() {InternalName = "RequiredField"},
                new SharepointFieldTo() {InternalName = "Loc",},
            };
            var asyncWorker = new SynchronousAsyncWorker();
            asyncWorker.Start(() => GetListFields(environmentModel, sharepointSource, sharepointListTo), columnList =>
            {
                if (columnList != null)
                {
                    var fieldMappings = columnList
                    .Where(to => sharepointFieldsToKeep.Any(fieldTo => fieldTo.InternalName == to.InternalName))
                    .Select(mapping =>
                    {

                        var recordsetDisplayValue = DataListUtil.CreateRecordsetDisplayValue(sharepointListTo.FullName.Replace(" ", "").Replace(".", ""), GetValidVariableName(mapping), "*");
                        var sharepointReadListTo = new SharepointReadListTo(DataListUtil.AddBracketsToValueIfNotExist(recordsetDisplayValue), mapping.Name, mapping.InternalName, mapping.Type.ToString()) { IsRequired = mapping.IsRequired };
                        return sharepointReadListTo;
                    }).ToList();
                    if (createListItemActivity.ReadListItems == null || createListItemActivity.ReadListItems.Count == 0)
                    {
                        createListItemActivity.ReadListItems = fieldMappings;
                    }
                    else
                    {
                        foreach (var sharepointReadListTo in fieldMappings)
                        {
                            var listTo = sharepointReadListTo;
                            var readListTo = createListItemActivity.ReadListItems.FirstOrDefault(to => to.FieldName == listTo.FieldName);
                            if (readListTo == null)
                            {
                                createListItemActivity.ReadListItems.Add(sharepointReadListTo);
                            }
                        }
                    }

                }
            });

            _commonSteps.AddVariableToVariableList("[[AcceptanceTesting_Create().Title]]");
            _commonSteps.AddVariableToVariableList("[[AcceptanceTesting_Create().Name]]");
            _commonSteps.AddVariableToVariableList("[[AcceptanceTesting_Create().IntField]]");
            _commonSteps.AddVariableToVariableList("[[AcceptanceTesting_Create().CurrencyField]]");
            _commonSteps.AddVariableToVariableList("[[AcceptanceTesting_Create().DateField]]");
            _commonSteps.AddVariableToVariableList("[[AcceptanceTesting_Create().DateTimeField]]");
            _commonSteps.AddVariableToVariableList("[[AcceptanceTesting_Create().BoolField]]");
            _commonSteps.AddVariableToVariableList("[[AcceptanceTesting_Create().MultilineTextField]]");
            _commonSteps.AddVariableToVariableList("[[AcceptanceTesting_Create().Loc]] ");
            _commonSteps.AddVariableToVariableList("[[AcceptanceTesting_Create().RequiredField]]");
            _commonSteps.AddActivityToActivityList(parentName, activityName, createListItemActivity);

        }

        static string GetValidVariableName(ISharepointFieldTo mapping)
        {
            var fixedName = mapping.Name.Replace(" ", "").Replace(".", "").Replace(":", "").Replace(",", "");
            fixedName = XmlConvert.EncodeName(fixedName);
            var startIndexOfEncoding = fixedName.IndexOf("_", StringComparison.OrdinalIgnoreCase);
            var endIndexOfEncoding = fixedName.LastIndexOf("_", StringComparison.OrdinalIgnoreCase);
            if (startIndexOfEncoding > 0 && endIndexOfEncoding > 0)
            {
                fixedName = fixedName.Remove(startIndexOfEncoding - 1, endIndexOfEncoding - startIndexOfEncoding);
            }
            if (fixedName[0] == 'f' || fixedName[0] == '_' || char.IsNumber(fixedName[0]))
            {
                fixedName = fixedName.Remove(0, 1);
            }
            return fixedName;
        }

        List<ISharepointFieldTo> GetListFields(IServer server, ISharepointSource source, SharepointListTo list)
        {
            var columns = server.ResourceRepository.GetSharepointListFields(source, list, true);
            return columns ?? new List<ISharepointFieldTo>();
        }
        [Given(@"""(.*)"" contains SharepointDeleteFile ""(.*)"" as")]
        public void GivenContainsSharepointDeleteFileAs(string parentName, string activityName, Table table)
        {
            var environmentModel = ServerRepository.Instance.Source;
            environmentModel.Connect();

            var sources = environmentModel.ResourceRepository.FindSourcesByType<SharepointSource>(environmentModel, enSourceType.SharepointServerSource) ?? new List<SharepointSource>();

            var result = table.Rows[0]["Result"];
            var name = table.Rows[0]["Server"];
            var sharepointList = table.Rows[0]["SharepointList"];
            var sharepointServerResourceId = ConfigurationManager.AppSettings[name].ToGuid();
            var sharepointSource = sources.Single(source => source.ResourceID == sharepointServerResourceId);
            var deleteFileActivity = new SharepointDeleteListItemActivity()
            {
                DisplayName = activityName,
                SharepointServerResourceId = sharepointSource.ResourceID,
                SharepointList = sharepointList,
                DeleteCount = result,
                FilterCriteria = new List<SharepointSearchTo>()
                {
                    new SharepointSearchTo("Title","=",Guid.NewGuid().ToString(),1)
                    {
                        InternalName = "Title"
                    }
                }
                ,
                RequireAllCriteriaToMatch = true

            };

            _commonSteps.AddVariableToVariableList(result);
            _commonSteps.AddActivityToActivityList(parentName, activityName, deleteFileActivity);
        }

        [Given(@"""(.*)"" contains SharepointUploadFile ""(.*)"" as")]
        public void GivenContainsSharepointUploadFileAs(string parentName, string activityName, Table table)
        {
            var environmentModel = ServerRepository.Instance.Source;
            environmentModel.Connect();

            var sources = environmentModel.ResourceRepository.FindSourcesByType<SharepointSource>(environmentModel, enSourceType.SharepointServerSource) ?? new List<SharepointSource>();
            var result = table.Rows[0]["Result"];
            var name = table.Rows[0]["Server"];
            var localPathFrom = table.Rows[0]["LocalPathFrom"];
            var serverPathTo = table.Rows[0]["ServerPathTo"];
            var serverPathToUniqueNameGuid = CommonSteps.GetGuid();
            serverPathTo = CommonSteps.AddGuidToPath(serverPathTo, serverPathToUniqueNameGuid);
            ScenarioContext.Current.Add("serverPathToUniqueNameGuid", serverPathToUniqueNameGuid);
            var sharepointServerResourceId = ConfigurationManager.AppSettings[name].ToGuid();
            var sharepointSource = sources.Single(source => source.ResourceID == sharepointServerResourceId);
            var fileUploadActivity = new SharepointFileUploadActivity
            {
                DisplayName = activityName
                ,
                SharepointServerResourceId = sharepointSource.ResourceID
                ,
                Result = result
                ,
                LocalInputPath = localPathFrom
                ,
                ServerInputPath = serverPathTo,

            };
            _commonSteps.AddVariableToVariableList(result);
            _commonSteps.AddActivityToActivityList(parentName, activityName, fileUploadActivity);
        }


        [Given(@"""(.*)"" contains SharepointDelete ""(.*)"" as")]
        public void GivenContainsSharepointDeleteAs(string parentName, string activityName, Table table)
        {
            var sourceName = table.Rows[0]["Source"];
            var list = table.Rows[0]["List"];

            var deleteListItemActivity = new SharepointDeleteListItemActivity
            {
                SharepointServerResourceId = ConfigurationManager.AppSettings[sourceName].ToGuid()
                ,
                DisplayName = activityName
                ,
                SharepointList = list
            };
            _commonSteps.AddActivityToActivityList(parentName, activityName, deleteListItemActivity);
        }

        [Given(@"""(.*)"" contains a Web Delete ""(.*)"" result as ""(.*)""")]
        public void GivenContainsAWebDeleteResultAs(string parentName, string activityName, string result)
        {
            var environmentModel = ServerRepository.Instance.Source;
            environmentModel.Connect();
            var environmentConnection = environmentModel.Connection;
            var controllerFactory = new CommunicationControllerFactory();
            var _proxyLayer = new StudioServerProxy(controllerFactory, environmentConnection);
            var mock = new Mock<IShellViewModel>();
            var manageWebServiceModel = new ManageWebServiceModel(
                                                                                    new StudioResourceUpdateManager(controllerFactory, environmentConnection)
                                                                                    , _proxyLayer.QueryManagerProxy
                                                                                    , mock.Object
                                                                                    , environmentModel);
            var pluginSources = _proxyLayer.QueryManagerProxy.FetchWebServiceSources().ToList();
            var a = pluginSources.Single(source => source.Id == "3032b7fd-f12a-4ab8-be7d-2f4705c31317".ToGuid());
            var webServiceDefinition = new WebServiceDefinition()
            {
                Name = "Delete",
                Path = "",
                Source = a,
                Inputs = new List<IServiceInput>(),
                OutputMappings = new List<IServiceOutputMapping>(),
                QueryString = "",
                Id = a.Id,
                Headers = new List<INameValue>(),
                Method = WebRequestMethod.Delete
            };
            var testResult = manageWebServiceModel.TestService(webServiceDefinition);

            var serializer = new Dev2JsonSerializer();

            var dsfWebDeleteActivity = new DsfWebDeleteActivity
            {
                DisplayName = activityName
                ,
                SourceId = a.Id
            };

            RecordsetList recordsetList;
            using (var responseService = serializer.Deserialize<WebService>(testResult))
            {
                recordsetList = responseService.Recordsets;
                if (recordsetList.Any(recordset => recordset.HasErrors))
                {
                    var errorMessage = string.Join(Environment.NewLine, recordsetList.Select(recordset => recordset.ErrorMessage));
                    throw new Exception(errorMessage);
                }
                dsfWebDeleteActivity.OutputDescription = responseService.GetOutputDescription();
            }

            var outputMapping = recordsetList.SelectMany(recordset => recordset.Fields, (recordset, recordsetField) =>
            {
                var serviceOutputMapping = new ServiceOutputMapping(recordsetField.Name, recordsetField.Alias, recordset.Name) { Path = recordsetField.Path };
                return serviceOutputMapping;
            }).Cast<IServiceOutputMapping>().ToList();

            dsfWebDeleteActivity.Outputs = outputMapping;
            dsfWebDeleteActivity.Headers = new List<INameValue>();
            dsfWebDeleteActivity.QueryString = string.Empty;
            _commonSteps.AddActivityToActivityList(parentName, activityName, dsfWebDeleteActivity);
        }

        [Given(@"""(.*)"" contains a Web Post ""(.*)"" result as ""(.*)""")]
        public void GivenContainsAWebPostResultAs(string parentName, string activityName, string result)
        {
            var environmentModel = ServerRepository.Instance.Source;
            environmentModel.Connect();
            var environmentConnection = environmentModel.Connection;
            var controllerFactory = new CommunicationControllerFactory();
            var _proxyLayer = new StudioServerProxy(controllerFactory, environmentConnection);
            var mock = new Mock<IShellViewModel>();
            var manageWebServiceModel = new ManageWebServiceModel(
                  new StudioResourceUpdateManager(controllerFactory, environmentConnection)
                  , _proxyLayer.QueryManagerProxy
                  , mock.Object
                  , environmentModel);

            var webSources = _proxyLayer.QueryManagerProxy.FetchWebServiceSources().ToList();
            var a = webSources.Single(source => source.Id == "ab4d5ab5-ad44-421d-8125-adfcc3aa655b".ToGuid());
            var webServiceDefinition = new WebServiceDefinition()
            {
                Name = "Post",
                Path = "",
                Source = a,
                Inputs = new List<IServiceInput>(),
                OutputMappings = new List<IServiceOutputMapping>(),
                QueryString = "",
                Id = a.Id,
                Headers = new List<INameValue>(),
                Method = WebRequestMethod.Post
            };
            var testResult = manageWebServiceModel.TestService(webServiceDefinition);

            var serializer = new Dev2JsonSerializer();

            var dsfWebPostActivity = new DsfWebPostActivity
            {
                DisplayName = activityName
                ,
                PostData = string.Empty
                ,
                SourceId = a.Id
            };

            RecordsetList recordsetList;
            using (var responseService = serializer.Deserialize<WebService>(testResult))
            {
                recordsetList = responseService.Recordsets;
                if (recordsetList.Any(recordset => recordset.HasErrors))
                {
                    var errorMessage = string.Join(Environment.NewLine, recordsetList.Select(recordset => recordset.ErrorMessage));
                    throw new Exception(errorMessage);
                }
                dsfWebPostActivity.OutputDescription = responseService.GetOutputDescription();
            }

            var outputMapping = recordsetList.SelectMany(recordset => recordset.Fields, (recordset, recordsetField) =>
            {
                var serviceOutputMapping = new ServiceOutputMapping(recordsetField.Name, recordsetField.Alias, recordset.Name) { Path = recordsetField.Path };
                return serviceOutputMapping;
            }).Cast<IServiceOutputMapping>().ToList();

            dsfWebPostActivity.Outputs = outputMapping;
            dsfWebPostActivity.Headers = new List<INameValue>();
            dsfWebPostActivity.QueryString = string.Empty;
            _commonSteps.AddActivityToActivityList(parentName, activityName, dsfWebPostActivity);
        }

        [Given(@"""(.*)"" contains a Web Get ""(.*)"" result as ""(.*)""")]
        public void GivenContainsAWebGetResultAs(string parentName, string activityName, string result)
        {
            var environmentModel = ServerRepository.Instance.Source;
            environmentModel.Connect();
            var environmentConnection = environmentModel.Connection;
            var controllerFactory = new CommunicationControllerFactory();
            var _proxyLayer = new StudioServerProxy(controllerFactory, environmentConnection);
            var mock = new Mock<IShellViewModel>();
            var manageWebServiceModel = new ManageWebServiceModel(
                                                                                    new StudioResourceUpdateManager(controllerFactory, environmentConnection)
                                                                                    , _proxyLayer.QueryManagerProxy
                                                                                    , mock.Object
                                                                                    , environmentModel);
            var pluginSources = _proxyLayer.QueryManagerProxy.FetchWebServiceSources().ToList();
            var a = pluginSources.Single(source => source.Id == "e541d860-cd10-4aec-b2fe-79eca3c62c25".ToGuid());
            var webServiceDefinition = new WebServiceDefinition()
            {
                Name = "Get",
                Path = "",
                Source = a,
                Inputs = new List<IServiceInput>(),
                OutputMappings = new List<IServiceOutputMapping>(),
                QueryString = "",
                Id = a.Id,
                Headers = new List<INameValue>(),
                Method = WebRequestMethod.Get
            };
            var testResult = manageWebServiceModel.TestService(webServiceDefinition);

            var serializer = new Dev2JsonSerializer();

            var dsfWebGetActivity = new WebGetActivity
            {
                DisplayName = activityName
                ,
                SourceId = a.Id
            };

            RecordsetList recordsetList;
            using (var responseService = serializer.Deserialize<WebService>(testResult))
            {
                recordsetList = responseService.Recordsets;
                if (recordsetList.Any(recordset => recordset.HasErrors))
                {
                    var errorMessage = string.Join(Environment.NewLine, recordsetList.Select(recordset => recordset.ErrorMessage));
                    throw new Exception(errorMessage);
                }
                dsfWebGetActivity.OutputDescription = responseService.GetOutputDescription();
            }

            var outputMapping = recordsetList.SelectMany(recordset => recordset.Fields, (recordset, recordsetField) =>
            {
                var serviceOutputMapping = new ServiceOutputMapping(recordsetField.Name, recordsetField.Alias, recordset.Name) { Path = recordsetField.Path };
                return serviceOutputMapping;
            }).Cast<IServiceOutputMapping>().ToList();

            dsfWebGetActivity.Outputs = outputMapping;
            dsfWebGetActivity.Headers = new List<INameValue>
            {
                new NameValue("Content-Type","text/html")
            };
            dsfWebGetActivity.QueryString = string.Empty;
            _commonSteps.AddVariableToVariableList(result);
            _commonSteps.AddActivityToActivityList(parentName, activityName, dsfWebGetActivity);
        }

        [Given(@"""(.*)"" contains a Web Put ""(.*)"" result as ""(.*)""")]
        public void GivenContainsAWebPutResultAs(string parentName, string activityName, string result)
        {
            var environmentModel = ServerRepository.Instance.Source;
            environmentModel.Connect();
            var environmentConnection = environmentModel.Connection;
            var controllerFactory = new CommunicationControllerFactory();
            var _proxyLayer = new StudioServerProxy(controllerFactory, environmentConnection);
            var mock = new Mock<IShellViewModel>();
            var manageWebServiceModel = new ManageWebServiceModel(
                                                                                    new StudioResourceUpdateManager(controllerFactory, environmentConnection)
                                                                                    , _proxyLayer.QueryManagerProxy
                                                                                    , mock.Object
                                                                                    , environmentModel);
            var pluginSources = _proxyLayer.QueryManagerProxy.FetchWebServiceSources().ToList();
            var a = pluginSources.Single(source => source.Id == "0fb49fec-e454-4357-a06f-08f329558b18".ToGuid());
            var webServiceDefinition = new WebServiceDefinition()
            {
                Name = "Put",
                Path = "",
                Source = a,
                Inputs = new List<IServiceInput>(),
                OutputMappings = new List<IServiceOutputMapping>(),
                QueryString = "",
                Id = a.Id,
                Headers = new List<INameValue>(),
                Method = WebRequestMethod.Put
            };
            var testResult = manageWebServiceModel.TestService(webServiceDefinition);

            var serializer = new Dev2JsonSerializer();

            var webPutActivity = new DsfWebPutActivity
            {
                DisplayName = activityName
                ,
                SourceId = a.Id
                ,
                PutData = "{\"Id\":\"\"}"
            };

            RecordsetList recordsetList;
            using (var responseService = serializer.Deserialize<WebService>(testResult))
            {
                recordsetList = responseService.Recordsets;
                if (recordsetList.Any(recordset => recordset.HasErrors))
                {
                    var errorMessage = string.Join(Environment.NewLine, recordsetList.Select(recordset => recordset.ErrorMessage));
                    throw new Exception(errorMessage);
                }
                webPutActivity.OutputDescription = responseService.GetOutputDescription();
            }

            var outputMapping = recordsetList.SelectMany(recordset => recordset.Fields, (recordset, recordsetField) =>
            {
                var serviceOutputMapping = new ServiceOutputMapping(recordsetField.Name, recordsetField.Alias, recordset.Name) { Path = recordsetField.Path };
                return serviceOutputMapping;
            }).Cast<IServiceOutputMapping>().ToList();

            webPutActivity.Outputs = outputMapping;
            webPutActivity.Headers = new List<INameValue>()
            {
                new NameValue("Content-Type","text/html")
            };
            webPutActivity.QueryString = string.Empty;
            _commonSteps.AddVariableToVariableList(result);
            _commonSteps.AddActivityToActivityList(parentName, activityName, webPutActivity);
        }



        [Given(@"""(.*)"" contains an Delete ""(.*)"" as")]

        public void GivenContainsAnDeleteAs(string parentName, string activityName, Table table)

        {
            var del = new DsfPathDelete { InputPath = table.Rows[0][0], Result = table.Rows[0][1], DisplayName = activityName };
            _commonSteps.AddVariableToVariableList(table.Rows[0][1]);
            _commonSteps.AddActivityToActivityList(parentName, activityName, del);
        }

        [Given(@"""(.*)"" contains a Foreach ""(.*)"" as ""(.*)"" executions ""(.*)""")]
        public void GivenContainsAForeachAsExecutions(string parentName, string activityName, string numberOfExecutions, string executionCount)
        {
            Enum.TryParse(numberOfExecutions, true, out enForEachType forEachType);
            var forEach = new DsfForEachActivity { DisplayName = activityName, ForEachType = forEachType };
            switch (forEachType)
            {
                case enForEachType.NumOfExecution:
                    forEach.NumOfExections = executionCount;
                    break;
                case enForEachType.InRecordset:
                    forEach.Recordset = executionCount;
                    break;
                case enForEachType.InRange:
                    break;
                case enForEachType.InCSV:
                    break;
                default:
                    break;
            }
            _commonSteps.AddActivityToActivityList(parentName, activityName, forEach);
            _scenarioContext.Add(activityName, forEach);
        }

        [Given(@"""(.*)"" contains a SelectAndApply ""(.*)"" DataSource ""(.*)"" Alias ""(.*)""")]
        public void GivenContainsASelectAndApplyDataSourceAlias(string parentName, string activityName, string datasource, string alias)
        {
            var selectAndApplyActivity = new DsfSelectAndApplyActivity { DisplayName = activityName, DataSource = datasource, Alias = alias };
            _commonSteps.AddActivityToActivityList(parentName, activityName, selectAndApplyActivity);
            _scenarioContext.Add(activityName, selectAndApplyActivity);
        }


        [Given(@"""(.*)"" contains workflow ""(.*)"" with mapping as")]

        public void GivenContainsWorkflowWithMappingAs(string forEachName, string nestedWF, Table mappings)

        {
            var forEachAct = (DsfForEachActivity)_scenarioContext[forEachName];
            var environmentModel = LocalEnvModel;
            if (!environmentModel.IsConnected)
            {
                environmentModel.Connect();
            }

            var resource = environmentModel.ResourceRepository.Find(a => a.Category == @"Acceptance Testing Resources\" + nestedWF).FirstOrDefault();
            if (resource == null)
            {
                environmentModel.LoadResources();
                resource = environmentModel.ResourceRepository.Find(a => a.Category == @"Acceptance Testing Resources\" + nestedWF).FirstOrDefault();
            }
            if (resource == null)
            {

                throw new ArgumentNullException("resource");

            }
            var dataMappingViewModel = GetDataMappingViewModel(resource, mappings);

            var inputMapping = dataMappingViewModel.GetInputString(dataMappingViewModel.Inputs);
            var outputMapping = dataMappingViewModel.GetOutputString(dataMappingViewModel.Outputs);
            var activity = new DsfActivity
            {

                ServiceName = resource.Category,
                ResourceID = resource.ID,
                EnvironmentID = environmentModel.EnvironmentID,
                UniqueID = resource.ID.ToString(),
                InputMapping = inputMapping,
                OutputMapping = outputMapping


            };

            var activityFunction = new ActivityFunc<string, bool> { Handler = activity, DisplayName = nestedWF };
            forEachAct.DataFunc = activityFunction;
        }


        [Given(@"""(.*)"" contains Find Record Index ""(.*)"" into result as ""(.*)""")]
        public void GivenContainsFindRecordIndexIntoResultAs(string parentName, string activityName, string result, Table table)
        {
            var act = new DsfFindRecordsMultipleCriteriaActivity { DisplayName = activityName, Result = result };
            foreach (var rule in table.Rows)
            {
                act.ResultsCollection.Add(new FindRecordsTO(rule[4], rule[3], 0));
                act.FieldsToSearch = string.IsNullOrEmpty(act.FieldsToSearch) ? rule[1] : "," + rule[1];
                act.RequireAllFieldsToMatch = rule[5].ToUpper().Trim() == "YES";
                act.RequireAllTrue = rule[6].ToUpper().Trim() == "YES";
            }
            _commonSteps.AddActivityToActivityList(parentName, activityName, act);
        }


        [Given(@"""(.*)"" contains Length ""(.*)"" on ""(.*)"" into ""(.*)""")]
        public void GivenContainsLengthOnInto(string parentName, string activityName, string recordSet, string result)
        {
            var len = new DsfRecordsetNullhandlerLengthActivity { DisplayName = activityName, RecordsLength = result, RecordsetName = recordSet };
            _commonSteps.AddActivityToActivityList(parentName, activityName, len);
        }

        public void SaveToWorkspace(IContextualResourceModel resourceModel)
        {
            BuildDataList();

            var activityList = _commonSteps.GetActivityList();

            var flowSteps = new List<FlowStep>();

            TestStartNode = new FlowStep();
            flowSteps.Add(TestStartNode);
            if (activityList != null)
            {
                foreach (var activity in activityList)
                {
                    if (TestStartNode.Action == null)
                    {
                        TestStartNode.Action = activity.Value;
                    }
                    else
                    {
                        var flowStep = new FlowStep { Action = activity.Value };
                        flowSteps.Last().Next = flowStep;
                        flowSteps.Add(flowStep);
                    }
                }
            }
            TryGetValue("environment", out IServer server);
            TryGetValue("resourceRepo", out IResourceRepository repository);

            var currentDl = CurrentDl;
            resourceModel.DataList = currentDl.Replace("root", "DataList");
            var helper = new WorkflowHelper();
            var xamlDefinition = helper.GetXamlDefinition(FlowchartActivityBuilder);
            resourceModel.WorkflowXaml = xamlDefinition;
            repository.Save(resourceModel);
        }

        [When(@"""(.*)"" is executed")]
        public void WhenIsExecuted(string workflowName)
        {
            var resourceModel = SaveAWorkflow(workflowName);
            ExecuteWorkflow(resourceModel);
        }

        [When(@"'(.*)' unsaved WF ""(.*)"" is executed")]
        public void WhenUnsavedWFIsExecuted(int numberToExecute, string unsavedName)
        {
            var unsavedWFs = Get<List<IContextualResourceModel>>("unsavedWFS");
            if (unsavedWFs != null && unsavedWFs.Count > 0)
            {
                var wfs = unsavedWFs.Where(model => model.ResourceName == unsavedName).ToList();
                if (wfs.Count >= numberToExecute)
                {
                    var resourceModel = wfs[numberToExecute - 1];
                    EnsureEnvironmentConnected(resourceModel.Environment, EnvironmentConnectionTimeout);
                    SaveToWorkspace(resourceModel);
                    var debugTo = new DebugTO { XmlData = "<DataList></DataList>", SessionID = Guid.NewGuid(), IsDebugMode = true };
                    var clientContext = resourceModel.Environment.Connection;
                    if (clientContext != null)
                    {
                        var dataList = XElement.Parse(debugTo.XmlData);
                        dataList.Add(new XElement("BDSDebugMode", debugTo.IsDebugMode));
                        dataList.Add(new XElement("DebugSessionID", debugTo.SessionID));
                        dataList.Add(new XElement("EnvironmentID", resourceModel.Environment.EnvironmentID));
                        WebServer.Send(resourceModel, dataList.ToString(), new SynchronousAsyncWorker());
                        _resetEvt.WaitOne(1000);
                    }
                }
            }
        }

        public void ExecuteWorkflow(IContextualResourceModel resourceModel)
        {
            if (resourceModel?.Environment == null)
            {
                return;
            }

            var debugTo = new DebugTO { XmlData = "<DataList></DataList>", SessionID = Guid.NewGuid(), IsDebugMode = true };
            EnsureEnvironmentConnected(resourceModel.Environment, EnvironmentConnectionTimeout);
            var clientContext = resourceModel.Environment.Connection;
            if (clientContext != null)
            {
                var dataList = XElement.Parse(debugTo.XmlData);
                dataList.Add(new XElement("BDSDebugMode", debugTo.IsDebugMode));
                dataList.Add(new XElement("DebugSessionID", debugTo.SessionID));
                dataList.Add(new XElement("EnvironmentID", resourceModel.Environment.EnvironmentID));
                WebServer.Send(resourceModel, dataList.ToString(), new SynchronousAsyncWorker());
                _resetEvt.WaitOne(3000);
            }
        }

        [When(@"workflow ""(.*)"" is saved ""(.*)"" time")]
        [Then(@"workflow ""(.*)"" is saved ""(.*)"" time")]
        [Given(@"workflow ""(.*)"" is saved ""(.*)"" time")]
        public void WhenWorkflowIsSavedTime(string workflowName, int count)
        {
            TryGetValue("SavedId", out Guid id);
            if (id == Guid.Empty)
            {
                id = Guid.NewGuid();
                _scenarioContext.Add("SavedId", id);
            }
            Save(workflowName, count, id);
        }

        [When(@"I reload Server resources")]
        public void WhenIReloadServerResources()
        {
            TryGetValue("environment", out IServer server);
            server.ResourceRepository.Load(true);
        }

        void Save(string workflowName, int count, Guid id)
        {
            BuildDataList();

            var activityList = _commonSteps.GetActivityList();

            var flowSteps = new List<FlowStep>();

            TestStartNode = new FlowStep();
            flowSteps.Add(TestStartNode);

            if (activityList != null)
            {
                foreach (var activity in activityList)
                {
                    if (TestStartNode.Action == null)
                    {
                        TestStartNode.Action = activity.Value;
                    }
                    else
                    {
                        var flowStep = new FlowStep { Action = activity.Value };
                        flowSteps.Last().Next = flowStep;
                        flowSteps.Add(flowStep);
                    }
                }
            }
            TryGetValue(workflowName, out IContextualResourceModel resourceModel);
            TryGetValue("environment", out IServer server);
            TryGetValue("resourceRepo", out IResourceRepository repository);

            var currentDl = CurrentDl;
            resourceModel.DataList = currentDl.Replace("root", "DataList");
            var helper = new WorkflowHelper();
            var xamlDefinition = helper.GetXamlDefinition(FlowchartActivityBuilder);
            resourceModel.WorkflowXaml = xamlDefinition;
            resourceModel.ID = id;

            for (var i = 0; i < count; i++)
            {
                repository.SaveToServer(resourceModel);
            }

        }

        [Given(@"I save workflow ""(.*)""")]
        [When(@"I save workflow ""(.*)""")]
        [Then(@"I save workflow ""(.*)""")]
        void Save(string workflowName)
        {
            BuildDataList();

            var activityList = _commonSteps.GetActivityList();

            var flowSteps = new List<FlowStep>();

            TestStartNode = new FlowStep();
            flowSteps.Add(TestStartNode);

            foreach (var activity in activityList)
            {
                if (TestStartNode.Action == null)
                {
                    TestStartNode.Action = activity.Value;
                }
                else
                {
                    var flowStep = new FlowStep { Action = activity.Value };
                    flowSteps.Last().Next = flowStep;
                    flowSteps.Add(flowStep);
                }
            }
            TryGetValue(workflowName, out IContextualResourceModel resourceModel);
            TryGetValue("environment", out IServer server);
            TryGetValue("resourceRepo", out IResourceRepository repository);

            var currentDl = CurrentDl;
            resourceModel.DataList = currentDl.Replace("root", "DataList");
            var helper = new WorkflowHelper();
            var xamlDefinition = helper.GetXamlDefinition(FlowchartActivityBuilder);
            resourceModel.WorkflowXaml = xamlDefinition;
            repository.SaveToServer(resourceModel);

        }

        [Then(@"workflow ""(.*)"" is deleted as cleanup")]
        public void ThenWorkflowIsDeletedAsCleanup(string workflowName)
        {
            TryGetValue(workflowName, out IContextualResourceModel resourceModel);
            TryGetValue("environment", out IServer server);
            TryGetValue("resourceRepo", out IResourceRepository repository);

            repository.DeleteResourceFromWorkspace(resourceModel);
            repository.DeleteResource(resourceModel);
        }

        [Then(@"the folder ""(.*)"" is deleted from the server as cleanup")]
        public void ThenTheFolderIsDeletedFromTheServerAsCleanup(string shapointLocalFolder)
        {
            var folderToDelete = Path.Combine(EnvironmentVariables.ResourcePath, shapointLocalFolder);
            Directory.Delete(folderToDelete, true);
        }

        [Then(@"workflow ""(.*)"" has ""(.*)"" Versions in explorer")]
        public void ThenWorkflowHasVersionsInExplorer(string workflowName, int numberOfVersions)
        {
            TryGetValue("SavedId", out Guid id);
            TryGetValue(workflowName, out IContextualResourceModel resourceModel);
            TryGetValue("environment", out IServer server);
            TryGetValue("resourceRepo", out IResourceRepository repository);
            var rep = new Studio.Core.VersionManagerProxy(new CommunicationControllerFactory(), server.Connection);
            var versions = rep.GetVersions(id);
            _scenarioContext["Versions"] = versions;
            Assert.AreEqual(numberOfVersions, versions.Count);
        }

        [Then(@"explorer as")]
        public void ThenExplorerAs(Table table)
        {
            var versions = _scenarioContext["Versions"] as IList<IExplorerItem>;
            if (versions == null || versions.Count == table.RowCount)
            {
                Assert.Fail("InvalidVersions");
            }
            else
            {
                for (var i = 0; i < versions.Count; i++)
                {
                    var v1 = table.Rows[i + 1][0].Split(' ');
                    Assert.IsTrue(versions[i].DisplayName.Contains(v1[0]));

                }
            }

        }

        [Given(@"""(.*)"" contains a Sequence ""(.*)"" as")]
        public void GivenContainsASequenceAs(string parentName, string activityName)
        {
            var dsfSequence = new DsfSequenceActivity { DisplayName = activityName };
            _commonSteps.AddActivityToActivityList(parentName, activityName, dsfSequence);
        }

        [Given(@"the workflow contains an Assign ""(.*)"" as")]
        [Then(@"the workflow contains an Assign ""(.*)"" as")]
        public void ThenContainsAnAssignAs(string assignName, Table table)
        {
            TryGetValue("parentWorkflowName", out string parentWorkflowName);
            ThenContainsAnAssignAs(parentWorkflowName, assignName, table);
        }

        [Then(@"I update ""(.*)"" by adding ""(.*)"" as")]
        public void ThenIUpdateByAddingAs(string parentName, string activityName, Table table)
        {
            var assignActivity = new DsfMultiAssignActivity { DisplayName = activityName };

            foreach (var tableRow in table.Rows)
            {
                var value = tableRow["value"];
                var variable = tableRow["variable"];

                value = value.Replace('"', ' ').Trim();

                if (value.StartsWith("="))
                {
                    value = value.Replace("=", "");
                    value = $"!~calculation~!{value}!~~calculation~!";
                }
                _scenarioContext.TryGetValue("fieldCollection", out List<ActivityDTO> fieldCollection);

                _commonSteps.AddVariableToVariableList(variable);

                assignActivity.FieldsCollection.Add(new ActivityDTO(variable, value, 1, true));
            }
            _commonSteps.AddActivityToActivityList(parentName, activityName, assignActivity);
        }


        [Then(@"I update ""(.*)"" inputs in ""(.*)"" as")]
        public void ThenIUpdateInputsInAs(string assignName, string parentName, Table table)
        {
            var assignActivity = _commonSteps.GetActivityList()
                .FirstOrDefault(p => p.Key == assignName)
                .Value as DsfMultiAssignActivity;

            foreach (var tableRow in table.Rows)
            {
                var value = tableRow["value"];
                var variable = tableRow["variable"];
                value = value.Replace('"', ' ').Trim();

                _commonSteps.AddVariableToVariableList(variable);
                assignActivity.FieldsCollection.Add(new ActivityDTO(variable, value, 1, true));
            }
        }

        [Given(@"""(.*)"" contains an Assign ""(.*)"" as")]
        [Then(@"""(.*)"" contains an Assign ""(.*)"" as")]
        public void ThenContainsAnAssignAs(string parentName, string assignName, Table table)
        {
            var assignActivity = new DsfMultiAssignActivity { DisplayName = assignName };

            foreach (var tableRow in table.Rows)
            {
                var value = tableRow["value"];
                var variable = tableRow["variable"];

                value = value.Replace('"', ' ').Trim();

                if (value.StartsWith("="))
                {
                    value = value.Replace("=", "");
                    value = $"!~calculation~!{value}!~~calculation~!";
                }
                if (value.Equals("TestingDotnetDllCascading.Food.ToJson"))
                {
                    var serializer = new Dev2JsonSerializer();
                    var serialize = serializer.Serialize(new Food());
                    value = serialize;
                }

                _scenarioContext.TryGetValue("fieldCollection", out List<ActivityDTO> fieldCollection);

                _commonSteps.AddVariableToVariableList(variable);

                assignActivity.FieldsCollection.Add(new ActivityDTO(variable, value, 1, true));
            }
            _commonSteps.AddActivityToActivityList(parentName, assignName, assignActivity);
        }

        [Given(@"""(.*)"" contains a recordset name randomizing Assign ""(.*)"" as")]
        [Then(@"""(.*)"" contains a recordset name randomizing Assign ""(.*)"" as")]
        public void ThenContainsARecordsetNameRandomizingAssignAs(string parentName, string assignName, Table table)
        {
            var assignActivity = new DsfMultiAssignActivity { DisplayName = assignName };
            var recordsetNameRandomizer = new Random().Next(60) + 1;
            ScenarioContext.Current.Add("recordsetNameRandomizer", recordsetNameRandomizer);

            foreach (var tableRow in table.Rows)
            {
                var value = tableRow["value"];
                var variable = tableRow["variable"];

                var endOfRecordsetName = variable.IndexOf('(');
                variable = variable.Substring(0, endOfRecordsetName) + "_" + recordsetNameRandomizer.ToString() + variable.Substring(endOfRecordsetName, variable.Length - endOfRecordsetName);

                _scenarioContext.TryGetValue("fieldCollection", out List<ActivityDTO> fieldCollection);

                _commonSteps.AddVariableToVariableList(variable);

                assignActivity.FieldsCollection.Add(new ActivityDTO(variable, value, 1, true));
            }
            _commonSteps.AddActivityToActivityList(parentName, assignName, assignActivity);
        }

        [Given(@"""(.*)"" contains an Assign for Json ""(.*)"" as")]
        [Then(@"""(.*)"" contains an Assign for Json ""(.*)"" as")]
        public void GivenContainsAnAssignForJsonAs(string parentName, string assignName, Table table)
        {
            var assignActivity = new DsfMultiAssignActivity { DisplayName = assignName };
            foreach (var tableRow in table.Rows)
            {
                var value = "{\"Name\":\"Bob\"}";
                var variable = tableRow["variable"];

                if (value.StartsWith("="))
                {
                    value = value.Replace("=", "");
                    value = $"!~calculation~!{value}!~~calculation~!";
                }

                _scenarioContext.TryGetValue("fieldCollection", out List<ActivityDTO> fieldCollection);

                _commonSteps.AddVariableToVariableList(variable);

                assignActivity.FieldsCollection.Add(new ActivityDTO(variable, value, 1, true));
            }
            _commonSteps.AddActivityToActivityList(parentName, assignName, assignActivity);
        }

        [Given(@"""(.*)"" contains a DsfRabbitMQPublish ""(.*)"" into ""(.*)""")]
        [Then(@"""(.*)"" contains a DsfRabbitMQPublish ""(.*)"" into ""(.*)""")]
        [When(@"""(.*)"" contains a DsfRabbitMQPublish ""(.*)"" into ""(.*)""")]
        public void GivenContainsADsfRabbitMQPublishInto(string parentName, string rabbitMqname, string result)
        {
            var jsonMsg = new Human().SerializeToJsonStringBuilder().ToString();
            var dsfPublishRabbitMqActivity = new DsfPublishRabbitMQActivity
            {
                RabbitMQSourceResourceId = ConfigurationManager.AppSettings["testRabbitMQSource"].ToGuid()
              ,
                Result = result
              ,
                DisplayName = rabbitMqname,
                Message = jsonMsg
            };
            _commonSteps.AddActivityToActivityList(parentName, rabbitMqname, dsfPublishRabbitMqActivity);
        }

        [Given(@"""(.*)"" contains a RabbitMQPublish ""(.*)"" into ""(.*)""")]
        [Then(@"""(.*)"" contains a RabbitMQPublish ""(.*)"" into ""(.*)""")]
        [When(@"""(.*)"" contains a RabbitMQPublish ""(.*)"" into ""(.*)""")]
        public void GivenContainsARabbitMQPublishInto(string parentName, string rabbitMqname, string result)
        {
            var jsonMsg = new Human().SerializeToJsonStringBuilder().ToString();
            var dsfPublishRabbitMqActivity = new PublishRabbitMQActivity
            {
                RabbitMQSourceResourceId = ConfigurationManager.AppSettings["testRabbitMQSource"].ToGuid()
              ,
                Result = result,
                DisplayName = rabbitMqname,
                Message = jsonMsg
            };
            _commonSteps.AddActivityToActivityList(parentName, rabbitMqname, dsfPublishRabbitMqActivity);
        }

        [Given(@"""(.*)"" contains an DotNet DLL ""(.*)"" as")]
        [Then(@"""(.*)"" contains an DotNet DLL ""(.*)"" as")]
        public void GivenContainsAnDotNetDLLAs(string parentName, string dotNetServiceName, Table table)
        {
            var dsfEnhancedDotNetDllActivity = new DsfEnhancedDotNetDllActivity()
            {
                IsObject = true,
                DisplayName = dotNetServiceName
            };
            var Source = table.Rows[0]["Source"];
            var ClassName = table.Rows[0]["ClassName"];
            var ObjectName = table.Rows[0]["ObjectName"];
            var Action = table.Rows[0]["Action"];
            var ActionOutputVaribale = table.Rows[0]["ActionOutputVaribale"];
            dsfEnhancedDotNetDllActivity.ObjectName = ObjectName;
            var proxy = new StudioServerProxy(new CommunicationControllerFactory(), LocalEnvModel.Connection);
            var pluginSource = proxy.QueryManagerProxy.FetchPluginSources().Single(source => source.Name.Equals(Source, StringComparison.InvariantCultureIgnoreCase));
            var namespaceItems = proxy.QueryManagerProxy.FetchNamespacesWithJsonRetunrs(pluginSource);
            var namespaceItem = namespaceItems.Single(item => item.FullName.Equals(ClassName, StringComparison.CurrentCultureIgnoreCase));
            var pluginActions = proxy.QueryManagerProxy.PluginActionsWithReturns(pluginSource, namespaceItem);
            allPluginActions = pluginActions.ToList();
            var pluginAction = pluginActions.Single(action => action.Method.Equals(Action, StringComparison.InvariantCultureIgnoreCase));
            var pluginConstructors = proxy.QueryManagerProxy.PluginConstructors(pluginSource, namespaceItem);
            const string recNumber = "[[rec(*).number]]";
            foreach (var serviceInput in pluginAction.Inputs)
            {
                serviceInput.Value = recNumber;
            }
            dsfEnhancedDotNetDllActivity.Namespace = namespaceItem;
            dsfEnhancedDotNetDllActivity.SourceId = pluginSource.Id;
            ScenarioContext.Current.Add(dotNetServiceName, dsfEnhancedDotNetDllActivity);
            ScenarioContext.Current.Add("pluginConstructors", pluginConstructors);
            _commonSteps.AddVariableToVariableList(ObjectName);
            _commonSteps.AddVariableToVariableList(ActionOutputVaribale);
            _commonSteps.AddVariableToVariableList(recNumber);
            _commonSteps.AddActivityToActivityList(parentName, dotNetServiceName, dsfEnhancedDotNetDllActivity);
        }

        [Given(@"""(.*)"" contains a DropboxUpload ""(.*)"" Setup as")]
        public void GivenContainsADropboxUploadSetupAs(string parentName, string dotNetServiceName, Table table)
        {
            var uploadActivity = new DsfDropBoxUploadActivity()
            {
                DisplayName = dotNetServiceName,

            };

            var dropBoxSource = GetDropBoxSource();
            uploadActivity.SelectedSource = dropBoxSource;
            var localFile = table.Rows[0]["Local File"];
            var localFileUniqueNameGuid = CommonSteps.GetGuid();
            localFile = CommonSteps.AddGuidToPath(localFile, localFileUniqueNameGuid);
            ScenarioContext.Current.Add("localFileUniqueNameGuid", localFileUniqueNameGuid);
            Console.WriteLine("Generated new local file path as " + localFileUniqueNameGuid + ".");
            var overwriteOrAdd = table.Rows[0]["OverwriteOrAdd"];
            var dropboxFile = table.Rows[0]["DropboxFile"];
            var serverPathToUniqueNameGuid = CommonSteps.GetGuid();
            dropboxFile = CommonSteps.AddGuidToPath(dropboxFile, serverPathToUniqueNameGuid);
            ScenarioContext.Current.Add("serverPathToUniqueNameGuid", serverPathToUniqueNameGuid);
            Console.WriteLine("Generated new server path for dropbox server path as " + serverPathToUniqueNameGuid + ".");
            var result = table.Rows[0]["Result"];
            uploadActivity.FromPath = localFile;
            uploadActivity.OverWriteMode = overwriteOrAdd.ToLower() == "Overwrite".ToLower();
            uploadActivity.ToPath = dropboxFile;

            File.Create(localFile).Close();
            _commonSteps.AddVariableToVariableList(result);
            _commonSteps.AddActivityToActivityList(parentName, dotNetServiceName, uploadActivity);
        }

        [Given(@"""(.*)"" contains a DropboxList ""(.*)"" Setup as")]
        public void GivenContainsADropboxListSetupAs(string parentName, string dotNetServiceName, Table table)
        {
            var listActivity = new DsfDropboxFileListActivity()
            {
                DisplayName = dotNetServiceName,
            };
            var dropBoxSource = GetDropBoxSource();
            listActivity.SelectedSource = dropBoxSource;
            var result = table.Rows[0]["Result"];
            var DropboxFile = table.Rows[0]["DropboxFile"];
            listActivity.ToPath = DropboxFile;
            var read = table.Rows[0]["Read"];
            var loadSubFolders = table.Rows[0]["LoadSubFolders"];
            switch (read)
            {
                case "Files":
                    listActivity.IsFilesSelected = true;
                    break;
                case "Folders":
                    listActivity.IsFoldersSelected = true;
                    break;
                case "All":
                    listActivity.IsFilesAndFoldersSelected = true;
                    break;
                default:
                    break;
            }
            var b = bool.Parse(loadSubFolders);
            listActivity.IsRecursive = b;
            _commonSteps.AddVariableToVariableList(result);
            _commonSteps.AddActivityToActivityList(parentName, dotNetServiceName, listActivity);
        }


        static DropBoxSource GetDropBoxSource()
        {
            var guid = "dc163197-7a76-4f4c-a783-69d6d53b2f3a".ToGuid();
            var resourceList = LocalEnvModel.ResourceRepository.LoadContextualResourceModel(guid);
            var serviceDefinition = resourceList.ToServiceDefinition();
            var dropBoxSource = new DropBoxSource(serviceDefinition.ToXElement());
            return dropBoxSource;
        }

        [Given(@"""(.*)"" contains a DropboxDownLoad ""(.*)"" Setup as")]
        public void GivenContainsADropboxDownLoadSetupAs(string parentName, string dotNetServiceName, Table table)
        {
            var downloadActivity = new DsfDropBoxDownloadActivity()
            {
                DisplayName = dotNetServiceName,

            };
            var dropBoxSource = GetDropBoxSource();
            downloadActivity.SelectedSource = dropBoxSource;
            downloadActivity.FromPath = table.Rows[0]["Local File"];
            var serverPathUniqueNameGuid = ScenarioContext.Current.Get<string>("serverPathToUniqueNameGuid");
            downloadActivity.ToPath = CommonSteps.AddGuidToPath(table.Rows[0]["DropboxFile"], serverPathUniqueNameGuid);
            var overwriteOrAdd = table.Rows[0]["OverwriteOrAdd"];
            downloadActivity.OverwriteFile = overwriteOrAdd.ToLower() == "Overwrite".ToLower();
            var result = table.Rows[0]["Result"];
            _commonSteps.AddVariableToVariableList(result);
            _commonSteps.AddActivityToActivityList(parentName, dotNetServiceName, downloadActivity);
        }

        [Given(@"""(.*)"" contains a DropboxDelete ""(.*)"" Setup as")]
        public void GivenContainsADropboxDeleteSetupAs(string parentName, string dotNetServiceName, Table table)
        {
            var deleteActivity = new DsfDropBoxDeleteActivity()
            {
                DisplayName = dotNetServiceName,

            };
            var dropBoxSource = GetDropBoxSource();
            deleteActivity.SelectedSource = dropBoxSource;
            var dropboxFile = table.Rows[0]["DropboxFile"];
            var serverPathUniqueNameGuid = ScenarioContext.Current.Get<string>("serverPathToUniqueNameGuid");
            dropboxFile = CommonSteps.AddGuidToPath(dropboxFile, serverPathUniqueNameGuid);
            deleteActivity.DeletePath = dropboxFile;
            var result = table.Rows[0]["Result"];
            _commonSteps.AddVariableToVariableList(result);
            _commonSteps.AddActivityToActivityList(parentName, dotNetServiceName, deleteActivity);
        }

        [Given(@"""(.*)"" contains an COM DLL ""(.*)"" as")]
        [Then(@"""(.*)"" contains an COM DLL ""(.*)"" as")]
        public void GivenContainsAnCOMDLLAs(string parentName, string dotNetServiceName, Table table)
        {
            var dsfEnhancedDotNetDllActivity = new DsfComDllActivity()
            {
                DisplayName = dotNetServiceName
            };
            var namespaceSelected = table.Rows[0]["Namespace"];
            var Action = table.Rows[0]["Action"];

            var controllerFactory = new CommunicationControllerFactory();
            var sourceId = "ed7c3655-4922-4f24-9881-83462661832d".ToGuid();
            var environmentConnection = LocalEnvModel.Connection;
            var mock = new Mock<IShellViewModel>();
            var proxy = new StudioServerProxy(controllerFactory, environmentConnection);
            var fetchComPluginSources = proxy.QueryManagerProxy.FetchComPluginSources();
            var pluginSource = fetchComPluginSources.Single(source => source.Id == sourceId);
            var dbServiceModel = new ManageComPluginServiceModel(new StudioResourceUpdateManager(controllerFactory, environmentConnection)
                                                                                   , proxy.QueryManagerProxy
                                                                                   , mock.Object
                                                                                   , new Server(Guid.NewGuid(), environmentConnection));
            var namespaceItems = dbServiceModel.GetNameSpaces(pluginSource);
            var namespaceItem = namespaceItems.Single(item => item?.FullName?.Equals(namespaceSelected, StringComparison.CurrentCultureIgnoreCase) ?? false);
            var pluginActions = dbServiceModel.GetActions(pluginSource, namespaceItem);
            allPluginActions = pluginActions.ToList();
            var pluginAction = pluginActions.First(action => action.Method.Equals(Action, StringComparison.InvariantCultureIgnoreCase));
            var comPluginServiceDefinition = new ComPluginServiceDefinition()
            {
                Action = pluginAction,
                Source = pluginSource,
            };
            var testResult = dbServiceModel.TestService(comPluginServiceDefinition);
            var serializer = new Dev2JsonSerializer();
            var responseService = serializer.Deserialize<RecordsetListWrapper>(testResult);
            if (responseService != null)
            {
                if (responseService.RecordsetList.Any(recordset => recordset.HasErrors))
                {
                    var errorMessage = string.Join(Environment.NewLine, responseService.RecordsetList.Select(recordset => recordset.ErrorMessage));
                    throw new Exception(errorMessage);
                }
                var _recordsetList = responseService.RecordsetList;
                if (_recordsetList.Any(recordset => recordset.HasErrors))
                {
                    var errorMessage = string.Join(Environment.NewLine, _recordsetList.Select(recordset => recordset.ErrorMessage));
                    throw new Exception(errorMessage);
                }
                dsfEnhancedDotNetDllActivity.OutputDescription = responseService.Description;

                var outputMapping = _recordsetList.SelectMany(recordset => recordset.Fields, (recordset, recordsetField) =>
                {
                    var serviceOutputMapping = new ServiceOutputMapping(recordsetField.Name, recordsetField.Alias, recordset.Name) { Path = recordsetField.Path };
                    return serviceOutputMapping;
                }).Cast<IServiceOutputMapping>().ToList();

                dsfEnhancedDotNetDllActivity.Outputs = outputMapping;
            }


            dsfEnhancedDotNetDllActivity.Method = pluginAction;
            dsfEnhancedDotNetDllActivity.Namespace = namespaceItem;
            dsfEnhancedDotNetDllActivity.SourceId = pluginSource.Id;

            _commonSteps.AddActivityToActivityList(parentName, dotNetServiceName, dsfEnhancedDotNetDllActivity);
        }

        List<IPluginAction> allPluginActions { get; set; }
        [Given(@"""(.*)"" constructorinputs (.*) with inputs as")]
        public void GivenConstructorWithInputsAs(string serviceName, int p1, Table table)
        {
            var dsfEnhancedDotNetDllActivity = ScenarioContext.Current.Get<DsfEnhancedDotNetDllActivity>(serviceName);
            var pluginConstructors = ScenarioContext.Current.Get<IList<IPluginConstructor>>("pluginConstructors");
            var pluginConstructor = pluginConstructors.FirstOrDefault(constructor => constructor.Inputs.Count == p1);
            dsfEnhancedDotNetDllActivity.Constructor = pluginConstructor;
            dsfEnhancedDotNetDllActivity.ConstructorInputs = new List<IServiceInput>();
            foreach (var tableRow in table.Rows)
            {
                var inputName = tableRow["parameterName"];
                var value = tableRow["value"];
                var type = tableRow["type"];
                dsfEnhancedDotNetDllActivity.ConstructorInputs.Add(new ServiceInput(inputName, value)
                {
                    TypeName = type
                });
            }
        }

        [Given(@"""(.*)"" service Action ""(.*)"" with inputs and output ""(.*)"" as")]
        public void GivenServiceActionWithInputsAndOutputAs(string serviceName, string action, string outputVar, Table table)
        {
            var dsfEnhancedDotNetDllActivity = ScenarioContext.Current.Get<DsfEnhancedDotNetDllActivity>(serviceName);
            var pluginAction = allPluginActions.Single(action1 => action1.Method == action);
            pluginAction.OutputVariable = outputVar;
            foreach (var tableRow in table.Rows)
            {
                var inputName = tableRow["parameterName"];
                var value = tableRow["value"];
                var serviceInput = pluginAction.Inputs.Single(input => input.Name == inputName);
                serviceInput.Value = value;
            }
            dsfEnhancedDotNetDllActivity.MethodsToRun.Add(pluginAction);
        }

        [Given(@"""(.*)"" contains an Assign Object ""(.*)"" as")]
        [Then(@"""(.*)"" contains an Assign Object ""(.*)"" as")]
        public void GivenContainsAnAssignObjectAs(string parentName, string assignName, Table table)
        {

            var assignActivity = new DsfMultiAssignObjectActivity { DisplayName = assignName };

            foreach (var tableRow in table.Rows)
            {
                var value = tableRow["value"];
                var variable = tableRow["variable"];

                value = value.Replace('"', ' ').Trim();

                if (value.StartsWith("="))
                {
                    value = value.Replace("=", "");
                    value = $"!~calculation~!{value}!~~calculation~!";
                }

                _scenarioContext.TryGetValue("fieldCollection", out List<ActivityDTO> fieldCollection);

                _commonSteps.AddVariableToVariableList(variable);

                assignActivity.FieldsCollection.Add(new AssignObjectDTO(variable, value, 1, true));
            }
            _commonSteps.AddActivityToActivityList(parentName, assignName, assignActivity);
        }

        [When(@"I rollback ""(.*)"" to version ""(.*)""")]
        public void WhenIRollbackToVersion(string workflowName, string version)
        {
            TryGetValue("SavedId", out Guid id);
            TryGetValue(workflowName, out IContextualResourceModel resourceModel);
            TryGetValue("environment", out IServer server);
            TryGetValue("resourceRepo", out IResourceRepository repository);
            var rep = new VersionManagerProxy(new CommunicationControllerFactory(), server.Connection);
            rep.RollbackTo(id, version);
        }

        [When(@"""(.*)"" is executed without saving")]
        public void WhenIsExecutedWithoutSaving(string workflowName)
        {
            TryGetValue(workflowName, out IContextualResourceModel resourceModel);
            TryGetValue("environment", out IServer server);
            TryGetValue("resourceRepo", out IResourceRepository repository);

            var debugStates = Get<List<IDebugState>>("debugStates").ToList();
            debugStates.Clear();

            ExecuteWorkflow(resourceModel);
        }

        [Then(@"I set logging to ""(.*)""")]
        public void ThenISetLoggingTo(string logLevel)
        {
            var allowedLogLevels = new[] { "DEBUG", "NONE" };
            // TODO: refactor null empty checking into extension method
            if (logLevel == null ||
                !allowedLogLevels.Contains(logLevel.ToUpper()))
            {
                return;
            }

            var loggingSettingsTo = new LoggingSettingsTo { FileLoggerLogLevel = logLevel, EventLogLoggerLogLevel = logLevel, FileLoggerLogSize = 200 };
            var controller = new CommunicationControllerFactory().CreateController("LoggingSettingsWriteService");
            var serializer = new Dev2JsonSerializer();
            controller.AddPayloadArgument("LoggingSettings", serializer.SerializeToBuilder(loggingSettingsTo).ToString());
            TryGetValue("environment", out IServer server);
            controller.ExecuteCommand<StringBuilder>(server.Connection, Guid.Empty);
        }

        [When(@"""(.*)"" is executed ""(.*)""")]
        public void WhenIsExecuted(string workflowName, string executionLabel)
        {
            var st = new Stopwatch();
            st.Start();
            WhenIsExecuted(workflowName);
            _scenarioContext.Add(executionLabel, st.ElapsedMilliseconds);
        }

        [Then(@"the delta between ""(.*)"" and ""(.*)"" is less than ""(.*)"" milliseconds")]
        public void ThenTheDeltaBetweenAndIsLessThanMilliseconds(string executionLabelFirst, string executionLabelSecond, int maxDeltaMilliseconds)
        {
            var e1 = Convert.ToInt32(_scenarioContext[executionLabelFirst]);
            var e2 = Convert.ToInt32(_scenarioContext[executionLabelSecond]);
            var d = maxDeltaMilliseconds;
            d.Should().BeGreaterThan(Math.Abs(e1 - e2), $"async logging should not add more than {d} milliseconds to the execution");
        }

        [Given(@"""(.*)"" contains an Unique ""(.*)"" as")]
        public void GivenContainsAnUniqueAs(string parentName, string activityName, Table table)
        {
            var activity = new DsfUniqueActivity { DisplayName = activityName };
            foreach (var tableRow in table.Rows)
            {
                var inFields = tableRow["In Field(s)"];
                var returnFields = tableRow["Return Fields"];
                var result = tableRow["Result"];


                _commonSteps.AddVariableToVariableList(result);

                activity.Result = result;
                activity.ResultFields = returnFields;
                activity.InFields = inFields;
            }
            _commonSteps.AddActivityToActivityList(parentName, activityName, activity);
        }

        [Given(@"""(.*)"" contains WebRequest ""(.*)"" as")]
        public void GivenContainsWebRequestAs(string parentName, string toolName, Table table)
        {
            var resultVariable = table.Rows[0]["Result"];
            var url = table.Rows[0]["Url"];

            _commonSteps.AddVariableToVariableList(resultVariable);
            var dsfWebGetRequestActivity = new DsfWebGetRequestActivity
            {
                DisplayName = toolName
                ,
                Url = url
                ,
                Result = resultVariable
            };

            _commonSteps.AddActivityToActivityList(parentName, toolName, dsfWebGetRequestActivity);
        }

        [Given(@"""(.*)"" contains Advanced Recordset ""(.*)"" with Query ""(.*)""")]
        [When(@"""(.*)"" contains Advanced Recordset ""(.*)"" with Query ""(.*)""")]
        [Then(@"""(.*)"" contains Advanced Recordset ""(.*)"" with Query ""(.*)""")]
        public void GivenContainsAdvancedRecordsetWithQuery(string workflow, string toolname, string query, Table table)
        {
            var activity = new AdvancedRecordsetActivity
            {
                DisplayName = toolname,
                SqlQuery = query,
                Outputs = new List<IServiceOutputMapping>(),
                RecordsetName = "TableCopy",
            };
            foreach (var tableRow in table.Rows)
            {
                var output = tableRow["MappedTo"];
                var toVariable = tableRow["MappedFrom"];
                var recSetName = DataListUtil.ExtractRecordsetNameFromValue(toVariable);
                activity.Outputs.Add(new ServiceOutputMapping(output, toVariable, recSetName));
                _commonSteps.AddVariableToVariableList(toVariable);
            }
            _commonSteps.AddActivityToActivityList(workflow, toolname, activity);
        }


        [Given(@"""(.*)"" contains Calculate ""(.*)"" with formula ""(.*)"" into ""(.*)""")]
        public void GivenCalculateWithFormulaInto(string parentName, string activityName, string formula, string resultVariable)
        {
            _commonSteps.AddVariableToVariableList(resultVariable);

            var calculateActivity = new DsfCalculateActivity { Expression = formula, Result = resultVariable, DisplayName = activityName };

            _commonSteps.AddActivityToActivityList(parentName, activityName, calculateActivity);

        }
        [Given(@"""(.*)"" contains XPath \\""(.*)"" with source ""(.*)""")]
        public void GivenContainsXPathWithResultAs(string parentName, string xpathName, string source)
        {
            const string a = "<XPATH-EXAMPLE>  <CUSTOMER id=\"1\" type=\"B\">Mr.  Jones</CUSTOMER><CUSTOMER id=\"2\" type=\"C\">Mr.  Johnson</CUSTOMER></XPATH-EXAMPLE> ";
            var dsfXPathActivity = new DsfXPathActivity
            {

                SourceString = a
                ,
                DisplayName = xpathName
                ,
                ResultsCollection = new List<XPathDTO>
                {
                    new XPathDTO("[[singleValue]]", source, 1, true)
                }
            };
            _commonSteps.AddActivityToActivityList(parentName, xpathName, dsfXPathActivity);
        }

        [Given(@"""(.*)"" contains Aggregate Calculate ""(.*)"" with formula ""(.*)"" into ""(.*)""")]
        public void GivenAggregateCalculateWithFormulaInto(string parentName, string activityName, string formula, string resultVariable)
        {
            _commonSteps.AddVariableToVariableList(resultVariable);

            var calculateActivity = new DsfAggregateCalculateActivity { Expression = formula, Result = resultVariable, DisplayName = activityName };

            _commonSteps.AddActivityToActivityList(parentName, activityName, calculateActivity);

        }

        [Given(@"the workflow contains Count Record ""(.*)"" on ""(.*)"" into ""(.*)""")]
        public void GivenCountOnIntoTheWorkflow(string activityName, string recordSet, string result)
        {
            TryGetValue("parentWorkflowName", out string parentWorkflowName);
            GivenCountOnInto(parentWorkflowName, activityName, recordSet, result);
        }

        [Given(@"""(.*)"" contains Count Record ""(.*)"" on ""(.*)"" into ""(.*)""")]
        public void GivenCountOnInto(string parentName, string activityName, string recordSet, string result)
        {
            _commonSteps.AddVariableToVariableList(result);

            var countRecordsetNullHandlerActivity = new DsfCountRecordsetNullHandlerActivity { CountNumber = result, RecordsetName = recordSet, DisplayName = activityName };

            _commonSteps.AddActivityToActivityList(parentName, activityName, countRecordsetNullHandlerActivity);
        }

        [Given(@"""(.*)"" contains Delete ""(.*)"" as")]
        public void GivenItContainsDeleteAs(string parentName, string activityName, Table table)
        {
            var nullHandlerActivity = new DsfDeleteRecordActivity { DisplayName = activityName };
            foreach (var tableRow in table.Rows)
            {
                var result = tableRow["result"];
                var variable = tableRow["Variable"];

                _commonSteps.AddVariableToVariableList(result);
                nullHandlerActivity.RecordsetName = variable;
                nullHandlerActivity.Result = result;
            }

            _commonSteps.AddActivityToActivityList(parentName, activityName, nullHandlerActivity);
        }

        [Given(@"""(.*)"" contains NullHandlerDelete ""(.*)"" as")]
        public void GivenContainsNullHandlerDeleteAs(string parentName, string activityName, Table table)
        {
            var nullHandlerActivity = new DsfDeleteRecordNullHandlerActivity { DisplayName = activityName };
            foreach (var tableRow in table.Rows)
            {
                var result = tableRow["result"];
                var variable = tableRow["Variable"];

                _commonSteps.AddVariableToVariableList(result);
                nullHandlerActivity.RecordsetName = variable;
                nullHandlerActivity.Result = result;
            }

            _commonSteps.AddActivityToActivityList(parentName, activityName, nullHandlerActivity);
        }


        [Given(@"""(.*)"" contains Find Record Index ""(.*)"" search ""(.*)"" and result ""(.*)"" as")]
        public void GivenItContainsFindRecordIndexSearchAndResultAs(string parentName, string activityName, string recsetToSearch, string resultVariable, Table table)
        {
            var result = resultVariable;
            var recset = recsetToSearch;
            _commonSteps.AddVariableToVariableList(result);
            var activity = new DsfFindRecordsMultipleCriteriaActivity { RequireAllFieldsToMatch = false, RequireAllTrue = false, Result = result, FieldsToSearch = recset, DisplayName = activityName };
            foreach (var tableRow in table.Rows)
            {
                var matchType = tableRow["Match Type"];
                var matchValue = tableRow["Match"];

                activity.ResultsCollection.Add(new FindRecordsTO(matchValue, matchType, 1, true));
            }

            _commonSteps.AddActivityToActivityList(parentName, activityName, activity);
        }

        [Given(@"""(.*)"" contains find unique ""(.*)"" as")]
        public void GivenItContainFindUniqueAs(string parentName, string activityName, Table table)
        {
            var activity = new DsfUniqueActivity { DisplayName = activityName };
            foreach (var tableRow in table.Rows)
            {
                var inFields = tableRow["In Fields"];
                var returnFields = tableRow["Return Fields"];
                var result = tableRow["Result"];

                activity.InFields = inFields;
                activity.ResultFields = returnFields;
                activity.Result = result;

                _commonSteps.AddVariableToVariableList(result);
            }
            _commonSteps.AddActivityToActivityList(parentName, activityName, activity);
        }

        [Given(@"""(.*)"" contains case convert ""(.*)"" as")]
        public void GivenItContainsCaseConvertAs(string parentName, string activityName, Table table)
        {
            var activity = new DsfCaseConvertActivity { DisplayName = activityName };
            foreach (var tableRow in table.Rows)
            {
                var variableToConvert = tableRow["Variable"];
                var conversionType = tableRow["Type"];

                activity.ConvertCollection.Add(new CaseConvertTO(variableToConvert, conversionType, variableToConvert, 1, true));
            }

            _commonSteps.AddActivityToActivityList(parentName, activityName, activity);
        }

        [Given(@"""(.*)"" contains Gather System Info ""(.*)"" as")]
        public void GivenItContainsGatherSystemInfoAs(string parentName, string activityName, Table table)
        {
            var activity = new DsfGatherSystemInformationActivity { DisplayName = activityName };
            foreach (var tableRow in table.Rows)
            {
                var variable = tableRow["Variable"];

                _commonSteps.AddVariableToVariableList(variable);

                var systemInfo = (enTypeOfSystemInformationToGather)Dev2EnumConverter.GetEnumFromStringDiscription(tableRow["Selected"], typeof(enTypeOfSystemInformationToGather));
                activity.SystemInformationCollection.Add(new GatherSystemInformationTO(systemInfo, variable, 1));
            }

            _commonSteps.AddActivityToActivityList(parentName, activityName, activity);
        }

        [Given(@"""(.*)"" contains Random ""(.*)"" as")]
        public void GivenItContainsRandomAs(string parentName, string activityName, Table table)
        {
            var activity = new DsfRandomActivity { DisplayName = activityName };
            foreach (var tableRow in table.Rows)
            {
                var type = (enRandomType)Enum.Parse(typeof(enRandomType), tableRow["Type"]);
                var from = tableRow["From"];
                var to = tableRow["To"];
                var result = tableRow["Result"];

                _commonSteps.AddVariableToVariableList(result);

                activity.RandomType = type;
                activity.To = to;
                activity.From = from;
                activity.Result = result;

            }

            _commonSteps.AddActivityToActivityList(parentName, activityName, activity);
        }

        [Given(@"""(.*)"" contains Format Number ""(.*)"" as")]
        public void GivenItContainsFormatNumberAs(string parentName, string activityName, Table table)
        {
            var activity = new DsfNumberFormatActivity { DisplayName = activityName };
            foreach (var tableRow in table.Rows)
            {
                var number = tableRow["Number"];
                var roundTo = tableRow["Rounding To"];
                var roundingType = tableRow["Rounding Selected"];
                var decimalPlacesToShow = tableRow["Decimal to show"];
                var result = tableRow["Result"];

                _commonSteps.AddVariableToVariableList(result);

                activity.Expression = number;
                activity.RoundingType = roundingType;
                activity.RoundingDecimalPlaces = roundTo;
                activity.DecimalPlacesToShow = decimalPlacesToShow;
                activity.Result = result;

            }

            _commonSteps.AddActivityToActivityList(parentName, activityName, activity);
        }


        [Given(@"""(.*)"" contains Date and Time ""(.*)"" as")]
        public void GivenItContainsDateAndTimeAs(string parentName, string activityName, Table table)
        {
            var activity = new DsfDateTimeActivity { DisplayName = activityName };
            foreach (var tableRow in table.Rows)
            {
                var input1 = tableRow["Input"];
                var outputFormat = tableRow["Output Format"];
                var inputFormat = tableRow["Input Format"];
                var timeModifierAmount = tableRow["Add Time"];
                var result = tableRow["Result"];

                _commonSteps.AddVariableToVariableList(result);

                activity.DateTime = input1;
                activity.InputFormat = inputFormat;
                activity.OutputFormat = outputFormat;
                activity.TimeModifierAmountDisplay = timeModifierAmount;
                activity.TimeModifierType = "Years";
                activity.Result = result;

            }

            _commonSteps.AddActivityToActivityList(parentName, activityName, activity);
        }


        [Given(@"""(.*)"" contains Date and Time Difference ""(.*)"" as")]
        public void GivenItContainsDateAndTimeDifferenceAs(string parentName, string activityName, Table table)
        {
            var activity = new DsfDateTimeDifferenceActivity { DisplayName = activityName };
            foreach (var tableRow in table.Rows)
            {
                var input1 = tableRow["Input1"];
                var input2 = tableRow["Input2"];
                var inputFormat = tableRow["Input Format"];
                var output = tableRow["Output In"];
                var result = tableRow["Result"];

                _commonSteps.AddVariableToVariableList(result);

                activity.Input1 = input1;
                activity.Input2 = input2;
                activity.InputFormat = inputFormat;
                activity.OutputType = output;
                activity.Result = result;

            }

            _commonSteps.AddActivityToActivityList(parentName, activityName, activity);
        }


        [Given(@"""(.*)"" contains Data Split ""(.*)"" as")]
        public void GivenItContainsDataSplitAs(string parentName, string activityName, Table table)
        {
            var activity = new DsfDataSplitActivity { DisplayName = activityName };
            foreach (var tableRow in table.Rows)
            {
                var valueToSplit = string.IsNullOrEmpty(tableRow["String"]) ? "" : tableRow["String"];
                var variable = tableRow["Variable"];
                var type = tableRow["Type"];
                var at = tableRow["At"];
                var include = tableRow["Include"] == "Selected";
                _commonSteps.AddVariableToVariableList(variable);
                if (!string.IsNullOrEmpty(valueToSplit))
                {
                    activity.SourceString = valueToSplit;
                }
                _commonSteps.AddVariableToVariableList(variable);
                activity.ResultsCollection.Add(new DataSplitDTO(variable, type, at, 1, include, true));
            }

            _commonSteps.AddActivityToActivityList(parentName, activityName, activity);
        }

        [Given(@"""(.*)"" contains Replace ""(.*)"" into ""(.*)"" as")]
        public void GivenItContainsReplaceIntoAs(string parentName, string activityName, string resultVariable, Table table)
        {
            var activity = new DsfReplaceActivity { Result = resultVariable, DisplayName = activityName };
            foreach (var tableRow in table.Rows)
            {
                var variable = tableRow["In Fields"];
                var find = tableRow["Find"];
                var replaceValue = tableRow["Replace With"];

                activity.FieldsToSearch = variable;
                activity.Find = find;
                activity.ReplaceWith = replaceValue;
            }
            _commonSteps.AddVariableToVariableList(resultVariable);
            _commonSteps.AddActivityToActivityList(parentName, activityName, activity);
        }


        [Given(@"""(.*)"" contains Find Index ""(.*)"" into ""(.*)"" as")]
        public void GivenItContainsFindIndexIntoAs(string parentName, string activityName, string resultVariable, Table table)
        {
            var activity = new DsfIndexActivity { Result = resultVariable, DisplayName = activityName };
            foreach (var tableRow in table.Rows)
            {
                var variable = tableRow["In Fields"];
                var index = tableRow["Index"];
                var character = tableRow["Character"];
                var direction = tableRow["Direction"];

                activity.InField = variable;
                activity.Index = index;
                activity.Characters = character;
                activity.Direction = direction;
            }
            _commonSteps.AddVariableToVariableList(resultVariable);
            _commonSteps.AddActivityToActivityList(parentName, activityName, activity);
        }

        [Given(@"""(.*)"" contains an Create ""(.*)"" as")]
        public void GivenContainsAnCreateAs(string parentName, string activityName, Table table)
        {
            var activity = new DsfPathCreate { DisplayName = activityName };
            foreach (var tableRow in table.Rows)
            {
                var variable = tableRow["File or Folder"];
                var exist = tableRow["If it exits"];
                var userName = tableRow["Username"];
                var password = tableRow["Password"];
                var result = tableRow["Result"];

                activity.Result = result;
                activity.Username = userName;
                activity.Password = password;
                activity.Overwrite = exist == "True";
                activity.OutputPath = variable;

                _commonSteps.AddVariableToVariableList(result);
            }
            _commonSteps.AddActivityToActivityList(parentName, activityName, activity);
        }

        [Given(@"""(.*)"" contains an Rename ""(.*)"" as")]
        public void GivenContainsAnWriteRenameAs(string parentName, string activityName, Table table)
        {
            var activity = new DsfPathRename { DisplayName = activityName };
            foreach (var tableRow in table.Rows)
            {
                var variable = tableRow["File or Folder"];
                var destination = tableRow["Destination"];
                var exist = tableRow["If it exits"];
                var userName = tableRow["Username"];
                var password = tableRow["Password"];
                var result = tableRow["Result"];

                activity.Result = result;
                activity.Username = userName;
                activity.Password = password;
                activity.Overwrite = exist == "True";
                activity.OutputPath = destination;
                activity.InputPath = variable;

                _commonSteps.AddVariableToVariableList(result);
            }
            _commonSteps.AddActivityToActivityList(parentName, activityName, activity);
        }

        [Given(@"""(.*)"" contains an Zip ""(.*)"" as")]
        public void GivenContainsAnZipAs(string parentName, string activityName, Table table)
        {
            var activity = new DsfZip { DisplayName = activityName };
            foreach (var tableRow in table.Rows)
            {
                var variable = tableRow["File or Folder"];
                var destination = tableRow["Destination"];
                var exist = tableRow["If it exits"];
                var userName = tableRow["Username"];
                var password = tableRow["Password"];
                var result = tableRow["Result"];

                activity.Result = result;
                activity.Username = userName;
                activity.Password = password;
                activity.Overwrite = exist == "True";
                activity.OutputPath = destination;
                activity.InputPath = variable;

                _commonSteps.AddVariableToVariableList(result);
            }
            _commonSteps.AddActivityToActivityList(parentName, activityName, activity);
        }

        [Given(@"""(.*)"" contains an UnZip ""(.*)"" as")]
        public void GivenContainsAnUnZipAs(string parentName, string activityName, Table table)
        {
            var activity = new DsfUnZip { DisplayName = activityName };
            foreach (var tableRow in table.Rows)
            {
                var variable = tableRow["File or Folder"];
                var destination = tableRow["Destination"];
                var exist = tableRow["If it exits"];
                var userName = tableRow["Username"];
                var password = tableRow["Password"];
                var result = tableRow["Result"];

                activity.Result = result;
                activity.Username = userName;
                activity.Password = password;
                activity.Overwrite = exist == "True";
                activity.OutputPath = destination;
                activity.InputPath = variable;

                _commonSteps.AddVariableToVariableList(result);
            }
            _commonSteps.AddActivityToActivityList(parentName, activityName, activity);
        }




        [Given(@"""(.*)"" contains an Move ""(.*)"" as")]
        public void GivenContainsAnMoveAs(string parentName, string activityName, Table table)
        {
            var activity = new DsfPathMove { DisplayName = activityName };
            foreach (var tableRow in table.Rows)
            {
                var source = tableRow["File or Folder"];
                var exist = tableRow["If it exits"];
                var userName = tableRow["Username"];
                var password = tableRow["Password"];
                var result = tableRow["Result"];
                var desc = tableRow["Destination"];

                activity.Result = result;
                activity.Username = userName;
                activity.Password = password;
                activity.Overwrite = exist == "True";
                activity.OutputPath = desc;
                activity.InputPath = source;



                _commonSteps.AddVariableToVariableList(result);
            }
            _commonSteps.AddActivityToActivityList(parentName, activityName, activity);
        }

        [Given(@"""(.*)"" contains an Read Folder ""(.*)"" as")]
        public void GivenContainsAnReadFolderAs(string parentName, string activityName, Table table)
        {
            var activity = new DsfFolderRead { DisplayName = activityName };
            foreach (var tableRow in table.Rows)
            {
                var source = tableRow["File or Folder"];
                var userName = tableRow["Username"];
                var password = tableRow["Password"];
                var result = tableRow["Result"];
                var isFoldersSelected = tableRow["Folders"];

                activity.Result = result;
                activity.Username = userName;
                activity.Password = password;
                activity.InputPath = source;
                activity.IsFoldersSelected = isFoldersSelected == "True";
                _commonSteps.AddVariableToVariableList(result);
            }
            _commonSteps.AddActivityToActivityList(parentName, activityName, activity);
        }

        [Given(@"I create temp file as ""(.*)""")]
        public void GivenICreateTempFileAs(string fileName)
        {
            using (var sw = File.Create(fileName))
            {
                Assert.IsNotNull(sw);
            }
        }


        [Given(@"I create temp file to read from as ""(.*)""")]
        public void GivenICreateTempFileToReadFromAs(string path)
        {
            using (var sw = File.CreateText(path))
            {
                sw.WriteLine("Hello");
            }
        }

        [Given(@"""(.*)"" contains RabbitMQPublish ""(.*)"" into ""(.*)""")]
        public void GivenContainsRabbitMQPublishInto(string parentName, string activityName, string variable)
        {
            var dsfPublishRabbitMqActivity = new PublishRabbitMQActivity
            {
                RabbitMQSourceResourceId = ConfigurationManager.AppSettings["testRabbitMQSource"].ToGuid(),
                Result = variable,
                DisplayName = activityName
            };
            _commonSteps.AddActivityToActivityList(parentName, activityName, dsfPublishRabbitMqActivity);
        }

        [Given(@"""(.*)"" contains DsfRabbitMQPublish and Queue1 ""(.*)"" into ""(.*)""")]
        public void GivenContainsDsfRabbitandQueue1MQPublishInto(string parentName, string activityName, string variable)
        {
            var dsfPublishRabbitMqActivity = new DsfPublishRabbitMQActivity
            {
                RabbitMQSourceResourceId = ConfigurationManager.AppSettings["testRabbitMQSource"].ToGuid()
                ,
                Result = variable
                ,
                DisplayName = activityName,
                QueueName = "Queue1",
                Message = "msg"
            };
            _commonSteps.AddActivityToActivityList(parentName, activityName, dsfPublishRabbitMqActivity);
        }
        [Given(@"""(.*)"" contains RabbitMQPublish and Queue1 - CorrelationID ""(.*)"" into ""(.*)""")]
        public void GivenContainsRabbitandQueue1MQPublishInto(string parentName, string activityName, string variable)
        {
            var dsfPublishRabbitMqActivity = new PublishRabbitMQActivity
            {
                RabbitMQSourceResourceId = ConfigurationManager.AppSettings["testRabbitMQSource"].ToGuid(),
                Result = variable,
                DisplayName = activityName,
                QueueName = "Queue1",
                Message = "msg"
            };
           // dsfPublishRabbitMqActivity.BasicProperties.CorrelationID = "CorrelationID";
            _commonSteps.AddActivityToActivityList(parentName, activityName, dsfPublishRabbitMqActivity);
        }

        [Given(@"""(.*)"" contains RabbitMQConsume ""(.*)"" into ""(.*)""")]
        public void GivenContainsRabbitMQConsumeInto(string parentName, string activityName, string variable)
        {
            _containerOps = new Depends(Depends.ContainerType.RabbitMQ);
            var dsfConsumeRabbitMqActivity = new DsfConsumeRabbitMQActivity
            {
                RabbitMQSourceResourceId = ConfigurationManager.AppSettings["testRabbitMQSource"].ToGuid()

                ,
                Response = variable
                ,
                DisplayName = activityName
            };

            ScenarioContext.Current.Add("RabbitMqTool", dsfConsumeRabbitMqActivity);
            _commonSteps.AddActivityToActivityList(parentName, activityName, dsfConsumeRabbitMqActivity);
        }

        [Given(@"""(.*)"" is object is set to ""(.*)""")]
        public void GivenIsObjectIsSetTo(string toolName, string isObjectString)
        {
            var isObject = bool.Parse(isObjectString);
            var dsfConsumeRabbitMqActivity = ScenarioContext.Current.Get<DsfConsumeRabbitMQActivity>("RabbitMqTool");
            dsfConsumeRabbitMqActivity.IsObject = isObject;
        }

        [Given(@"""(.*)"" objectname as ""(.*)""")]
        public void GivenObjectnameAs(string toolName, string Objectname)
        {
            var dsfConsumeRabbitMqActivity = ScenarioContext.Current.Get<DsfConsumeRabbitMQActivity>("RabbitMqTool");
            dsfConsumeRabbitMqActivity.ObjectName = Objectname;
        }

        [Given(@"Queue Name as ""(.*)""")]
        public void GivenQueueNameAs(string queueName)
        {
            var dsfConsumeRabbitMqActivity = ScenarioContext.Current.Get<DsfConsumeRabbitMQActivity>("RabbitMqTool");
            dsfConsumeRabbitMqActivity.QueueName = queueName;
        }


        [Given(@"""(.*)"" contains RabbitMQConsume ""(.*)"" with timeout (.*) seconds into ""(.*)""")]
        public void GivenContainsRabbitMQConsumeWithTimeoutSecondsInto(string parentName, string activityName, int timeout, string variable)
        {
            _containerOps = new Depends(Depends.ContainerType.RabbitMQ);
            var dsfConsumeRabbitMqActivity = new DsfConsumeRabbitMQActivity
            {
                RabbitMQSourceResourceId = ConfigurationManager.AppSettings["testRabbitMQSource"].ToGuid()
                ,
                Response = variable
                ,
                DisplayName = activityName,
                TimeOut = timeout == -1 ? "" : timeout.ToString(),
                QueueName = "Queue1",
            };
            _commonSteps.AddActivityToActivityList(parentName, activityName, dsfConsumeRabbitMqActivity);
        }


        [Given(@"""(.*)"" contains RabbitMQConsume ""(.*)"" and Queue Name '(.*)' into ""(.*)""")]
        public void GivenContainsRabbitMQConsumeAndQueueNameInto(string parentName, string activityName, string queueName, string resultVariable)
        {
            var dsfConsumeRabbitMqActivity = new DsfConsumeRabbitMQActivity
            {
                RabbitMQSourceResourceId = ConfigurationManager.AppSettings["testRabbitMQSource"].ToGuid(),
                Response = resultVariable,
                QueueName = queueName,
                DisplayName = activityName
            };
            _commonSteps.AddActivityToActivityList(parentName, activityName, dsfConsumeRabbitMqActivity);
        }

        [Given(@"""(.*)"" contains an Read File ""(.*)"" as")]
        public void GivenContainsAnReadFileAs(string parentName, string activityName, Table table)
        {

            var activity = new DsfFileRead { DisplayName = activityName };
            foreach (var tableRow in table.Rows)
            {
                var source = tableRow["File or Folder"];
                var userName = tableRow["Username"];
                var password = tableRow["Password"];
                var result = tableRow["Result"];

                activity.Result = result;
                activity.Username = userName;
                activity.Password = password;
                activity.InputPath = source;
                _commonSteps.AddVariableToVariableList(result);
            }
            _commonSteps.AddActivityToActivityList(parentName, activityName, activity);
        }

        [Given(@"""(.*)"" contains an Write File ""(.*)"" as")]
        public void GivenContainsAnWriteFileAs(string parentName, string activityName, Table table)
        {
            var activity = new DsfFileWrite { DisplayName = activityName };
            foreach (var tableRow in table.Rows)
            {
                var source = tableRow["File or Folder"];
                var userName = tableRow["Username"];
                var password = tableRow["Password"];
                var result = tableRow["Result"];
                var overWrite = tableRow["If it exits"];

                activity.Result = result;
                activity.Username = userName;
                activity.Password = password;
                activity.OutputPath = source;
                activity.Overwrite = overWrite == "True";
                _commonSteps.AddVariableToVariableList(result);
            }
            _commonSteps.AddActivityToActivityList(parentName, activityName, activity);
        }




        [Given(@"""(.*)"" contains an Delete Folder ""(.*)"" as")]
        public void GivenContainsAnDeleteFolderAs(string parentName, string activityName, Table table)
        {
            var activity = new DsfPathDelete { DisplayName = activityName };
            foreach (var tableRow in table.Rows)
            {
                var variable = tableRow["Recordset"];
                var result = tableRow["Result"];

                activity.Result = result;
                activity.InputPath = variable;

                _commonSteps.AddVariableToVariableList(result);
            }
            _commonSteps.AddActivityToActivityList(parentName, activityName, activity);
        }

        [Given(@"""(.*)"" contains Data Merge ""(.*)"" into ""(.*)"" as")]
        public void GivenItContainsDataMergeAs(string parentName, string activityName, string resultVariable, Table table)
        {
            var activity = new DsfDataMergeActivity { Result = resultVariable, DisplayName = activityName };
            foreach (var tableRow in table.Rows)
            {
                var variable = tableRow["Variable"];
                var type = tableRow["Type"];
                var at = tableRow["Using"];
                var padding = tableRow["Padding"];
                var alignment = tableRow["Alignment"];

                activity.MergeCollection.Add(new DataMergeDTO(variable, type, at, 1, padding, alignment, true));
            }
            _commonSteps.AddVariableToVariableList(resultVariable);
            _commonSteps.AddActivityToActivityList(parentName, activityName, activity);
        }

        [Given(@"""(.*)"" contains Base convert ""(.*)"" as")]
        public void GivenItContainsBaseConvertAs(string parentName, string activityName, Table table)
        {
            var activity = new DsfBaseConvertActivity { DisplayName = activityName };
            foreach (var tableRow in table.Rows)
            {
                var variableToConvert = tableRow["Variable"];
                var from = tableRow["From"];
                var to = tableRow["To"];

                activity.ConvertCollection.Add(new BaseConvertTO(variableToConvert, from, to, variableToConvert, 1, true));
            }

            _commonSteps.AddActivityToActivityList(parentName, activityName, activity);
        }

        [Given(@"""(.*)"" contains a postgre tool using ""(.*)"" with mappings for testing as")]
        public void GivenContainsAPostgreToolUsingWithMappingsForTestingAs(string parentName, string serviceName, Table table)
        {
            var inputs = GetServiceInputs(table);
            //Load Source based on the name
            var environmentModel = ServerRepository.Instance.Source;
            environmentModel.Connect();
            var environmentConnection = environmentModel.Connection;
            var controllerFactory = new CommunicationControllerFactory();
            var _proxyLayer = new StudioServerProxy(controllerFactory, environmentConnection);
            var mock = new Mock<IShellViewModel>();
            var dbServiceModel = new ManageDbServiceModel(new StudioResourceUpdateManager(controllerFactory, environmentConnection)
                                                                                    , _proxyLayer.QueryManagerProxy
                                                                                    , mock.Object
                                                                                    , environmentModel);
            var dbSources = _proxyLayer.QueryManagerProxy.FetchDbSources().ToList();
            var dbSource = dbSources.Single(source => source.Id == "f8b1a579-2394-489e-835e-21b42e304e09".ToGuid());

            var databaseService = new DatabaseService
            {
                Source = dbSource,
                Inputs = inputs,
                Action = new DbAction()
                {
                    Name = serviceName,
                    SourceId = dbSource.Id,
                    Inputs = inputs,
                    ExecuteAction = serviceName
                },
                Name = "tab_val_func"
                ,
                Id = dbSource.Id

            };
            var testResults = dbServiceModel.TestService(databaseService);

            var mappings = new List<IServiceOutputMapping>();

            if (testResults?.Columns.Count > 1)
            {
                var recordsetName = string.IsNullOrEmpty(testResults.TableName) ? serviceName.Replace(".", "_") : testResults.TableName;
                for (int i = 0; i < testResults.Columns.Count; i++)
                {
                    var column = testResults.Columns[i];
                    var dbOutputMapping = new ServiceOutputMapping(column.ToString(), column.ToString().Replace(" ", ""), recordsetName);
                    mappings.Add(dbOutputMapping);
                }
            }



            var postGreActivity = new DsfPostgreSqlActivity
            {
                ProcedureName = serviceName,
                DisplayName = serviceName,
                SourceId = dbSource.Id,
                Outputs = new List<IServiceOutputMapping>(),
                Inputs = new List<IServiceInput>()
            };

            postGreActivity.Inputs = new List<IServiceInput>()
            {
                new ServiceInput("Prefix","K"),
            };
            postGreActivity.Outputs = mappings;
            _commonSteps.AddVariableToVariableList("[[V1]]");
            _commonSteps.AddActivityToActivityList(parentName, serviceName, postGreActivity);
        }


        [Given(@"""(.*)"" contains a postgre tool using ""(.*)"" with mappings as")]
        public void GivenContainsAPostgreToolUsingWithMappingsAs(string parentName, string serviceName, Table table)
        {

            var resourceId = "62652f31-813a-4b97-b488-02e4c16150ff".ToGuid();

            var postGreActivity = new DsfPostgreSqlActivity
            {
                ProcedureName = serviceName,
                DisplayName = serviceName,
                SourceId = resourceId,
                Outputs = new List<IServiceOutputMapping>(),
                Inputs = new List<IServiceInput>()
            };
            foreach (var tableRow in table.Rows)
            {
                var output = tableRow["Output from Service"];
                var toVariable = tableRow["To Variable"];
                var recSetName = DataListUtil.ExtractRecordsetNameFromValue(toVariable);
                postGreActivity.Outputs.Add(new ServiceOutputMapping(output, toVariable, recSetName));
                _commonSteps.AddVariableToVariableList(toVariable);

                var input = tableRow["Input to Service"];
                var fromVariable = tableRow["From Variable"];
                if (!string.IsNullOrEmpty(input))
                {
                    postGreActivity.Inputs.Add(new ServiceInput(input, fromVariable));
                    _commonSteps.AddVariableToVariableList(fromVariable);
                }
            }
            _commonSteps.AddActivityToActivityList(parentName, serviceName, postGreActivity);
        }

        [Given(@"""(.*)"" contains a mysql database service ""(.*)"" with mappings for testing as")]
        public void GivenContainsAMysqlDatabaseServiceWithMappingsfortesting(string parentName, string serviceName, Table table)
        {
            //Load Source based on the name
            var environmentModel = ServerRepository.Instance.Source;
            environmentModel.Connect();
            _containerOps = new Depends(Depends.ContainerType.MySQL);
            var environmentConnection = environmentModel.Connection;
            var controllerFactory = new CommunicationControllerFactory();
            var _proxyLayer = new StudioServerProxy(controllerFactory, environmentConnection);
            var mock = new Mock<IShellViewModel>();
            var dbServiceModel = new ManageDbServiceModel(new StudioResourceUpdateManager(controllerFactory, environmentConnection)
                                                                                    , _proxyLayer.QueryManagerProxy
                                                                                    , mock.Object
                                                                                    , environmentModel);
            var dbSources = _proxyLayer.QueryManagerProxy.FetchDbSources().ToList();
            var dbSource = dbSources.Single(source => source.Id == "97d6272e-15a1-483f-afdb-a076f602604f".ToGuid());

            var databaseService = new DatabaseService
            {
                Source = dbSource,
                Inputs = new List<IServiceInput>(),
                Action = new DbAction()
                {
                    Name = serviceName,
                    SourceId = dbSource.Id,
                    Inputs = new List<IServiceInput>(),
                    ExecuteAction = serviceName
                },
                Name = serviceName,
                Id = dbSource.Id,

            };
            var testResults = dbServiceModel.TestService(databaseService);



            var mySqlDatabaseActivity = new DsfMySqlDatabaseActivity()
            {
                ProcedureName = serviceName,
                DisplayName = serviceName,
                SourceId = dbSource.Id,
                Outputs = new List<IServiceOutputMapping>(),
                Inputs = databaseService.Inputs
            };

            var mappings = new List<IServiceOutputMapping>();

            if (testResults?.Columns.Count > 1)
            {
                var recordsetName = string.IsNullOrEmpty(testResults.TableName) ? serviceName.Replace(".", "_") : testResults.TableName;
                for (int i = 0; i < testResults.Columns.Count; i++)
                {
                    var column = testResults.Columns[i];
                    var dbOutputMapping = new ServiceOutputMapping(column.ToString(), column.ToString().Replace(" ", ""), recordsetName);
                    mappings.Add(dbOutputMapping);
                }
            }
            mySqlDatabaseActivity.Outputs = mappings;
            mySqlDatabaseActivity.ProcedureName = serviceName;

            _commonSteps.AddVariableToVariableList("[[MySqlEmail(1).name]]");
            _commonSteps.AddVariableToVariableList("[[MySqlEmail(1).email]]");
            _commonSteps.AddActivityToActivityList(parentName, serviceName, mySqlDatabaseActivity);
        }

        [Given(@"""(.*)"" contains a mysql database service ""(.*)""")]
        public void GivenContainsAMysqlDatabaseService(string parentName, string serviceName)
        {
            var mySqlDatabaseActivity = new DsfMySqlDatabaseActivity
            {
                ProcedureName = serviceName,
                DisplayName = serviceName,
                Outputs = new List<IServiceOutputMapping>(),
                Inputs = new List<IServiceInput>(),
                IsEndedOnError = true
            };
            _commonSteps.AddActivityToActivityList(parentName, serviceName, mySqlDatabaseActivity);
        }

        [Given(@"""(.*)"" contains a mysql database service ""(.*)"" with mappings as")]
        public void GivenContainsAMysqlDatabaseServiceWithMappings(string parentName, string serviceName, Table table)
        {

            var mySqlResourceId = "97d6272e-15a1-483f-afdb-a076f602604f".ToGuid();
            var mySqlDatabaseActivity = new DsfMySqlDatabaseActivity
            {
                ProcedureName = serviceName,
                DisplayName = serviceName,
                SourceId = mySqlResourceId,
                Outputs = new List<IServiceOutputMapping>(),
                Inputs = new List<IServiceInput>()
            };
            foreach (var tableRow in table.Rows)
            {
                var output = tableRow["Output from Service"];
                var toVariable = tableRow["To Variable"];
                var recSetName = DataListUtil.ExtractRecordsetNameFromValue(toVariable);
                mySqlDatabaseActivity.Outputs.Add(new ServiceOutputMapping(output, toVariable, recSetName));
                _commonSteps.AddVariableToVariableList(toVariable);

                var input = tableRow["Input to Service"];
                var fromVariable = tableRow["From Variable"];
                if (!string.IsNullOrEmpty(input))
                {
                    mySqlDatabaseActivity.Inputs.Add(new ServiceInput(input, fromVariable));
                    _commonSteps.AddVariableToVariableList(fromVariable);
                }
            }
            _commonSteps.AddActivityToActivityList(parentName, serviceName, mySqlDatabaseActivity);
        }

        [Given(@"""(.*)"" contains a oracle database service ""(.*)"" with mappings as")]
        public void GivenContainsAOracleDatabaseServiceWithMappingsAs(string parentName, string serviceName, Table table)
        {
            var inputs = GetServiceInputs(table);
            //Load Source based on the name
            var environmentModel = ServerRepository.Instance.Source;
            environmentModel.Connect();
            var environmentConnection = environmentModel.Connection;
            var controllerFactory = new CommunicationControllerFactory();
            var _proxyLayer = new StudioServerProxy(controllerFactory, environmentConnection);
            var mock = new Mock<IShellViewModel>();
            var dbServiceModel = new ManageDbServiceModel(new StudioResourceUpdateManager(controllerFactory, environmentConnection)
                                                                                    , _proxyLayer.QueryManagerProxy
                                                                                    , mock.Object
                                                                                    , environmentModel);
            var dbSources = _proxyLayer.QueryManagerProxy.FetchDbSources().ToList();
            var dbSource = dbSources.Single(source => source.Id == "b1c12282-1712-419c-9929-5dfe42c90210".ToGuid());

            var databaseService = new DatabaseService
            {
                Source = dbSource,
                Inputs = inputs,
                Action = new DbAction()
                {
                    Name = serviceName,
                    SourceId = dbSource.Id,
                    Inputs = inputs,
                    ExecuteAction = serviceName
                },
                Name = serviceName,
                Id = dbSource.Id,

            };
            var testResults = dbServiceModel.TestService(databaseService);



            var mySqlDatabaseActivity = new DsfSqlServerDatabaseActivity
            {
                ProcedureName = serviceName,
                DisplayName = serviceName,
                SourceId = dbSource.Id,
                Outputs = new List<IServiceOutputMapping>(),
                Inputs = databaseService.Inputs
            };

            var mappings = new List<IServiceOutputMapping>();

            if (testResults?.Columns.Count > 1)
            {
                var recordsetName = string.IsNullOrEmpty(testResults.TableName) ? serviceName.Replace(".", "_") : testResults.TableName;
                for (int i = 0; i < testResults.Columns.Count; i++)
                {
                    var column = testResults.Columns[i];
                    var dbOutputMapping = new ServiceOutputMapping(column.ToString(), column.ToString().Replace(" ", ""), recordsetName);
                    mappings.Add(dbOutputMapping);
                }
            }
            mySqlDatabaseActivity.Outputs = mappings;
            mySqlDatabaseActivity.ProcedureName = serviceName;

            _commonSteps.AddVariableToVariableList("[[HR_GET_EMP_RS(107).MANAGER_ID]]");
            _commonSteps.AddVariableToVariableList("[[HR_GET_EMP_RS(107).DEPARTMENT_ID]]");
            _commonSteps.AddVariableToVariableList("[[HR_GET_EMP_RS(107).EMPLOYEE_ID]]");
            _commonSteps.AddVariableToVariableList("[[HR_GET_EMP_RS(107).FIRST_NAME]]");
            _commonSteps.AddVariableToVariableList("[[HR_GET_EMP_RS(107).LAST_NAME]]");
            _commonSteps.AddVariableToVariableList("[[HR_GET_EMP_RS(107).EMAIL]]");
            _commonSteps.AddVariableToVariableList("[[HR_GET_EMP_RS(107).PHONE_NUMBER]]");
            _commonSteps.AddVariableToVariableList("[[HR_GET_EMP_RS(107).HIRE_DATE]]");
            _commonSteps.AddVariableToVariableList("[[HR_GET_EMP_RS(107).JOB_ID]]");
            _commonSteps.AddVariableToVariableList("[[HR_GET_EMP_RS(107).SALARY]]");
            _commonSteps.AddVariableToVariableList("[[HR_GET_EMP_RS(107).COMMISSION_PCT]]");
            _commonSteps.AddVariableToVariableList("[[HR_GET_EMP_RS(107).MANAGER_ID]]");
            _commonSteps.AddVariableToVariableList("[[HR_GET_EMP_RS(107).DEPARTMENT_ID]]");
            _commonSteps.AddActivityToActivityList(parentName, serviceName, mySqlDatabaseActivity);
        }


        [Given(@"""(.*)"" contains a sqlserver database service ""(.*)"" with mappings for testing as")]
        public void GivenContainsASqlServerDatabaseServiceWithMappingsForTesting(string parentName, string serviceName, Table table)
        {
            _containerOps = new Depends(Depends.ContainerType.MSSQL);
            var inputs = GetServiceInputs(table);
            var resourceId = "b9184f70-64ea-4dc5-b23b-02fcd5f91082".ToGuid();
            //Load Source based on the name
            var environmentModel = ServerRepository.Instance.Source;
            environmentModel.Connect();
            var environmentConnection = environmentModel.Connection;
            var controllerFactory = new CommunicationControllerFactory();
            var _proxyLayer = new StudioServerProxy(controllerFactory, environmentConnection);
            var mock = new Mock<IShellViewModel>();
            var dbServiceModel = new ManageDbServiceModel(new StudioResourceUpdateManager(controllerFactory, environmentConnection)
                                                                                    , _proxyLayer.QueryManagerProxy
                                                                                    , mock.Object
                                                                                    , environmentModel);
            var dbSources = _proxyLayer.QueryManagerProxy.FetchDbSources().ToList();
            var dbSource = dbSources.Single(source => source.Id == resourceId);
            dbSource.ServerName += "," + _containerOps.Container.Port;
            var databaseService = new DatabaseService
            {
                Source = dbSource,
                Inputs = inputs,
                Action = new DbAction()
                {
                    Name = serviceName,
                    SourceId = dbSource.Id,
                    Inputs = inputs,
                    ExecuteAction = serviceName
                },
                Name = serviceName,
                Id = dbSource.Id
            };
            var testResults = dbServiceModel.TestService(databaseService);

            var mySqlDatabaseActivity = new DsfSqlServerDatabaseActivity
            {
                ProcedureName = serviceName,
                DisplayName = serviceName,
                SourceId = dbSource.Id,
                Outputs = new List<IServiceOutputMapping>(),
                Inputs = databaseService.Inputs
            };

            var mappings = new List<IServiceOutputMapping>();

            if (testResults?.Columns.Count > 1)
            {
                var recordsetName = string.IsNullOrEmpty(testResults.TableName) ? serviceName.Replace(".", "_") : testResults.TableName;
                for (int i = 0; i < testResults.Columns.Count; i++)
                {
                    var column = testResults.Columns[i];
                    var dbOutputMapping = new ServiceOutputMapping(column.ToString(), column.ToString().Replace(" ", ""), recordsetName);
                    mappings.Add(dbOutputMapping);
                }
            }
            mySqlDatabaseActivity.Outputs = mappings;
            mySqlDatabaseActivity.ProcedureName = serviceName;

            _commonSteps.AddVariableToVariableList("[[dbo_Pr_CitiesGetCountries(2).CountryID]]");
            _commonSteps.AddVariableToVariableList("[[dbo_Pr_CitiesGetCountries(2).Description]]");
            _commonSteps.AddActivityToActivityList(parentName, serviceName, mySqlDatabaseActivity);
        }

        static List<IServiceInput> GetServiceInputs(Table table)
        {
            return table.Rows.Select(a => new ServiceInput(a["ParameterName"], a["ParameterValue"]))
                .Cast<IServiceInput>()
                .ToList();
        }

        [Given(@"""(.*)"" contains a sqlserver database service ""(.*)"" with mappings as")]
        public void GivenContainsASqlServerDatabaseServiceWithMappings(string parentName, string serviceName, Table table)
        {
            _containerOps = new Depends(Depends.ContainerType.MSSQL);
            var resourceId = "b9184f70-64ea-4dc5-b23b-02fcd5f91082".ToGuid();

            var mySqlDatabaseActivity = new DsfSqlServerDatabaseActivity
            {
                ProcedureName = serviceName,
                DisplayName = serviceName,
                SourceId = resourceId,
                Outputs = new List<IServiceOutputMapping>(),
                Inputs = new List<IServiceInput>()
            };
            foreach (var tableRow in table.Rows)
            {
                var output = tableRow["Output from Service"];
                var toVariable = tableRow["To Variable"];
                var recSetName = DataListUtil.ExtractRecordsetNameFromValue(toVariable);
                mySqlDatabaseActivity.Outputs.Add(new ServiceOutputMapping(output, toVariable, recSetName));
                _commonSteps.AddVariableToVariableList(toVariable);

                var input = tableRow["Input to Service"];
                var fromVariable = tableRow["From Variable"];
                if (!string.IsNullOrEmpty(input))
                {
                    mySqlDatabaseActivity.Inputs.Add(new ServiceInput(input, fromVariable));
                    _commonSteps.AddVariableToVariableList(fromVariable);
                }
            }
            _commonSteps.AddActivityToActivityList(parentName, serviceName, mySqlDatabaseActivity);
        }


        [Given(@"I create a new unsaved workflow with name ""(.*)""")]
        [When(@"I create a new unsaved workflow with name ""(.*)""")]
        [Then(@"I create a new unsaved workflow with name ""(.*)""")]
        public void GivenICreateANewUnsavedWorkflowWithName(string serviceName)
        {
            var environmentModel = ServerRepository.Instance.Source;
            var tempResource = ResourceModelFactory.CreateResourceModel(environmentModel, @"WorkflowService",
                serviceName);
            tempResource.Category = @"Unassigned\" + serviceName;
            tempResource.ResourceName = serviceName;
            tempResource.DisplayName = serviceName;
            tempResource.IsNewWorkflow = true;

            environmentModel.ResourceRepository.Add(tempResource);
            _debugWriterSubscriptionService = new SubscriptionService<DebugWriterWriteMessage>(environmentModel.Connection.ServerEvents);

            _debugWriterSubscriptionService.Subscribe(msg => Append(msg.DebugState));
            if (_scenarioContext.ContainsKey("unsavedWFS"))
            {
                var unsavedWFs = Get<List<IContextualResourceModel>>("unsavedWFS");
                unsavedWFs.Add(tempResource);
            }
            else
            {
                Add("unsavedWFS", new List<IContextualResourceModel> { tempResource });
            }

            environmentModel.ResourceRepository.Add(tempResource);
            _debugWriterSubscriptionService = new SubscriptionService<DebugWriterWriteMessage>(environmentModel.Connection.ServerEvents);

            _debugWriterSubscriptionService.Subscribe(msg => Append(msg.DebugState));
            if (_scenarioContext.ContainsKey(serviceName))
            {
                _scenarioContext[serviceName] = tempResource;
                _scenarioContext["parentWorkflowName"] = serviceName;
                _scenarioContext["environment"] = environmentModel;
                _scenarioContext["resourceRepo"] = environmentModel.ResourceRepository;
                _scenarioContext["debugStates"] = new List<IDebugState>();

            }
            else
            {
                Add(serviceName, tempResource);
                Add("parentWorkflowName", serviceName);
                Add("environment", environmentModel);
                Add("resourceRepo", environmentModel.ResourceRepository);
                Add("debugStates", new List<IDebugState>());
            }
        }

        [When(@"workflow ""(.*)"" merge is opened")]
        public void WhenWorkflowMergeIsOpened(string mergeWfName)
        {
            var environmentModel = ServerRepository.Instance.Source;
            var serverRepository = new Mock<IServerRepository>();
            serverRepository.Setup(p => p.ActiveServer).Returns(new Mock<IServer>().Object);
            serverRepository.Setup(p => p.Source).Returns(new Mock<IServer>().Object);
            var evntArg = new Mock<IEventAggregator>().Object;
            var versionChecker = new Mock<IVersionChecker>().Object;
            var explorer = new Mock<IExplorerViewModel>().Object;
            var viewFact = new Mock<IViewFactory>().Object;
            var versions = _scenarioContext["Versions"] as IList<IExplorerItem>;
            var repo = _scenarioContext.Get<IResourceRepository>("resourceRepo") as ResourceRepository;
            var localResource = repo.LoadContextualResourceModel(versions.First().ResourceId);
            var remoteResource = repo.LoadContextualResourceModel(versions.Last().ResourceId);
            var vm = new Mock<IMergeWorkflowViewModel>();
            var wdvm = new Mock<IMergePreviewWorkflowDesignerViewModel>();
            vm.Setup(p => p.MergePreviewWorkflowDesignerViewModel).Returns(wdvm.Object);
        }

        [Given(@"Public ""(.*)"" Permissions to Execute ""(.*)""")]
        [When(@"Public ""(.*)"" Permissions to Execute ""(.*)""")]
        [Then(@"Public ""(.*)"" Permissions to Execute ""(.*)""")]
        public void GivenPublicPermissionsToExecuteButNotNested(string permited, string resourceName)
        {
            var hasPermissions = !string.IsNullOrEmpty(permited);
            TryGetValue("environment", out IServer environmentModel);
            EnsureEnvironmentConnected(environmentModel);
            var resourceRepository = environmentModel.ResourceRepository;
            var settings = resourceRepository.ReadSettings(environmentModel);
            environmentModel.ForceLoadResources();
            if (hasPermissions)
            {
                AddPermissionsForResource(resourceName, environmentModel, resourceRepository, settings);
            }
        }

        [Given(@"""(.*)"" contains a Decision ""(.*)"" as")]
        public void GivenContainsADecisionAs(string workflowName, string decisionName, Table decisionConfig)
        {
            var activity = new DsfDecision
            {
                DisplayName = decisionName,
                Conditions = new Dev2DecisionStack
                {
                    TheStack = new List<Dev2Decision>()
                }
            };
            foreach (var tableRow in decisionConfig.Rows)
            {
                activity.Conditions.AddModelItem(new Data.SystemTemplates.Models.Dev2Decision
                {
                    Col1 = tableRow["ItemToCheck"],
                    EvaluationFn = DecisionDisplayHelper.GetValue(tableRow["Condition"]),
                    Col2 = tableRow["ValueToCompareTo"]
                });
                Add(decisionName, (TrueArm: tableRow["TrueArmToolName"], FalseArm: tableRow["FalseArmToolName"]));
            }

            _commonSteps.AddActivityToActivityList(workflowName, decisionName, activity);
        }

        [Then(@"the ""(.*)"" number '(.*)' in WorkFlow ""(.*)"" debug inputs as")]
        public void ThenTheNumberInWorkFlowDebugInputsAs(string toolName, int toolNum, string workflowName, Table table)
        {
            TryGetValue("activityList", out Dictionary<string, Activity> activityList);
            TryGetValue("parentWorkflowName", out string parentWorkflowName);

            var debugStates = Get<List<IDebugState>>("debugStates");
            var workflowId = Guid.Empty;

            if (parentWorkflowName != workflowName)
            {
                if (toolName != null && workflowName != null)
                {
                    workflowId = debugStates.First(wf => wf.DisplayName.Equals(workflowName)).ID;
                }
                else
                {
                    throw new InvalidOperationException("SpecFlow broke.");
                }
            }

            var toolSpecificDebug =
                debugStates.Where(ds => ds.ParentID.GetValueOrDefault() == workflowId && ds.DisplayName.Equals(toolName)).Skip(toolNum - 1).ToList();
            if (!toolSpecificDebug.Any())
            {
                toolSpecificDebug =
                debugStates.Where(ds => ds.DisplayName.Equals(toolName)).ToList();
            }
            // Data Merge breaks our debug scheme, it only ever has 1 value, not the expected 2 ;)
            var isDataMergeDebug = toolSpecificDebug.Count == 1 && toolSpecificDebug.Any(t => t.Name == "Data Merge");
            IDebugState inputState;
            if (toolSpecificDebug.Count > 1 && toolSpecificDebug.Any(state => state.StateType == StateType.End))
            {
                inputState = toolSpecificDebug.FirstOrDefault(state => state.StateType == StateType.End);
            }
            else
            {
                inputState = toolSpecificDebug.Skip(toolNum - 1).FirstOrDefault();
            }

            if (inputState != null && inputState.Inputs != null)
            {
                var SelectResults = inputState.Inputs.SelectMany(s => s.ResultsList);
                if (SelectResults != null && SelectResults.ToList() != null)
                {
                    _commonSteps.ThenTheDebugInputsAs(table, SelectResults.ToList());
                    return;
                }
                Assert.Fail(inputState.Inputs.ToList() + " debug outputs found on " + workflowName + " does not include " + toolName + ".");
            }
            Assert.Fail("No debug input found for " + workflowName + ".");
        }

        [Then(@"the ""(.*)"" number '(.*)' in WorkFlow ""(.*)"" has ""(.*)"" nested children")]
        public void ThenTheNumberInWorkFlowHasNestedChildren(string toolName, int itemNumber, string workflowName, int count)
        {
            TryGetValue("activityList", out Dictionary<string, Activity> activityList);
            TryGetValue("parentWorkflowName", out string parentWorkflowName);
            var debugStates = Get<List<IDebugState>>("debugStates").ToList();

            var id =
                debugStates.Where(ds => ds.DisplayName.Equals(toolName)).Skip(itemNumber - 1).ToList().Select(a => a.ID).First();
            var children = debugStates.Count(a => a.ParentID.GetValueOrDefault() == id);
            Assert.AreEqual(count, children);
        }

        [Then(@"the ""(.*)"" in step (.*) for ""(.*)"" number '(.*)' debug inputs as")]
        public void ThenTheInStepForNumberDebugInputsAs(string toolName, int stepNumber, string forEachName, int itemNumber, Table table)
        {
            TryGetValue("activityList", out Dictionary<string, Activity> activityList);
            TryGetValue("parentWorkflowName", out string parentWorkflowName);

            var debugStates = Get<List<IDebugState>>("debugStates").ToList();
            var workflowId = debugStates.Where(wf => wf.DisplayName.Equals(forEachName)).Skip(itemNumber - 1).First().ID;

            if (parentWorkflowName == forEachName)
            {
                workflowId = Guid.Empty;
            }

            var toolSpecificDebug =
                debugStates.Where(ds => ds.ParentID.GetValueOrDefault() == workflowId && ds.DisplayName.Equals(toolName)).ToList();

            Assert.IsTrue(toolSpecificDebug.Count >= stepNumber);
            var debugToUse = DebugToUse(stepNumber, toolSpecificDebug);

            _commonSteps.ThenTheDebugInputsAs(table, debugToUse.Inputs
                                                    .SelectMany(item => item.ResultsList).ToList());
        }

        [Then(@"the ""(.*)"" in step (.*) for ""(.*)"" number '(.*)' debug outputs as")]
        public void ThenTheInStepForNumberDebugOutputsAs(string toolName, int stepNumber, string forEachName, int itemNumber, Table table)
        {
            TryGetValue("activityList", out Dictionary<string, Activity> activityList);
            TryGetValue("parentWorkflowName", out string parentWorkflowName);

            var debugStates = Get<List<IDebugState>>("debugStates").ToList();
            var workflowId = debugStates.Where(wf => wf.DisplayName.Equals(forEachName)).Skip(itemNumber - 1).First().ID;

            if (parentWorkflowName == forEachName)
            {
                workflowId = Guid.Empty;
            }

            var toolSpecificDebug =
                debugStates.Where(ds => ds.ParentID.GetValueOrDefault() == workflowId && ds.DisplayName.Equals(toolName)).ToList();
            Assert.IsTrue(toolSpecificDebug.Count >= stepNumber);
            var debugToUse = DebugToUse(stepNumber, toolSpecificDebug);


            var outputDebugItems = debugToUse.Outputs
                .SelectMany(s => s.ResultsList).ToList();
            _commonSteps.ThenTheDebugOutputAs(table, outputDebugItems);
        }



        private static void AddPermissionsForResource(string resourceName, IServer environmentModel, IResourceRepository resourceRepository, Data.Settings.Settings settings)
        {
            var resourceModel = resourceRepository.FindSingle(model => model.Category.Equals(resourceName, StringComparison.InvariantCultureIgnoreCase));
            Assert.IsNotNull(resourceModel, "Did not find: " + resourceName);
            settings.Security.WindowsGroupPermissions.RemoveAll(permission => permission.ResourceID == resourceModel.ID);
            var resourcePerm = new WindowsGroupPermission
            {
                WindowsGroup = "Public",
                ResourceID = resourceModel.ID,
                ResourceName = resourceName,
                IsServer = false,
                Permissions = SecPermissions.Execute
            };
            settings.Security.WindowsGroupPermissions.Add(resourcePerm);
            var SettingsWriteResult = resourceRepository.WriteSettings(environmentModel, settings);
            Assert.IsFalse(SettingsWriteResult.HasError, "Cannot setup for security spec.\n Error writing initial resource permissions settings to localhost server.\n" + SettingsWriteResult.Message);
        }

        static void EnsureEnvironmentConnected(IServer server)
        {
            if (!server.IsConnected)
            {
                server.Connect();
            }
        }

        [Given(@"I have server running at ""(.*)""")]
        public void GivenIHaveServerRunningAt(string server)
        {
            var environmentModel = ServerRepository.Instance.Source;
            environmentModel.Connect();
            environmentModel.ResourceRepository.Load(true);
            Add("environment", environmentModel);
            Assert.IsNotNull(environmentModel);
            Assert.AreEqual(server, environmentModel.Name);
        }
        [When(@"I resume the workflow ""(.*)"" at ""(.*)"" from version ""(.*)""")]
        public void WhenIResumeTheWorkflowAtFromVersion(string workflow, string activity, string versionNumber)
        {
            var assignActivity = _commonSteps.GetActivityList()
                .FirstOrDefault(p => p.Key == activity)
                .Value as DsfMultiAssignActivity;

            TryGetValue("environment", out IServer environmentModel);

            var resourceModel = environmentModel.ResourceRepository.FindSingle(resource => resource.ResourceName == workflow);
            Assert.IsNotNull(resourceModel);
            var env = new ExecutionEnvironment();
            var serEnv = env.ToJson();
            var msg = environmentModel.ResourceRepository.ResumeWorkflowExecution(resourceModel, serEnv, Guid.Parse(assignActivity.UniqueID), versionNumber);
            Add("resumeMessage", msg);
        }

        [Given(@"I resume workflow ""(.*)""")]
        [When(@"I resume workflow ""(.*)""")]
        [Then(@"I resume workflow ""(.*)""")]
        public void GivenIResumeWorkflow(string resourceId)
        {
            TryGetValue("environment", out IServer environmentModel);
            var resourceModel = environmentModel.ResourceRepository.FindSingle(resource => resource.ID.ToString() == resourceId);
            Assert.IsNotNull(resourceModel);
            var env = new ExecutionEnvironment();
            env.Assign("[[Name]]", "Bob", 0);
            env.Assign("[[Rec(1).Name]]", "Bob", 0);
            env.Assign("[[Rec(3).SurName]]", "Bob", 0);
            env.Assign("[[Rec(4).Name]]", "Bob", 0);
            env.Assign("[[R(*).FName]]", "Bob", 0);
            env.Assign("[[RecSet().Field]]", "Bob", 0);
            env.Assign("[[RecSet().Field]]", "Jane", 0);
            env.AssignJson(new AssignValue("[[@Person]]", "{\"Name\":\"B\"}"), 0);
            var serEnv = env.ToJson();
            var msg = environmentModel.ResourceRepository.ResumeWorkflowExecution(resourceModel, serEnv, Guid.Parse("670132e7-80d4-4e41-94af-ba4a71b28118"), null);
            Add("resumeMessage", msg);
        }
        [Then(@"an error ""(.*)""")]
        public void ThenAnError(string message)
        {
            TryGetValue("resumeMessage", out ExecuteMessage executeMessage);
            Assert.IsNotNull(executeMessage);
            Assert.IsTrue(executeMessage.HasError);
            Assert.AreEqual(message, executeMessage.Message.ToString());
        }

        [Then(@"Resume has ""(.*)"" error")]
        public void WhenResumeHasError(string error)
        {
            TryGetValue("resumeMessage", out ExecuteMessage executeMessage);
            if (error == "AN")
            {
                Assert.IsTrue(executeMessage.HasError);
            }
            else
            {
                Assert.IsFalse(executeMessage.HasError);
            }
        }

        [Then(@"Resume message is ""(.*)""")]
        public void ThenResumeMessageIs(string message)
        {
            TryGetValue("resumeMessage", out ExecuteMessage executeMessage);
            Assert.IsNotNull(executeMessage);
            Assert.AreEqual(message, executeMessage.Message.ToString());
        }

        [Then(@"the ""(.*)"" in Workflow ""(.*)"" has an error")]
        public void ThenTheInWorkflowHasAnError(string toolName, string workflow)
        {
            var debugStates = Get<List<IDebugState>>("debugStates");
            var toolSpecificDebug =
                debugStates.Where(ds => ds.DisplayName.Equals(toolName)).ToList();
            Assert.IsFalse(string.IsNullOrEmpty(toolSpecificDebug[0].ErrorMessage));
        }

        [Then(@"execution stopped on error and did not execute ""(.*)""")]
        public void ThenExecutionForStoppedOnErrorAndDidNotExecute(string toolName)
        {
            var debugStates = Get<List<IDebugState>>("debugStates");
            Assert.IsFalse(debugStates.Any(ds => ds.DisplayName.Equals(toolName)));
        }

        [When(@"I startup the mysql container")]
        public void WhenIStartupTheContainer()
        {
            _containerOps = new Depends(Depends.ContainerType.MySQL);
        }

        ResourceCatalog ResourceCat { get; set; }
        ActivityParser Parser { get; set; }

        [Given(@"Workflow ""(.*)"" has ""(.*)"" activity")]
        [When(@"Workflow ""(.*)"" has ""(.*)"" activity")]
        [Then(@"Workflow ""(.*)"" has ""(.*)"" activity")]
        public void GivenWorkflowHasActivity(string workflow, string activityName)
        {
            TryGetValue(workflow, out IResourceModel resourceModel);
            var selectedActivity = GetActivity(activityName, resourceModel) as Activity;
            Assert.IsNotNull(selectedActivity, "The tool does not exist on the surface");
            _commonSteps.AddActivityToActivityList(workflow, activityName, selectedActivity);
        }

        private IDev2Activity GetActivity(string activityName, IResourceModel resourceModel)
        {
            ResourceCat = ResourceCat ?? new ResourceCatalog();
            Parser = Parser ?? new ActivityParser();

            var service = ResourceCat.GetService(GlobalConstants.ServerWorkspaceID, resourceModel.ID, resourceModel.ResourceName);
            var sa = service.Actions.FirstOrDefault();
            ResourceCat.MapServiceActionDependencies(GlobalConstants.ServerWorkspaceID, sa);
            var activity = ResourceCat.GetActivity(sa);
            var dev2Act = Parser.Parse(activity);
            var allNodes = Parser.ParseToLinkedFlatList(dev2Act);
            var selectedActivity = allNodes.FirstOrDefault(p => p.GetDisplayName() == activityName);
            return selectedActivity;
        }

        [Given(@"I resume workflow ""(.*)"" at ""(.*)"" tool")]
        [When(@"I resume workflow ""(.*)"" at ""(.*)"" tool")]
        [Then(@"I resume workflow ""(.*)"" at ""(.*)"" tool")]
        public void WhenIResumeWorkflowAtTool(string workflow, string toolToResumeFrom)
        {
            var uniqueId = GetActivityUniqueId(toolToResumeFrom);
            TryGetValue("environment", out IServer environmentModel);
            var resourceModel = environmentModel.ResourceRepository.FindSingle(resource => resource.ResourceName == workflow);
            Assert.IsNotNull(resourceModel);

            _debugWriterSubscriptionService = new SubscriptionService<DebugWriterWriteMessage>(environmentModel.Connection.ServerEvents);

            _debugWriterSubscriptionService.Subscribe(debugMsg => Append(debugMsg.DebugState));

            var env = "{\"Environment\":{\"scalars\":{\"number\":1},\"record_sets\":{},\"json_objects\":{}},\"Errors\":[],\"AllErrors\":[\"Service Execution Error:    at Dev2.Services.Execution.DatabaseServiceExecution.ExecuteService(Int32 update, ErrorResultTO& errors, IOutputFormatter formater) in C:\\\\Repos\\\\Warewolf\\\\Dev\\\\Dev2.Services.Execution\\\\DatabaseServiceExecution.cs:line 104\\r\\n   at Dev2.Services.Execution.ServiceExecutionAbstract`2.ExecuteService(ErrorResultTO& errors, Int32 update, IOutputFormatter formater) in C:\\\\Repos\\\\Warewolf\\\\Dev\\\\Dev2.Services.Execution\\\\ServiceExecutionAbstract.cs:line 372\"]}";
            var msg = environmentModel.ResourceRepository.ResumeWorkflowExecution(resourceModel, env, uniqueId, "");
            Add("resumeMessage", msg);
        }

        private Guid GetActivityUniqueId(string toolToResumeFrom)
        {
            var activities = _commonSteps.GetActivityList();
            var abstartActivity = activities[toolToResumeFrom] as DsfActivityAbstract<string>;
            if (abstartActivity != null)
            {
                return Guid.Parse(abstartActivity.UniqueID);
            }
            var activity = activities[toolToResumeFrom] as DsfActivity;
            return Guid.Parse(activity.UniqueID);
        }

        [When(@"I select ""(.*)"" Action for ""(.*)"" tool")]
        public void WhenISelectActionForTool(string action, string toolName)
        {
            var activities = _commonSteps.GetActivityList();
            var activity = activities[toolName] as DsfMySqlDatabaseActivity;
            activity.ProcedureName = action;
            activity.Inputs.Add(new ServiceInput("name", "S"));
        }

        [When(@"I select ""(.*)"" for ""(.*)"" as Source")]
        public void WhenISelectForAsSource(string source, string toolName)
        {
            var activities = _commonSteps.GetActivityList();
            var activity = activities[toolName] as DsfMySqlDatabaseActivity;

            var environmentModel = ServerRepository.Instance.Source;
            environmentModel.Connect();
            environmentModel.LoadExplorer(true);
            var environmentConnection = environmentModel.Connection;
            var controllerFactory = new CommunicationControllerFactory();
            var _proxyLayer = new StudioServerProxy(controllerFactory, environmentConnection);

            var dbSources = _proxyLayer.QueryManagerProxy.FetchDbSources().ToList();
            var dbSource = dbSources.Single(s => s.Name == source);
            activity.SourceId = dbSource.Id;
        }

        [Given(@"The detailed log file does not exist for ""(.*)""")]
        public void GivenTheDetailedLogFileDoesNotExistFor(string workflowName)
        {
            var detailLogInfo = new DetailLogInfo(workflowName, this);
            detailLogInfo.DeleteIfExists();
            Add($"DetailLogInfo {workflowName}", detailLogInfo);
        }

        [Given(@"The detailed log file does not exist for id ""(.*)"" - ""(.*)""")]
        public void GivenTheDetailedLogFileDoesNotExistForId(string id, string workflowName)
        {
            var detailLogInfo = new DetailLogInfo(workflowName, id);
            detailLogInfo.DeleteIfExists();
            Add($"DetailLogInfo {workflowName}", detailLogInfo);
        }

        [Then(@"The detailed log file is created for ""(.*)""")]
        public void ThenTheDetailedLogFileIsCreatedFor(string workflowName)
        {
            TryGetValue($"DetailLogInfo {workflowName}", out DetailLogInfo detailLogInfo);
            var logFileContent = detailLogInfo.ReadAllText();
            AddLogFileContentToContext(logFileContent);
            Assert.IsTrue(logFileContent.Length > 0);
        }

        [Then(@"The Log file contains Logging for ""(.*)""")]
        public void ThenTheLogFileContainsLoggingFor(string workflowName)
        {
            TryGetValue($"DetailLogInfo {workflowName}", out DetailLogInfo detailLogInfo);
            var logContent = detailLogInfo.ReadAllText();
            Assert.IsTrue(logContent.Contains("header:LogPreExecuteState"));
            Assert.IsTrue(logContent.Contains("header:LogPostExecuteState"));
            Assert.IsTrue(logContent.Contains("header:LogExecuteCompleteState"));
            Assert.IsFalse(logContent.Contains("header:LogStopExecutionState"));
        }

        [Then(@"The Log file contains Logging for stopped ""(.*)""")]
        public void ThenTheLogFileContainsLoggingForStopped(string workflowName)
        {
            TryGetValue($"DetailLogInfo {workflowName}", out DetailLogInfo detailLogInfo);
            var logContent = detailLogInfo.ReadAllText();
            Assert.IsTrue(logContent.Contains("header:LogPreExecuteState"));
            Assert.IsTrue(logContent.Contains("header:LogPostExecuteState"));
            Assert.IsFalse(logContent.Contains("header:LogExecuteCompleteState"));
            Assert.IsTrue(logContent.Contains("header:LogStopExecutionState"));
        }

        [Then(@"The Log file for ""(.*)"" contains additional Logging")]
        public void ThenTheLogFileContainsAdditionalLogging(string workflowName)
        {
            TryGetValue($"DetailLogInfo {workflowName}", out DetailLogInfo detailLogInfo);
            var previousLogFileSize = detailLogInfo.PreviousLength;
            var logContent = detailLogInfo.ReadAllText();
            Assert.IsTrue(logContent.Length > previousLogFileSize);
            var sizeDifference = logContent.Length / (double)previousLogFileSize;
            Assert.IsTrue(sizeDifference > 1.9);
            Assert.IsTrue(sizeDifference < 2.1);
        }

        [Then(@"The Log file for ""(.*)"" contains Logging matching ""(.*)""")]
        public void ThenTheLogFileForContainsLoggingMatching(string workflowName, string searchString)
        {
            TryGetValue($"DetailLogInfo {workflowName}", out DetailLogInfo detailLogInfo);
            var logFileContent = detailLogInfo.ReadAllText();
            AddLogFileContentToContext(logFileContent);
            Assert.IsTrue(logFileContent.Contains(searchString));
        }

        private void AddLogFileContentToContext(string logFileContent)
        {
            TryGetValue("LogFileContent", out string fileContent);
            if (fileContent == null)
            {
                Add("LogFileContent", logFileContent);
            }
            else
            {
                _scenarioContext.Remove("LogFileContent");
                Add("LogFileContent", logFileContent);
            }
        }

        [Then(@"The Log file contains Logging matching ""(.*)""")]
        public void ThenTheLogFileContainsLoggingMatching(string searchString)
        {
            TryGetValue("LogFileContent", out string logFileContent);
            Assert.IsTrue(logFileContent.Contains(searchString), $"detailed log file does not contain {searchString}");
        }
        [DeploymentItem(@"x86\SQLite.Interop.dll")]
        [Given(@"the audit database is empty")]
        public void GivenTheAuditDatabaseIsEmpty()
        {
            //TODO: Fix in new Implementation
            //Dev2StateAuditLogger.ClearAuditLog();
        }

        [Then(@"The audit database has ""(.*)"" search results containing ""(.*)"" with type """"(.*)""Decision""(.*)""Hello World"" as")]
        public void ThenTheAuditDatabaseHasSearchResultsContainingWithTypeDecisionHelloWorldAs(int expectedCount, string searchString, string auditType, string activityName, string workflowName, Table table)
        {
            //TODO: Fix in new Implementation
            //var results = Dev2StateAuditLogger.Query(item =>
            //(workflowName == "" || item.WorkflowName.Equals(workflowName)) &&
            //(auditType == "" || item.AuditType.Equals(auditType)) &&
            //(activityName == "" || (item.PreviousActivity != null && item.PreviousActivity.Contains(activityName))));
            //Assert.AreEqual(expectedCount, results.Count());
            //var prop = table.Rows[0][0];
            //var val = table.Rows[0][1];
            //foreach (var item in results)
            //{
            //    var value = item.GetType().GetProperty(prop).GetValue(item, null);
            //    Assert.AreEqual(val, value);
            //}
        }

        [DeploymentItem(@"x86\SQLite.Interop.dll")]
        [Then(@"The audit database has ""(.*)"" search results containing ""(.*)"" with type ""(.*)"" for ""(.*)"" as")]
        public void ThenTheAuditDatabaseHasSearchResultsContainingWithTypeWithActivityForAs(int expectedCount, string activityName, string auditType, string workflowName, Table table)
        {
            //TODO: correct with new implementation
            //Thread.Sleep(1000);
            //var results = Dev2StateAuditLogger.Query(item =>
            //   (workflowName == "" || item.WorkflowName.Equals(workflowName)) &&
            //   (auditType == "" || item.AuditType.Equals(auditType)) &&
            //   (activityName == "" || (
            //       (item.NextActivity != null && item.NextActivity.Contains(activityName)) ||
            //       (item.NextActivityType != null && item.NextActivityType.Contains(activityName)) ||
            //       (item.PreviousActivity != null && item.PreviousActivity.Contains(activityName)) ||
            //       (item.PreviousActivityType != null && item.PreviousActivityType.Contains(activityName))
            //   ))
            //);
            //   Assert.AreEqual(expectedCount, results.Count(), string.Join(" ", results.Select(entry => entry.WorkflowName + " " + entry.AuditDate + " " + entry.AdditionalDetail).ToList()));

            //if (results.Any() && table.Rows.Count > 0)
            //{
            //    var index = 0;
            //    foreach (var row in table.Rows)
            //    {
            //        var currentResult = results.ToArray()[index];
            //        Assert.AreEqual(row["AuditType"], currentResult.AuditType);
            //        Assert.AreEqual(row["WorkflowName"], currentResult.WorkflowName);
            //        Assert.AreEqual(row["PreviousActivityType"], currentResult.PreviousActivityType ?? "null");
            //        Assert.AreEqual(row["NextActivityType"], currentResult.NextActivityType ?? "null");
            //        index++;
            //    }
            //}
        }

        [DeploymentItem(@"x86\SQLite.Interop.dll")]
        [Then(@"The audit database has ""(.*)"" search results for ""(.*)"" as")]
        public void ThenTheAuditDatabaseHasSearchResultsForAs(int expectedCount, string workflowName, Table table)
        {
            //TODO: This will use the new server
            //var results = Dev2StateAuditLogger.Query(item =>
            //   (workflowName == "" || item.WorkflowName.Equals(workflowName))
            //);
            //Assert.AreEqual(expectedCount, results.Count());
            //if (results.Count() > 0 && table.Rows.Count > 0)
            //{
            //    var index = 0;
            //    foreach (var row in table.Rows)
            //    {
            //        var currentResult = results.ToArray()[index];
            //        Assert.AreEqual(row["WorkflowName"], currentResult.WorkflowName);
            //        Assert.AreEqual(row["AuditType"], currentResult.AuditType);
            //        Assert.AreEqual(row["VersionNumber"], currentResult.VersionNumber ?? "null");
            //        index++;
            //    }
            //}
        }
        [DeploymentItem(@"x86\SQLite.Interop.dll")]
        [Then(@"The audit database has ""(.*)"" search results containing ""(.*)"" with log type ""(.*)"" for ""(.*)""")]
        public void ThenTheLogFileSearchResultsContainFor(int expectedCount, string activityName, string logType, string workflowName)
        {
            //TODO: This will use the new server
            // var results = StateAuditLogger.Query(item =>
            //    (workflowName == "" || item.WorkflowName.Equals(workflowName)) &&
            //    (logType == "" || item.AuditType.Equals(logType)) &&
            //    (activityName == "" || (
            //        (item.NextActivity != null && item.NextActivity.Contains(activityName)) ||
            //        (item.NextActivityType != null && item.NextActivityType.Contains(activityName)) ||
            //        (item.PreviousActivity != null && item.PreviousActivity.Contains(activityName)) ||
            //        (item.PreviousActivityType != null && item.PreviousActivityType.Contains(activityName))
            //    ))
            //);
            // Assert.AreEqual(expectedCount, results.Count());
        }

        private static bool GetMatchingNames(string toolName, string displayName)
        {
            var updateDisplayName = GetMatchingName(displayName);
            var updateToolName = GetMatchingName(toolName);

            return updateDisplayName.Equals(updateToolName);
        }

        private static string GetMatchingName(string name)
        {
            var updateName = name;
            if (name.Contains("("))
            {
                var endIdx = name.IndexOf("(");
                updateName = name.Substring(0, endIdx).TrimEnd();
            }

            return updateName;
        }

        class DetailLogInfo
        {
            readonly IContextualResourceModel _resourceModel;
            readonly string _logFilePath;
            readonly FileWrapper _fileWrapper;
            public int PreviousLength { get; private set; }
            private DetailLogInfo()
            {
                _fileWrapper = new FileWrapper();
            }
            public DetailLogInfo(string workflowName, WorkflowExecutionSteps workflowExecutionSteps)
                : this()
            {
                workflowExecutionSteps.TryGetValue(workflowName, out _resourceModel);
                _logFilePath = Path.Combine(EnvironmentVariables.WorkflowDetailLogPath(_resourceModel.ID, _resourceModel.ResourceName), "Detail.log");
            }
            public DetailLogInfo(string workflowName, string resourceModelId)
                : this()
            {
                _logFilePath = Path.Combine(EnvironmentVariables.WorkflowDetailLogPath(Guid.Parse(resourceModelId), workflowName), "Detail.log");
            }

            internal void DeleteIfExists()
            {
                if (_fileWrapper.Exists(_logFilePath))
                {
                    _fileWrapper.Delete(_logFilePath);
                }
            }

            internal string ReadAllText()
            {
                var result = _fileWrapper.ReadAllText(_logFilePath);
                PreviousLength = result.Length;
                return result;
            }
        }
    }
}
