﻿using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Windows;
using Caliburn.Micro;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Hosting;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Communication;
using Dev2.Controller;
using Dev2.Data;
using Dev2.Data.Decisions.Operations;
using Dev2.Data.Interfaces.Enums;
using Dev2.Data.SystemTemplates.Models;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Data.Util;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Core.AppResources.Repositories;
using Dev2.Studio.Core.Models;
using Dev2.Studio.Core.Models.DataList;
using Dev2.Studio.Interfaces;
using Dev2.Studio.ViewModels.DataList;
using Dev2.Threading;
using Dev2.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Studio.ViewModels;
using Dev2.Studio.Interfaces.Enums;
using Dev2.Activities.Specs.BaseTypes;
using System.IO;
using Dev2.Common.Interfaces.Scheduler.Interfaces;

namespace Dev2.Activities.Specs.TestFramework
{
    [Binding]
    public class StudioTestFrameworkSteps
    {
        static IServer _environmentModel;
        const int EXPECTED_NUMBER_OF_RESOURCES = 105;
        public static IDirectoryHelper DirectoryHelperInstance()
        {
            return new DirectoryHelper();
        }        
        public StudioTestFrameworkSteps(ScenarioContext scenarioContext)
        {
            MyContext = scenarioContext ?? throw new ArgumentNullException(nameof(scenarioContext));
        }
       
        ScenarioContext MyContext { get; }

        [BeforeFeature("StudioTestFramework")]
        static void SetupFeature()
        {
            ConnectAndLoadServer();
            Assert.AreEqual(EXPECTED_NUMBER_OF_RESOURCES, _environmentModel.ResourceRepository.All().Count);
        }

        [AfterFeature("StudioTestFramework")]
        public static void ScenarioCleaning()
        {
            ((ResourceRepository)_environmentModel.ResourceRepository).DeleteAlltests(new List<string> { "0bdc3207-ff6b-4c01-a5eb-c7060222f75d" });
        }

        [BeforeScenario("StudioTestFramework")]
        public static void SetupScenario()
        {
            Assert.AreEqual(EXPECTED_NUMBER_OF_RESOURCES, _environmentModel.ResourceRepository.All().Count);
        }

        [AfterScenario("StudioTestFrameworkWithDropboxTools")]
        public static void DropboxScenarioCleaning()
        {
            if (ScenarioContext.Current.ContainsKey("localFileUniqueNameGuid"))
            {
                var localFileUniqueNameGuid = ScenarioContext.Current.Get<string>("localFileUniqueNameGuid");
                var localFile = "C:\\Home.Delete";
                if (File.Exists(localFile))
                {
                    File.Delete(localFile);
                }
                localFile = CommonSteps.AddGuidToPath(localFile, localFileUniqueNameGuid);
                if (File.Exists(localFile))
                {
                    File.Delete(localFile);
                }
            }
        }

        [AfterScenario("StudioTestFramework")]
        public void CleanupTestFramework()
        {
            if (_environmentModel == null)
            {
                ConnectAndLoadServer();
            }
            var allValues = MyContext.Values;
            foreach(var value in allValues)
            {
                if (value is ResourceModel resource)
                {
                    ((ResourceRepository)_environmentModel.ResourceRepository).DeleteResource(resource);
                }
            }
            if (MyContext.TryGetValue("testFramework", out ServiceTestViewModel serviceTest))
            {
                serviceTest?.Dispose();
            }
        }

        private static void ConnectAndLoadServer()
        {
            _environmentModel = ServerRepository.Instance.Source;
            _environmentModel.Connect();
            _environmentModel.ResourceRepository.Load(true);
        }

        [Given(@"test folder is cleaned")]
        [When(@"test folder is cleaned")]
        public void GivenTestFolderIsCleaned()
        {
            if (_environmentModel == null)
            {
                ConnectAndLoadServer();
            }

            DirectoryHelperInstance().CleanUp(EnvironmentVariables.TestPath);
            var commsController = new CommunicationController { ServiceName = "ReloadAllTests" };
            commsController.ExecuteCommand<ExecuteMessage>(_environmentModel.Connection, GlobalConstants.ServerWorkspaceID);
        }

        [When(@"I reload tests")]
        public void WhenIReloadTests()
        {
            var commsController = new CommunicationController { ServiceName = "ReloadAllTests" };
            commsController.ExecuteCommand<ExecuteMessage>(_environmentModel.Connection, GlobalConstants.ServerWorkspaceID);            
        }

        [Then(@"test folder is cleaned")]
        public void ThenTestFolderIsCleaned()
        {
            ((ResourceRepository)_environmentModel.ResourceRepository).DeleteAlltests(new List<string>() { "0bdc3207-ff6b-4c01-a5eb-c7060222f75d" });
        }

        FlowNode CreateFlowNode(Guid id, string displayName)
        {
            return new FlowStep
            {
                Action = new DsfMultiAssignActivity
                {
                    DisplayName = displayName,
                    UniqueID = id.ToString()
                }
            };
        }

