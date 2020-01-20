using Dev2.Studio.Core;
using Dev2.Studio.Interfaces.DataList;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TechTalk.SpecFlow;
using Dev2.Common.Interfaces.DB;
using ActivityUnitTests;
using Dev2.Activities.Specs.BaseTypes;
using System;
using System.Xml.Linq;
using Dev2.Studio.Core.Models;
using Dev2.Data.Util;
using Dev2.Studio.Core.Models.DataList;
using Dev2.Studio.Interfaces;
using Dev2.Services;
using Dev2.Messages;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using System.Threading;
using WarewolfParserInterop;
using Dev2.Common;
using Dev2.Session;
using Dev2.Studio.Core.Network;
using Dev2.Threading;
using System.Activities.Statements;
using Dev2.Utilities;
using Dev2.Studio.Interfaces.Enums;
using Dev2.Controller;
using Warewolf.Studio.ViewModels;
using Dev2.Activities;
using System.Activities;
using Dev2.Common.Interfaces.ServerProxyLayer;

namespace Warewolf.Tools.Specs.Toolbox.Database
{
    [Binding]
    public class DatabaseToolsSteps : BaseActivityUnitTest
    {
        readonly ScenarioContext _scenarioContext;
        SubscriptionService<DebugWriterWriteMessage> _debugWriterSubscriptionService;
        readonly AutoResetEvent _resetEvt = new AutoResetEvent(false);
        StudioServerProxy _proxyLayer;
        readonly CommonSteps _commonSteps;
        const int EnvironmentConnectionTimeout = 15;

        public DatabaseToolsSteps(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext ?? throw new ArgumentNullException("scenarioContext");
            _commonSteps = new CommonSteps(_scenarioContext);            
            TryADD("server", ServerRepository.Instance.Source);
            TryADD("debugStates", new List<IDebugState>());
            TryADD("resourceRepo", ServerRepository.Instance.Source.ResourceRepository);
        }
       
        public void WorkflowIsExecuted(string workflowName)
        {
            var resourceModel = SaveAWorkflow(workflowName);
            ExecuteWorkflow(resourceModel);
        }

        public void ValidateErrorsAfterExecution(string workflowName, string hasError, string error)
        {
            TryGetValue("activityList", out Dictionary<string, Activity> activityList);
            var debugStates = Get<List<IDebugState>>("debugStates").ToList();

            if (hasError == "AN")
            {
                var innerWfHasErrorState = debugStates.FirstOrDefault(state => state.HasError && state.DisplayName.Equals(workflowName));
                Assert.IsNotNull(innerWfHasErrorState);
                if (!string.IsNullOrEmpty(error))
                {
                    var allErrors = string.Empty;
                    foreach (var item in debugStates)
                    {
                        allErrors = item.ErrorMessage + Environment.NewLine;
                    }
                    Assert.IsTrue(debugStates.Any(p => p.ErrorMessage.Contains(error)), error + " : Did not occur. But " + allErrors + "occured");
                }
            }
            else
            {
                debugStates.ForEach(p => Assert.IsFalse(p.HasError, "Error: "+ p.ErrorMessage));
            }
        }
        
        public void CleanupForTimeOutSpecs()
        {
            if (_debugWriterSubscriptionService != null)
            {
                _debugWriterSubscriptionService.Unsubscribe();
                _debugWriterSubscriptionService.Dispose();
            }
            _resetEvt?.Close();
            var environmentModel = _scenarioContext.Get<IServer>("server");
            var resourceModel = _scenarioContext.Get<ResourceModel>("resourceModel");
            environmentModel?.ResourceRepository?.DeleteResource(resourceModel);
        }

        [BeforeScenario("@OpeningSavedWorkflowWithPostgresServerTool", "@ChangeTheSourceOnExistingPostgresql", "@ChangeTheActionOnExistingPostgresql", "@ChangeTheRecordsetOnExistingPostgresqlTool", "@ChangingSqlServerFunctions", "@CreatingOracleToolInstance", "@ChangingOracleActions")]
        public void InitChangingFunction()
        {
            var mock = new Mock<IDataListViewModel>();
            mock.Setup(model => model.ScalarCollection).Returns(new ObservableCollection<IScalarItemModel>());
            if (DataListSingleton.ActiveDataList == null)
            {
                DataListSingleton.SetDataList(mock.Object);
            }
        }

        public void CreateNewResourceModel(string workflowName, IServer environmentModel)
        {
            var resourceModel = new ResourceModel(environmentModel)
            {
                ID = Guid.NewGuid(),
                ResourceName = workflowName,
                Category = "Acceptance Tests\\" + workflowName,
                ResourceType = ResourceType.WorkflowService
            };
            environmentModel.ResourceRepository.Add(resourceModel);
            _scenarioContext.Add("resourceModel", resourceModel);
        }

