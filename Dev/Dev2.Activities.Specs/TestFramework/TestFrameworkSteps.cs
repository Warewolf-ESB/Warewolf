using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Hosting;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Communication;
using Dev2.Controller;
using Dev2.Data;
using Dev2.Data.Binary_Objects;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Data.Util;
using Dev2.Runtime.Hosting;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Models;
using Dev2.Studio.Core.Models.DataList;
using Dev2.Studio.ViewModels.DataList;
using Dev2.Threading;
using Dev2.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TechTalk.SpecFlow;
using Warewolf.Studio.ServerProxyLayer;
using Warewolf.Studio.ViewModels;

// ReSharper disable UnusedMember.Global

namespace Dev2.Activities.Specs.TestFramework
{
    [Binding]
    public class StudioTestFrameworkSteps
    {
        public StudioTestFrameworkSteps(ScenarioContext scenarioContext)
        {
            if (scenarioContext == null) throw new ArgumentNullException(nameof(scenarioContext));
            ScenarioContext = scenarioContext;
        }

        ScenarioContext ScenarioContext { get; }

        [Given(@"I have ""(.*)"" with inputs as")]
        public void GivenIHaveWithInputsAs(string workflowName, Table inputVariables)
        {
            var environmentModel = EnvironmentRepository.Instance.Source;
            environmentModel.Connect();
            var resourceModel = BuildResourceModel(workflowName, environmentModel);
            var workflowHelper = new WorkflowHelper();
            var builder = workflowHelper.CreateWorkflow(workflowName);
            resourceModel.WorkflowXaml = workflowHelper.GetXamlDefinition(builder);

            var datalistViewModel = new DataListViewModel();
            datalistViewModel.InitializeDataListViewModel(resourceModel);
            foreach (var variablesRow in inputVariables.Rows)
            {
                AddVariables(variablesRow["Input Var Name"], datalistViewModel, enDev2ColumnArgumentDirection.Input);
            }
            datalistViewModel.WriteToResourceModel();
            ScenarioContext.Add(workflowName, resourceModel);
            ScenarioContext.Add($"{workflowName}dataListViewModel", datalistViewModel);
            var popupController = new Mock<Common.Interfaces.Studio.Controller.IPopupController>();
            popupController.Setup(controller => controller.ShowDeleteConfirmation(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
            popupController.Setup(controller => controller.Show(It.IsAny<string>(), It.IsAny<string>(), MessageBoxButton.OK, MessageBoxImage.Error, null, false, true, false, false)).Verifiable();
            popupController.Setup(controller => controller.Show(It.IsAny<string>(), It.IsAny<string>(), MessageBoxButton.OK, MessageBoxImage.Information, null, false, true, false, false)).Verifiable();
            CustomContainer.Register(popupController.Object);
            ScenarioContext["popupController"] = popupController;

            var shellViewModel = new Mock<IShellViewModel>();
            CustomContainer.Register(shellViewModel.Object);
            ScenarioContext["shellViewModel"] = shellViewModel;
        }

        private static ResourceModel BuildResourceModel(string workflowName, IEnvironmentModel environmentModel)
        {
            var resourceModel = new ResourceModel(environmentModel)
            {
                ResourceName = workflowName,
                DisplayName = workflowName,
                DataList = "",
                ID = Guid.NewGuid(),
                Category = workflowName
            };
            return resourceModel;
        }

        readonly object _syncRoot = new object();
        string ResourceName { get; set; }
        const string json = "{\"$type\":\"Dev2.Data.ServiceTestModelTO,Dev2.Data\",\"OldTestName\":null,\"TestName\":\"Test 1\",\"UserName\":null,\"Password\":null,\"LastRunDate\":\"0001-01-01T00:00:00\",\"Inputs\":null,\"Outputs\":null,\"NoErrorExpected\":false,\"ErrorExpected\":false,\"TestPassed\":false,\"TestFailing\":false,\"TestInvalid\":false,\"TestPending\":false,\"Enabled\":true,\"IsDirty\":false,\"AuthenticationType\":0,\"ResourceId\":\"00000000-0000-0000-0000-000000000000\"}";

        [Given(@"I have a resouce ""(.*)""")]
        public void GivenIHaveAResouce(string resourceName)
        {
            _resourceForTests = resourceName;
            var resourceId = Guid.NewGuid();
            // ReSharper disable once UnusedVariable
            var environmentModel = EnvironmentRepository.Instance.Source;
            var resourceModel = new ResourceModel(environmentModel)
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

            environmentModel.ResourceRepository.SaveToServer(resourceModel);

            ScenarioContext.Current.Add(resourceName + "id", resourceId);
        }
        string _resourceForTests = "";
        [Given(@"I add ""(.*)"" as tests")]
        public void GivenIAddAsTests(string p0)
        {

            var environmentModel = EnvironmentRepository.Instance.Source;
            var serviceTestModelTos = new List<IServiceTestModelTO>() { };
            environmentModel.ResourceRepository.ForceLoad();
            var savedSource = environmentModel.ResourceRepository.All().First(model => model.ResourceName.Equals(_resourceForTests, StringComparison.InvariantCultureIgnoreCase));
            ScenarioContext["PluginSource" + "id"] = savedSource.ID;

            var resourceID = ScenarioContext.Get<Guid>("PluginSourceid");
            lock (_syncRoot)
            {
                var testNamesNames = p0.Split(',');
                foreach (var resourceName in testNamesNames)
                {
                    Dev2JsonSerializer serializer = new Dev2JsonSerializer();
                    var serviceTestModelTO = serializer.Deserialize<ServiceTestModelTO>(json);
                    serviceTestModelTO.TestName = resourceName;
                    serviceTestModelTO.ResourceId = resourceID;
                    serviceTestModelTO.Inputs = new List<IServiceTestInput>();
                    serviceTestModelTO.Outputs = new List<IServiceTestOutput>();
                    serviceTestModelTO.AuthenticationType = AuthenticationType.Windows;

                    serviceTestModelTos.Add(serviceTestModelTO);
                }
            }
            // ReSharper disable once UnusedVariable
            var resourceModel = new ResourceModel(environmentModel)
            {
                ID = savedSource.ID,
                Category = "",
                ResourceName = _resourceForTests
            };
            var executeMessage = environmentModel.ResourceRepository.SaveTests(resourceModel, serviceTestModelTos);
        }
        const string _resourceCat = "ResourceCat\\";
        [Then(@"""(.*)"" has (.*) tests")]
        public void ThenHasTests(string resourceName, int numberOdTests)
        {
            var environmentModel = EnvironmentRepository.Instance.Source;
            var resourceID = ScenarioContext.Get<Guid>(resourceName + "id");
            var serviceTestModelTos = environmentModel.ResourceRepository.LoadResourceTests(resourceID);
            Assert.AreEqual(numberOdTests, serviceTestModelTos.Count);
        }

        [When(@"I delete resource ""(.*)""")]
        public void WhenIDeleteResource(string resourceName)
        {
            var environmentModel = EnvironmentRepository.Instance.Source;
            var resourceID = ScenarioContext.Get<Guid>(resourceName + "id");
            // ReSharper disable once UnusedVariable
            var savedSource = environmentModel.ResourceRepository.All().First(model => model.ResourceName.Equals(_resourceForTests, StringComparison.InvariantCultureIgnoreCase));
            var deleteResource = environmentModel.ResourceRepository.DeleteResource(savedSource);

        }

        private static void AddVariables(string variableName, DataListViewModel datalistViewModel, enDev2ColumnArgumentDirection ioDirection)
        {

            if (DataListUtil.IsValueScalar(variableName))
            {
                var scalarName = DataListUtil.RemoveLanguageBrackets(variableName);
                var scalarItemModel = new ScalarItemModel(scalarName, ioDirection);
                if (!scalarItemModel.HasError)
                {
                    datalistViewModel.ScalarCollection.Add(scalarItemModel);
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
                    datalistViewModel.RecsetCollection.Add(recordSetItemModel);
                    recordSetItemModel.Children.Add(new RecordSetFieldItemModel(fieldName,
                        recordSetItemModel, ioDirection));
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

        [Given(@"""(.*)"" has outputs as")]
        public void GivenHasOutputsAs(string workflowName, Table outputVariables)
        {
            ResourceModel resourceModel;
            if (ScenarioContext.TryGetValue(workflowName, out resourceModel))
            {
                DataListViewModel dataListViewModel;
                if (ScenarioContext.TryGetValue($"{workflowName}dataListViewModel", out dataListViewModel))
                {
                    foreach (var variablesRow in outputVariables.Rows)
                    {
                        AddVariables(variablesRow["Ouput Var Name"], dataListViewModel, enDev2ColumnArgumentDirection.Output);
                    }
                    dataListViewModel.WriteToResourceModel();
                    var environmentModel = EnvironmentRepository.Instance.Source;
                    environmentModel.ResourceRepository.SaveToServer(resourceModel);
                }
                else
                {
                    Assert.Fail("No Datalist found");
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
            ResourceModel resourceModel;
            if (ScenarioContext.TryGetValue(workflowName, out resourceModel))
            {
                var testFramework = new ServiceTestViewModel(resourceModel, new SynchronousAsyncWorker());
                Assert.IsNotNull(testFramework);
                Assert.IsNotNull(testFramework.ResourceModel);
                ScenarioContext.Add("testFramework", testFramework);
            }
            else
            {
                Assert.Fail($"Resource Model for {workflowName} not found");
            }
        }

        [Given(@"Tab Header is ""(.*)""")]
        [When(@"Tab Header is ""(.*)""")]
        [Then(@"Tab Header is ""(.*)""")]
        public void GivenTabHeaderIs(string expectedTabHeader)
        {
            ServiceTestViewModel serviceTest = GetTestFrameworkFromContext();
            Assert.AreEqual(expectedTabHeader, serviceTest.DisplayName);
        }

        [Given(@"there are no tests")]
        [When(@"there are no tests")]
        [Then(@"there are no tests")]
        public void GivenThereAreNoTests()
        {
            var currentTests = GetTestForCurrentTestFramework();
            Assert.IsFalse(currentTests.Any());
        }

        [Then(@"the tab is closed")]
        public void ThenTheTabIsClosed()
        {
            Mock<IShellViewModel> shellViewModel = ScenarioContext.Get<Mock<IShellViewModel>>("shellViewModel");
            shellViewModel.Verify(vm => vm.CloseResourceTestView(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>()));
        }

        [Then(@"The ""(.*)"" popup is shown I click Ok")]
        public void ThenThePopupIsShownIClickOk(string popupViewName)
        {
            Mock<Common.Interfaces.Studio.Controller.IPopupController> popupController = ScenarioContext.Get<Mock<Common.Interfaces.Studio.Controller.IPopupController>>("popupController");

            switch (popupViewName)
            {
                case "Delete Confirmation":
                    popupController.Verify(controller => controller.ShowDeleteConfirmation(It.IsAny<string>()));
                    break;
                case "Workflow Deleted":
                    popupController.Verify(controller => controller.Show(Warewolf.Studio.Resources.Languages.Core.ServiceTestResourceDeletedMessage, Warewolf.Studio.Resources.Languages.Core.ServiceTestResourceDeletedHeader, It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>()));
                    break;
                case "Workflow changed":
                    popupController.Verify(controller => controller.Show(Warewolf.Studio.Resources.Languages.Core.ServiceTestResourceCategoryChangedMessage, Warewolf.Studio.Resources.Languages.Core.ServiceTestResourceCategoryChangedHeader, It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>()));
                    break;
            }
        }

        [Given(@"I click New Test")]
        [When(@"I click New Test")]
        [Then(@"I click New Test")]
        public void WhenIClickNewTest()
        {
            ServiceTestViewModel serviceTest = GetTestFrameworkFromContext();
            serviceTest.CreateTestCommand.Execute(null);

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
            ServiceTestViewModel serviceTest = GetTestFrameworkFromContext();

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

        [Then(@"there are (.*) tests")]
        public void ThenThereAreTests(int testCount)
        {
            var currentTests = GetTestForCurrentTestFramework();
            Assert.AreEqual(testCount, currentTests.Count());
        }

        [Then(@"test name starts with ""(.*)""")]
        public void ThenTestNameStartsWith(string testName)
        {
            ServiceTestViewModel serviceTest = GetTestFrameworkFromContext();
            Assert.AreEqual(testName, serviceTest.SelectedServiceTest.TestName);
        }

        [Then(@"""(.*)"" is selected")]
        public void ThenIsSelected(string testName)
        {
            ServiceTestViewModel serviceTest = GetTestFrameworkFromContext();
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
            ServiceTestViewModel serviceTest = GetTestFrameworkFromContext();
            Assert.AreEqual(null, serviceTest.SelectedServiceTest.UserName);
        }

        [Then(@"password is blank")]
        public void ThenPasswordIsBlank()
        {
            ServiceTestViewModel serviceTest = GetTestFrameworkFromContext();
            Assert.AreEqual(null, serviceTest.SelectedServiceTest.Password);
        }

        [Then(@"inputs are")]
        public void ThenInputsAs(Table table)
        {
            ServiceTestViewModel serviceTest = GetTestFrameworkFromContext();
            var inputs = serviceTest.SelectedServiceTest.Inputs;
            Assert.AreNotEqual(0, inputs.Count);
            var i = 0;
            foreach (var tableRow in table.Rows)
            {
                Assert.AreEqual(tableRow["Variable Name"], inputs[i].Variable);
                var expected = tableRow["Value"];
                //                if (string.IsNullOrEmpty(expected))
                //                {
                //                    expected = null;
                //                }
                Assert.AreEqual(expected, inputs[i].Value);
                i++;
            }

        }

        [When(@"I updated the inputs as")]
        public void WhenIUpdatedTheInputsAs(Table table)
        {
            ServiceTestViewModel serviceTest = GetTestFrameworkFromContext();
            var inputs = serviceTest.SelectedServiceTest.Inputs;
            foreach (var tableRow in table.Rows)
            {
                var valueToSet = tableRow["Value"];
                if (!string.IsNullOrEmpty(valueToSet))
                {
                    var varName = tableRow["Variable Name"];
                    var foundInput = inputs.FirstOrDefault(input => input.Variable == varName);
                    if (foundInput != null)
                    {
                        foundInput.Value = valueToSet;
                    }
                }

            }
        }


        [Then(@"outputs as")]
        public void ThenOutputsAs(Table table)
        {
            ServiceTestViewModel serviceTest = GetTestFrameworkFromContext();
            var outputs = serviceTest.SelectedServiceTest.Outputs;
            Assert.AreNotEqual(0, outputs.Count);
            var i = 0;
            foreach (var tableRow in table.Rows)
            {
                Assert.AreEqual(tableRow["Variable Name"], outputs[i].Variable);
                var expected = tableRow["Value"];
                //                if (string.IsNullOrEmpty(expected))
                //                {
                //                    expected = null;
                //                }
                Assert.AreEqual(expected, outputs[i].Value);
                i++;
            }
        }

        [When(@"""(.*)"" is deleted")]
        public void WhenIsDeleted(string workflowName)
        {
            ResourceModel resourceModel;
            if (ScenarioContext.TryGetValue(workflowName, out resourceModel))
            {
                var env = EnvironmentRepository.Instance.Source;
                env.ResourceRepository.DeleteResource(resourceModel);
            }
        }

        [When(@"""(.*)"" is moved")]
        public void WhenIsMoved(string workflowName)
        {
            ResourceModel resourceModel;
            if (ScenarioContext.TryGetValue(workflowName, out resourceModel))
            {
                var env = EnvironmentRepository.Instance.Source;
                resourceModel.Category = "bob\\" + workflowName;
                env.ResourceRepository.SaveToServer(resourceModel);
            }
        }

        [Given(@"save is enabled")]
        [Then(@"save is disabled")]
        public void ThenSaveIsDisabled()
        {
            ServiceTestViewModel serviceTest = GetTestFrameworkFromContext();
            Assert.IsFalse(serviceTest.CanSave);
        }
        [Then(@"save is enabled")]
        [When(@"save is enabled")]
        public void ThenSaveIsEnabled()
        {
            ServiceTestViewModel serviceTest = GetTestFrameworkFromContext();
            Assert.IsTrue(serviceTest.CanSave);
        }

        [Then(@"test status is pending")]
        public void ThenTestStatusIsPending()
        {
            ServiceTestViewModel serviceTest = GetTestFrameworkFromContext();
            Assert.IsTrue(serviceTest.SelectedServiceTest.TestPending);

        }

        [Then(@"test is enabled")]
        public void ThenTestIsEnabled()
        {
            ServiceTestViewModel serviceTest = GetTestFrameworkFromContext();
            Assert.IsTrue(serviceTest.SelectedServiceTest.Enabled);
        }

        [Given(@"I save")]
        [When(@"I save")]
        [Then(@"I save")]
        public void WhenISave()
        {
            ServiceTestViewModel serviceTest = GetTestFrameworkFromContext();
            serviceTest.Save();
        }

        [Then(@"I close the test builder")]
        public void ThenICloseTheTestBuilder()
        {
            ScenarioContext.Current.Remove("testFramework");
        }


        [Then(@"Inputs are empty")]
        public void ThenInputsAreEmpty()
        {
            ServiceTestViewModel serviceTest = GetTestFrameworkFromContext();
            var hasNoInputs = serviceTest.SelectedServiceTest.Inputs == null;
            Assert.IsTrue(hasNoInputs);
        }

        [Then(@"Outputs are empty")]
        public void ThenOutputsAreEmpty()
        {
            ServiceTestViewModel serviceTest = GetTestFrameworkFromContext();
            var hasNoOutputs = serviceTest.SelectedServiceTest.Outputs == null;
            Assert.IsTrue(hasNoOutputs);
        }

        [Then(@"No Error selected")]
        public void ThenNoErrorSelected()
        {
            ServiceTestViewModel serviceTest = GetTestFrameworkFromContext();
            var noErrorExpected = serviceTest.SelectedServiceTest.NoErrorExpected;
            Assert.IsTrue(noErrorExpected);
        }

        [When(@"I change the test name to ""(.*)""")]
        public void WhenIChangeTheTestNameTo(string testName)
        {
            ServiceTestViewModel serviceTest = GetTestFrameworkFromContext();
            serviceTest.SelectedServiceTest.TestName = testName;
        }

        [Then(@"test URL is ""(.*)""")]
        public void ThenTestURLIs(string testUrl)
        {
            ServiceTestViewModel serviceTest = GetTestFrameworkFromContext();
            serviceTest.SelectedServiceTest.RunSelectedTestUrl = testUrl;
        }

        [Then(@"Test name is ""(.*)""")]
        public void ThenTestNameIs(string testName)
        {
            ServiceTestViewModel serviceTest = GetTestFrameworkFromContext();
            Assert.AreEqual(testName, serviceTest.SelectedServiceTest.TestName);
        }

        [Then(@"Name for display is ""(.*)"" and test is edited")]
        public void ThenNameForDisplayIsAndTestIsEdited(string nameForDisplay)
        {
            ServiceTestViewModel serviceTest = GetTestFrameworkFromContext();
            Assert.IsTrue(serviceTest.SelectedServiceTest.IsDirty);
            Assert.AreEqual(nameForDisplay, serviceTest.SelectedServiceTest.NameForDisplay);
        }

        [Then(@"Name for display is ""(.*)"" and test is not edited")]
        public void ThenNameForDisplayIs(string nameForDisplay)
        {
            ServiceTestViewModel serviceTest = GetTestFrameworkFromContext();
            Assert.IsFalse(serviceTest.SelectedServiceTest.IsDirty);
            Assert.AreEqual(nameForDisplay, serviceTest.SelectedServiceTest.NameForDisplay);
        }

        [Given(@"I select ""(.*)""")]
        [When(@"I select ""(.*)""")]
        [Then(@"I select ""(.*)""")]
        public void GivenISelect(string testName)
        {
            ServiceTestViewModel serviceTest = GetTestFrameworkFromContext();
            var serviceTestModel = serviceTest.Tests.Single(model => string.Equals(model.TestName, testName, StringComparison.InvariantCultureIgnoreCase));
            serviceTest.SelectedServiceTest = serviceTestModel;
        }

        [Given(@"I set Test Values as")]
        [When(@"I set Test Values as")]
        [Then(@"I set Test Values as")]
        public void GivenISetTestValuesAs(Table table)
        {
            ServiceTestViewModel serviceTest = GetTestFrameworkFromContext();
            foreach (var tableRow in table.Rows)
            {
                var testName = tableRow["TestName"];
                var authenticationType = tableRow["AuthenticationType"];
                AuthenticationType authent;
                Enum.TryParse(authenticationType, true, out authent);
                var error = tableRow["Error"];
                serviceTest.SelectedServiceTest.TestName = testName;
                serviceTest.SelectedServiceTest.ErrorExpected = bool.Parse(error);
                serviceTest.SelectedServiceTest.AuthenticationType = authent;

            }
        }

        [Then(@"Test Status saved is ""(.*)""")]
        public void ThenTestStatusSavedIs(string testStatus)
        {
            ServiceTestViewModel serviceTest = GetTestFrameworkFromContext();

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
            ServiceTestViewModel serviceTest = GetTestFrameworkFromContext();
            Assert.AreEqual(hasError, serviceTest.SelectedServiceTest.NoErrorExpected);

        }

        [Then(@"Authentication is Public")]
        public void ThenAuthenticationIsPublic()
        {
            ServiceTestViewModel serviceTest = GetTestFrameworkFromContext();
            Assert.AreEqual(AuthenticationType.Public, serviceTest.SelectedServiceTest.AuthenticationType);
        }

        [When(@"I disable ""(.*)""")]
        public void WhenIDisable(string testName)
        {
            ServiceTestViewModel serviceTest = GetTestFrameworkFromContext();
            var serviceTestModel = serviceTest.Tests.Single(model => string.Equals(model.TestName, testName, StringComparison.InvariantCultureIgnoreCase));
            serviceTestModel.Enabled = false;
        }

        [When(@"I enable ""(.*)""")]
        public void WhenIEnable(string testName)
        {
            ServiceTestViewModel serviceTest = GetTestFrameworkFromContext();
            var serviceTestModel = serviceTest.Tests.Single(model => string.Equals(model.TestName, testName, StringComparison.InvariantCultureIgnoreCase));
            serviceTestModel.Enabled = true;
        }


        [Then(@"Delete is disabled for ""(.*)""")]
        public void ThenDeleteIsDisabledFor(string testName)
        {
            ServiceTestViewModel serviceTest = GetTestFrameworkFromContext();
            var serviceTestModel = serviceTest.Tests.Single(model => string.Equals(model.TestName, testName, StringComparison.InvariantCultureIgnoreCase));
            var canDelete = serviceTest.DeleteTestCommand.CanExecute(serviceTestModel);
            Assert.IsFalse(canDelete);
        }

        [Then(@"Delete is enabled for ""(.*)""")]
        public void ThenDeleteIsEnabledFor(string testName)
        {
            ServiceTestViewModel serviceTest = GetTestFrameworkFromContext();
            var serviceTestModel = serviceTest.Tests.Single(model => string.Equals(model.TestName, testName, StringComparison.InvariantCultureIgnoreCase));
            var canDelete = serviceTest.DeleteTestCommand.CanExecute(serviceTestModel);
            Assert.IsTrue(canDelete);
        }


        [Given(@"I set inputs as")]
        [When(@"I set inputs as")]
        [Then(@"I set inputs as")]
        public void GivenISetInputsAs(Table table)
        {
            ServiceTestViewModel serviceTest = GetTestFrameworkFromContext();

            foreach (var tableRow in table.Rows)
            {
                var vname = tableRow["Variable Name"];
                var value = tableRow["Value"];
                serviceTest.SelectedServiceTest.Inputs.Add
                    (
                            new ServiceTestInput(vname, value)
                    );
            }
        }

        [Given(@"I set outputs as")]
        [When(@"I set outputs as")]
        [Then(@"I set outputs as")]
        public void GivenISetOutputsAs(Table table)
        {
            ServiceTestViewModel serviceTest = GetTestFrameworkFromContext();

            foreach (var tableRow in table.Rows)
            {
                var vname = tableRow["Variable Name"];
                var value = tableRow["Value"];
                serviceTest.SelectedServiceTest.Outputs.Add
                    (
                       new ServiceTestOutput(vname, value)
                    );
            }
        }

        [Then(@"Delete is enabled")]
        public void ThenDeleteIsEnabled()
        {
            ServiceTestViewModel serviceTest = GetTestFrameworkFromContext();
            var canDelete = serviceTest.DeleteTestCommand.CanExecute(null);
            Assert.IsTrue(canDelete);
        }

        [Then(@"Run is enabled")]
        public void ThenRunIsEnabled()
        {
            ServiceTestViewModel serviceTest = GetTestFrameworkFromContext();
            var canDelete = serviceTest.RunSelectedTestCommand.CanExecute(null);
            Assert.IsTrue(canDelete);
        }

        [When(@"I delete ""(.*)""")]
        public void WhenIDelete(string testName)
        {
            ServiceTestViewModel serviceTest = GetTestFrameworkFromContext();
            var serviceTestModel = serviceTest.Tests.Single(model => string.Equals(model.TestName, testName, StringComparison.InvariantCultureIgnoreCase));
            serviceTest.DeleteTestCommand.Execute(serviceTestModel);
        }

        [When(@"I delete selected Test")]
        public void WhenIDeleteSelectedTest()
        {
            ServiceTestViewModel serviceTest = GetTestFrameworkFromContext();
            serviceTest.DeleteTestCommand.Execute(null);

        }

        [Then(@"The Confirmation popup is shown")]
        public void ThenTheConfirmationPopupIsShown()
        {
            var mock = ScenarioContext["popupController"] as Mock<Common.Interfaces.Studio.Controller.IPopupController>;
            // ReSharper disable once PossibleNullReferenceException
            mock.VerifyAll();
        }

        [Then(@"The Pending Changes Confirmation popup is shown I click Ok")]
        public void ThenThePendingChangesConfirmationPopupIsShownIClickOk()
        {
            var mock = (Mock<Common.Interfaces.Studio.Controller.IPopupController>)ScenarioContext["popupController"];
            mock.Verify(controller => controller.Show(It.IsAny<string>(), It.IsAny<string>(), MessageBoxButton.OK, MessageBoxImage.Information, null, false, true, false, false));
        }

        [Then(@"Error is ""(.*)""")]
        public void ThenErrorIs(string hasError)
        {
            var error = bool.Parse(hasError);
            ServiceTestViewModel serviceTest = GetTestFrameworkFromContext();
            var errorExpected = serviceTest.SelectedServiceTest.ErrorExpected;
            Assert.AreEqual(error, errorExpected);
        }



        [When(@"test is disabled")]
        public void WhenTestIsDisabled()
        {
            ServiceTestViewModel serviceTest = GetTestFrameworkFromContext();
            var enabled = serviceTest.SelectedServiceTest.Enabled;
            Assert.IsFalse(enabled);
        }

        [When(@"I click ""(.*)""")]
        public void WhenIClick(string testName)
        {
            ServiceTestViewModel serviceTest = GetTestFrameworkFromContext();
            var serviceTestModel = serviceTest.Tests.Single(model => model.TestName == testName);
            serviceTest.SelectedServiceTest = serviceTestModel;
        }


        [Then(@"Duplicate Test is visible")]
        public void ThenDuplicateTestIsVisible()
        {
            ServiceTestViewModel serviceTest = GetTestFrameworkFromContext();
            var canExecute = serviceTest.DuplicateTestCommand.CanExecute(null);
            Assert.IsTrue(canExecute);
        }

        [When(@"I run selected test")]
        public void WhenIRunSelectedTest()
        {
            ServiceTestViewModel serviceTest = GetTestFrameworkFromContext();
            serviceTest.RunSelectedTestCommand.Execute(null);
        }

        [When(@"I run selected test in browser")]
        public void WhenIRunSelectedTestBrowser()
        {
            ServiceTestViewModel serviceTest = GetTestFrameworkFromContext();
            serviceTest.RunSelectedTestInBrowserCommand.Execute(null);
        }

        [When(@"I run all tests")]
        public void WhenIRunAllTests()
        {
            ServiceTestViewModel serviceTest = GetTestFrameworkFromContext();
            serviceTest.RunAllTestsCommand.Execute(null);
        }

        [When(@"I run all tests in browser")]
        public void WhenIRunAllTestsInBrowser()
        {
            ServiceTestViewModel serviceTest = GetTestFrameworkFromContext();
            serviceTest.RunAllTestsInBrowserCommand.Execute(null);
        }

        [When(@"I click delete test")]
        public void WhenIClickDeleteTest()
        {
            ServiceTestViewModel serviceTest = GetTestFrameworkFromContext();
            serviceTest.DeleteTestCommand.Execute(null);
        }

        [When(@"I click duplicate")]
        public void WhenIClickDuplicate()
        {
            ServiceTestViewModel serviceTest = GetTestFrameworkFromContext();
            serviceTest.DuplicateTestCommand.Execute(null);
        }

        [Then(@"the duplicated tests is ""(.*)""")]
        public void ThenTheDuplicatedTestsIs(string dupTestName)
        {
            ServiceTestViewModel serviceTest = GetTestFrameworkFromContext();
            Assert.IsTrue(serviceTest.SelectedServiceTest.TestName == dupTestName);
            var count = serviceTest.Tests.Count(model => model.TestName == dupTestName);
            Assert.AreEqual(2, count);
        }
        [Then(@"Duplicate Test in not Visible")]
        public void ThenDuplicateTestInNotVisible()
        {
            ServiceTestViewModel serviceTest = GetTestFrameworkFromContext();
            var canExecute = serviceTest.DuplicateTestCommand.CanExecute(null);
            Assert.IsFalse(canExecute);
        }

        [Then(@"Duplicate Test in Visible")]
        public void ThenDuplicateTestInVisible()
        {
            ServiceTestViewModel serviceTest = GetTestFrameworkFromContext();
            var canExecute = serviceTest.DuplicateTestCommand.CanExecute(null);
            Assert.IsTrue(canExecute);
        }


        [Then(@"The duplicate Name popup is shown")]
        public void ThenTheDuplicateNamePopupIsShown()
        {
            var mock = (Mock<Common.Interfaces.Studio.Controller.IPopupController>)ScenarioContext["popupController"];
            mock.Verify(controller => controller.Show(It.IsAny<string>(), It.IsAny<string>(), MessageBoxButton.OK, MessageBoxImage.Error, null, false, true, false, false));
        }

        [Given(@"I have a folder ""(.*)""")]
        public void GivenIHaveAFolder(string foldername)
        {
            var category = _resourceCat + foldername;
            var folderPath = Path.Combine(EnvironmentVariables.ResourcePath, category);
            //if (!Directory.Exists(folderPath))
            //{
            //    Directory.CreateDirectory(folderPath);
            //}
            ScenarioContext.Add("folderPath", category);
        }

        [Given(@"I have a resouce workflow ""(.*)"" inside Home")]
        public void GivenIHaveAResouceWorkflowInsideHome(string resourceName)
        {
            var path = ScenarioContext.Get<string>("folderPath");
            var environmentModel = EnvironmentRepository.Instance.Source;


            var resourceId = Guid.NewGuid();
            // ReSharper disable once UnusedVariable
            var resourceModel = new ResourceModel(environmentModel)
            {
                ResourceName = resourceName,
                DisplayName = resourceName,
                DataList = "",
                ID = resourceId,
                Category = path
            };
            var workflowHelper = new WorkflowHelper();
            var builder = workflowHelper.CreateWorkflow(resourceName);
            resourceModel.WorkflowXaml = workflowHelper.GetXamlDefinition(builder);
            environmentModel.ResourceRepository.SaveToServer(resourceModel);
            ScenarioContext.Current.Add(resourceName + "id", resourceId);
        }

        [Given(@"I add ""(.*)"" to ""(.*)""")]
        public void GivenIAddTo(string testNames, string rName)
        {

            var path = ScenarioContext.Get<string>("folderPath");
            var environmentModel = EnvironmentRepository.Instance.Source;
            var serviceTestModelTos = new List<IServiceTestModelTO>() { };
            environmentModel.ResourceRepository.ForceLoad();
            var savedSource = environmentModel.ResourceRepository.All().Single(model => model.ResourceName.Equals(rName, StringComparison.InvariantCultureIgnoreCase));
            ScenarioContext[rName + "id"] = savedSource.ID;
            var resourceID = ScenarioContext.Get<Guid>(rName + "id");
            lock (_syncRoot)
            {
                var testNamesNames = testNames.Split(',');
                foreach (var resourceName in testNamesNames)
                {
                    Dev2JsonSerializer serializer = new Dev2JsonSerializer();
                    var serviceTestModelTO = serializer.Deserialize<ServiceTestModelTO>(json);
                    serviceTestModelTO.TestName = resourceName;
                    serviceTestModelTO.ResourceId = resourceID;
                    serviceTestModelTO.Inputs = new List<IServiceTestInput>();
                    serviceTestModelTO.Outputs = new List<IServiceTestOutput>();
                    serviceTestModelTO.AuthenticationType = AuthenticationType.Windows;

                    serviceTestModelTos.Add(serviceTestModelTO);
                }
            }

            // ReSharper disable once UnusedVariable
            var resourceModel = new ResourceModel(environmentModel)
            {
                ID = savedSource.ID,
                Category = path,
                ResourceName = rName
            };
            var executeMessage = environmentModel.ResourceRepository.SaveTests(resourceModel, serviceTestModelTos);
            Assert.IsTrue(executeMessage.Result == SaveResult.Success || executeMessage.Result == SaveResult.ResourceUpdated);
        }
        [When(@"I delete folder ""(.*)""")]
        public void WhenIDeleteFolder(string folderName)
        {
            var path = ScenarioContext.Get<string>("folderPath");
            var environmentModel = EnvironmentRepository.Instance.Source;

            var controller = new CommunicationController { ServiceName = "DeleteItemService" };
            controller.AddPayloadArgument("folderToDelete", path);
            var result = controller.ExecuteCommand<IExplorerRepositoryResult>(environmentModel.Connection, GlobalConstants.ServerWorkspaceID);
            if (result.Status != ExecStatus.Success)
            {
                throw new WarewolfSaveException(result.Message, null);
            }

        }







        private IEnumerable<IServiceTestModel> GetTestForCurrentTestFramework()
        {
            var testFrameworkFromContext = GetTestFrameworkFromContext();
            var serviceTestModels = testFrameworkFromContext.Tests.Where(model => model.GetType() != typeof(DummyServiceTest));
            return serviceTestModels;
        }

        ServiceTestViewModel GetTestFrameworkFromContext()
        {
            ServiceTestViewModel serviceTest;
            if (ScenarioContext.TryGetValue("testFramework", out serviceTest))
            {
                return serviceTest;
            }
            Assert.Fail("Test Framework ViewModel not found");
            return null;
        }

    }
}
