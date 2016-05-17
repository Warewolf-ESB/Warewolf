
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
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
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using Dev2.Activities.Specs.Composition.DBSource;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Enums.Enums;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Versioning;
using Dev2.Communication;
using Dev2.Controller;
using Dev2.Data.Enums;
using Dev2.Data.ServiceModel;
using Dev2.Data.Util;
using Dev2.Messages;
using Dev2.Models;
using Dev2.Network;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Services;
using Dev2.Services.Security;
using Dev2.Session;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.AppResources.Repositories;
using Dev2.Studio.Core.Interfaces;
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
using Dev2.Common.Interfaces.Enums;
using Dev2.Common.Interfaces.Monitoring;
using Dev2.PerformanceCounters.Counters;
using Dev2.PerformanceCounters.Management;
using Warewolf.Tools.Specs.BaseTypes;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedParameter.Global

namespace Dev2.Activities.Specs.Composition
{
    [Binding]
    public class WorkflowExecutionSteps : RecordSetBases
    {
        const int EnvironmentConnectionTimeout = 3000;

        private SubscriptionService<DebugWriterWriteMessage> _debugWriterSubscriptionService;
        private readonly AutoResetEvent _resetEvt = new AutoResetEvent(false);
        protected override void BuildDataList()
        {
            BuildShapeAndTestData();
        }

        [BeforeScenario]
        public void Setup()
        {
            if(_debugWriterSubscriptionService != null)
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
        }

        [Given(@"All environments disconnected")]
        public void GivenAllEnvironmentsDisconnected()
        {
            IEnvironmentModel environmentModel;
            TryGetValue("environment", out environmentModel);
            if(environmentModel != null && environmentModel.IsConnected)
            {
                environmentModel.Disconnect();
            }
        }

        [Given(@"Debug states are cleared")]
        public void GivenDebugStatesAreCleared()
        {
            List<IDebugState> debugStates;
            TryGetValue("debugStates", out debugStates);
            if(debugStates != null)
            {
                debugStates.Clear();
            }
        }

        [Given(@"Debug events are reset")]
        public void GivenDebugEventsAreReset()
        {
            if(_debugWriterSubscriptionService != null)
            {
                _debugWriterSubscriptionService.Unsubscribe();
                _debugWriterSubscriptionService.Dispose();
            }
        }

        [Then(@"the workflow execution has ""(.*)"" error")]
        public void ThenTheWorkflowExecutionHasError(string hasError)
        {
            Dictionary<string, Activity> activityList;
            string parentWorkflowName;
            TryGetValue("activityList", out activityList);
            TryGetValue("parentWorkflowName", out parentWorkflowName);
            var debugStates = Get<List<IDebugState>>("debugStates");

            if(hasError == "AN")
            {
                var hasErrorState = debugStates.FirstOrDefault(state => state.HasError);
                Assert.IsNotNull(hasErrorState);
            }
        }

        [Then(@"the ""(.*)"" workflow execution has ""(.*)"" error")]
        public void ThenTheWorkflowExecutionHasError(string workflowName, string hasError)
        {
            Dictionary<string, Activity> activityList;
            string parentWorkflowName;
            TryGetValue("activityList", out activityList);
            TryGetValue("parentWorkflowName", out parentWorkflowName);
            var debugStates = Get<List<IDebugState>>("debugStates");

            if(hasError == "AN")
            {
                var innerWfHasErrorState = debugStates.FirstOrDefault(state => state.HasError && state.DisplayName.Equals(workflowName));
                var parentWfhasErrorState = debugStates.FirstOrDefault(state => state.HasError && state.DisplayName.Equals(parentWorkflowName));
                Assert.IsNotNull(innerWfHasErrorState);
                Assert.IsNotNull(parentWfhasErrorState);
            }
        }

        [Given(@"I have server a ""(.*)"" with workflow ""(.*)""")]
        public void GivenIHaveAWorkflowOnServer(string serverName, string workflow)
        {
            AppSettings.LocalHost = "http://localhost:3142";
            var environmentModel = EnvironmentRepository.Instance.Source;
            environmentModel.Connect();
            environmentModel.ResourceRepository.ForceLoad();

            // connect to the remove environment now ;)
            var remoteServerList = environmentModel.ResourceRepository.FindSourcesByType<Connection>(environmentModel, enSourceType.Dev2Server);
            if(remoteServerList != null && remoteServerList.Count > 0)
            {
                var remoteServer = remoteServerList.FirstOrDefault(r => r.ResourceName == serverName);

                if(remoteServer != null)
                {
                    ServerProxy connection;
                    if(remoteServer.AuthenticationType == AuthenticationType.Windows || remoteServer.AuthenticationType == AuthenticationType.Anonymous)
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

                    var newEnvironment = new EnvironmentModel(remoteServer.ResourceID, connection) { Name = remoteServer.ResourceName, Category = remoteServer.ResourcePath };
                    EnsureEnvironmentConnected(newEnvironment, EnvironmentConnectionTimeout);
                    newEnvironment.ForceLoadResources();

                    // now find the bloody resource model for execution
                    var resourceModel = newEnvironment.ResourceRepository.Find(r => r.ResourceName == workflow).FirstOrDefault();

                    _debugWriterSubscriptionService = new SubscriptionService<DebugWriterWriteMessage>(newEnvironment.Connection.ServerEvents);

                    _debugWriterSubscriptionService.Subscribe(msg => Append(msg.DebugState));

                    Add(workflow, resourceModel);
                    Add("parentWorkflowName", workflow);
                    Add("environment", newEnvironment);
                    Add("resourceRepo", newEnvironment.ResourceRepository);
                    Add("debugStates", new List<IDebugState>());

                }
            }
        }

        [Given(@"I have a workflow ""(.*)""")]
        public void GivenIHaveAWorkflow(string workflowName)
        {
            AppSettings.LocalHost = "http://localhost:3142";
            IEnvironmentModel environmentModel = EnvironmentRepository.Instance.Source;
            EnsureEnvironmentConnected(environmentModel, EnvironmentConnectionTimeout);
            var resourceModel = new ResourceModel(environmentModel) { Category = "Acceptance Tests\\" + workflowName, ResourceName = workflowName, ID = Guid.NewGuid(), ResourceType = ResourceType.WorkflowService };

            environmentModel.ResourceRepository.Add(resourceModel);
            _debugWriterSubscriptionService = new SubscriptionService<DebugWriterWriteMessage>(environmentModel.Connection.ServerEvents);

            _debugWriterSubscriptionService.Subscribe(msg => Append(msg.DebugState));
            Add(workflowName, resourceModel);
            Add("parentWorkflowName", workflowName);
            Add("environment", environmentModel);
            Add("resourceRepo", environmentModel.ResourceRepository);
            Add("debugStates", new List<IDebugState>());
        }

        [Given(@"I have reset local perfromance Counters")]
        public void GivenIHaveResetLocalPerfromanceCounters()
        {
                try
                {
                    try
                    {
                        PerformanceCounterCategory.Delete("Warewolf");
                    }
                    // ReSharper disable once EmptyGeneralCatchClause
                    catch { }
                    WarewolfPerformanceCounterRegister register = new WarewolfPerformanceCounterRegister(new List<IPerformanceCounter>
                                                            {   new WarewolfCurrentExecutionsPerformanceCounter(),
                                                                new WarewolfNumberOfErrors(),   
                                                                new WarewolfRequestsPerSecondPerformanceCounter(),
                                                                new WarewolfAverageExecutionTimePerformanceCounter(),
                                                                new WarewolfNumberOfAuthErrors(),
                                                                new WarewolfServicesNotFoundCounter()
                                                            }, new List<IResourcePerformanceCounter>());
                    CustomContainer.Register<IWarewolfPerformanceCounterLocater>(new WarewolfPerformanceCounterManager(register.Counters, new List<IResourcePerformanceCounter>(), register, new Mock<IPerformanceCounterPersistence>().Object));
                }
                catch 
                {
                    // ignored
                    Assert.Fail("failed to delete existing counters");
                }
            }
            // ReSharper disable once EmptyGeneralCatchClause
        