        public void CreateDBServiceModel(IServer environmentModel)
        {
            var environmentConnection = environmentModel.Connection;
            var controllerFactory = new CommunicationControllerFactory();
            _proxyLayer = new StudioServerProxy(controllerFactory, environmentConnection);
            var mock = new Mock<IShellViewModel>();
            var dbServiceModel = new ManageDbServiceModel(new StudioResourceUpdateManager(controllerFactory, environmentConnection)
                                                                                    , _proxyLayer.QueryManagerProxy
                                                                                    , mock.Object
                                                                                    , environmentModel);
            _scenarioContext.Add("dbServiceModel", dbServiceModel);
            _scenarioContext.Add("proxyLayer", _proxyLayer);
        }

        public static void AssertAgainstServiceInputs(Table table, ICollection<IServiceInput> inputs)
        {
            var rowNum = 0;
            foreach (var row in table.Rows)
            {
                var inputValue = row["Input"];
                var value = row["Value"];
                var serviceInputs = inputs.ToList();
                var serviceInput = serviceInputs[rowNum];
                Assert.AreEqual(inputValue, serviceInput.Name);
                Assert.AreEqual(value, serviceInput.Value);
                rowNum++;
            }
        }

        public IContextualResourceModel SaveAWorkflow(string workflowName)
        {
            Get<List<IDebugState>>("debugStates").Clear();
            BuildShapeAndTestData();

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
            TryGetValue("resourceModel", out IContextualResourceModel resourceModel);
            TryGetValue("server", out IServer server);
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

        void EnsureEnvironmentConnected(IServer server, int timeout)
        {
            var startTime = DateTime.UtcNow;
            server.Connect();


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
                _scenarioContext.Add("ConnectTimeoutCountdown", timeout);
                throw new TimeoutException("Connection to Warewolf server \"" + server.Name + "\" timed out.");
            }
        }
        public void DebugWriterSubscribe(IServer environmentModel)
        {
            _debugWriterSubscriptionService = new SubscriptionService<DebugWriterWriteMessage>(environmentModel.Connection.ServerEvents);
            _debugWriterSubscriptionService.Subscribe(msg => Append(msg.DebugState));
        }
        void Append(IDebugState debugState)
        {
            TryGetValue("debugStates", out List<IDebugState> debugStates);
            TryGetValue("debugStatesDuration", out List<IDebugState> debugStatesDuration);
            TryGetValue("parentName", out string workflowName);
            TryGetValue("server", out IServer server);
            if (debugStatesDuration == null)
            {
                debugStatesDuration = new List<IDebugState>();
                Add("debugStatesDuration", debugStatesDuration);
            }
            if (debugState.WorkspaceID == server.Connection.WorkspaceID)
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


        public void SetDbSource(string activityName, IDbSource dbSource)
        {
            var activities = _commonSteps.GetActivityList();
            if (activityName.Contains("MySql"))
            {
                var activity = activities[activityName] as DsfMySqlDatabaseActivity;
                activity.SourceId = dbSource.Id;
            }
            if (activityName.Contains("SqlServer"))
            {
                var activity = activities[activityName] as DsfSqlServerDatabaseActivity;
                activity.SourceId = dbSource.Id;
            }
            if (activityName.Contains("Oracle"))
            {
                var activity = activities[activityName] as DsfOracleDatabaseActivity;
                activity.SourceId = dbSource.Id;
            }
            if (activityName.Contains("Postgre"))
            {
                var activity = activities[activityName] as DsfPostgreSqlActivity;
                activity.SourceId = dbSource.Id;
            }
        }

        public void SetDbAction(string activityName, string actionName)
        {
            var activities = _commonSteps.GetActivityList();
            if (activityName.Contains("MySql"))
            {
                var sqlactivity = activities[activityName] as DsfMySqlDatabaseActivity;
                Assert.IsNotNull(sqlactivity, "Activity is null");
                sqlactivity.ProcedureName = actionName;
            }
            if (activityName.Contains("SqlServer"))
            {
                var sqlactivity = activities[activityName] as DsfSqlServerDatabaseActivity;
                Assert.IsNotNull(sqlactivity, "Activity is null");
                sqlactivity.ProcedureName = actionName;
            }
            if (activityName.Contains("Oracle"))
            {
                var sqlactivity = activities[activityName] as DsfOracleDatabaseActivity;
                Assert.IsNotNull(sqlactivity, "Activity is null");
                sqlactivity.ProcedureName = actionName;
            }
            if (activityName.Contains("Postgre"))
            {
                var sqlactivity = activities[activityName] as DsfPostgreSqlActivity;
                Assert.IsNotNull(sqlactivity, "Activity is null");
                sqlactivity.ProcedureName = actionName;
            }
        }


        public void SetCommandTimeout(string activityName, int timeout)
        {
            var activities = _commonSteps.GetActivityList();
            if (activityName.Contains("MySql"))
            {
                var sqlactivity = activities[activityName] as DsfMySqlDatabaseActivity;
                Assert.IsNotNull(sqlactivity, "Activity is null");
                sqlactivity.CommandTimeout = timeout;
            }
            if (activityName.Contains("SqlServer"))
            {
                var sqlactivity = activities[activityName] as DsfSqlServerDatabaseActivity;
                Assert.IsNotNull(sqlactivity, "Activity is null");
                sqlactivity.CommandTimeout = timeout;
            }
            if (activityName.Contains("Oracle"))
            {
                var sqlactivity = activities[activityName] as DsfOracleDatabaseActivity;
                Assert.IsNotNull(sqlactivity, "Activity is null");
                sqlactivity.CommandTimeout = timeout;
            }
            if (activityName.Contains("Postgres"))
            {
                var sqlactivity = activities[activityName] as DsfPostgreSqlActivity;
                Assert.IsNotNull(sqlactivity, "Activity is null");
                sqlactivity.CommandTimeout = timeout;
            }
        }


        void Add(string key, object value) => _scenarioContext.Add(key, value);
        public T Get<T>(string keyName)
        {
            return _scenarioContext.Get<T>(keyName);
        }
        public void TryGetValue<T>(string keyName, out T value)
        {
            _scenarioContext.TryGetValue(keyName, out value);
        }
        public void TryADD<T>(string keyName, T value)
        {
            var val = value;
            if (!_scenarioContext.TryGetValue(keyName, out value))
            {                
                _scenarioContext.Add(keyName, val);
            }
        }
        
        protected void BuildShapeAndTestData()
        {
            var shape = new XElement("root");
            var data = new XElement("root");
            
            var row = 0;
            _scenarioContext.TryGetValue("variableList", out dynamic variableList);
            if (variableList != null)
            {
                try
                {
                    foreach (dynamic variable in variableList)
                    {
                        var variableName = DataListUtil.AddBracketsToValueIfNotExist(variable.Item1);
                        if (!string.IsNullOrEmpty(variable.Item1) && !string.IsNullOrEmpty(variable.Item2))
                        {
                            string value = variable.Item2 == "blank" ? "" : variable.Item2;
                            if (value.ToUpper() == "NULL")
                            {
                                DataObject.Environment.AssignDataShape(variable.Item1);
                            }
                            else
                            {
                                DataObject.Environment.Assign(variableName, value, 0);
                            }
                        }
                        if (DataListUtil.IsValueScalar(variableName))
                        {
                            var scalarName = DataListUtil.RemoveLanguageBrackets(variableName);
                            var scalarItemModel = new ScalarItemModel(scalarName);
                            if (!scalarItemModel.HasError)
                            {
                                DataListSingleton.ActiveDataList.Add(scalarItemModel);
                            }
                        }
                        if (DataListUtil.IsValueRecordsetWithFields(variableName))
                        {
                            var rsName = DataListUtil.ExtractRecordsetNameFromValue(variableName);
                            var fieldName = DataListUtil.ExtractFieldNameOnlyFromValue(variableName);
                            var rs = DataListSingleton.ActiveDataList.RecsetCollection.FirstOrDefault(model => model.Name == rsName);
                            if (rs == null)
                            {
                                var recordSetItemModel = new RecordSetItemModel(rsName);
                                DataListSingleton.ActiveDataList.Add(recordSetItemModel);
                                recordSetItemModel.Children.Add(new RecordSetFieldItemModel(fieldName,
                                    recordSetItemModel));
                            }
                            else
                            {
                                var recordSetFieldItemModel = rs.Children.FirstOrDefault(model => model.Name == fieldName);
                                if (recordSetFieldItemModel == null)
                                {
                                    rs.Children.Add(new RecordSetFieldItemModel(fieldName, rs));
                                }
                            }
                        }
                        //Build(variable, shape, data, row)
                        row++;
                    }
                    DataListSingleton.ActiveDataList.WriteToResourceModel();
                }
                catch
                {
                    //Exception
                }
            }

            var isAdded = _scenarioContext.TryGetValue("rs", out List<Tuple<string, string>> emptyRecordset);
            if (isAdded)
            {
                foreach (Tuple<string, string> emptyRecord in emptyRecordset)
                {
                    DataObject.Environment.Assign(DataListUtil.AddBracketsToValueIfNotExist(emptyRecord.Item1), emptyRecord.Item2, 0);
                }
            }

            _scenarioContext.TryGetValue("objList", out dynamic objList);
            if (objList != null)
            {
                try
                {
                    foreach (dynamic variable in objList)
                    {
                        if (!string.IsNullOrEmpty(variable.Item1) && !string.IsNullOrEmpty(variable.Item2))
                        {
                            string value = variable.Item2 == "blank" ? "" : variable.Item2;
                            if (value.ToUpper() == "NULL")
                            {
                                DataObject.Environment.AssignDataShape(variable.Item1);
                            }
                            else
                            {
                                DataObject.Environment.AssignJson(new AssignValue(DataListUtil.AddBracketsToValueIfNotExist(variable.Item1), value), 0);
                            }
                        }
                    }
                }
                catch
                {
                    //Exception
                }
            }

            CurrentDl = shape.ToString();
            TestData = data.ToString();
        }
    }
}