        [Given(@"I have ""(.*)"" with inputs as")]
        public void GivenIHaveWithInputsAs(string workflowName, Table inputVariables)
        {
            var resourceModel = BuildResourceModel(workflowName, _environmentModel);
            MyContext.Add(workflowName + "Resourceid", resourceModel.ID);
            var workflowHelper = new WorkflowHelper();
            var builder = workflowHelper.CreateWorkflow(workflowName);
            if (workflowName.Equals("WorkflowWithTests", StringComparison.InvariantCultureIgnoreCase))
            {
                var flowNode = CreateFlowNode(resourceModel.ID, workflowName);
                ((Flowchart)builder.Implementation).StartNode = flowNode;
            }

            resourceModel.WorkflowXaml = workflowHelper.GetXamlDefinition(builder);

            var datalistViewModel = new DataListViewModel();
            datalistViewModel.InitializeDataListViewModel(resourceModel);
            foreach (var variablesRow in inputVariables.Rows)
            {
                AddVariables(variablesRow["Input Var Name"], datalistViewModel, enDev2ColumnArgumentDirection.Input);
            }
            datalistViewModel.WriteToResourceModel();
            MyContext.Add(workflowName, resourceModel);
            MyContext.Add($"{workflowName}dataListViewModel", datalistViewModel);
            if (!MyContext.ContainsKey("popupController"))
            {
                var popupController = new Mock<Common.Interfaces.Studio.Controller.IPopupController>();
                popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
                popupController.Setup(controller => controller.Show(It.IsAny<string>(), It.IsAny<string>(), MessageBoxButton.OK, MessageBoxImage.Error, null, false, true, false, false, false, false)).Verifiable();
                popupController.Setup(controller => controller.Show(It.IsAny<string>(), It.IsAny<string>(), MessageBoxButton.OK, MessageBoxImage.Information, null, false, true, false, false, false, false)).Verifiable();
                CustomContainer.Register(popupController.Object);
                MyContext["popupController"] = popupController;
            }
            var shellViewModel = new Mock<IShellViewModel>();
            shellViewModel.Setup(model => model.CloseResourceTestView(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>()));
            CustomContainer.Register(shellViewModel.Object);
            MyContext["shellViewModel"] = shellViewModel;
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

        readonly object _syncRoot = new object();
        const string SimpleJson = "{\"$type\":\"Dev2.Data.ServiceTestModelTO,Dev2.Data\",\"OldTestName\":null,\"TestName\":\"Test 1\",\"UserName\":null,\"Password\":null,\"LastRunDate\":\"0001-01-01T00:00:00\",\"Inputs\":null,\"Outputs\":null,\"NoErrorExpected\":false,\"ErrorExpected\":false,\"TestPassed\":false,\"TestFailing\":false,\"TestInvalid\":false,\"TestPending\":false,\"Enabled\":true,\"IsDirty\":false,\"AuthenticationType\":0,\"ResourceId\":\"00000000-0000-0000-0000-000000000000\"}";

        [Given(@"I have a resouce ""(.*)""")]
        public void GivenIHaveAResouce(string resourceName)
        {
            _resourceForTests = resourceName;
            var resourceId = Guid.NewGuid();

            var resourceModel = new ResourceModel(_environmentModel)
            {
                ResourceName = resourceName,
                DisplayName = resourceName,
                DataList = "",
                ID = resourceId,
                Category = resourceName
            };
            var workflowHelper = new WorkflowHelper();
            var builder = workflowHelper.CreateWorkflow(resourceName);
            resourceModel.WorkflowXaml = workflowHelper.GetXamlDefinition(builder);

            _environmentModel.ResourceRepository.SaveToServer(resourceModel);

            ScenarioContext.Current.Add(resourceName + "id", resourceId);
        }

        string _resourceForTests = "";

        [Given(@"I add ""(.*)"" as tests")]
        public void GivenIAddAsTests(string p0)
        {
            var serviceTestModelTos = new List<IServiceTestModelTO>();
            var savedSource = _environmentModel.ResourceRepository.All().First(model => model.ResourceName.Equals(_resourceForTests, StringComparison.InvariantCultureIgnoreCase));
            MyContext["PluginSource" + "id"] = savedSource.ID;

            var resourceID = MyContext.Get<Guid>("PluginSourceid");
            lock (_syncRoot)
            {
                var testNamesNames = p0.Split(',');
                foreach (var resourceName in testNamesNames)
                {
                    var serializer = new Dev2JsonSerializer();
                    var serviceTestModelTO = serializer.Deserialize<ServiceTestModelTO>(SimpleJson);
                    serviceTestModelTO.TestName = resourceName;
                    serviceTestModelTO.ResourceId = resourceID;
                    serviceTestModelTO.Inputs = new List<IServiceTestInput>();
                    serviceTestModelTO.Outputs = new List<IServiceTestOutput>();
                    serviceTestModelTO.AuthenticationType = AuthenticationType.Windows;
                    serviceTestModelTO.TestSteps = new List<IServiceTestStep>();
                    serviceTestModelTos.Add(serviceTestModelTO);
                }
            }

            var resourceModel = new ResourceModel(_environmentModel)
            {
                ID = savedSource.ID,
                Category = "",
                ResourceName = _resourceForTests
            };
            _environmentModel.ResourceRepository.SaveTests(resourceModel, serviceTestModelTos);
        }

        const string ResourceCat = "ResourceCat\\";

        [Then(@"""(.*)"" has (.*) tests")]
        public void ThenHasTests(string resourceName, int numberOdTests)
        {
            var resourceID = MyContext.Get<Guid>(resourceName + "id");
            var serviceTestModelTos = _environmentModel.ResourceRepository.LoadResourceTests(resourceID);
            Assert.AreEqual(numberOdTests, serviceTestModelTos.Count, "Number count is not the same for resource - " + resourceName);
        }

        [When(@"I delete resource ""(.*)""")]
        public void WhenIDeleteResource(string resourceName)
        {
            MyContext.Get<Guid>(resourceName + "id");

            var savedSource = _environmentModel.ResourceRepository.All().First(model => model.ResourceName.Equals(_resourceForTests, StringComparison.InvariantCultureIgnoreCase));
            _environmentModel.ResourceRepository.DeleteResource(savedSource);
        }

        static void AddVariables(string variableName, DataListViewModel datalistViewModel, enDev2ColumnArgumentDirection ioDirection)
        {
            if (DataListUtil.IsValueScalar(variableName))
            {
                var scalarName = DataListUtil.RemoveLanguageBrackets(variableName);
                var scalarItemModel = new ScalarItemModel(scalarName, ioDirection);
                if (!scalarItemModel.HasError)
                {
                    datalistViewModel.Add(scalarItemModel);
                }
            }
            if (DataListUtil.IsValueRecordsetWithFields(variableName))
            {
                var rsName = DataListUtil.ExtractRecordsetNameFromValue(variableName);
                var fieldName = DataListUtil.ExtractFieldNameOnlyFromValue(variableName);
                var rs = datalistViewModel.RecsetCollection.FirstOrDefault(model => model.Name == rsName);
                if (rs == null)
                {
                    var recordSetItemModel = new RecordSetItemModel(rsName);
                    datalistViewModel.Add(recordSetItemModel);
                    recordSetItemModel.Children.Add(new RecordSetFieldItemModel(fieldName, recordSetItemModel, ioDirection));
                }
                else
                {
                    var recordSetFieldItemModel = rs.Children.FirstOrDefault(model => model.Name == fieldName);
                    if (recordSetFieldItemModel == null)
                    {
                        rs.Children.Add(new RecordSetFieldItemModel(fieldName, rs, ioDirection));
                    }
                }
            }
        }

        private static string GetActualResult(IServiceTestModel serviceTestModel)
        {
            var actual = string.Empty;

            if (serviceTestModel.TestPending)
            {
                actual = "Pending";
            }
            if (serviceTestModel.TestFailing)
            {
                actual = "Failing";
            }
            if (serviceTestModel.TestInvalid)
            {
                actual = "Invalid";
            }
            if (serviceTestModel.TestPassed)
            {
                actual = "Passed";
            }

            return actual;
        }

        [Given(@"""(.*)"" Tests as")]
        public void GivenTestsAs(string workFlowName, Table table)
        {
            var resourceIdKey = workFlowName + "Resourceid";
            var resourceID = MyContext.Get<Guid>(resourceIdKey);
            var serviceTestModelTos = new List<IServiceTestModelTO>();
            foreach (var tableRow in table.Rows)
            {
                var serializer = new Dev2JsonSerializer();
                var serviceTestModelTO = serializer.Deserialize<ServiceTestModelTO>(SimpleJson);
                serviceTestModelTO.TestName = tableRow["TestName"];
                serviceTestModelTO.ResourceId = resourceID;
                serviceTestModelTO.TestFailing = bool.Parse(tableRow["TestFailing"]);
                serviceTestModelTO.TestPending = bool.Parse(tableRow["TestPending"]);
                serviceTestModelTO.TestInvalid = bool.Parse(tableRow["TestInvalid"]);
                serviceTestModelTO.TestPassed = bool.Parse(tableRow["TestPassed"]);
                serviceTestModelTO.Inputs = new List<IServiceTestInput>();
                serviceTestModelTO.Outputs = new List<IServiceTestOutput>();
                serviceTestModelTO.AuthenticationType = AuthenticationType.Windows;
                serviceTestModelTO.TestSteps = new List<IServiceTestStep>();

                serviceTestModelTos.Add(serviceTestModelTO);
            }

            var resourceModel = new ResourceModel(_environmentModel)
            {
                ID = resourceID,
                Category = "",
                ResourceName = _resourceForTests
            };
            _environmentModel.ResourceRepository.SaveTests(resourceModel, serviceTestModelTos);
        }

        [Then(@"""(.*)"" is passing")]
        public void ThenIsPassing(string testName)
        {
            var serviceTest = GetTestFrameworkFromContext();
            var serviceTestModel = serviceTest.Tests.Single(model => model.TestName.Equals(testName, StringComparison.InvariantCultureIgnoreCase));
            var actual = GetActualResult(serviceTestModel);
            Assert.IsTrue(serviceTestModel.TestPassed, "Expected " + testName + " to be Passed. Actual result is " + actual);
        }

        [Then(@"""(.*)"" is failing")]
        public void ThenIsFailing(string testName)
        {
            var serviceTest = GetTestFrameworkFromContext();
            var serviceTestModel = serviceTest.Tests.Single(model => model.TestName.Equals(testName, StringComparison.InvariantCultureIgnoreCase));
            var actual = GetActualResult(serviceTestModel);
            Assert.IsTrue(serviceTestModel.TestFailing, "Expected " + testName + " to be Passed. Actual result is " + actual);
        }

        [Then(@"""(.*)"" is invalid")]
        public void ThenIsInvalid(string testName)
        {
            var serviceTest = GetTestFrameworkFromContext();
            var serviceTestModel = serviceTest.Tests.Single(model => model.TestName.Equals(testName, StringComparison.InvariantCultureIgnoreCase));
            var actual = GetActualResult(serviceTestModel);
            Assert.IsTrue(serviceTestModel.TestInvalid, "Expected " + testName + " to be Passed. Actual result is " + actual);
        }

        [Given(@"there are no tests")]
        [When(@"there are no tests")]
        [Then(@"there are no tests")]
        public void GivenThereAreNoTests()
        {
            var serviceTest = GetTestForCurrentTestFramework();
            Assert.AreEqual(0, serviceTest.Count());
        }

        [Then(@"""(.*)"" is pending")]
        public void ThenIsPending(string testName)
        {
            var serviceTest = GetTestFrameworkFromContext();
            var serviceTestModel = serviceTest.Tests.Single(model => model.TestName.Equals(testName, StringComparison.InvariantCultureIgnoreCase));
            var actual = GetActualResult(serviceTestModel);
            Assert.IsTrue(serviceTestModel.TestPending, "Expected " + testName + " to be Passed. Actual result is " + actual);
        }

        [Then(@"debug window is visible")]
        public void ThenDebugWindowIsVisible()
        {
            var serviceTest = GetTestFrameworkFromContext();
            var count = serviceTest.SelectedServiceTest.DebugForTest.Any(state => state.DisplayName == "Hello World");
            Assert.IsTrue(count);
        }

        [Given(@"""(.*)"" has outputs as")]
        public void GivenHasOutputsAs(string workflowName, Table outputVariables)
        {
            if (MyContext.TryGetValue(workflowName, out ResourceModel resourceModel))
            {
                if (MyContext.TryGetValue($"{workflowName}dataListViewModel", out DataListViewModel dataListViewModel))
                {
                    foreach (var variablesRow in outputVariables.Rows)
                    {
                        AddVariables(variablesRow["Ouput Var Name"], dataListViewModel, enDev2ColumnArgumentDirection.Output);
                    }
                    dataListViewModel.WriteToResourceModel();
                    _environmentModel.ResourceRepository.SaveToServer(resourceModel);
                }
                else
                {
                    Assert.Fail("No Datalist found for Workflow - " + workflowName);
                }
            }
            else
            {
                Assert.Fail($"Resource Model for {workflowName} not found");
            }
        }

        [Given(@"the test builder is open with ""(.*)""")]
        [When(@"the test builder is open with ""(.*)""")]
        [Then(@"the test builder is open with ""(.*)""")]
        public void GivenTheTestBuilderIsOpenWith(string workflowName)
        {
            if (MyContext.TryGetValue(workflowName, out ResourceModel resourceModel))
            {
                var vm = new ServiceTestViewModel(resourceModel, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new SpecExternalProcessExecutor(), new Mock<IWorkflowDesignerViewModel>().Object);
                vm.WebClient = new Mock<IWarewolfWebClient>().Object;
                Assert.IsNotNull(vm);
                Assert.IsNotNull(vm.ResourceModel);
                MyContext.Add("testFramework", vm);
                var firstOrDefault = MyContext.FirstOrDefault(pair => pair.Value.ToString() == resourceModel.ID.ToString()).Value;
                if (firstOrDefault == null)
                {
                    MyContext.Add(workflowName + "Id", resourceModel.ID);
                }
                return;
            }
            var resourceId = ConfigurationManager.AppSettings[workflowName].ToGuid();
            var sourceResourceRepository = ServerRepository.Instance.Source.ResourceRepository;
            var loadContextualResourceModel = sourceResourceRepository.LoadContextualResourceModel(resourceId);
            Assert.IsNotNull(loadContextualResourceModel, "Cannot find " + workflowName);
            var msg = sourceResourceRepository.FetchResourceDefinition(loadContextualResourceModel.Environment, GlobalConstants.ServerWorkspaceID, resourceId, false);
            loadContextualResourceModel.WorkflowXaml = msg.Message;
            var testFramework = new ServiceTestViewModel(loadContextualResourceModel, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new SpecExternalProcessExecutor(), new Mock<IWorkflowDesignerViewModel>().Object);
            testFramework.WebClient = new Mock<IWarewolfWebClient>().Object;
            Assert.IsNotNull(testFramework, "ServiceTestViewModel expects Not Null using Workflow - " + workflowName);
            Assert.IsNotNull(testFramework.ResourceModel, "ServiceTestViewModel ResourceModel expects Not Null using Workflow - " + workflowName);
            MyContext.Add("testFramework", testFramework);
        }

        [Given(@"I update inputs as")]
        [When(@"I update inputs as")]
        [Then(@"I update inputs as")]
        public void WhenIUpdatedTheInputsAs(Table table)
        {
            var serviceTest = GetTestFrameworkFromContext();
            var inputs = serviceTest.SelectedServiceTest.Inputs;
            foreach (var tableRow in table.Rows)
            {
                var valueToSet = tableRow["Value"];
                var varName = tableRow["Variable Name"];
                var containsKey = tableRow.ContainsKey("EmptyIsNull");
                var isNull = false;
                if (containsKey)
                {
                    var emptyIsNull = tableRow["EmptyIsNull"];
                    isNull = bool.Parse(emptyIsNull);
                }

                if (!string.IsNullOrEmpty(varName))
                {
                    var foundInput = inputs.FirstOrDefault(input => input.Variable == varName);
                    if (foundInput != null)
                    {
                        foundInput.Value = valueToSet;
                        foundInput.EmptyIsNull = isNull;
                    }
                }
            }
        }

        [Then(@"the service debug assert message contains ""(.*)""")]
        [Given(@"the service debug assert message contains ""(.*)""")]
        [When(@"the service debug assert message contains ""(.*)""")]
        public void ThenTheServiceDebugAssertMessageContains(string assertString)
        {
            var serviceTestViewModel = GetTestFrameworkFromContext();
            var debugForTest = serviceTestViewModel.SelectedServiceTest.DebugForTest;

            var debugItemResults = debugForTest.LastOrDefault(state => state.StateType == StateType.End).AssertResultList.First().ResultsList;

            var actualAssetMessage = debugItemResults.Select(result => result.Value).First();
            StringAssert.Contains(actualAssetMessage.ToLower(), assertString.ToLower());
        }

        [Then(@"the service debug assert Aggregate message contains ""(.*)""")]
        [Given(@"the service debug assert Aggregate message contains ""(.*)""")]
        [When(@"the service debug assert Aggregate message contains ""(.*)""")]
        public void ThenTheServiceDebugAssertAggregateMessageContains(string assertString)
        {
            var serviceTestViewModel = GetTestFrameworkFromContext();
            var debugForTest = serviceTestViewModel.SelectedServiceTest.DebugForTest;

            var debugItemResults = debugForTest.LastOrDefault(state => state.StateType == StateType.TestAggregate).AssertResultList.First().ResultsList;

            var actualAssetMessage = debugItemResults.Select(result => result.Value).First();
            StringAssert.Contains(actualAssetMessage.ToLower(), assertString.ToLower());
        }

        [Then(@"the service debug assert Json message contains ""(.*)""")]
        [When(@"the service debug assert Json message contains ""(.*)""")]
        [Given(@"the service debug assert Json message contains ""(.*)""")]
        public void ThenTheServiceDebugAssertJsonMessageContains(string assertString)
        {
            var serviceTestViewModel = GetTestFrameworkFromContext();
            var debugForTest = serviceTestViewModel.SelectedServiceTest.DebugForTest;

            var debugItemResults = debugForTest.LastOrDefault(state => state.StateType == StateType.TestAggregate).AssertResultList.First().ResultsList;
            var externalProcessExecutor = new SpecExternalProcessExecutor();
            var first = debugItemResults.Select(result =>
            {
                externalProcessExecutor.OpenInBrowser(new Uri(result.MoreLink));
                var downloadStrings = externalProcessExecutor.WebResult[0];
                return downloadStrings;
            }).First();
            StringAssert.Contains(first.ToLower(), assertString.ToLower());
        }

        [Then(@"All test pieces are pending")]
        public void ThenAllTestPiecesArePending()
        {
            var serviceTestViewModel = GetTestFrameworkFromContext();
            var testPending = serviceTestViewModel.SelectedServiceTest.TestPending;
            Assert.IsTrue(testPending, "Expected Test Pending to be True. TestName - " + serviceTestViewModel.SelectedServiceTest.TestName);
            var stepsPending = serviceTestViewModel.SelectedServiceTest.TestSteps.All(step => ((ServiceTestStep)step).TestPending);
            var serviceTestSteps = serviceTestViewModel.SelectedServiceTest.TestSteps.Flatten(step => step.Children).ToList();
            var allPending = serviceTestSteps.All(step => ((ServiceTestStep)step).TestPending && ((ServiceTestStep)step).Result.RunTestResult == RunResult.TestPending);
            var allOutputsPending = serviceTestViewModel.SelectedServiceTest.Outputs.All(output => ((ServiceTestOutput)output).TestPending && output.Result?.RunTestResult == RunResult.TestPending);
            Assert.IsTrue(stepsPending, "Expected Pending Step to be True");
            Assert.IsTrue(allPending, "Expected All Pending Steps to be True");
            Assert.IsTrue(allOutputsPending, "Expected All Output Pending Steps to be True");

            foreach (var serviceTestStep in serviceTestSteps)
            {
                var allStepOutPutspending = serviceTestStep.StepOutputs.All(output => output.Result?.RunTestResult == RunResult.TestPending);
                Assert.IsTrue(allStepOutPutspending, "Expected All Step Outputs Pending to be True. Step Description - " + serviceTestStep.StepDescription);
            }
        }

        [When(@"I change step ""(.*)"" to Mock")]
        public void WhenIChangeStepToMock(string stepname)
        {
            var serviceTestViewModel = GetTestFrameworkFromContext();
            var serviceTestStep = serviceTestViewModel.SelectedServiceTest.TestSteps.Single(step => step.StepDescription.TrimEnd().Equals(stepname));
            serviceTestStep.Type = StepType.Mock;
        }

        [Then(@"step ""(.*)"" is Pending")]
        public void ThenStepIsPending(string stepname)
        {
            var serviceTestViewModel = GetTestFrameworkFromContext();
            var serviceTestStep = serviceTestViewModel.SelectedServiceTest.TestSteps.Single(step => step.StepDescription.TrimEnd().Equals(stepname));
            var testPending = ((ServiceTestStep)serviceTestStep).TestPending;
            Assert.IsTrue(testPending, "Expected Step Pending to be True. Step Description - " + stepname);
        }

        [Then(@"I update outputs as")]
        public void ThenIUpdateOutputsAs(Table table)
        {
            var serviceTest = GetTestFrameworkFromContext();
            var outputs = serviceTest.SelectedServiceTest.Outputs;
            foreach (var tableRow in table.Rows)
            {
                var valueToSet = tableRow["Value"];
                var varName = tableRow["Variable Name"];
                if (!string.IsNullOrEmpty(varName))
                {
                    var foundInput = outputs.FirstOrDefault(output => output.Variable == varName);
                    if (foundInput != null)
                    {
                        foundInput.Value = valueToSet;
                    }
                }
            }
        }

        [Given(@"Tab Header is ""(.*)""")]
        [When(@"Tab Header is ""(.*)""")]
        [Then(@"Tab Header is ""(.*)""")]
        public void GivenTabHeaderIs(string expectedTabHeader)
        {
            var serviceTest = GetTestFrameworkFromContext();
            Assert.AreEqual(expectedTabHeader, serviceTest.DisplayName);
        }

        [When(@"I run the test")]
        public void WhenIRunTheTest()
        {
            var serviceTest = GetTestFrameworkFromContext();
            serviceTest.RunSelectedTestCommand.Execute(null);
        }

        [When(@"I run selected test in Web")]
        public void WhenIRunSelectedTestInWeb()
        {
            var serviceTest = GetTestFrameworkFromContext();
            serviceTest.RunSelectedTestInBrowserCommand.Execute(null);
        }

        [Then(@"all tests pass")]
        public void ThenAllTestsPass()
        {
            var testModels = GetTestForCurrentTestFramework();
            var allPassed = testModels.All(model => model.TestPassed);
            Assert.IsTrue(allPassed);
        }

        [Then(@"I run all tests")]
        public void ThenIRunAllTests()
        {
            var serviceTest = GetTestFrameworkFromContext();
            serviceTest.RunAllTestsCommand.Execute(null);
        }

        [When(@"I run all tests in Web")]
        public void WhenIRunAllTestsInWeb()
        {
            var serviceTest = GetTestFrameworkFromContext();
            serviceTest.RunAllTestsInBrowserCommand.Execute(null);
        }

        [Then(@"test result is Passed")]
        public void ThenTestResultIsPassed()
        {
            var serviceTest = GetTestFrameworkFromContext();
            var test = serviceTest.SelectedServiceTest;
            Assert.IsNotNull(test, "Workflow Service Test expects Not Null for Test - " + test.TestName);
            Assert.IsTrue(test.TestPassed, "Workflow Service Test expects Passed for Test - " + test.TestName);
            Assert.IsFalse(test.TestFailing, "Workflow Service Test expects Failed to be false for Test - " + test.TestName);
        }

        [Then(@"I change Decision ""(.*)"" arm to ""(.*)""")]
        public void ThenIChangeDecisionArmTo(string decisionName, string ArmInput)
        {
            var serviceTest = GetTestFrameworkFromContext();
            var serviceTestStep = serviceTest.SelectedServiceTest.TestSteps.Single(step => step.StepDescription.TrimEnd().Equals(decisionName));
            var serviceTestOutput = serviceTestStep.StepOutputs.Single();
            var value = serviceTestOutput.OptionsForValue?.Single(s => s.Equals(ArmInput, StringComparison.InvariantCultureIgnoreCase)) ?? ArmInput;
            serviceTestOutput.Value = value;
        }

        [Then(@"I change Switch ""(.*)"" arm to ""(.*)""")]
        public void ThenIChangeSwitchArmTo(string switchName, string ArmInput)
        {
            var serviceTest = GetTestFrameworkFromContext();
            var serviceTestStep = serviceTest.SelectedServiceTest.TestSteps.Single(step => step.StepDescription.TrimEnd().Equals(switchName));
            var serviceTestOutput = serviceTestStep.StepOutputs.Single();
            var value = serviceTestOutput.OptionsForValue?.Single(s => s.Equals(ArmInput, StringComparison.InvariantCultureIgnoreCase)) ?? ArmInput;
            serviceTestOutput.Value = value;
        }

        [Then(@"test result is invalid")]
        public void ThenTestResultIsInvalid()
        {
            var serviceTest = GetTestFrameworkFromContext();
            var test = serviceTest.SelectedServiceTest;
            Assert.IsNotNull(test, "Workflow Service Test expects Not Null for Test - " + test.TestName);
            Assert.IsNotNull(test, "Workflow Service Test expects Not Null for Test - " + test.TestName);
            Assert.IsTrue(test.TestInvalid, "Workflow Service Test expects Invalid for Test - " + test.TestName);
        }

        [Then(@"test result is Failed")]
        public void ThenTestResultIsFailed()
        {
            var serviceTest = GetTestFrameworkFromContext();
            var test = serviceTest.SelectedServiceTest;
            Assert.IsNotNull(test, "Workflow Service Test expects Not Null for Test - " + test.TestName);
            Assert.IsFalse(test.TestPassed, "Workflow Service Test expects Passed to be false for Test - " + test.TestName);
            Assert.IsTrue(test.TestFailing, "Workflow Service Test expects Failed for Test - " + test.TestName);
        }

        [When(@"I remove input ""(.*)"" from workflow ""(.*)""")]
        public void WhenIRemoveInputFromWorkflow(string input, string workflow)
        {
            var resourceIdKey = workflow + "Resourceid";
            var resourceId = MyContext.Get<Guid>(resourceIdKey);
            var resourceToChange = _environmentModel.ResourceRepository.FindResourcesByID(_environmentModel, new[] { resourceId.ToString() }, ResourceType.WorkflowService).Single();
            var newDatalist = resourceToChange.DataList.Replace(input, input + "Newname");
            resourceToChange.DataList = newDatalist;
            _resourceModelWithDifInputs = resourceToChange;
        }

        IResourceModel _resourceModelWithDifInputs;

        [When(@"I save Workflow\t""(.*)""")]
        public void WhenISaveWorkflow(string workflow)
        {
            _environmentModel.ResourceRepository.SaveToServer(_resourceModelWithDifInputs);
        }

        [When(@"all test are invalid")]
        public void WhenAllTestAreInvalid()
        {
            var serviceTestModels = GetTestForCurrentTestFramework();
            var allInvalid = serviceTestModels.All(model => model.TestInvalid);
            Assert.IsTrue(allInvalid);
        }

        [Then(@"the tab is closed")]
        public void ThenTheTabIsClosed()
        {
            var shellViewModel = MyContext.Get<Mock<IShellViewModel>>("shellViewModel");
            shellViewModel.Verify(vm => vm.CloseResourceTestView(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>()));
        }

        [Then(@"The ""(.*)"" popup is shown I click Ok")]
        public void ThenThePopupIsShownIClickOk(string popupViewName)
        {
            var popupController = GetPopupController();
            switch (popupViewName)
            {
                case "Delete Confirmation":
                    popupController.Verify(controller => controller.ShowDeleteConfirmation(It.IsAny<string>()));
                    break;
                case "Workflow Deleted":
                    popupController.Verify(controller => controller.Show(Warewolf.Studio.Resources.Languages.Core.ServiceTestResourceDeletedMessage, Warewolf.Studio.Resources.Languages.Core.ServiceTestResourceDeletedHeader, It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>()));
                    break;
                case "Workflow changed":
                    popupController.Verify(controller => controller.Show(Warewolf.Studio.Resources.Languages.Core.ServiceTestResourceCategoryChangedMessage, Warewolf.Studio.Resources.Languages.Core.ServiceTestResourceCategoryChangedHeader, It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>()));
                    break;
                default:
                    break;
            }
        }

        private Mock<Common.Interfaces.Studio.Controller.IPopupController> GetPopupController()
        {            
            if (!MyContext.ContainsKey("popupController"))
            {
                var popupController = new Mock<Common.Interfaces.Studio.Controller.IPopupController>();
                popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
                popupController.Setup(controller => controller.Show(It.IsAny<string>(), It.IsAny<string>(), MessageBoxButton.OK, MessageBoxImage.Error, null, false, true, false, false, false, false)).Verifiable();
                popupController.Setup(controller => controller.Show(It.IsAny<string>(), It.IsAny<string>(), MessageBoxButton.OK, MessageBoxImage.Information, null, false, true, false, false, false, false)).Verifiable();
                CustomContainer.Register(popupController.Object);
                MyContext["popupController"] = popupController;
                return popupController;
            }
            return MyContext.Get<Mock<Common.Interfaces.Studio.Controller.IPopupController>>("popupController");
        }

        [Given(@"I click New Test")]
        [When(@"I click New Test")]
        [Then(@"I click New Test")]
        public void WhenIClickNewTest()
        {
            var serviceTest = GetTestFrameworkFromContext();
            serviceTest.CreateTestCommand.Execute(null);
        }

        [Given(@"a decision variable ""(.*)"" value ""(.*)""")]
        public void GivenADecisionVariableValue(string variable, string value)
        {
            MyContext.TryGetValue("variableList", out List<Tuple<string, string>> variableList);

            if (variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                MyContext.Add("variableList", variableList);
            }

            variableList.Add(new Tuple<string, string>(variable, value));
        }

        [Given(@"decide if ""(.*)"" ""(.*)""")]
        public void GivenDecideIf(string variable1, string decision)
        {
            MyContext.TryGetValue("decisionModels", out List<Tuple<string, enDecisionType, string, string>> decisionModels);

            if (decisionModels == null)
            {
                decisionModels = new List<Tuple<string, enDecisionType, string, string>>();
                MyContext.Add("decisionModels", decisionModels);
            }

            decisionModels.Add(new Tuple<string, enDecisionType, string, string>(
                    variable1, (enDecisionType)Enum.Parse(typeof(enDecisionType), decision), null, null));
        }

        [Given(@"I need to switch on variable ""(.*)"" with the value ""(.*)""")]
        public void GivenINeedToSwitchOnVariableWithTheValue(string variable, string value)
        {
            MyContext.TryGetValue("variableList", out List<Tuple<string, string>> variableList);

            if (variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                MyContext.Add("variableList", variableList);
            }

            variableList.Add(new Tuple<string, string>(variable, value));
        }

        [Then(@"a new test is added")]
        public void ThenANewTestIsAdded()
        {
            var currentTests = GetTestForCurrentTestFramework();
            Assert.AreNotEqual(0, currentTests.Count());
        }

        [Then(@"Test Status is ""(.*)""")]
        public void ThenTestStatusIs(string expectedStatus)
        {
            var serviceTest = GetTestFrameworkFromContext();

            switch (expectedStatus)
            {
                case "TestPending":
                    Assert.IsTrue(serviceTest.Tests[0].TestPending);
                    break;
                case "TestPassed":
                    Assert.IsTrue(serviceTest.Tests[0].TestPassed);
                    break;
                case "TestFailing":
                    Assert.IsTrue(serviceTest.Tests[0].TestFailing);
                    break;
                case "TestInvalid":
                    Assert.IsTrue(serviceTest.Tests[0].TestInvalid);
                    break;
                default:
                    Assert.IsTrue(serviceTest.Tests[0].TestPending);
                    break;
            }
        }

        [Then(@"""(.*)"" test is visible")]
        public void ThenTestIsVisible(string testCommand)
        {
            var serviceTest = GetTestFrameworkFromContext();

            switch (testCommand)
            {
                case "Stop":
                    Assert.IsTrue(serviceTest.StopTestCommand.CanExecute(null));
                    break;
                case "Run":
                    Assert.IsTrue(serviceTest.RunSelectedTestCommand.CanExecute(null));
                    break;
                case "Save":
                    Assert.IsTrue(serviceTest.CanSave);
                    break;
                default:
                    Assert.Fail("Incorrect Command Option!");
                    break;
            }
        }

        [Then(@"there are (.*) tests")]
        public void ThenThereAreTests(int testCount)
        {
            var currentTests = GetTestForCurrentTestFramework();
            Assert.AreEqual(testCount, currentTests.Count());
        }

        [Then(@"there are (.*) tests in directory")]
        public void ThenThereAreTestsInDirectory(int testCount)
        {
            var serviceTest = GetTestFrameworkFromContext();
            var resourceId = serviceTest.SelectedServiceTest.ParentId;
            var path = Path.Combine(EnvironmentVariables.TestPath, resourceId.ToString());
            var fyles = DirectoryHelperInstance().GetFiles(path);
            Assert.AreEqual(testCount, fyles.Count());
        }

        [Then(@"test name starts with ""(.*)""")]
        public void ThenTestNameStartsWith(string testName)
        {
            var serviceTest = GetTestFrameworkFromContext();
            Assert.AreEqual(testName, serviceTest.SelectedServiceTest.TestName);
        }

        [Then(@"""(.*)"" is selected")]
        public void ThenIsSelected(string testName)
        {
            var serviceTest = GetTestFrameworkFromContext();
            if (testName == "Dummy Test")
            {
                Assert.IsNull(serviceTest.SelectedServiceTest);
            }
            else
            {
                Assert.AreEqual(testName, serviceTest.SelectedServiceTest.TestName);
            }
        }

        [Then(@"username is blank")]
        public void ThenUsernameIsBlank()
        {
            var serviceTest = GetTestFrameworkFromContext();
            Assert.AreEqual(null, serviceTest.SelectedServiceTest.UserName);
        }

        [Then(@"test AuthenticationType as ""(.*)""")]
        [When(@"test AuthenticationType as ""(.*)""")]
        [Given(@"test AuthenticationType as ""(.*)""")]
        public void ThenTestAuthenticationTypeAs(string AuthType)
        {
            var serviceTest = GetTestFrameworkFromContext();
            Enum.TryParse(AuthType, true, out AuthenticationType auth);
            serviceTest.SelectedServiceTest.AuthenticationType = auth;
        }

        [Then(@"username is ""(.*)""")]
        [When(@"username is ""(.*)""")]
        [Given(@"username is ""(.*)""")]
        public void ThenUsernameIs(string p0)
        {
            var serviceTest = GetTestFrameworkFromContext();
            serviceTest.SelectedServiceTest.UserName = p0;
        }

        [Then(@"password is ""(.*)""")]
        [When(@"password is ""(.*)""")]
        [Given(@"password is ""(.*)""")]
        public void ThenPasswordIs(string p0)
        {
            var serviceTest = GetTestFrameworkFromContext();
            serviceTest.SelectedServiceTest.Password = p0;
        }

        [Then(@"password is blank")]
        public void ThenPasswordIsBlank()
        {
            var serviceTest = GetTestFrameworkFromContext();
            Assert.AreEqual(null, serviceTest.SelectedServiceTest.Password);
        }

        [Then(@"inputs are")]
        public void ThenInputsAs(Table table)
        {
            var serviceTest = GetTestFrameworkFromContext();
            var inputs = serviceTest.SelectedServiceTest.Inputs;
            Assert.AreNotEqual(0, inputs.Count);
            var i = 0;
            foreach (var tableRow in table.Rows)
            {
                Assert.AreEqual(tableRow["Variable Name"], inputs[i].Variable);
                var expected = tableRow["Value"];
                Assert.AreEqual(expected, inputs[i].Value);
                i++;
            }
        }

        [Then(@"outputs as")]
        public void ThenOutputsAs(Table table)
        {
            var serviceTest = GetTestFrameworkFromContext();
            var outputs = serviceTest.SelectedServiceTest.Outputs;
            Assert.AreNotEqual(0, outputs.Count);
            var i = 0;
            foreach (var tableRow in table.Rows)
            {
                Assert.AreEqual(tableRow["Variable Name"], outputs[i].Variable);
                var expected = tableRow["Value"];
                Assert.AreEqual(expected, outputs[i].Value);
                i++;
            }
        }

        [Then(@"I Add outputs as")]
        public void ThenIAddOutputsAs(Table table)
        {
            var serviceTest = GetTestFrameworkFromContext();
            var serviceTestStep = serviceTest.SelectedServiceTest.Outputs.First();

            foreach (var tableRow in table.Rows)
            {
                var varName = tableRow["Variable Name"];
                var condition = tableRow["Condition"];
                var value = tableRow["Value"];
                serviceTestStep.Variable = varName;
                serviceTestStep.AssertOp = condition;
                serviceTestStep.Value = value;
            }
        }

        [Then(@"The WebResponse as")]
        public void ThenTheWebResponseAs(Table table)
        {
            var serviceTest = GetTestFrameworkFromContext();
            var fieldInfo = typeof(ServiceTestViewModel).GetField("_processExecutor", BindingFlags.NonPublic | BindingFlags.Instance);
            if (fieldInfo?.GetValue(serviceTest) is ISpecExternalProcessExecutor specExternalProcessExecutor)
            {
                var webResult = specExternalProcessExecutor.WebResult;
                foreach (var result in webResult)
                {
                    var jToken = JToken.Parse(result);
                    if (jToken.IsEnumerable())
                    {
                        var jObject = JArray.Parse(result);
                        foreach (var tableRow in table.Rows)
                        {
                            foreach (var resultPairs in jObject)
                            {
                                var testObj = resultPairs as JObject;

                                var testName = testObj.Property("Test Name").Value.ToString();
                                if (testName != tableRow["Test Name"])
                                {
                                    continue;
                                }
                                var testResult = testObj.Property("Result").Value.ToString();
                                Assert.AreEqual(tableRow["Result"], testResult, "Result message dont match for Test - " + testName);
                                var hasMessage = testObj.TryGetValue("Message", out JToken testMessageToken);
                                if (hasMessage)
                                {
                                    var testMessage = testMessageToken.ToString();
                                    Assert.AreEqual(tableRow["Message"], testMessage.Replace("\n", "").Replace("\r", "").Replace(Environment.NewLine, ""), "Error message dont match for Test - " + testName);
                                }
                            }
                        }
                    }
                    else if (jToken.IsObject())
                    {
                        var jObject = JObject.Parse(result);
                        foreach (var tableRow in table.Rows)
                        {
                            foreach (var resultPairs in jObject)
                            {
                                if (resultPairs.Key == "Test Name")
                                {
                                    Assert.AreEqual(tableRow["Test Name"], resultPairs.Value, "value message dont match");
                                }
                                if (resultPairs.Key == "Result")
                                {
                                    Assert.AreEqual(tableRow["Result"], resultPairs.Value, "Result message dont match");
                                }

                                if (resultPairs.Key == "Message")
                                {
                                    Assert.AreEqual(tableRow["Message"], resultPairs.Value.ToString().Replace("\n", "").Replace("\r", "").Replace(Environment.NewLine, ""), "error message dont match");
                                }
                            }
                        }
                    }
                }
            }
        }

        [When(@"""(.*)"" is deleted")]
        public void WhenIsDeleted(string workflowName)
        {
            if (MyContext.TryGetValue(workflowName, out ResourceModel resourceModel))
            {
                var env = ServerRepository.Instance.Source;
                env.ResourceRepository.DeleteResource(resourceModel);
            }
        }

        [When(@"""(.*)"" is moved")]
        public void WhenIsMoved(string workflowName)
        {
            if (MyContext.TryGetValue(workflowName, out ResourceModel resourceModel))
            {
                var env = ServerRepository.Instance.Source;
                resourceModel.Category = "bob\\" + workflowName;
                env.ResourceRepository.SaveToServer(resourceModel);
            }
        }

        [Given(@"save is enabled")]
        [Then(@"save is disabled")]
        public void ThenSaveIsDisabled()
        {
            var serviceTest = GetTestFrameworkFromContext();
            Assert.IsFalse(serviceTest.CanSave);
        }

        [Then(@"save is enabled")]
        [When(@"save is enabled")]
        public void ThenSaveIsEnabled()
        {
            var serviceTest = GetTestFrameworkFromContext();
            Assert.IsTrue(serviceTest.CanSave);
        }

        [Then(@"test status is pending")]
        public void ThenTestStatusIsPending()
        {
            var serviceTest = GetTestFrameworkFromContext();
            Assert.IsTrue(serviceTest.SelectedServiceTest.TestPending);
        }

        [Then(@"test is enabled")]
        public void ThenTestIsEnabled()
        {
            var serviceTest = GetTestFrameworkFromContext();
            Assert.IsTrue(serviceTest.SelectedServiceTest.Enabled);
        }

        [Given(@"I save")]
        [When(@"I save")]
        [Then(@"I save")]
        public void WhenISave()
        {
            var serviceTest = GetTestFrameworkFromContext();
            serviceTest.Save();
        }

        [Given(@"I close the test builder")]
        [When(@"I close the test builder")]
        [Then(@"I close the test builder")]
        public void ThenICloseTheTestBuilder()
        {
            var serviceTest = GetTestFrameworkFromContext();
            serviceTest?.Dispose();
            ScenarioContext.Current.Remove("testFramework");
        }

        [Then(@"Inputs are empty")]
        public void ThenInputsAreEmpty()
        {
            var serviceTest = GetTestFrameworkFromContext();
            var hasNoInputs = serviceTest.SelectedServiceTest.Inputs == null;
            Assert.IsTrue(hasNoInputs);
        }

        [Then(@"Outputs are empty")]
        public void ThenOutputsAreEmpty()
        {
            var serviceTest = GetTestFrameworkFromContext();
            var hasNoOutputs = serviceTest.SelectedServiceTest.Outputs == null;
            Assert.IsTrue(hasNoOutputs);
        }

        [Then(@"No Error selected")]
        public void ThenNoErrorSelected()
        {
            var serviceTest = GetTestFrameworkFromContext();
            var noErrorExpected = serviceTest.SelectedServiceTest.NoErrorExpected;
            Assert.IsTrue(noErrorExpected);
        }

        [When(@"I change the test name to ""(.*)""")]
        public void WhenIChangeTheTestNameTo(string testName)
        {
            var serviceTest = GetTestFrameworkFromContext();
            serviceTest.SelectedServiceTest.TestName = testName;
        }

        [When(@"tests count is ""(.*)""")]
        public void WhenTestsCountIs(int testCount)
        {
            var serviceTest = GetTestFrameworkFromContext();            
            var workflowTests =_environmentModel.ResourceRepository.LoadAllTests();
            var filteredTests = workflowTests.Where(test => test.ResourceId == serviceTest.ResourceID);
            Assert.AreEqual(testCount, filteredTests.Count());
        }


        [Then(@"I set ErrorExpected to ""(.*)""")]
        public void ThenISetErrorExpectedTo(string value)
        {
            var serviceTest = GetTestFrameworkFromContext();
            if (value == "true")
            {
                serviceTest.SelectedServiceTest.ErrorExpected = true;
            }
        }

        [Then(@"change ErrorContainsText to ""(.*)""")]
        public void ThenChangeErrorContainsTextTo(string errorContainsText)
        {
            var serviceTest = GetTestFrameworkFromContext();
            serviceTest.SelectedServiceTest.ErrorContainsText = errorContainsText;
        }

        [Then(@"test URL is ""(.*)""")]
        public void ThenTestURLIs(string testUrl)
        {
            var serviceTest = GetTestFrameworkFromContext();
            serviceTest.SelectedServiceTest.RunSelectedTestUrl = testUrl;
        }

        [Then(@"Test name is ""(.*)""")]
        public void ThenTestNameIs(string testName)
        {
            var serviceTest = GetTestFrameworkFromContext();
            Assert.AreEqual(testName, serviceTest.SelectedServiceTest.TestName);
        }

        [Then(@"Name for display is ""(.*)"" and test is edited")]
        public void ThenNameForDisplayIsAndTestIsEdited(string nameForDisplay)
        {
            var serviceTest = GetTestFrameworkFromContext();
            var selectedServiceTest = serviceTest.SelectedServiceTest;
            Assert.IsTrue(selectedServiceTest.IsDirty, "Workflow Service Test expects IsDirty for Test - " + selectedServiceTest.TestName);
            Assert.AreEqual(nameForDisplay, selectedServiceTest.NameForDisplay, "Workflow Service Test expects Name for Display to be " + nameForDisplay + " for Test - " + selectedServiceTest.TestName);
        }

        [Then(@"Name for display is ""(.*)"" and test is not edited")]
        public void ThenNameForDisplayIs(string nameForDisplay)
        {
            var serviceTest = GetTestFrameworkFromContext();
            var selectedServiceTest = serviceTest.SelectedServiceTest;
            Assert.IsFalse(selectedServiceTest.IsDirty, "Workflow Service Test expects IsDirty to be false for Test - " + selectedServiceTest.TestName);
            Assert.AreEqual(nameForDisplay, selectedServiceTest.NameForDisplay, "Workflow Service Test expects Name for Display to be " + nameForDisplay + " for Test - " + selectedServiceTest.TestName);
        }

        [When(@"I ""(.*)"" the selected test")]
        public void WhenITheSelectedTest(string status)
        {
            var serviceTest = GetTestFrameworkFromContext();
            serviceTest.SelectedServiceTest.Enabled = status == "Enable";
        }

        [Then(@"DeleteCommand is ""(.*)""")]
        public void ThenDeleteCommandIs(string status)
        {
            var serviceTest = GetTestFrameworkFromContext();
            if (status == "Active")
            {
                Assert.IsTrue(serviceTest.DeleteTestCommand.CanExecute(null), "Expected the Delete test command to be Enabled");
            }
            else
            {
                Assert.IsFalse(serviceTest.DeleteTestCommand.CanExecute(null), "Expected the Delete test command to be Disabled");
            }
        }

        [Given(@"I select ""(.*)""")]
        [When(@"I select ""(.*)""")]
        [Then(@"I select ""(.*)""")]
        public void GivenISelect(string testName)
        {
            var serviceTest = GetTestFrameworkFromContext();
            var serviceTestModel = serviceTest.Tests.Single(model => string.Equals(model.TestName, testName, StringComparison.InvariantCultureIgnoreCase));
            serviceTest.SelectedServiceTest = serviceTestModel;
        }

        [Given(@"I set Test Values as")]
        [When(@"I set Test Values as")]
        [Then(@"I set Test Values as")]
        public void GivenISetTestValuesAs(Table table)
        {
            var serviceTest = GetTestFrameworkFromContext();
            foreach (var tableRow in table.Rows)
            {
                var testName = tableRow["TestName"];
                var authenticationType = tableRow["AuthenticationType"];
                Enum.TryParse(authenticationType, true, out AuthenticationType authent);
                var error = tableRow["Error"];
                serviceTest.SelectedServiceTest.TestName = testName;
                serviceTest.SelectedServiceTest.ErrorExpected = bool.Parse(error);
                serviceTest.SelectedServiceTest.AuthenticationType = authent;
            }
        }

        [Then(@"Test Status saved is ""(.*)""")]
        public void ThenTestStatusSavedIs(string testStatus)
        {
            var serviceTest = GetTestFrameworkFromContext();

            switch (testStatus)
            {
                case "TestPending":
                    serviceTest.SelectedServiceTest.TestPending = true;
                    break;
                case "TestInvalid":
                    serviceTest.SelectedServiceTest.TestInvalid = true;
                    break;
                case "TestFailing":
                    serviceTest.SelectedServiceTest.TestFailing = true;
                    break;
                case "TestPassed":
                    serviceTest.SelectedServiceTest.TestPassed = true;
                    break;
                default:
                    serviceTest.SelectedServiceTest.TestPending = true;
                    break;
            }
        }

        [Then(@"NoErrorExpected is ""(.*)""")]
        public void ThenNoErrorExpectedIs(string error)
        {
            var hasError = bool.Parse(error);
            var serviceTest = GetTestFrameworkFromContext();
            Assert.AreEqual(hasError, serviceTest.SelectedServiceTest.NoErrorExpected, "Workflow Service Test expects NoErrorExpected for Test - " + serviceTest.SelectedServiceTest.TestName);
        }

        [Then(@"Authentication is Public")]
        public void ThenAuthenticationIsPublic()
        {
            var serviceTest = GetTestFrameworkFromContext();
            Assert.AreEqual(AuthenticationType.Public, serviceTest.SelectedServiceTest.AuthenticationType);
        }

        [When(@"I disable ""(.*)""")]
        public void WhenIDisable(string testName)
        {
            var serviceTest = GetTestFrameworkFromContext();
            var serviceTestModel = serviceTest.Tests.Single(model => string.Equals(model.TestName, testName, StringComparison.InvariantCultureIgnoreCase));
            serviceTestModel.Enabled = false;
        }

        [When(@"I enable ""(.*)""")]
        public void WhenIEnable(string testName)
        {
            var serviceTest = GetTestFrameworkFromContext();
            var serviceTestModel = serviceTest.Tests.Single(model => string.Equals(model.TestName, testName, StringComparison.InvariantCultureIgnoreCase));
            serviceTestModel.Enabled = true;
        }

        [Then(@"Delete is disabled for ""(.*)""")]
        public void ThenDeleteIsDisabledFor(string testName)
        {
            var serviceTest = GetTestFrameworkFromContext();
            var serviceTestModel = serviceTest.Tests.Single(model => string.Equals(model.TestName, testName, StringComparison.InvariantCultureIgnoreCase));
            var canDelete = serviceTest.DeleteTestCommand.CanExecute(serviceTestModel);
            Assert.IsFalse(canDelete);
        }

        [Then(@"Delete is enabled for ""(.*)""")]
        public void ThenDeleteIsEnabledFor(string testName)
        {
            var serviceTest = GetTestFrameworkFromContext();
            var serviceTestModel = serviceTest.Tests.Single(model => string.Equals(model.TestName, testName, StringComparison.InvariantCultureIgnoreCase));
            var canDelete = serviceTest.DeleteTestCommand.CanExecute(serviceTestModel);
            Assert.IsTrue(canDelete);
        }

        [Given(@"I set inputs as")]
        [When(@"I set inputs as")]
        [Then(@"I set inputs as")]
        public void GivenISetInputsAs(Table table)
        {
            var serviceTest = GetTestFrameworkFromContext();

            foreach (var tableRow in table.Rows)
            {
                var vname = tableRow["Variable Name"];
                var value = tableRow["Value"];
                serviceTest.SelectedServiceTest.Inputs.Add(new ServiceTestInput(vname, value));
            }
        }

        [Given(@"I set outputs as")]
        [When(@"I set outputs as")]
        [Then(@"I set outputs as")]
        public void GivenISetOutputsAs(Table table)
        {
            var serviceTest = GetTestFrameworkFromContext();

            foreach (var tableRow in table.Rows)
            {
                var vname = tableRow["Variable Name"];
                var value = tableRow["Value"];
                var from = tableRow["From"];
                var to = tableRow["To"];
                serviceTest.SelectedServiceTest.Outputs.Add(new ServiceTestOutput(vname, value, from, to));
            }
        }

        [Then(@"Delete is enabled")]
        public void ThenDeleteIsEnabled()
        {
            var serviceTest = GetTestFrameworkFromContext();
            var canDelete = serviceTest.DeleteTestCommand.CanExecute(null);
            Assert.IsTrue(canDelete);
        }

        [Then(@"Run is enabled")]
        public void ThenRunIsEnabled()
        {
            var serviceTest = GetTestFrameworkFromContext();
            var canDelete = serviceTest.RunSelectedTestCommand.CanExecute(null);
            Assert.IsTrue(canDelete);
        }

        [When(@"I delete ""(.*)""")]
        [Then(@"I delete ""(.*)""")]
        [Given(@"I delete ""(.*)""")]
        public void WhenIDelete(string testName)
        {
            var serviceTest = GetTestFrameworkFromContext();
            var serviceTestModel = serviceTest.Tests.Single(model => string.Equals(model.TestName, testName, StringComparison.InvariantCultureIgnoreCase));
            serviceTest.DeleteTestCommand.Execute(serviceTestModel);
        }

        [When(@"I delete selected Test")]
        public void WhenIDeleteSelectedTest()
        {
            var serviceTest = GetTestFrameworkFromContext();
            serviceTest.DeleteTestCommand.Execute(null);
        }

        [Then(@"The Confirmation popup is shown")]
        public void ThenTheConfirmationPopupIsShown()
        {
            var mock = MyContext["popupController"] as Mock<Common.Interfaces.Studio.Controller.IPopupController>;
            mock.VerifyAll();
        }

        [Then(@"The Pending Changes Confirmation popup is shown I click Ok")]
        public void ThenThePendingChangesConfirmationPopupIsShownIClickOk()
        {
            var mock = (Mock<Common.Interfaces.Studio.Controller.IPopupController>)MyContext["popupController"];
            mock.Verify(controller => controller.Show(It.IsAny<string>(), It.IsAny<string>(), MessageBoxButton.OK, MessageBoxImage.Information, null, false, true, false, false, false, false));
        }

        [Then(@"Error is ""(.*)""")]
        public void ThenErrorIs(string hasError)
        {
            var error = bool.Parse(hasError);
            var serviceTest = GetTestFrameworkFromContext();
            var errorExpected = serviceTest.SelectedServiceTest.ErrorExpected;
            Assert.AreEqual(error, errorExpected);
        }

        [When(@"test is disabled")]
        public void WhenTestIsDisabled()
        {
            var serviceTest = GetTestFrameworkFromContext();
            var enabled = serviceTest.SelectedServiceTest.Enabled;
            Assert.IsFalse(enabled);
        }

        [When(@"I click ""(.*)""")]
        public void WhenIClick(string testName)
        {
            var serviceTest = GetTestFrameworkFromContext();
            var serviceTestModel = serviceTest.Tests.Single(model => model.TestName == testName);
            serviceTest.SelectedServiceTest = serviceTestModel;
        }

        [Then(@"Duplicate Test is visible")]
        public void ThenDuplicateTestIsVisible()
        {
            var serviceTest = GetTestFrameworkFromContext();
            var canExecute = serviceTest.DuplicateTestCommand.CanExecute(null);
            Assert.IsTrue(canExecute);
        }

        [When(@"I run selected test")]
        public void WhenIRunSelectedTest()
        {
            var serviceTest = GetTestFrameworkFromContext();
            serviceTest.RunSelectedTestCommand.Execute(null);
        }

        [When(@"I run selected test in browser")]
        public void WhenIRunSelectedTestBrowser()
        {
            var serviceTest = GetTestFrameworkFromContext();
            serviceTest.RunSelectedTestInBrowserCommand.Execute(null);
        }

        [When(@"selected test is empty")]
        public void WhenSelectedTestIsEmpty()
        {
            var serviceTest = GetTestFrameworkFromContext();
            Assert.IsNull(serviceTest.SelectedServiceTest);
        }

        [When(@"I run all tests")]
        public void WhenIRunAllTests()
        {
            var serviceTest = GetTestFrameworkFromContext();
            serviceTest.RunAllTestsCommand.Execute(null);
        }

        [When(@"I run all tests in browser")]
        public void WhenIRunAllTestsInBrowser()
        {
            var serviceTest = GetTestFrameworkFromContext();
            serviceTest.RunAllTestsInBrowserCommand.Execute(null);
        }

        [When(@"I click delete test")]
        public void WhenIClickDeleteTest()
        {
            var serviceTest = GetTestFrameworkFromContext();
            serviceTest.DeleteTestCommand.Execute(null);
        }

        [When(@"I click duplicate")]
        public void WhenIClickDuplicate()
        {
            var serviceTest = GetTestFrameworkFromContext();
            serviceTest.DuplicateTestCommand.Execute(null);
        }

        [Then(@"the duplicated tests is ""(.*)""")]
        public void ThenTheDuplicatedTestsIs(string dupTestName)
        {
            var serviceTest = GetTestFrameworkFromContext();
            Assert.IsTrue(serviceTest.SelectedServiceTest.TestName == dupTestName);
            var count = serviceTest.Tests.Count(model => dupTestName.Contains(model.TestName));
            Assert.AreEqual(2, count);
        }
        [Then(@"Duplicate Test in not Visible")]
        public void ThenDuplicateTestInNotVisible()
        {
            var serviceTest = GetTestFrameworkFromContext();
            var canExecute = serviceTest.DuplicateTestCommand.CanExecute(null);
            Assert.IsFalse(canExecute);
        }

        [Then(@"Duplicate Test is ""(.*)""")]
        public void ThenDuplicateTestIs(string visibilityStatus)
        {
            var serviceTest = GetTestFrameworkFromContext();
            var canExecute = serviceTest.DuplicateTestCommand.CanExecute(null);
            if (visibilityStatus == "Enabled")
            {
                Assert.IsTrue(canExecute);
            }
            else
            {
                Assert.IsFalse(canExecute);
            }
        }

        [Then(@"Duplicate Test in Visible")]
        public void ThenDuplicateTestInVisible()
        {
            var serviceTest = GetTestFrameworkFromContext();
            var canExecute = serviceTest.DuplicateTestCommand.CanExecute(null);
            Assert.IsTrue(canExecute);
        }


        [Then(@"The duplicate Name popup is shown")]
        public void ThenTheDuplicateNamePopupIsShown()
        {
            var mock = (Mock<Common.Interfaces.Studio.Controller.IPopupController>)MyContext["popupController"];
            mock.Verify(controller => controller.Show(It.IsAny<string>(), It.IsAny<string>(), MessageBoxButton.OK, MessageBoxImage.Error, null, false, true, false, false, false, false));
        }

        [Given(@"I have a folder ""(.*)""")]
        public void GivenIHaveAFolder(string foldername)
        {
            var category = ResourceCat + foldername;
            MyContext.Add("folderPath", category);
        }

        [Then(@"I expect Error ""(.*)""")]
        public void ThenIExpectError(string p0)
        {
            var testFrameworkFromContext = GetTestFrameworkFromContext();
            testFrameworkFromContext.SelectedServiceTest.ErrorExpected = true;
            testFrameworkFromContext.SelectedServiceTest.ErrorContainsText = p0;
        }

        [Given(@"I have a resouce workflow ""(.*)"" inside Home")]
        public void GivenIHaveAResouceWorkflowInsideHome(string resourceName)
        {
            var path = MyContext.Get<string>("folderPath");

            var resourceId = Guid.NewGuid();
            var resourceModel = new ResourceModel(_environmentModel)
            {
                ResourceName = resourceName,
                DisplayName = resourceName,
                DataList = "",
                ID = resourceId,
                Category = path + "\\" + resourceName
            };
            var workflowHelper = new WorkflowHelper();
            var builder = workflowHelper.CreateWorkflow(resourceName);
            resourceModel.WorkflowXaml = workflowHelper.GetXamlDefinition(builder);
            _environmentModel.ResourceRepository.SaveToServer(resourceModel);
            ScenarioContext.Current.Add(resourceName + "id", resourceId);
        }

        [Given(@"I add ""(.*)"" to ""(.*)""")]
        public void GivenIAddTo(string testNames, string rName)
        {
            MyContext.TryGetValue("folderPath", out string path);
            var serviceTestModelTos = new List<IServiceTestModelTO>();            
            if (!string.IsNullOrEmpty(path))
            {

                var savedSource = _environmentModel.ResourceRepository.All().First(model => model.Category.Equals(path + "\\" + rName, StringComparison.InvariantCultureIgnoreCase));
                MyContext[rName + "id"] = savedSource.ID;
                var resourceID = MyContext.Get<Guid>(rName + "id");
                lock (_syncRoot)
                {
                    var testNamesNames = testNames.Split(',');
                    foreach (var resourceName in testNamesNames)
                    {
                        var serializer = new Dev2JsonSerializer();
                        var serviceTestModelTO = serializer.Deserialize<ServiceTestModelTO>(SimpleJson);
                        serviceTestModelTO.TestName = resourceName;
                        serviceTestModelTO.ResourceId = resourceID;
                        serviceTestModelTO.Inputs = new List<IServiceTestInput>();
                        serviceTestModelTO.Outputs = new List<IServiceTestOutput>();
                        serviceTestModelTO.AuthenticationType = AuthenticationType.Windows;
                        serviceTestModelTO.TestSteps = new List<IServiceTestStep>();
                        serviceTestModelTos.Add(serviceTestModelTO);
                    }
                }


                var resourceModel = new ResourceModel(_environmentModel)
                {
                    ID = savedSource.ID,
                    Category = path + "\\" + rName,
                    ResourceName = rName
                };
                var executeMessage = _environmentModel.ResourceRepository.SaveTests(resourceModel, serviceTestModelTos);
                Assert.IsTrue(executeMessage.Result == SaveResult.Success || executeMessage.Result == SaveResult.ResourceUpdated);
            }
            else
            {
                var resourceId = new Guid("acb75027-ddeb-47d7-814e-a54c37247ec1");
                lock (_syncRoot)
                {
                    var testNamesNames = testNames.Split(',');
                    foreach (var resourceName in testNamesNames)
                    {
                        var serializer = new Dev2JsonSerializer();
                        var serviceTestModelTO = serializer.Deserialize<ServiceTestModelTO>(SimpleJson);
                        serviceTestModelTO.TestName = resourceName;
                        serviceTestModelTO.TestSteps = new List<IServiceTestStep>();
                        serviceTestModelTO.ResourceId = resourceId;
                        serviceTestModelTO.Inputs = new List<IServiceTestInput>();
                        serviceTestModelTO.Outputs = new List<IServiceTestOutput>();
                        serviceTestModelTO.AuthenticationType = AuthenticationType.Windows;

                        serviceTestModelTos.Add(serviceTestModelTO);
                    }
                }

                var testFrameworkFromContext = GetTestFrameworkFromContext();

                var executeMessage = _environmentModel.ResourceRepository.SaveTests(testFrameworkFromContext.ResourceModel, serviceTestModelTos);
                Assert.IsTrue(executeMessage.Result == SaveResult.Success || executeMessage.Result == SaveResult.ResourceUpdated);
                testFrameworkFromContext = new ServiceTestViewModel(testFrameworkFromContext.ResourceModel, new SynchronousAsyncWorker(), new Mock<IEventAggregator>().Object, new SpecExternalProcessExecutor(), new Mock<IWorkflowDesignerViewModel>().Object);
                testFrameworkFromContext.WebClient = new Mock<IWarewolfWebClient>().Object;
                MyContext["testFramework"] = testFrameworkFromContext;

            }
        }
        [When(@"I delete folder ""(.*)""")]
        public void WhenIDeleteFolder(string folderName)
        {
            var path = MyContext.Get<string>("folderPath");
            
            var controller = new CommunicationController { ServiceName = "DeleteItemService" };
            controller.AddPayloadArgument("folderToDelete", path);
            var result = controller.ExecuteCommand<IExplorerRepositoryResult>(_environmentModel.Connection, GlobalConstants.ServerWorkspaceID);
            if (result.Status != ExecStatus.Success)
            {
                throw new WarewolfSaveException(result.Message, null);
            }

        }

        [Given(@"the test builder is open with existing service ""(.*)""")]
        public void GivenTheTestBuilderIsOpenWithExistingService(string workflowName)
        {
            var env = ServerRepository.Instance.Source;
            env.ForceLoadResources();
            var sourceResourceRepository = env.ResourceRepository;
            var res = sourceResourceRepository.FindSingle(model => model.ResourceName.Equals(workflowName, StringComparison.InvariantCultureIgnoreCase), true);
            if (res != null)
            {
                var contextualResource = sourceResourceRepository.LoadContextualResourceModel(res.ID);
                var msg = sourceResourceRepository.FetchResourceDefinition(contextualResource.Environment,
                    GlobalConstants.ServerWorkspaceID, res.ID, false);
                contextualResource.WorkflowXaml = msg.Message;
                var serviceTestVm = new ServiceTestViewModel(contextualResource, new SynchronousAsyncWorker(),
                    new Mock<IEventAggregator>().Object, new SpecExternalProcessExecutor(),
                    new Mock<IWorkflowDesignerViewModel>().Object);
                serviceTestVm.WebClient = new Mock<IWarewolfWebClient>().Object;
                Assert.IsNotNull(serviceTestVm);
                Assert.IsNotNull(serviceTestVm.ResourceModel);
                MyContext.Add("testFramework", serviceTestVm);
            }
            else
            {
                Assert.Fail("Resource " + workflowName + " not found in local Warewolf %programdata% resources.");
            }
        }


        [Then(@"service debug inputs as")]
        public void ThenServiceDebugInputsAs(Table table)
        {
            var serviceTest = GetTestFrameworkFromContext();
            var test = serviceTest.SelectedServiceTest;
            Assert.IsNotNull(test);
            Assert.IsNotNull(test.DebugForTest);
            var debugStates = test.DebugForTest;
            var serviceStartDebug = debugStates[0];
            foreach (var tableRow in table.Rows)
            {
                var variableName = tableRow["Variable"];
                var variableValue = tableRow["Value"];
                IDebugItemResult debugItemResult = null;
                var debugItem = serviceStartDebug.Inputs.FirstOrDefault(item =>
                {
                    debugItemResult = item.ResultsList.FirstOrDefault(result => result.Variable.Equals(variableName, StringComparison.InvariantCultureIgnoreCase));
                    return debugItemResult != null;
                });
                Assert.IsNotNull(debugItem);
                Assert.IsNotNull(debugItemResult);
                Assert.AreEqual(variableValue, debugItemResult.Value);
            }
        }

        [Then(@"the service debug outputs as")]
        public void ThenTheServiceDebugOutputsAs(Table table)
        {
            var serviceTest = GetTestFrameworkFromContext();
            var test = serviceTest.SelectedServiceTest;
            Assert.IsNotNull(test);
            Assert.IsNotNull(test.DebugForTest);
            var debugStates = test.DebugForTest;
            var serviceEndDebug = debugStates.First(state => state.StateType == StateType.End);
            foreach (var tableRow in table.Rows)
            {
                var variableName = tableRow["Variable"];
                var variableValue = tableRow["Value"];
                IDebugItemResult debugItemResult = null;
                var debugItem = serviceEndDebug.Outputs.FirstOrDefault(item =>
                {
                    debugItemResult = item.ResultsList.FirstOrDefault(result => result.Variable.Equals(variableName, StringComparison.InvariantCultureIgnoreCase));
                    return debugItemResult != null;
                });
                Assert.IsNotNull(debugItem);
                Assert.IsNotNull(debugItemResult);
                Assert.AreEqual(variableValue, debugItemResult.Value);
            }
        }

        [Then(@"Test debug results contain pending results ""(.*)""")]
        public void ThenTestDebugResultsContainPendingResults(string pendingResult)
        {
            var serviceTest = GetTestFrameworkFromContext();
            var test = serviceTest.SelectedServiceTest;
            Assert.IsNotNull(test);
            Assert.IsNotNull(test.DebugForTest);
            var debugStates = test.DebugForTest;
            var serviceEndDebug = debugStates.First(state => state.StateType == StateType.TestAggregate);
            var assertResult = serviceEndDebug.AssertResultList[0];
            var errorValues = assertResult.ResultsList[0].Value;
            var strings = errorValues.Split('\n');
            var hasPendingResults = strings.Any(s => s.TrimEnd('\r').Equals(pendingResult, StringComparison.InvariantCultureIgnoreCase));
            Assert.IsTrue(hasPendingResults);
        }



        [Then(@"I add mock steps as")]
        public void ThenIAddMockStepsAs(Table table)
        {
            var serviceTest = GetTestFrameworkFromContext();
            var test = serviceTest.SelectedServiceTest;
            var helper = new WorkflowHelper();
            var builder = helper.ReadXamlDefinition(serviceTest.ResourceModel.WorkflowXaml);
            Assert.IsNotNull(builder);
            var act = (Flowchart)builder.Implementation;
            foreach (var tableRow in table.Rows)
            {
                var actNameToFind = tableRow["Step Name"];
                var actType = tableRow["Activity Type"];
                if (actNameToFind != null)
                {
                    if (string.Equals(actType, "Decision", StringComparison.InvariantCultureIgnoreCase))
                    {
                        var foundNode = act.Nodes.FirstOrDefault(node =>
                        {
                            if (node is FlowDecision searchNode)
                            {
                                return searchNode.DisplayName.TrimEnd(' ').Equals(actNameToFind, StringComparison.InvariantCultureIgnoreCase);
                            }
                            return false;
                        });
                        if (foundNode != null)
                        {
                            var decisionNode = foundNode as FlowDecision;

                            var condition = decisionNode.Condition;
                            var activity = (DsfFlowNodeActivity<bool>)condition;
                            var expression = activity.ExpressionText;
                            if (expression != null)
                            {
                                var eval = Dev2DecisionStack.ExtractModelFromWorkflowPersistedData(expression);

                                if (!string.IsNullOrEmpty(eval))
                                {
                                    var ser = new Dev2JsonSerializer();
                                    var dds = ser.Deserialize<Dev2DecisionStack>(eval);
                                    var armToUse = tableRow["Output Value"];

                                    if (dds.FalseArmText.Equals(armToUse, StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        var serviceTestOutputs = new ObservableCollection<IServiceTestOutput> { new ServiceTestOutput(GlobalConstants.ArmResultText, dds.FalseArmText, "", "") };
                                        test.AddTestStep(activity.UniqueID, activity.DisplayName, typeof(DsfDecision).Name, serviceTestOutputs, StepType.Mock);
                                    }
                                    else if (dds.TrueArmText.Equals(armToUse, StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        var serviceTestOutputs = new ObservableCollection<IServiceTestOutput> { new ServiceTestOutput(GlobalConstants.ArmResultText, dds.TrueArmText, "", "") };
                                        test.AddTestStep(activity.UniqueID, activity.DisplayName, typeof(DsfDecision).Name, serviceTestOutputs, StepType.Mock);
                                    }
                                }
                            }

                        }
                    }
                    else
                    {
                        var foundNode = act.Nodes.FirstOrDefault(node =>
                        {
                            if (node is FlowStep searchNode)
                            {
                                return searchNode.Action.DisplayName.TrimEnd(' ').Equals(actNameToFind, StringComparison.InvariantCultureIgnoreCase);
                            }
                            return false;
                        });
                        var decisionNode = foundNode as FlowStep;

                        var action = decisionNode.Action;
                        var activity = (DsfActivityAbstract<string>)action;
                        var var = tableRow["Output Variable"];
                        var value = tableRow["Output Value"];
                        var from = tableRow["Output From"];
                        var to = tableRow["Output To"];
                        var serviceTestOutputs = new ObservableCollection<IServiceTestOutput> { new ServiceTestOutput(var, value, from, to) };
                        var type = activity.GetType();
                        test.AddTestStep(activity.UniqueID, activity.DisplayName, type.Name, serviceTestOutputs, StepType.Mock);
                    }
                }
            }
        }

        [Then(@"I Add all TestSteps")]
        [When(@"I Add all TestSteps")]
        [Given(@"I Add all TestSteps")]
        public void ThenIAddAllTestSteps()
        {

            var serviceTest = GetTestFrameworkFromContext();
            var helper = new WorkflowHelper();
            var builder = helper.ReadXamlDefinition(serviceTest.ResourceModel.WorkflowXaml);
            Assert.IsNotNull(builder);
            var act = (Flowchart)builder.Implementation;
            foreach (var flowNode in act.Nodes)
            {
                var modelItem = ModelItemUtils.CreateModelItem(flowNode);
                var methodInfo = typeof(ServiceTestViewModel).GetMethod("ItemSelectedAction", BindingFlags.Instance | BindingFlags.NonPublic);
                methodInfo.Invoke(serviceTest, new object[] { modelItem });
            }
        }

        [Then(@"I remove all Test Steps")]
        [When(@"I remove all Test Steps")]
        public void ThenIRemoveAllTestSteps()
        {
            var serviceTestViewModel = GetTestFrameworkFromContext();
            serviceTestViewModel.SelectedServiceTest.TestSteps = new ObservableCollection<IServiceTestStep>();
        }

        [Then(@"I remove outputs from TestStep ""(.*)""")]
        public void ThenIRemoveOutputsFromTestStep(string stepName)
        {
            var serviceTestViewModel = GetTestFrameworkFromContext();
            var serviceTestStep = serviceTestViewModel.SelectedServiceTest.TestSteps.Single(step => step.StepDescription.TrimEnd().TrimStart().Equals(stepName));
            serviceTestStep.StepOutputs.Clear();
        }


        [Then(@"I Add Decision ""(.*)"" as TestStep")]
        [Given(@"I Add Decision ""(.*)"" as TestStep")]
        [When(@"I Add Decision ""(.*)"" as TestStep")]

        public void ThenIAddDecisionAsTestStep(string actNameToFind)
        {
            var serviceTest = GetTestFrameworkFromContext();
            var helper = new WorkflowHelper();
            var builder = helper.ReadXamlDefinition(serviceTest.ResourceModel.WorkflowXaml);
            Assert.IsNotNull(builder);
            var act = (Flowchart)builder.Implementation;
            var actStartNode = act.StartNode;
            if (act.Nodes.Count == 0 && actStartNode != null)
            {
                var searchNode = actStartNode as FlowStep ?? (dynamic)(actStartNode as FlowDecision);

                while (searchNode != null)
                {

                    bool isCorr;

                    if (searchNode is FlowDecision node)
                    {
                        isCorr = node.DisplayName.TrimEnd(' ').Equals(actNameToFind, StringComparison.InvariantCultureIgnoreCase);
                    }
                    else
                    {
                        isCorr = searchNode.Action.DisplayName.TrimEnd(' ').Equals(actNameToFind, StringComparison.InvariantCultureIgnoreCase);
                    }
                    if (isCorr)
                    {
                        var modelItem = ModelItemUtils.CreateModelItem(searchNode.Action);
                        var methodInfo = typeof(ServiceTestViewModel).GetMethod("ItemSelectedAction", BindingFlags.Instance | BindingFlags.NonPublic);
                        methodInfo.Invoke(serviceTest, new object[] { modelItem });
                        searchNode = null;
                    }
                    else
                    {
                        searchNode = searchNode.Next as FlowStep;
                    }
                }
            }
            else
            {
                foreach (var flowNode in act.Nodes)
                {
                    var searchNode = flowNode as FlowStep ?? (dynamic)(actStartNode as FlowDecision);
                    bool isCorr;
                    if (searchNode is FlowDecision node)
                    {
                        isCorr = node.DisplayName.TrimEnd(' ').Equals(actNameToFind, StringComparison.InvariantCultureIgnoreCase);
                    }
                    else
                    {
                        isCorr = searchNode != null && searchNode.Action.DisplayName.TrimEnd(' ').Equals(actNameToFind, StringComparison.InvariantCultureIgnoreCase);
                    }

                    if (isCorr)
                    {
                        var modelItem = ModelItemUtils.CreateModelItem(searchNode);
                        var methodInfo = typeof(ServiceTestViewModel).GetMethod("ItemSelectedAction", BindingFlags.Instance | BindingFlags.NonPublic);
                        methodInfo.Invoke(serviceTest, new object[] { modelItem });
                        break;
                    }
                }
            }
        }

        [Then(@"I Add ""(.*)"" as TestStep All Assert")]
        public void ThenIAddAsTestStepAllAssert(string actNameToFind)
        {
            ThenIAddAsTestStep(actNameToFind);
            var serviceTest = GetTestFrameworkFromContext();

            foreach (var serviceTestStep in serviceTest.SelectedServiceTest.TestSteps)
            {
                var testSteps = serviceTestStep.Children.Flatten(step => step.Children ?? new ObservableCollection<IServiceTestStep>());
                foreach (var s in testSteps)
                {
                    s.Type = StepType.Assert;
                }
            }
        }

        [Then(@"I Add Switch ""(.*)"" as TestStep")]
        [Then(@"I Add Switch ""(.*)"" as TestStep")]
        [Then(@"I Add Switch ""(.*)"" as TestStep")]
        public void ThenIAddSwitchAsTestStep(string actNameToFind)
        {
            var serviceTest = GetTestFrameworkFromContext();
            var helper = new WorkflowHelper();
            var builder = helper.ReadXamlDefinition(serviceTest.ResourceModel.WorkflowXaml);
            Assert.IsNotNull(builder);
            var act = (Flowchart)builder.Implementation;
            var actStartNode = act.StartNode;
            if (act.Nodes.Count == 0 && actStartNode != null)
            {
                var searchNode = actStartNode as FlowStep ?? (dynamic)(actStartNode as FlowSwitch<string>);

                while (searchNode != null)
                {

                    bool isCorr;

                    if (searchNode is FlowSwitch<string> node)
                    {
                        isCorr = node.DisplayName.TrimEnd(' ').Equals(actNameToFind, StringComparison.InvariantCultureIgnoreCase);
                    }
                    else
                    {
                        isCorr = searchNode.Action.DisplayName.TrimEnd(' ').Equals(actNameToFind, StringComparison.InvariantCultureIgnoreCase);
                    }
                    if (isCorr)
                    {
                        var modelItem = ModelItemUtils.CreateModelItem(searchNode.Action);
                        var methodInfo = typeof(ServiceTestViewModel).GetMethod("ItemSelectedAction", BindingFlags.Instance | BindingFlags.NonPublic);
                        methodInfo.Invoke(serviceTest, new object[] { modelItem });
                        searchNode = null;
                    }
                    else
                    {
                        searchNode = searchNode.Next as FlowStep;
                    }
                }
            }
            else
            {
                foreach (var flowNode in act.Nodes)
                {
                    var searchNode = flowNode as FlowStep ?? (dynamic)(actStartNode as FlowSwitch<string>);
                    bool isCorr;
                    if (searchNode == null)
                    {
                        searchNode = flowNode as FlowSwitch<string>;
                    }
                    if (searchNode is FlowSwitch<string> node)
                    {
                        isCorr = node.DisplayName.TrimEnd(' ').Equals(actNameToFind, StringComparison.InvariantCultureIgnoreCase);
                    }
                    else
                    {
                        isCorr = searchNode != null && searchNode.Action.DisplayName.TrimEnd(' ').Equals(actNameToFind, StringComparison.InvariantCultureIgnoreCase);
                    }

                    if (isCorr)
                    {
                        var modelItem = ModelItemUtils.CreateModelItem(searchNode);
                        var methodInfo = typeof(ServiceTestViewModel).GetMethod("ItemSelectedAction", BindingFlags.Instance | BindingFlags.NonPublic);
                        methodInfo.Invoke(serviceTest, new object[] { modelItem });
                        break;
                    }
                }
            }
        }

        [Then(@"I Add ""(.*)"" as TestStep")]
        public void ThenIAddAsTestStep(string actNameToFind)
        {
            AddTestStep(actNameToFind);
        }

        [Then(@"I Add all ""(.*)"" as TestStep")]
        public void ThenIAddAllAsTestStep(string actNameToFind)
        {
            var serviceTest = GetTestFrameworkFromContext();
            var helper = new WorkflowHelper();
            var builder = helper.ReadXamlDefinition(serviceTest.ResourceModel.WorkflowXaml);
            Assert.IsNotNull(builder);
            var act = (Flowchart)builder.Implementation;
            foreach (var flowNode in act.Nodes)
            {
                var searchNode = flowNode as FlowStep;
                var isCorr = searchNode != null && searchNode.Action.DisplayName.TrimEnd(' ').Equals(actNameToFind, StringComparison.InvariantCultureIgnoreCase);
                if (isCorr)
                {
                    var modelItem = ModelItemUtils.CreateModelItem(flowNode);
                    var methodInfo = typeof(ServiceTestViewModel).GetMethod("ItemSelectedAction", BindingFlags.Instance | BindingFlags.NonPublic);
                    methodInfo.Invoke(serviceTest, new object[] { modelItem });
                }
            }
        }


        [Then(@"I Clear existing StepOutputs")]
        public void ThenIClearExistingStepOutputs()
        {
            var serviceTest = GetTestFrameworkFromContext();
            var serviceTestStep = serviceTest.SelectedServiceTest.TestSteps.First();
            serviceTestStep.StepOutputs = new BindableCollection<IServiceTestOutput>();
        }

        [Then(@"I Add ""(.*)"" as TestStep with")]
        public void ThenIAddAsTestStepWith(string actNameToFind, Table table)
        {
            var serviceTest = AddTestStep(actNameToFind);
            var serviceTestStep = serviceTest.SelectedServiceTest.TestSteps.FirstOrDefault(p => p.StepDescription == actNameToFind);
            serviceTestStep.StepOutputs = new BindableCollection<IServiceTestOutput>();
            foreach (var tableRow in table.Rows)
            {
                var varName = tableRow["Variable Name"];
                var condition = tableRow["Condition"];
                var value = tableRow["Value"];

                serviceTestStep.StepOutputs.Add(new ServiceTestOutput(varName, value, "", "")
                {
                    AssertOp = condition
                });
            }

        }

        ServiceTestViewModel AddTestStep(string actNameToFind)
        {
            var serviceTest = GetTestFrameworkFromContext();
            var helper = new WorkflowHelper();
            var builder = helper.ReadXamlDefinition(serviceTest.ResourceModel.WorkflowXaml);
            Assert.IsNotNull(builder);
            var act = (Flowchart)builder.Implementation;
            var actStartNode = act.StartNode;
            if (act.Nodes.Count == 0 && actStartNode != null)
            {
                var searchNode = actStartNode as FlowStep;
                while (searchNode != null)
                {
                    var isCorr = searchNode.Action.DisplayName.TrimEnd(' ').Equals(actNameToFind, StringComparison.InvariantCultureIgnoreCase);
                    if (isCorr)
                    {



                        var modelItem = ModelItemUtils.CreateModelItem(searchNode.Action);
                        var methodInfo = typeof(ServiceTestViewModel).GetMethod("ItemSelectedAction", BindingFlags.Instance | BindingFlags.NonPublic);
                        methodInfo.Invoke(serviceTest, new object[] { modelItem });
                        searchNode = null;
                    }
                    else
                    {
                        searchNode = searchNode.Next as FlowStep;
                    }
                }
            }
            else
            {
                foreach (var flowNode in act.Nodes)
                {
                    var searchNode = flowNode as FlowStep;
                    var isCorr = searchNode != null && searchNode.Action.DisplayName.TrimEnd(' ').Equals(actNameToFind, StringComparison.InvariantCultureIgnoreCase);
                    if (isCorr)
                    {
                        var modelItem = ModelItemUtils.CreateModelItem(flowNode);
                        var methodInfo = typeof(ServiceTestViewModel).GetMethod("ItemSelectedAction", BindingFlags.Instance | BindingFlags.NonPublic);
                        methodInfo.Invoke(serviceTest, new object[] { modelItem });
                        break;
                    }
                }
            }

            return serviceTest;
        }

        [Then(@"I add StepOutputs as")]
        public void ThenIAddStepOutputsAs(Table table)
        {
            var serviceTest = GetTestFrameworkFromContext();
            var serviceTestStep = serviceTest.SelectedServiceTest.TestSteps.First();
            serviceTestStep.StepOutputs = new BindableCollection<IServiceTestOutput>();
            foreach (var tableRow in table.Rows)
            {
                var varName = tableRow["Variable Name"];
                var condition = tableRow["Condition"];
                var value = tableRow["Value"];

                serviceTestStep.StepOutputs.Add(new ServiceTestOutput(varName, value, "", "")
                {
                    AssertOp = condition
                });


            }
        }


        [Then(@"I add ""(.*)"" StepOutputs as")]
        public void ThenIAddStepOutputsAs(string stepDesc, Table table)
        {
            var serviceTest = GetTestFrameworkFromContext();
            var serviceTestStep = serviceTest.SelectedServiceTest.TestSteps.First(step => step.StepDescription.Equals(stepDesc, StringComparison.CurrentCultureIgnoreCase));
            serviceTestStep.StepOutputs = new BindableCollection<IServiceTestOutput>();
            foreach (var tableRow in table.Rows)
            {
                var varName = tableRow["Variable Name"];
                var condition = tableRow["Condition"];
                var value = tableRow["Value"];

                serviceTestStep.StepOutputs.Add(new ServiceTestOutput(varName, value, "", "")
                {
                    AssertOp = condition
                });


            }
        }

        [Then(@"I add StepOutputs item as")]
        public void ThenIAddStepOutputsItemAs(Table table)
        {
            var serviceTest = GetTestFrameworkFromContext();
            var serviceTestStep = serviceTest.SelectedServiceTest.TestSteps.First();
            foreach (var tableRow in table.Rows)
            {
                var varName = tableRow["Variable Name"];
                var condition = tableRow["Condition"];
                var value = tableRow["Value"];

                serviceTestStep.StepOutputs.Add(new ServiceTestOutput(varName, value, "", "")
                {
                    AssertOp = condition
                });


            }
        }



        [Then(@"I add new StepOutputs as")]
        public void ThenIAddNewStepOutputsAs(Table table)
        {
            var serviceTest = GetTestFrameworkFromContext();
            foreach (var tableRow in table.Rows)
            {
                var varName = tableRow["Variable Name"];
                var condition = tableRow["Condition"];
                var value = tableRow["Value"];
                var serviceTestStep = serviceTest.SelectedServiceTest.TestSteps.First();
                serviceTestStep.StepOutputs.Add(new ServiceTestOutput(varName, value, "", "")
                {
                    AssertOp = condition
                });
            }
        }

        [Then(@"I add new Children StepOutputs as")]
        public void ThenIAddNewChildrenStepOutputsAs(Table table)
        {
            var serviceTest = GetTestFrameworkFromContext();
            var count = 1;
            foreach (var tableRow in table.Rows)
            {
                var varName = tableRow["Variable Name"];
                var condition = tableRow["Condition"];
                var value = tableRow["Value"];
                var serviceTestStep = serviceTest.SelectedServiceTest.TestSteps.First().Children.First();
                if (count == 1)
                {
                    serviceTestStep.StepOutputs = new BindableCollection<IServiceTestOutput>();
                }

                serviceTestStep.StepOutputs.Add(new ServiceTestOutput(varName, value, "", "")
                {
                    AssertOp = condition
                });
                count++;
            }
        }








        [Then(@"I add Assert steps as")]
        public void ThenIAddAssertStepsAs(Table table)
        {
            var serviceTest = GetTestFrameworkFromContext();
            var test = serviceTest.SelectedServiceTest;
            var helper = new WorkflowHelper();
            var builder = helper.ReadXamlDefinition(serviceTest.ResourceModel.WorkflowXaml);
            Assert.IsNotNull(builder);
            var act = (Flowchart)builder.Implementation;
            foreach (var tableRow in table.Rows)
            {
                var actNameToFind = tableRow["Step Name"];
                var actType = tableRow["Activity Type"];
                if (actNameToFind != null)
                {
                    if (string.Equals(actType, "Decision", StringComparison.InvariantCultureIgnoreCase))
                    {
                        var foundNode = act.Nodes.FirstOrDefault(node =>
                        {
                            if (node is FlowDecision searchNode)
                            {
                                return searchNode.DisplayName.TrimEnd(' ').Equals(actNameToFind, StringComparison.InvariantCultureIgnoreCase);
                            }
                            return false;
                        });
                        if (foundNode != null)
                        {
                            var decisionNode = foundNode as FlowDecision;

                            var condition = decisionNode.Condition;
                            var activity = (DsfFlowNodeActivity<bool>)condition;
                            var expression = activity.ExpressionText;
                            if (expression != null)
                            {
                                var eval = Dev2DecisionStack.ExtractModelFromWorkflowPersistedData(expression);

                                if (!string.IsNullOrEmpty(eval))
                                {
                                    var ser = new Dev2JsonSerializer();
                                    ser.Deserialize<Dev2DecisionStack>(eval);
                                    var armToUse = tableRow["Output Value"];
                                    test.AddTestStep(activity.UniqueID, activity.DisplayName, typeof(DsfDecision).Name, new ObservableCollection<IServiceTestOutput>() { new ServiceTestOutput(GlobalConstants.ArmResultText, armToUse, "", "") });

                                }
                            }

                        }
                    }
                    else
                    {
                        var foundNode = act.Nodes.FirstOrDefault(node =>
                        {
                            if (node is FlowStep searchNode)
                            {
                                return searchNode.Action.DisplayName.TrimEnd(' ').Equals(actNameToFind, StringComparison.InvariantCultureIgnoreCase);
                            }
                            return false;
                        });
                        var decisionNode = foundNode as FlowStep;

                        var action = decisionNode.Action;
                        var activity = (DsfActivityAbstract<string>)action;
                        var var = tableRow["Output Variable"];
                        var value = tableRow["Output Value"];
                        var from = tableRow["Output From"];
                        var to = tableRow["Output To"];
                        var serviceTestOutputs = new ObservableCollection<IServiceTestOutput> { new ServiceTestOutput(var, value, from, to) };
                        var type = activity.GetType();
                        test.AddTestStep(activity.UniqueID, activity.DisplayName, type.Name, serviceTestOutputs);
                    }
                }
            }
        }



        IEnumerable<IServiceTestModel> GetTestForCurrentTestFramework()
        {
            var testFrameworkFromContext = GetTestFrameworkFromContext();
            var serviceTestModels = testFrameworkFromContext.Tests.Where(model => model.GetType() != typeof(DummyServiceTest));
            return serviceTestModels;
        }

        ServiceTestViewModel GetTestFrameworkFromContext()
        {
            if (MyContext.TryGetValue("testFramework", out ServiceTestViewModel serviceTest))
            {
                return serviceTest;
            }
            Assert.Fail("Test Framework ViewModel not found");
            return null;
        }
    }
}