        [Then(@"the perfcounter raw values are")]
        public void ThenThePerfcounterRawValuesAre(Table table)
        {
            var performanceCounterCategory = new PerformanceCounterCategory("Warewolf");
            var counters =  performanceCounterCategory.GetCounters();
            var instanceNames = performanceCounterCategory.GetInstanceNames();
            foreach(var tableRow in table.Rows)
            {
                foreach (var counter in instanceNames)
                {
                    var performanceCounter = counters.First(a => a.CounterName == tableRow[0]);
                    if (performanceCounter != null)
                    {
                        using (PerformanceCounter cnt = new PerformanceCounter("Warewolf", tableRow[0], counter, true))
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

        static void EnsureEnvironmentConnected(IEnvironmentModel environmentModel, int timeout)
        {
            if (timeout <= 0)
            {
                ScenarioContext.Current.Add("ConnectTimeoutCountdown", 3000);
                throw new TimeoutException("Connection to Warewolf server \"" + environmentModel.Name + "\" timed out.");
            }
            environmentModel.Connect();
            if (!environmentModel.IsConnected)
            {
                timeout--;
                Thread.Sleep(100);
                EnsureEnvironmentConnected(environmentModel, timeout);
            }
        }

        void Add(string key, object value)
        {
            ScenarioContext.Current.Add(key, value);
        }

        void Append(IDebugState debugState)
        {
            List<IDebugState> debugStates;
            List<IDebugState> debugStatesDuration;
            string workflowName;
            IEnvironmentModel environmentModel;
            TryGetValue("debugStates", out debugStates);
            TryGetValue("debugStatesDuration", out debugStatesDuration);
            TryGetValue("parentWorkflowName", out workflowName);
            TryGetValue("environment", out environmentModel);
            if(debugStatesDuration == null)
            {
                debugStatesDuration = new List<IDebugState>();
                Add("debugStatesDuration",debugStatesDuration);
            }
            if(debugState.WorkspaceID == environmentModel.Connection.WorkspaceID)
            {
                if(debugState.StateType!=StateType.Duration)
                debugStates.Add(debugState);
                else
                debugStatesDuration.Add(debugState);
            }
            if(debugState.IsFinalStep() && debugState.DisplayName.Equals(workflowName))
            {
                _resetEvt.Set();
            }

        }

        [Then(@"the ""(.*)"" in step (.*) for ""(.*)"" debug inputs as")]
        public void ThenTheInStepForDebugInputsAs(string toolName, int stepNumber, string forEachName, Table table)
        {
            Dictionary<string, Activity> activityList;
            string parentWorkflowName;
            TryGetValue("activityList", out activityList);
            TryGetValue("parentWorkflowName", out parentWorkflowName);

            var debugStates = Get<List<IDebugState>>("debugStates");
            var workflowId = debugStates.First(wf => wf.DisplayName.Equals(forEachName)).ID;

            if(parentWorkflowName == forEachName)
            {
                workflowId = Guid.Empty;
            }

            var toolSpecificDebug =
                debugStates.Where(ds => ds.ParentID == workflowId && ds.DisplayName.Equals(toolName)).ToList();

            Assert.IsTrue(toolSpecificDebug.Count >= stepNumber);
            var debugToUse = DebugToUse(stepNumber, toolSpecificDebug);


            var commonSteps = new CommonSteps();
            commonSteps.ThenTheDebugInputsAs(table, debugToUse.Inputs
                                                    .SelectMany(item => item.ResultsList).ToList());
        }

        [Then(@"the ""(.*)"" in ""(.*)"" in step (.*) for ""(.*)"" debug inputs as")]
        public void ThenTheInInStepForDebugInputsAs(string toolName, string sequenceName, int stepNumber, string forEachName, Table table)
        {
            Dictionary<string, Activity> activityList;
            string parentWorkflowName;
            TryGetValue("activityList", out activityList);
            TryGetValue("parentWorkflowName", out parentWorkflowName);

            var debugStates = Get<List<IDebugState>>("debugStates");
            var workflowId = debugStates.First(wf => wf.DisplayName.Equals(forEachName)).ID;

            if(parentWorkflowName == forEachName)
            {
                workflowId = Guid.Empty;
            }

            var sequenceDebug = debugStates.Where(ds => ds.ParentID == workflowId).ToList();
            Assert.IsTrue(sequenceDebug.Count >= stepNumber);

            var sequenceId = sequenceDebug[stepNumber - 1].ID;
            var sequenceIsInForEach = sequenceDebug.Any(state => state.ID == sequenceId);
            Assert.IsTrue(sequenceIsInForEach);

            var toolSpecificDebug =
                debugStates.Where(ds => ds.ParentID == sequenceId && ds.DisplayName.Equals(toolName)).ToList();

            var commonSteps = new CommonSteps();
            commonSteps.ThenTheDebugInputsAs(table, toolSpecificDebug
                                                    .SelectMany(item => item.Inputs)
                                                    .SelectMany(item => item.ResultsList).ToList());

        }

        [Then(@"the ""(.*)"" in ""(.*)"" in step (.*) for ""(.*)"" debug outputs as")]
        public void ThenTheInInStepForDebugOutputsAs(string toolName, string sequenceName, int stepNumber, string forEachName, Table table)
        {
            Dictionary<string, Activity> activityList;
            string parentWorkflowName;
            TryGetValue("activityList", out activityList);
            TryGetValue("parentWorkflowName", out parentWorkflowName);

            var debugStates = Get<List<IDebugState>>("debugStates");
            var workflowId = debugStates.First(wf => wf.DisplayName.Equals(forEachName)).ID;

            if(parentWorkflowName == forEachName)
            {
                workflowId = Guid.Empty;
            }

            var sequenceDebug = debugStates.Where(ds => ds.ParentID == workflowId).ToList();
            Assert.IsTrue(sequenceDebug.Count >= stepNumber);

            var sequenceId = sequenceDebug[stepNumber - 1].ID;
            var sequenceIsInForEach = sequenceDebug.Any(state => state.ID == sequenceId);
            Assert.IsTrue(sequenceIsInForEach);

            var toolSpecificDebug =
                debugStates.Where(ds => ds.ParentID == sequenceId && ds.DisplayName.Equals(toolName)).ToList();

            var commonSteps = new CommonSteps();
            commonSteps.ThenTheDebugInputsAs(table, toolSpecificDebug
                                                    .SelectMany(item => item.Outputs)
                                                    .SelectMany(item => item.ResultsList).ToList());
        }


        static IDebugState DebugToUse(int stepNumber, List<IDebugState> toolSpecificDebug)
        {
            var debugToUse = toolSpecificDebug[stepNumber - 1];
            return debugToUse;
        }

        [Then(@"the ""(.*)"" in step (.*) for ""(.*)"" debug outputs as")]
        public void ThenTheInStepForDebugOutputsAs(string toolName, int stepNumber, string forEachName, Table table)
        {
            Dictionary<string, Activity> activityList;
            string parentWorkflowName;
            TryGetValue("activityList", out activityList);
            TryGetValue("parentWorkflowName", out parentWorkflowName);

            var debugStates = Get<List<IDebugState>>("debugStates");
            var workflowId = debugStates.First(wf => wf.DisplayName.Equals(forEachName)).ID;

            if(parentWorkflowName == forEachName)
            {
                workflowId = Guid.Empty;
            }

            var toolSpecificDebug =
                debugStates.Where(ds => ds.ParentID == workflowId && ds.DisplayName.Equals(toolName)).ToList();
            Assert.IsTrue(toolSpecificDebug.Count >= stepNumber);
            var debugToUse = DebugToUse(stepNumber, toolSpecificDebug);

            var commonSteps = new CommonSteps();
            commonSteps.ThenTheDebugOutputAs(table, debugToUse.Outputs
                                                    .SelectMany(s => s.ResultsList).ToList());
        }

        [Given(@"""(.*)"" contains a ""(.*)"" service ""(.*)"" with mappings")]
        public void GivenContainsADatabaseServiceWithMappings(string wf, string serviceType, string serviceName, Table table)
        {
            IEnvironmentModel environmentModel = EnvironmentRepository.Instance.Source;
            ResourceRepository repository = new ResourceRepository(environmentModel);
            repository.Load();
            var resource = repository.FindSingle(r => r.ResourceName.Equals(serviceName),true,true);
            if (resource == null)
            {
                throw new Exception("Local Warewolf service " + serviceName + " not found.");
            }
            var activity = GetServiceActivity(serviceType);
            if(activity != null)
            {
                var outputSb = GetOutputMapping(table, resource);
                var inputSb = GetInputMapping(table, resource);
                var outputMapping = outputSb.ToString();
                var inputMapping = inputSb.ToString();
                resource.Outputs = outputMapping;
                resource.Inputs = inputMapping;

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
                    switch(serviceType)
                    {
                        case "mysql database":
                            updatedActivity = ActivityUtils.GetDsfMySqlDatabaseActivity((DsfDatabaseActivity)activity, source, service);
                            break;
                        case "sqlserver database":
                            updatedActivity = ActivityUtils.GetDsfSqlServerDatabaseActivity((DsfDatabaseActivity)activity, service, source);
                            break;
                        case "oracle database":
                            updatedActivity = ActivityUtils.GetDsfOracleDatabaseActivity((DsfDatabaseActivity)activity, service, source);
                            break;
                        case "odbc database":
                            updatedActivity = ActivityUtils.GetDsfODBCDatabaseActivity((DsfDatabaseActivity)activity, service, source);
                            break;
                    }
                    CommonSteps.AddActivityToActivityList(wf, serviceName, updatedActivity);
                }
                else if(resource.ServerResourceType == "WebService")
                {
                    var updatedActivity = new DsfWebGetActivity();
                    var xml = resource.ToServiceDefinition(true).ToXElement();
                    var service = new WebService(xml);
                    var source = service.Source as WebSource;
                    updatedActivity.Headers = new List<INameValue>();
                    if(service.Headers != null)
                    {
                        service.Headers.AddRange(service.Headers);                        
                    }
                    updatedActivity.OutputDescription = service.OutputDescription;
                    updatedActivity.QueryString = service.RequestUrl;
                    updatedActivity.Inputs = ActivityUtils.TranslateInputMappingToInputs(inputMapping);
                    updatedActivity.Outputs = ActivityUtils.TranslateOutputMappingToOutputs(outputMapping);
                    updatedActivity.DisplayName = serviceName;
                    if(source != null)
                    {
                        updatedActivity.SourceId = source.ResourceID;
                    }
                    CommonSteps.AddActivityToActivityList(wf, serviceName, updatedActivity);
                }
                else
                {
                    CommonSteps.AddActivityToActivityList(wf, serviceName, activity);
                }
            }
        }

        [Given(@"""(.*)"" contains ""(.*)"" from server ""(.*)"" with mapping as")]
        public void GivenContainsFromServerWithMappingAs(string wf, string remoteWf, string server, Table mappings)
        {
            EnsureEnvironmentConnected(EnvironmentRepository.Instance.Source, EnvironmentConnectionTimeout);
            EnvironmentRepository.Instance.Source.ForceLoadResources();
            
            var remoteEnvironment = EnvironmentRepository.Instance.FindSingle(model => model.Name == server);
            if (remoteEnvironment == null)
            {
                var environments = EnvironmentRepository.Instance.LookupEnvironments(EnvironmentRepository.Instance.Source);
                remoteEnvironment = environments.FirstOrDefault(model => model.Name == server);
            }
            if(remoteEnvironment != null)
            {
                EnsureEnvironmentConnected(remoteEnvironment, EnvironmentConnectionTimeout);
                remoteEnvironment.ForceLoadResources();
                var splitNameAndCat = remoteWf.Split(new char[]{'/'});
                var resName = splitNameAndCat[splitNameAndCat.Length-1];
                var remoteResourceModel = remoteEnvironment.ResourceRepository.FindSingle(model => model.ResourceName == resName || model.Category == remoteWf.Replace('/', '\\'), true);
                if(remoteResourceModel != null)
                {
                    var dataMappingViewModel = GetDataMappingViewModel(remoteResourceModel, mappings);

                    var inputMapping = dataMappingViewModel.GetInputString(dataMappingViewModel.Inputs);
                    var outputMapping = dataMappingViewModel.GetOutputString(dataMappingViewModel.Outputs);
                    
                    var activity = new DsfWorkflowActivity();

                    remoteResourceModel.Outputs = outputMapping;
                    remoteResourceModel.Inputs = inputMapping;
                    var remoteServerId = remoteEnvironment.ID;
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
                    CommonSteps.AddActivityToActivityList(wf, remoteWf, activity);
                }
                else
                {
                    throw new Exception("Remote Warewolf service " + remoteWf + " not found on server " + server + ".");
                }
            }
            else
            {
                throw new Exception(string.Format("Remote server {0} not found", server));
            }
        }

        static DataMappingViewModel GetDataMappingViewModel(IResourceModel remoteResourceModel, Table mappings)
        {
            var webActivity = new WebActivity { ResourceModel = remoteResourceModel as ResourceModel };
            DataMappingViewModel dataMappingViewModel = new DataMappingViewModel(webActivity);
            foreach(var inputOutputViewModel in dataMappingViewModel.Inputs)
            {
                inputOutputViewModel.Value = "";
                inputOutputViewModel.RecordSetName = "";
                inputOutputViewModel.Name = "";
                inputOutputViewModel.MapsTo = "";
            }
            
            foreach(var inputOutputViewModel in dataMappingViewModel.Outputs)
            {
                inputOutputViewModel.Value = "";
                inputOutputViewModel.RecordSetName = "";
                inputOutputViewModel.Name = "";
            }
            foreach(var tableRow in mappings.Rows)
            {
                string output;
                tableRow.TryGetValue("Output from Service", out output);
                string toVariable;
                tableRow.TryGetValue("To Variable", out toVariable);
                if(!string.IsNullOrEmpty(output) && !string.IsNullOrEmpty(toVariable))
                {
                    var inputOutputViewModel = dataMappingViewModel.Outputs.FirstOrDefault(model => model.DisplayName == output);
                    if(inputOutputViewModel != null)
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
                        CommonSteps.AddVariableToVariableList(toVariable);
                    }
                }

                string input;
                tableRow.TryGetValue("Input to Service", out input);
                string fromVariable;
                tableRow.TryGetValue("From Variable", out fromVariable);

                if(!string.IsNullOrEmpty(input) && !string.IsNullOrEmpty(fromVariable))
                {
                    var inputOutputViewModel = dataMappingViewModel.Inputs.FirstOrDefault(model => model.DisplayName == input);
                    if(inputOutputViewModel != null)
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
                        CommonSteps.AddVariableToVariableList(fromVariable);
                    }
                }

            }
            return dataMappingViewModel;
        }

        static DsfActivity GetServiceActivity(string serviceType)
        {
            DsfActivity activity = null;
            switch(serviceType)
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
                case "plugin":
                    activity = new DsfPluginActivity();
                    break;
                case "webservice":
                    activity = new DsfWebserviceActivity();
                    break;
                case "workflow":
                    activity = new DsfWorkflowActivity();
                    break;
            }
            return activity;
        }

        static StringBuilder GetOutputMapping(Table table, IResourceModel resource)
        {
            var outputSb = new StringBuilder();
            outputSb.Append("<Outputs>");

            foreach(var tableRow in table.Rows)
            {
                var output = tableRow["Output from Service"];
                var toVariable = tableRow["To Variable"];

                CommonSteps.AddVariableToVariableList(toVariable);

                if(resource != null)
                {

                    var outputs = XDocument.Parse(resource.Outputs);

                    string recordsetName;
                    string fieldName;

                    if(DataListUtil.IsValueRecordset(output))
                    {
                        recordsetName = DataListUtil.ExtractRecordsetNameFromValue(output);
                        fieldName = DataListUtil.ExtractFieldNameFromValue(output);
                    }
                    else
                    {
                        recordsetName = fieldName = output;
                    }

                    var element = (from elements in outputs.Descendants("Output")
                                   where String.Equals((string)elements.Attribute("RecordsetAlias"), recordsetName, StringComparison.InvariantCultureIgnoreCase) &&
                                         String.Equals((string)elements.Attribute("OriginalName"), fieldName, StringComparison.InvariantCultureIgnoreCase)      
                                   select elements).SingleOrDefault();

                    if(element != null)
                    {
                        element.SetAttributeValue("Value", toVariable);
                    }

                    outputSb.Append(element);
                }
            }

            outputSb.Append("</Outputs>");
            return outputSb;
        }

        [Given(@"""(.*)"" in ""(.*)"" contains Data Merge ""(.*)"" into ""(.*)"" as")]
        public void GivenInContainsDataMergeIntoAs(string sequenceName, string forEachName, string activityName, string resultVariable, Table table)
        {
            DsfDataMergeActivity activity = new DsfDataMergeActivity { Result = resultVariable, DisplayName = activityName };
            foreach(var tableRow in table.Rows)
            {
                var variable = tableRow["Variable"];
                var type = tableRow["Type"];
                var at = tableRow["Using"];
                var padding = tableRow["Padding"];
                var alignment = tableRow["Alignment"];

                activity.MergeCollection.Add(new DataMergeDTO(variable, type, at, 1, padding, alignment, true));
            }
            CommonSteps.AddVariableToVariableList(resultVariable);
            AddActivityToSequenceInsideForEach(sequenceName, forEachName, activity);
        }

        static void AddActivityToSequenceInsideForEach(string sequenceName, string forEachName, Activity activity)
        {

            var activityList = CommonSteps.GetActivityList();
            var forEachActivity = activityList[forEachName] as DsfForEachActivity;
            if(forEachActivity != null)
            {
                var sequenceActivity = forEachActivity.DataFunc.Handler as DsfSequenceActivity;
                if(sequenceActivity != null && sequenceActivity.DisplayName == sequenceName)
                {
                    sequenceActivity.Activities.Add(activity);
                }
            }
        }

        [Given(@"""(.*)"" in ""(.*)"" contains Gather System Info ""(.*)"" as")]
        public void GivenInContainsGatherSystemInfoAs(string sequenceName, string forEachName, string activityName, Table table)
        {
            var activity = new DsfGatherSystemInformationActivity { DisplayName = activityName };
            foreach(var tableRow in table.Rows)
            {
                var variable = tableRow["Variable"];

                CommonSteps.AddVariableToVariableList(variable);

                enTypeOfSystemInformationToGather systemInfo = (enTypeOfSystemInformationToGather)Dev2EnumConverter.GetEnumFromStringDiscription(tableRow["Selected"], typeof(enTypeOfSystemInformationToGather));
                activity.SystemInformationCollection.Add(new GatherSystemInformationTO(systemInfo, variable, 1));
            }

            AddActivityToSequenceInsideForEach(sequenceName, forEachName, activity);
        }

        static StringBuilder GetInputMapping(Table table, IResourceModel resource)
        {
            var inputSb = new StringBuilder();
            inputSb.Append("<Inputs>");

            foreach(var tableRow in table.Rows)
            {
                var input = tableRow["Input to Service"];
                var fromVariable = tableRow["From Variable"];

                CommonSteps.AddVariableToVariableList(fromVariable);

                if(resource != null)
                {
                    var inputs = XDocument.Parse(resource.Inputs);

                    string recordsetName;
                    XElement element;
                    if(DataListUtil.IsValueRecordset(input))
                    {
                        recordsetName = DataListUtil.ExtractRecordsetNameFromValue(input);
                        string fieldName = DataListUtil.ExtractFieldNameFromValue(input);

                        element = (from elements in inputs.Descendants("Input")
                                   where String.Equals((string)elements.Attribute("Recordset"), recordsetName, StringComparison.InvariantCultureIgnoreCase) &&
                                         String.Equals((string)elements.Attribute("OriginalName"), fieldName, StringComparison.InvariantCultureIgnoreCase)      
                                   select elements).SingleOrDefault();

                        if(element != null)
                        {
                            element.SetAttributeValue("Value", fromVariable);
                        }

                        inputSb.Append(element);
                    }
                    else
                    {
                        recordsetName = input;

                        element = (from elements in inputs.Descendants("Input")
                                   where String.Equals((string)elements.Attribute("Name"), recordsetName, StringComparison.InvariantCultureIgnoreCase)
                                   select elements).SingleOrDefault();

                        if(element != null)
                        {
                            element.SetAttributeValue("Source", fromVariable);
                        }
                    }

                    if(element != null)
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
            // environment
            IEnvironmentModel environmentModel;
            IResourceRepository repository;
            IContextualResourceModel resourceModel;

            TryGetValue(workflowName, out resourceModel);
            TryGetValue("environment", out environmentModel);
            TryGetValue("resourceRepo", out repository);

            ExecuteWorkflow(resourceModel);
        }

        [When(@"""(.*)"" is executed")]
        public void WhenIsExecuted(string workflowName)
        {
            BuildDataList();

            var activityList = CommonSteps.GetActivityList();

            var flowSteps = new List<FlowStep>();

            TestStartNode = new FlowStep();
            flowSteps.Add(TestStartNode);

            foreach(var activity in activityList)
            {
                if(TestStartNode.Action == null)
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

            IContextualResourceModel resourceModel;
            IEnvironmentModel environmentModel;
            IResourceRepository repository;
            TryGetValue(workflowName, out resourceModel);
            TryGetValue("environment", out environmentModel);
            TryGetValue("resourceRepo", out repository);

            string currentDl = CurrentDl;
            resourceModel.DataList = currentDl.Replace("root", "DataList");
            WorkflowHelper helper = new WorkflowHelper();
            StringBuilder xamlDefinition = helper.GetXamlDefinition(FlowchartActivityBuilder);
            resourceModel.WorkflowXaml = xamlDefinition;

            repository.Save(resourceModel, false);
            repository.SaveToServer(resourceModel);

            ExecuteWorkflow(resourceModel);
        }


        [Then(@"the ""(.*)"" in WorkFlow ""(.*)"" debug inputs as")]
        public void ThenTheInWorkFlowDebugInputsAs(string toolName, string workflowName, Table table)
        {
            Dictionary<string, Activity> activityList;
            string parentWorkflowName;
            TryGetValue("activityList", out activityList);
            TryGetValue("parentWorkflowName", out parentWorkflowName);
            var debugStates = Get<List<IDebugState>>("debugStates");
            Guid workflowId = Guid.Empty;

            if(parentWorkflowName != workflowName)
            {
                if(toolName != null && workflowName != null)
                {
                    workflowId = debugStates.First(wf => wf.DisplayName.Equals(workflowName)).ID;
                }
                else
                {
                    throw new InvalidOperationException("SpecFlow broke.");
                }
            }

            var toolSpecificDebug =
                debugStates.Where(ds => ds.ParentID == workflowId && ds.DisplayName.Equals(toolName)).ToList();

            // Data Merge breaks our debug scheme, it only ever has 1 value, not the expected 2 ;)
            //bool isDataMergeDebug = toolSpecificDebug.Any(t => t.Name == "Data Merge");

            var commonSteps = new CommonSteps();
            commonSteps.ThenTheDebugInputsAs(table, toolSpecificDebug.Distinct()
                                                    .SelectMany(s => s.Inputs)
                                                    .SelectMany(s => s.ResultsList).ToList());
        }

        [Then(@"the ""(.*)"" has a start and end duration")]
        public void ThenTheHasAStartAndEndDuration(string workflowName)
        {
            Dictionary<string, Activity> activityList;
            string parentWorkflowName;
            TryGetValue("activityList", out activityList);
            TryGetValue("parentWorkflowName", out parentWorkflowName);
            var debugStates = Get<List<IDebugState>>("debugStates");

            //var start = debugStates.First(wf => wf.Name.Equals("Start"));
            //Assert.IsTrue(start.Duration.Ticks>0);
            var end = debugStates.First(wf => wf.Name.Equals("End"));
            Assert.IsTrue(end.Duration.Ticks > 0);
        }

        [Then(@"the nested ""(.*)"" in WorkFlow ""(.*)"" debug inputs as")]
        public void ThenTheNestedInWorkFlowDebugInputsAs(string toolName, string workflowName, Table table)
        {
            Dictionary<string, Activity> activityList;
            string parentWorkflowName;
            TryGetValue("activityList", out activityList);
            TryGetValue("parentWorkflowName", out parentWorkflowName);
            var debugStates = Get<List<IDebugState>>("debugStates");

            var toolSpecificDebug =
                debugStates.Where(ds => ds.DisplayName.Equals(toolName)).ToList();

            // Data Merge breaks our debug scheme, it only ever has 1 value, not the expected 2 ;)
            //bool isDataMergeDebug = toolSpecificDebug.Any(t => t.Name == "Data Merge");

            var commonSteps = new CommonSteps();
            commonSteps.ThenTheDebugInputsAs(table, toolSpecificDebug
                                                    .SelectMany(s => s.Inputs)
                                                    .SelectMany(s => s.ResultsList).ToList());
        }

        [Then(@"the ""(.*)"" in WorkFlow ""(.*)"" has  ""(.*)"" nested children")]
        public void ThenTheInWorkFlowHasNestedChildren(string toolName, string workflowName, int count)
        {
            Dictionary<string, Activity> activityList;
            string parentWorkflowName;
            TryGetValue("activityList", out activityList);
            TryGetValue("parentWorkflowName", out parentWorkflowName);
            var debugStates = Get<List<IDebugState>>("debugStates");

            var id =
                debugStates.Where(ds => ds.DisplayName.Equals(toolName)).ToList().Select(a => a.ID).First();
            var children = debugStates.Count(a => a.ParentID == id);
            Assert.AreEqual(count, children);
        }

        [Then(@"each nested debug item for ""(.*)"" in WorkFlow ""(.*)"" contains ""(.*)"" child                              \|")]
        public void ThenEachNestedDebugItemForInWorkFlowContainsChild(string toolName, string workFlowName, int childCount)
        {
            Dictionary<string, Activity> activityList;
            string parentWorkflowName;
            TryGetValue("activityList", out activityList);
            TryGetValue("parentWorkflowName", out parentWorkflowName);
            var debugStates = Get<List<IDebugState>>("debugStates");

            var id = debugStates.Where(ds => ds.DisplayName.Equals("DsfActivity")).ToList();
            id.ForEach(x => Assert.AreEqual(childCount, debugStates.Count(a => a.ParentID == x.ID && a.DisplayName == toolName)));

        }

        [Then(@"each ""(.*)"" contains debug outputs for ""(.*)"" as")]
        public void ThenEachContainsDebugOutputsForAs(string toolName, string nestedToolName, Table table)
        {
            Dictionary<string, Activity> activityList;
            string parentWorkflowName;
            TryGetValue("activityList", out activityList);
            TryGetValue("parentWorkflowName", out parentWorkflowName);
            var debugStates = Get<List<IDebugState>>("debugStates");

            var id = debugStates.Where(ds => ds.DisplayName.Equals("DsfActivity")).ToList();
            id.ForEach(x => Assert.AreEqual(1, debugStates.Count(a => a.ParentID == x.ID && a.DisplayName == nestedToolName)));
        }

        T Get<T>(string keyName)
        {
            return ScenarioContext.Current.Get<T>(keyName);
        }

        void TryGetValue<T>(string keyName, out T value)
        {
            ScenarioContext.Current.TryGetValue(keyName, out value);
        }


        public string GetServerMemory()
        {
            var stringBuilder = new StringBuilder();
            var winQuery = new ObjectQuery("SELECT * FROM Win32_Process Where Name LIKE '%Warewolf Server.exe%'");
            var searcher = new ManagementObjectSearcher(winQuery);
            foreach(var o in searcher.Get())
            {
                var item = (ManagementObject)o;
                var memory = Convert.ToString(item["WorkingSetSize"]);
                stringBuilder.Append(memory);
            }
            return stringBuilder.ToString();
        }

        // ReSharper disable InconsistentNaming
        public double GetServerCPUUsage()
        {
            PerformanceCounter processorTimeCounter = new PerformanceCounter(
                    "Process",
                    "% Processor Time",
                    "Warewolf Server", true);
            processorTimeCounter.NextValue();
            Thread.Sleep(1000);
            return processorTimeCounter.NextValue() / Environment.ProcessorCount;
        }

        [Then(@"the server CPU usage is less than (.*)%")]
        public void ThenTheServerCPUUsageIsLessThan(int maxCpu)
        {
            var serverCpuUsage = GetServerCPUUsage();

            Assert.IsTrue(serverCpuUsage < maxCpu, "Warewolf Server CPU usage: " + serverCpuUsage.ToString(CultureInfo.InvariantCulture));
        }

        [Given(@"I get the server memory")]
        public void GivenIGetTheServerMemory()
        {
            var serverMemory = GetServerMemory();
            Add("BeforeServerMemory", serverMemory);
        }

        [Then(@"the server memory difference is less than (.*) mb")]
        public void ThenTheServerMemoryDifferenceIsLessThanMb(int maxDiff)
        {
            var serverMemoryBefore = Get<string>("BeforeServerMemory");
            var serverMemoryAfter = GetServerMemory();

            var serverMemAfter = Convert.ToDecimal(serverMemoryAfter) / 1024 / 1024;
            var serverMemBefore = Convert.ToDecimal(serverMemoryBefore) / 1024 / 1024;

            var diffInMem = serverMemAfter - serverMemBefore;

            Assert.IsTrue(diffInMem < maxDiff, "Warewolf Server memory usage: " + diffInMem.ToString(CultureInfo.InvariantCulture));
        }

        [Then(@"the ""(.*)"" in Workflow ""(.*)"" has a debug Server Name of """"(.*)""""")]
        public void ThenTheInWorkflowHasADebugServerNameOf(string toolName, string workflowName, string remoteName)
        {
            Dictionary<string, Activity> activityList;
            string parentWorkflowName;
            TryGetValue("activityList", out activityList);
            TryGetValue("parentWorkflowName", out parentWorkflowName);

            var debugStates = Get<List<IDebugState>>("debugStates");
            var workflowId = debugStates.First(wf => wf.DisplayName.Equals(workflowName)).ID;

            if (parentWorkflowName == workflowName)
            {
                workflowId = Guid.Empty;
            }

            var toolSpecificDebug =
                debugStates.Where(ds => ds.ParentID == workflowId && ds.DisplayName.Equals(toolName)).ToList();

            Assert.IsTrue(toolSpecificDebug.All(a=>a.Server==remoteName));
            Assert.IsTrue(debugStates.Where(ds => ds.ParentID == workflowId && !ds.DisplayName.Equals(toolName)).All(a=>a.Server=="localhost"));
        }


        [Then(@"the ""(.*)"" in Workflow ""(.*)"" debug outputs as")]
        public void ThenTheInWorkflowDebugOutputsAs(string toolName, string workflowName, Table table)
        {
            Dictionary<string, Activity> activityList;
            string parentWorkflowName;
            TryGetValue("activityList", out activityList);
            TryGetValue("parentWorkflowName", out parentWorkflowName);

            var debugStates = Get<List<IDebugState>>("debugStates");
            Guid workflowId = Guid.Empty;

            if(parentWorkflowName != workflowName)
            {
                if(toolName != null && workflowName != null)
                {
                    workflowId = debugStates.First(wf => wf.DisplayName.Equals(workflowName)).ID;
                }
                else
                {
                    throw new InvalidOperationException("SpecFlow broke.");
                }
            }

            var toolSpecificDebug =
                debugStates.Where(ds => ds.ParentID == workflowId && ds.DisplayName.Equals(toolName)).ToList();

            // Data Merge breaks our debug scheme, it only ever has 1 value, not the expected 2 ;)
            bool isDataMergeDebug = toolSpecificDebug.Count == 1 && toolSpecificDebug.Any(t => t.Name == "Data Merge");

            var commonSteps = new CommonSteps();
            commonSteps.ThenTheDebugOutputAs(table, toolSpecificDebug
                                                    .SelectMany(s => s.Outputs)
                                                    .SelectMany(s => s.ResultsList).ToList(), isDataMergeDebug);
        }

        [Given(@"""(.*)"" contains an SQL Bulk Insert ""(.*)"" using database ""(.*)"" and table ""(.*)"" and KeepIdentity set ""(.*)"" and Result set ""(.*)"" as")]
        public void GivenContainsAnSqlBulkInsertAs(string workflowName, string activityName, string dbSrcName, string tableName, string keepIdentity, string result, Table table)
        {
            // Fetch source from source name ;)
            var resourceXml = XmlFetch.Fetch(dbSrcName);
            if(resourceXml != null)
            {
                // extract keepIdentity value ;)
                bool keepIdentityBool;
                bool.TryParse(keepIdentity, out keepIdentityBool);

                var dbSource = new DbSource(resourceXml);
                // Configure activity ;)
                var dsfSqlBulkInsert = new DsfSqlBulkInsertActivity { Result = result, DisplayName = activityName, TableName = tableName, Database = dbSource, KeepIdentity = keepIdentityBool };
                // build input mapping
                var mappings = new List<DataColumnMapping>();

                var pos = 1;
                // ReSharper disable LoopCanBeConvertedToQuery
                foreach(var row in table.Rows)
                // ReSharper restore LoopCanBeConvertedToQuery
                {
                    var outputColumn = row["Column"];
                    var inputColumn = row["Mapping"];
                    var isNullableStr = row["IsNullable"];
                    var dataTypeName = row["DataTypeName"];
                    var maxLengthStr = row["MaxLength"];
                    var isAutoIncrementStr = row["IsAutoIncrement"];

                    bool isNullable;
                    bool isAutoIncrement;
                    int maxLength;
                    bool.TryParse(isNullableStr, out isNullable);
                    bool.TryParse(isAutoIncrementStr, out isAutoIncrement);
                    Int32.TryParse(maxLengthStr, out maxLength);
                    SqlDbType dataType;
                    Enum.TryParse(dataTypeName, true, out dataType);

                    var mapping = new DataColumnMapping { IndexNumber = pos, InputColumn = inputColumn, OutputColumn = new DbColumn { ColumnName = outputColumn, IsAutoIncrement = isAutoIncrement, IsNullable = isNullable, MaxLength = maxLength, SqlDataType = dataType } };
                    mappings.Add(mapping);
                    pos++;
                }

                dsfSqlBulkInsert.InputMappings = mappings;

                CommonSteps.AddActivityToActivityList(workflowName, activityName, dsfSqlBulkInsert);
                CommonSteps.AddVariableToVariableList(result);
            }
            else
            {
                throw new Exception("Invalid Source Name [ " + dbSrcName + " ]. Ensure it has been properly added to the DBSource directory in this project.");
            }
        }

        [Given(@"""(.*)"" contains an Sort ""(.*)"" as")]
        public void GivenContainsAnSortAs(string parentName, string activityName, Table table)
        {
            var dsfSort = new DsfSortRecordsActivity { DisplayName = activityName, SortField = table.Rows[0][0], SelectedSort = table.Rows[0][1] };
            CommonSteps.AddActivityToActivityList(parentName, activityName, dsfSort);
        }

        [Given(@"""(.*)"" contains an Delete ""(.*)"" as")]
        // ReSharper disable InconsistentNaming
        public void GivenContainsAnDeleteAs(string parentName, string activityName, Table table)
        // ReSharper restore InconsistentNaming
        {
            var del = new DsfPathDelete { InputPath = table.Rows[0][0], Result = table.Rows[0][1], DisplayName = activityName };
            CommonSteps.AddVariableToVariableList(table.Rows[0][1]);
            CommonSteps.AddActivityToActivityList(parentName, activityName, del);
        }

        [Given(@"""(.*)"" contains a Foreach ""(.*)"" as ""(.*)"" executions ""(.*)""")]
        public void GivenContainsAForeachAsExecutions(string parentName, string activityName, string numberOfExecutions, string executionCount)
        {
            enForEachType forEachType;
            Enum.TryParse(numberOfExecutions, true, out forEachType);
            var forEach = new DsfForEachActivity { DisplayName = activityName, ForEachType = forEachType };
            switch(forEachType)
            {
                case enForEachType.NumOfExecution:
                    forEach.NumOfExections = executionCount;
                    break;
                case enForEachType.InRecordset:
                    forEach.Recordset = executionCount;
                    break;
            }
            CommonSteps.AddActivityToActivityList(parentName, activityName, forEach);
            ScenarioContext.Current.Add(activityName, forEach);
        }

        [Given(@"""(.*)"" contains workflow ""(.*)"" with mapping as")]
        // ReSharper disable InconsistentNaming
        public void GivenContainsWorkflowWithMappingAs(string forEachName, string nestedWF, Table mappings)
        // ReSharper restore InconsistentNaming
        {
            var forEachAct = (DsfForEachActivity)ScenarioContext.Current[forEachName];
            IEnvironmentModel environmentModel = EnvironmentRepository.Instance.Source;
            environmentModel.Connect();
            environmentModel.LoadResources();
            var resource = environmentModel.ResourceRepository.Find(a => a.Category == @"Acceptance Testing Resources\"+nestedWF).FirstOrDefault();
            if(resource == null)
            {
                // ReSharper disable NotResolvedInText
                throw new ArgumentNullException("resource");
                // ReSharper restore NotResolvedInText
            }
            var dataMappingViewModel = GetDataMappingViewModel(resource, mappings);

            var inputMapping = dataMappingViewModel.GetInputString(dataMappingViewModel.Inputs);
            var outputMapping = dataMappingViewModel.GetOutputString(dataMappingViewModel.Outputs);
            var activity = new DsfActivity
            {

                ServiceName = resource.Category,
                ResourceID = resource.ID,
                EnvironmentID = environmentModel.ID,
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
            DsfFindRecordsMultipleCriteriaActivity act = new DsfFindRecordsMultipleCriteriaActivity { DisplayName = activityName, Result = result };
            foreach(var rule in table.Rows)
            {
                act.ResultsCollection.Add(new FindRecordsTO(rule[4], rule[3], 0));
                act.FieldsToSearch = String.IsNullOrEmpty(act.FieldsToSearch) ? rule[1] : "," + rule[1];
                act.RequireAllFieldsToMatch = rule[5].ToUpper().Trim() == "YES";
                act.RequireAllTrue = rule[6].ToUpper().Trim() == "YES";
            }
            CommonSteps.AddActivityToActivityList(parentName, activityName, act);
        }


        [Given(@"""(.*)"" contains Length ""(.*)"" on ""(.*)"" into ""(.*)""")]
        public void GivenContainsLengthOnInto(string parentName, string activityName, string recordSet, string result)
        {
            DsfRecordsetLengthActivity len = new DsfRecordsetLengthActivity { DisplayName = activityName, RecordsLength = result, RecordsetName = recordSet };
            CommonSteps.AddActivityToActivityList(parentName, activityName, len);
        }


        public void ExecuteWorkflow(IContextualResourceModel resourceModel)
        {
            if(resourceModel == null || resourceModel.Environment == null)
            {
                return;
            }

            var debugTo = new DebugTO { XmlData = "<DataList></DataList>", SessionID = Guid.NewGuid(), IsDebugMode = true };
            EnsureEnvironmentConnected(resourceModel.Environment, EnvironmentConnectionTimeout);
            var clientContext = resourceModel.Environment.Connection;
            if(clientContext != null)
            {
                var dataList = XElement.Parse(debugTo.XmlData);
                dataList.Add(new XElement("BDSDebugMode", debugTo.IsDebugMode));
                dataList.Add(new XElement("DebugSessionID", debugTo.SessionID));
                dataList.Add(new XElement("EnvironmentID", resourceModel.Environment.ID));
                WebServer.Send(resourceModel, dataList.ToString(), new SynchronousAsyncWorker());
                _resetEvt.WaitOne(120000);
            }
        }
        [When(@"workflow ""(.*)"" is saved ""(.*)"" time")]
        public void WhenWorkflowIsSavedTime(string workflowName, int count)
        {
            Guid id;
            TryGetValue("SavedId", out id);
            if(id == Guid.Empty)
            {
                id = Guid.NewGuid();
                ScenarioContext.Current.Add("SavedId", id);

            }
            Save(workflowName, count, id);
        }

        void Save(string workflowName, int count, Guid id)
        {
            BuildDataList();

            var activityList = CommonSteps.GetActivityList();

            var flowSteps = new List<FlowStep>();

            TestStartNode = new FlowStep();
            flowSteps.Add(TestStartNode);

            foreach(var activity in activityList)
            {
                if(TestStartNode.Action == null)
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

            IContextualResourceModel resourceModel;
            IEnvironmentModel environmentModel;
            IResourceRepository repository;
            TryGetValue(workflowName, out resourceModel);
            TryGetValue("environment", out environmentModel);
            TryGetValue("resourceRepo", out repository);

            string currentDl = CurrentDl;
            resourceModel.DataList = currentDl.Replace("root", "DataList");
            WorkflowHelper helper = new WorkflowHelper();
            StringBuilder xamlDefinition = helper.GetXamlDefinition(FlowchartActivityBuilder);
            resourceModel.WorkflowXaml = xamlDefinition;
            resourceModel.ID = id;

            for(int i = 0; i < count; i++)
            {
                repository.Save(resourceModel, false);
                repository.SaveToServer(resourceModel);
            }

        }

        [Then(@"workflow ""(.*)"" has ""(.*)"" Versions in explorer")]
        public void ThenWorkflowHasVersionsInExplorer(string workflowName, int numberOfVersions)
        {
            Guid id;
            TryGetValue("SavedId", out id);
            IContextualResourceModel resourceModel;
            IEnvironmentModel environmentModel;
            IResourceRepository repository;
            TryGetValue(workflowName, out resourceModel);
            TryGetValue("environment", out environmentModel);
            TryGetValue("resourceRepo", out repository);
            IVersionRepository rep = new ServerExplorerVersionProxy(environmentModel.Connection);
            var versions = rep.GetVersions(id);
            ScenarioContext.Current["Versions"] = versions;
            Assert.AreEqual(numberOfVersions, versions.Count);
        }

        [Then(@"explorer as")]
        public void ThenExplorerAs(Table table)
        {
            var versions = ScenarioContext.Current["Versions"] as IList<IExplorerItem>;
            if(versions == null || versions.Count == table.RowCount)
                Assert.Fail("InvalidVersions");
            else
            {
                for(int i = 0; i < versions.Count; i++)
                {
                    var v1 = table.Rows[i + 1][0].Split(' ');
                    Assert.IsTrue(versions[i].DisplayName.Contains(v1[0]));

                }
            }

        }

        [Given(@"""(.*)"" contains an Assign ""(.*)"" as")]
        [Then(@"""(.*)"" contains an Assign ""(.*)"" as")]
        public void ThenContainsAnAssignAs(string parentName, string assignName, Table table)
        {
            DsfMultiAssignActivity assignActivity = new DsfMultiAssignActivity { DisplayName = assignName };

            foreach(var tableRow in table.Rows)
            {
                var value = tableRow["value"];
                var variable = tableRow["variable"];

                value = value.Replace('"', ' ').Trim();

                if(value.StartsWith("="))
                {
                    value = value.Replace("=", "");
                    value = string.Format("!~calculation~!{0}!~~calculation~!", value);
                }

                List<ActivityDTO> fieldCollection;
                ScenarioContext.Current.TryGetValue("fieldCollection", out fieldCollection);

                CommonSteps.AddVariableToVariableList(variable);

                assignActivity.FieldsCollection.Add(new ActivityDTO(variable, value, 1, true));
            }
            CommonSteps.AddActivityToActivityList(parentName, assignName, assignActivity);
        }

        [When(@"I rollback ""(.*)"" to version ""(.*)""")]
        public void WhenIRollbackToVersion(string workflowName, string version)
        {
            Guid id;
            TryGetValue("SavedId", out id);
            IContextualResourceModel resourceModel;
            IEnvironmentModel environmentModel;
            IResourceRepository repository;
            TryGetValue(workflowName, out resourceModel);
            TryGetValue("environment", out environmentModel);
            TryGetValue("resourceRepo", out repository);
            IVersionRepository rep = new ServerExplorerVersionProxy(environmentModel.Connection);
            rep.RollbackTo(id, version);
        }

        [Then(@"the ""(.*)"" in Workflow ""(.*)"" debug outputs does not exist\|")]
        public void ThenTheInWorkflowDebugOutputsDoesNotExist(string workflowName, string version)
        {

            Dictionary<string, Activity> activityList;
            string parentWorkflowName;
            TryGetValue("activityList", out activityList);
            TryGetValue("parentWorkflowName", out parentWorkflowName);

            var debugStates = Get<List<IDebugState>>("debugStates");
            var workflowId = debugStates.First(wf => wf.DisplayName.Equals(workflowName)).ID;

            if(parentWorkflowName == workflowName)
            {
                workflowId = Guid.Empty;
            }

            var toolSpecificDebug =
                debugStates.Where(ds => ds.ParentID == workflowId && ds.DisplayName.Equals(workflowName)).ToList();
            Assert.AreEqual(0, toolSpecificDebug.Count);
        }


        [When(@"""(.*)"" is executed without saving")]
        public void WhenIsExecutedWithoutSaving(string workflowName)
        {
            IContextualResourceModel resourceModel;
            IEnvironmentModel environmentModel;
            IResourceRepository repository;
            TryGetValue(workflowName, out resourceModel);
            TryGetValue("environment", out environmentModel);
            TryGetValue("resourceRepo", out repository);


            var debugStates = Get<List<IDebugState>>("debugStates");
            debugStates.Clear();

            ExecuteWorkflow(resourceModel);
        }


        [AfterScenario]
        public void CleanUp()
        {
            if(_debugWriterSubscriptionService != null)
            {
                _debugWriterSubscriptionService.Unsubscribe();
                _debugWriterSubscriptionService.Dispose();
            }
            if(_resetEvt != null)
            {
                _resetEvt.Close();
            }
        }

        [Then(@"I set logging to ""(.*)""")]
        public void ThenISetLoggingTo(string logLevel)
        {
            var allowedLogLevels = new []{"DEBUG","NONE"};
            // TODO: refactor null empty checking into extension method
            if (logLevel == null || 
                !allowedLogLevels.Contains(logLevel = logLevel.ToUpper()))
                return;

            var loggingSettingsTo = new LoggingSettingsTo { FileLoggerLogLevel = logLevel,EventLogLoggerLogLevel = logLevel, FileLoggerLogSize = 200};
            var controller = new CommunicationControllerFactory().CreateController("LoggingSettingsWriteService");
            var serializer = new Dev2JsonSerializer();
            controller.AddPayloadArgument("LoggingSettings", serializer.SerializeToBuilder(loggingSettingsTo).ToString());
            IEnvironmentModel environmentModel;
            TryGetValue("environment", out environmentModel);
            controller.ExecuteCommand<StringBuilder>(environmentModel.Connection, Guid.Empty);
        }

        [When(@"""(.*)"" is executed ""(.*)""")]
        public void WhenIsExecuted(string workflowName, string executionLabel)
        {
            Stopwatch st = new Stopwatch();
            st.Start();
            WhenIsExecuted(workflowName);
            ScenarioContext.Current.Add(executionLabel, st.ElapsedMilliseconds);
        }

        [Then(@"the delta between ""(.*)"" and ""(.*)"" is less than ""(.*)"" milliseconds")]
        public void ThenTheDeltaBetweenAndIsLessThanMilliseconds(string executionLabelFirst, string executionLabelSecond, int maxDeltaMilliseconds)
        {
            int e1 = Convert.ToInt32(ScenarioContext.Current[executionLabelFirst]),
                e2 = Convert.ToInt32(ScenarioContext.Current[executionLabelSecond]),
                d = maxDeltaMilliseconds;
            d.Should().BeGreaterThan(Math.Abs(e1 - e2), string.Format("async logging should not add more than {0} milliseconds to the execution", d));
        }

        //Steps from tools

        [Given(@"""(.*)"" contains an Assign ""(.*)"" as")]
        public void GivenContainsAnAssignAs(string parentName, string assignName, Table table)
        {
            DsfMultiAssignActivity assignActivity = new DsfMultiAssignActivity { DisplayName = assignName };

            foreach (var tableRow in table.Rows)
            {
                var value = tableRow["value"];
                var variable = tableRow["variable"];

                value = value.Replace('"', ' ').Trim();

                if (value.StartsWith("="))
                {
                    value = value.Replace("=", "");
                    value = string.Format("!~calculation~!{0}!~~calculation~!", value);
                }

                List<ActivityDTO> fieldCollection;
                ScenarioContext.Current.TryGetValue("fieldCollection", out fieldCollection);

                CommonSteps.AddVariableToVariableList(variable);

                assignActivity.FieldsCollection.Add(new ActivityDTO(variable, value, 1, true));
            }
            CommonSteps.AddActivityToActivityList(parentName, assignName, assignActivity);
        }

        [Given(@"""(.*)"" contains an Unique ""(.*)"" as")]
        public void GivenContainsAnUniqueAs(string parentName, string activityName, Table table)
        {
            DsfUniqueActivity activity = new DsfUniqueActivity { DisplayName = activityName };
            foreach (var tableRow in table.Rows)
            {
                var inFields = tableRow["In Field(s)"];
                var returnFields = tableRow["Return Fields"];
                var result = tableRow["Result"];


                CommonSteps.AddVariableToVariableList(result);

                activity.Result = result;
                activity.ResultFields = returnFields;
                activity.InFields = inFields;
            }
            CommonSteps.AddActivityToActivityList(parentName, activityName, activity);
        }

        [Given(@"""(.*)"" contains Calculate ""(.*)"" with formula ""(.*)"" into ""(.*)""")]
        public void GivenCalculateWithFormulaInto(string parentName, string activityName, string formula, string resultVariable)
        {
            CommonSteps.AddVariableToVariableList(resultVariable);

            DsfCalculateActivity calculateActivity = new DsfCalculateActivity { Expression = formula, Result = resultVariable, DisplayName = activityName };

            CommonSteps.AddActivityToActivityList(parentName, activityName, calculateActivity);

        }

        [Given(@"""(.*)"" contains Count Record ""(.*)"" on ""(.*)"" into ""(.*)""")]
        public void GivenCountOnInto(string parentName, string activityName, string recordSet, string result)
        {
            CommonSteps.AddVariableToVariableList(result);

            DsfCountRecordsetActivity countRecordsetActivity = new DsfCountRecordsetActivity { CountNumber = result, RecordsetName = recordSet, DisplayName = activityName };

            CommonSteps.AddActivityToActivityList(parentName, activityName, countRecordsetActivity);
        }

        [Given(@"""(.*)"" contains Delete ""(.*)"" as")]
        public void GivenItContainsDeleteAs(string parentName, string activityName, Table table)
        {
            DsfDeleteRecordActivity activity = new DsfDeleteRecordActivity { DisplayName = activityName };
            foreach (var tableRow in table.Rows)
            {
                var result = tableRow["result"];
                var variable = tableRow["Variable"];

                CommonSteps.AddVariableToVariableList(result);
                activity.RecordsetName = variable;
                activity.Result = result;
            }

            CommonSteps.AddActivityToActivityList(parentName, activityName, activity);
        }

        [Given(@"""(.*)"" contains Find Record Index ""(.*)"" search ""(.*)"" and result ""(.*)"" as")]
        public void GivenItContainsFindRecordIndexSearchAndResultAs(string parentName, string activityName, string recsetToSearch, string resultVariable, Table table)
        {
            var result = resultVariable;
            var recset = recsetToSearch;
            CommonSteps.AddVariableToVariableList(result);
            DsfFindRecordsMultipleCriteriaActivity activity = new DsfFindRecordsMultipleCriteriaActivity { RequireAllFieldsToMatch = false, RequireAllTrue = false, Result = result, FieldsToSearch = recset, DisplayName = activityName };
            foreach (var tableRow in table.Rows)
            {
                var matchType = tableRow["Match Type"];
                var matchValue = tableRow["Match"];

                activity.ResultsCollection.Add(new FindRecordsTO(matchValue, matchType, 1, true));
            }

            CommonSteps.AddActivityToActivityList(parentName, activityName, activity);
        }

        [Given(@"""(.*)"" contains find unique ""(.*)"" as")]
        public void GivenItContainFindUniqueAs(string parentName, string activityName, Table table)
        {
            DsfUniqueActivity activity = new DsfUniqueActivity { DisplayName = activityName };
            foreach (var tableRow in table.Rows)
            {
                var inFields = tableRow["In Fields"];
                var returnFields = tableRow["Return Fields"];
                var result = tableRow["Result"];

                activity.InFields = inFields;
                activity.ResultFields = returnFields;
                activity.Result = result;

                CommonSteps.AddVariableToVariableList(result);
            }
            CommonSteps.AddActivityToActivityList(parentName, activityName, activity);
        }

        [Given(@"""(.*)"" contains case convert ""(.*)"" as")]
        public void GivenItContainsCaseConvertAs(string parentName, string activityName, Table table)
        {
            DsfCaseConvertActivity activity = new DsfCaseConvertActivity { DisplayName = activityName };
            foreach (var tableRow in table.Rows)
            {
                var variableToConvert = tableRow["Variable"];
                var conversionType = tableRow["Type"];

                activity.ConvertCollection.Add(new CaseConvertTO(variableToConvert, conversionType, variableToConvert, 1, true));
            }

            CommonSteps.AddActivityToActivityList(parentName, activityName, activity);
        }

        [Given(@"""(.*)"" contains Gather System Info ""(.*)"" as")]
        public void GivenItContainsGatherSystemInfoAs(string parentName, string activityName, Table table)
        {
            var activity = new DsfGatherSystemInformationActivity { DisplayName = activityName };
            foreach (var tableRow in table.Rows)
            {
                var variable = tableRow["Variable"];

                CommonSteps.AddVariableToVariableList(variable);

                enTypeOfSystemInformationToGather systemInfo = (enTypeOfSystemInformationToGather)Dev2EnumConverter.GetEnumFromStringDiscription(tableRow["Selected"], typeof(enTypeOfSystemInformationToGather));
                activity.SystemInformationCollection.Add(new GatherSystemInformationTO(systemInfo, variable, 1));
            }

            CommonSteps.AddActivityToActivityList(parentName, activityName, activity);
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

                CommonSteps.AddVariableToVariableList(result);

                activity.RandomType = type;
                activity.To = to;
                activity.From = from;
                activity.Result = result;

            }

            CommonSteps.AddActivityToActivityList(parentName, activityName, activity);
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

                CommonSteps.AddVariableToVariableList(result);

                activity.Expression = number;
                activity.RoundingType = roundingType;
                activity.RoundingDecimalPlaces = roundTo;
                activity.DecimalPlacesToShow = decimalPlacesToShow;
                activity.Result = result;

            }

            CommonSteps.AddActivityToActivityList(parentName, activityName, activity);
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

                CommonSteps.AddVariableToVariableList(result);

                activity.DateTime = input1;
                activity.InputFormat = inputFormat;
                activity.OutputFormat = outputFormat;
                activity.TimeModifierAmountDisplay = timeModifierAmount;
                activity.TimeModifierType = "Years";
                activity.Result = result;

            }

            CommonSteps.AddActivityToActivityList(parentName, activityName, activity);
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

                CommonSteps.AddVariableToVariableList(result);

                activity.Input1 = input1;
                activity.Input2 = input2;
                activity.InputFormat = inputFormat;
                activity.OutputType = output;
                activity.Result = result;

            }

            CommonSteps.AddActivityToActivityList(parentName, activityName, activity);
        }


        [Given(@"""(.*)"" contains Data Split ""(.*)"" as")]
        public void GivenItContainsDataSplitAs(string parentName, string activityName, Table table)
        {
            DsfDataSplitActivity activity = new DsfDataSplitActivity { DisplayName = activityName };
            foreach (var tableRow in table.Rows)
            {
                var valueToSplit = string.IsNullOrEmpty(tableRow["String"]) ? "" : tableRow["String"];
                var variable = tableRow["Variable"];
                var type = tableRow["Type"];
                var at = tableRow["At"];
                var include = tableRow["Include"] == "Selected";
                //var escapeChar = tableRow["Escape"];
                CommonSteps.AddVariableToVariableList(variable);
                if (!string.IsNullOrEmpty(valueToSplit))
                {
                    activity.SourceString = valueToSplit;
                }
                CommonSteps.AddVariableToVariableList(variable);
                activity.ResultsCollection.Add(new DataSplitDTO(variable, type, at, 1, include, true));
            }

            CommonSteps.AddActivityToActivityList(parentName, activityName, activity);
        }

        [Given(@"""(.*)"" contains Replace ""(.*)"" into ""(.*)"" as")]
        public void GivenItContainsReplaceIntoAs(string parentName, string activityName, string resultVariable, Table table)
        {
            DsfReplaceActivity activity = new DsfReplaceActivity { Result = resultVariable, DisplayName = activityName };
            foreach (var tableRow in table.Rows)
            {
                var variable = tableRow["In Fields"];
                var find = tableRow["Find"];
                var replaceValue = tableRow["Replace With"];

                activity.FieldsToSearch = variable;
                activity.Find = find;
                activity.ReplaceWith = replaceValue;
            }
            CommonSteps.AddVariableToVariableList(resultVariable);
            CommonSteps.AddActivityToActivityList(parentName, activityName, activity);
        }


        [Given(@"""(.*)"" contains Find Index ""(.*)"" into ""(.*)"" as")]
        public void GivenItContainsFindIndexIntoAs(string parentName, string activityName, string resultVariable, Table table)
        {
            DsfIndexActivity activity = new DsfIndexActivity { Result = resultVariable, DisplayName = activityName };
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
            CommonSteps.AddVariableToVariableList(resultVariable);
            CommonSteps.AddActivityToActivityList(parentName, activityName, activity);
        }

        [Given(@"""(.*)"" contains an Create ""(.*)"" as")]
        public void GivenContainsAnCreateAs(string parentName, string activityName, Table table)
        {
            DsfPathCreate activity = new DsfPathCreate { DisplayName = activityName };
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

                CommonSteps.AddVariableToVariableList(result);
            }
            CommonSteps.AddActivityToActivityList(parentName, activityName, activity);
        }


        [Given(@"""(.*)"" contains an Delete Folder ""(.*)"" as")]
        public void GivenContainsAnDeleteFolderAs(string parentName, string activityName, Table table)
        {
            DsfPathDelete activity = new DsfPathDelete { DisplayName = activityName };
            foreach (var tableRow in table.Rows)
            {
                var variable = tableRow["Recordset"];
                //var userName = tableRow["Username"];
                //var password = tableRow["Password"];
                var result = tableRow["Result"];

                activity.Result = result;
                activity.InputPath = variable;
                //activity.Username = userName;
                //activity.Password = password;

                CommonSteps.AddVariableToVariableList(result);
            }
            CommonSteps.AddActivityToActivityList(parentName, activityName, activity);
        }

        [Given(@"""(.*)"" contains Data Merge ""(.*)"" into ""(.*)"" as")]
        public void GivenItContainsDataMergeAs(string parentName, string activityName, string resultVariable, Table table)
        {
            DsfDataMergeActivity activity = new DsfDataMergeActivity { Result = resultVariable, DisplayName = activityName };
            foreach (var tableRow in table.Rows)
            {
                var variable = tableRow["Variable"];
                var type = tableRow["Type"];
                var at = tableRow["Using"];
                var padding = tableRow["Padding"];
                var alignment = tableRow["Alignment"];

                activity.MergeCollection.Add(new DataMergeDTO(variable, type, at, 1, padding, alignment, true));
            }
            CommonSteps.AddVariableToVariableList(resultVariable);
            CommonSteps.AddActivityToActivityList(parentName, activityName, activity);
        }

        [Given(@"""(.*)"" contains Base convert ""(.*)"" as")]
        public void GivenItContainsBaseConvertAs(string parentName, string activityName, Table table)
        {
            DsfBaseConvertActivity activity = new DsfBaseConvertActivity { DisplayName = activityName };
            foreach (var tableRow in table.Rows)
            {
                var variableToConvert = tableRow["Variable"];
                var from = tableRow["From"];
                var to = tableRow["To"];

                activity.ConvertCollection.Add(new BaseConvertTO(variableToConvert, from, to, variableToConvert, 1, true));
            }

            CommonSteps.AddActivityToActivityList(parentName, activityName, activity);
        }


        [Given(@"""(.*)"" contains an Unique ""(.*)"" as")]
        public void GivenContainsAnUniqueAs(string parentName, string activityName, Table table)
        {
            throw new NotImplementedException("This step definition is not yet implemented and is required for this test to pass. - Ashley");
        }

        [Given(@"""(.*)"" contains Count Record ""(.*)"" on ""(.*)"" into ""(.*)""")]
        public void GivenContainsCountRecordOnInto(string parentName, string activityName, string recordsetName, string resultVariable)
        {
            var dsfCountRecord = new DsfCountRecordsetActivity { DisplayName = activityName, RecordsetName = recordsetName, CurrentResult = resultVariable};
            CommonSteps.AddActivityToActivityList(parentName, activityName, dsfCountRecord);
        }

        [Given(@"""(.*)"" contains case convert ""(.*)"" as")]
        public void GivenContainsCaseConvertAs(string parentName, string activityName, Table table)
        {
            throw new NotImplementedException("This step definition is not yet implemented and is required for this test to pass. - Ashley");
        }

        [Given(@"""(.*)"" contains Base convert ""(.*)"" as")]
        public void GivenContainsBaseConvertAs(string parentName, string activityName, Table table)
        {
            throw new NotImplementedException("This step definition is not yet implemented and is required for this test to pass. - Ashley");
        }

        [Given(@"""(.*)"" contains Calculate ""(.*)"" with formula ""(.*)"" into ""(.*)""")]
        public void GivenContainsCalculateWithFormulaInto(string parentName, string activityName, string formula, string resultVariable)
        {
            var dsfCalculate = new DsfCalculateActivity { DisplayName = activityName, Expression = formula, CurrentResult = resultVariable};
            CommonSteps.AddActivityToActivityList(parentName, activityName, dsfCalculate);
        }

        [Given(@"""(.*)"" contains Find Index ""(.*)"" into ""(.*)"" as")]
        public void GivenContainsFindIndexIntoAs(string parentName, string activityName, string p2, Table table)
        {
            throw new NotImplementedException("This step definition is not yet implemented and is required for this test to pass. - Ashley");
        }

        [Given(@"""(.*)"" contains Data Merge ""(.*)"" into ""(.*)"" as")]
        public void GivenContainsDataMergeIntoAs(string parentName, string activityName, string p2, Table table)
        {
            throw new NotImplementedException("This step definition is not yet implemented and is required for this test to pass. - Ashley");
        }

        [Given(@"""(.*)"" contains Date and Time ""(.*)"" as")]
        public void GivenContainsDateAndTimeAs(string parentName, string activityName, Table table)
        {
            throw new NotImplementedException("This step definition is not yet implemented and is required for this test to pass. - Ashley");
        }

        [Given(@"""(.*)"" contains Date and Time Difference ""(.*)"" as")]
        public void GivenContainsDateAndTimeDifferenceAs(string parentName, string activityName, Table table)
        {
            throw new NotImplementedException("This step definition is not yet implemented and is required for this test to pass. - Ashley");
        }

        [Given(@"""(.*)"" contains Format Number ""(.*)"" as")]
        public void GivenContainsFormatNumberAs(string parentName, string activityName, Table table)
        {
            throw new NotImplementedException("This step definition is not yet implemented and is required for this test to pass. - Ashley");
        }

        [Given(@"""(.*)"" contains Random ""(.*)"" as")]
        public void GivenContainsRandomAs(string parentName, string activityName, Table table)
        {
            throw new NotImplementedException("This step definition is not yet implemented and is required for this test to pass. - Ashley");
        }

        [Given(@"""(.*)"" contains Data Split ""(.*)"" as")]
        public void GivenContainsDataSplitAs(string parentName, string activityName, Table table)
        {
            throw new NotImplementedException("This step definition is not yet implemented and is required for this test to pass. - Ashley");
        }

        [Given(@"""(.*)"" contains Replace ""(.*)"" into ""(.*)"" as")]
        public void GivenContainsReplaceIntoAs(string parentName, string activityName, string p2, Table table)
        {
            throw new NotImplementedException("This step definition is not yet implemented and is required for this test to pass. - Ashley");
        }

        [Given(@"""(.*)"" contains Gather System Info ""(.*)"" as")]
        public void GivenContainsGatherSystemInfoAs(string parentName, string activityName, Table table)
        {
            throw new NotImplementedException("This step definition is not yet implemented and is required for this test to pass. - Ashley");
        }

        [Given(@"""(.*)"" contains Delete ""(.*)"" as")]
        public void GivenContainsDeleteAs(string parentName, string activityName, Table table)
        {
            throw new NotImplementedException("This step definition is not yet implemented and is required for this test to pass. - Ashley");
        }
    }
}
