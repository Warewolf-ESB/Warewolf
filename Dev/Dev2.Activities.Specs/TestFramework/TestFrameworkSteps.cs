using System;
using Dev2.Data.Binary_Objects;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Models;
using Dev2.Studio.Core.Models.DataList;
using Dev2.Studio.ViewModels.DataList;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TechTalk.SpecFlow;
using Warewolf.Studio.ViewModels;

// ReSharper disable UnusedMember.Global

namespace Dev2.Activities.Specs.TestFramework
{
    [Binding]
    public class StudioTestFrameworkSteps
    {
        public StudioTestFrameworkSteps(ScenarioContext scenarioContext)
        {
            if (scenarioContext == null) throw new ArgumentNullException("scenarioContext");
            ScenarioContext = scenarioContext;
        }

        ScenarioContext ScenarioContext { get; }

        [Given(@"I have ""(.*)"" with inputs as")]
        public void GivenIHaveWithInputsAs(string workflowName, Table inputVariables)
        {
            var resourceModel = new ResourceModel(new Mock<IEnvironmentModel>().Object)
            {
                ResourceName = workflowName,
                DisplayName = workflowName,
                DataList = ""
            };
            var datalistViewModel = new DataListViewModel();
            datalistViewModel.InitializeDataListViewModel(resourceModel);
            foreach(var variablesRow in inputVariables.Rows)
            {                
                datalistViewModel.ScalarCollection.Add(new ScalarItemModel(variablesRow[0],enDev2ColumnArgumentDirection.Input));
            }
            datalistViewModel.WriteToResourceModel();
            ScenarioContext.Add(workflowName,resourceModel);
            ScenarioContext.Add("dataListViewModel",datalistViewModel);
        }
        
        [Given(@"""(.*)"" has outputs as")]
        public void GivenHasOutputsAs(string workflowName, Table outputVariables)
        {
            ResourceModel resourceModel;
            if(ScenarioContext.TryGetValue(workflowName,out resourceModel))
            {
                DataListViewModel dataListViewModel;
                if(ScenarioContext.TryGetValue("dataListViewModel",out dataListViewModel))
                {
                    foreach (var variablesRow in outputVariables.Rows)
                    {
                        dataListViewModel.ScalarCollection.Add(new ScalarItemModel(variablesRow[0], enDev2ColumnArgumentDirection.Output));
                    }
                    dataListViewModel.WriteToResourceModel();
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
        public void GivenTheTestBuilderIsOpenWith(string workflowName)
        {
            ResourceModel resourceModel;
            if (ScenarioContext.TryGetValue(workflowName, out resourceModel))
            {
                var testFramework = new ServiceServiceTestViewModel(resourceModel);
                Assert.IsNotNull(testFramework);
                Assert.IsNotNull(testFramework.ResourceModel);
                ScenarioContext.Add("testFramework",testFramework);
            }
            else
            {
                Assert.Fail($"Resource Model for {workflowName} not found");
            }
        }

        [Given(@"there are no tests")]
        public void GivenThereAreNoTests()
        {
            ServiceServiceTestViewModel serviceServiceTest = GetTestFrameworkFromContext();
            Assert.IsNull(serviceServiceTest.Tests);
        }


        [When(@"I click New Test")]
        public void WhenIClickNewTest()
        {
            ServiceServiceTestViewModel serviceServiceTest = GetTestFrameworkFromContext();
            serviceServiceTest.CreateTestCommand.Execute(null);
            
        }


        [Then(@"a new test is added")]
        public void ThenANewTestIsAdded()
        {
            ServiceServiceTestViewModel serviceServiceTest = GetTestFrameworkFromContext();
            Assert.AreNotEqual(0, serviceServiceTest.Tests.Count);
        }


        [Then(@"test name starts with ""(.*)""")]
        public void ThenTestNameStartsWith(string testName)
        {
            ServiceServiceTestViewModel serviceServiceTest = GetTestFrameworkFromContext();
            Assert.AreEqual(testName,serviceServiceTest.SelectedServiceTest.TestName);
        }

        [Then(@"username is blank")]
        public void ThenUsernameIsBlank()
        {
            ServiceServiceTestViewModel serviceServiceTest = GetTestFrameworkFromContext();
            Assert.AreEqual(null, serviceServiceTest.SelectedServiceTest.UserName);
        }

        [Then(@"password is blank")]
        public void ThenPasswordIsBlank()
        {
            ServiceServiceTestViewModel serviceServiceTest = GetTestFrameworkFromContext();
            Assert.AreEqual(null, serviceServiceTest.SelectedServiceTest.Password);
        }

        [Then(@"inputs as")]
        public void ThenInputsAs(Table table)
        {
            ServiceServiceTestViewModel serviceServiceTest = GetTestFrameworkFromContext();
            var inputs = serviceServiceTest.SelectedServiceTest.Inputs;
            Assert.AreNotEqual(0,inputs.Count);
            var i = 0;
            foreach(var tableRow in table.Rows)
            {
                Assert.AreEqual(tableRow["Variable Name"],inputs[i].Variable);
                var expected = tableRow["Value"];
                if (string.IsNullOrEmpty(expected))
                {
                    expected = null;
                }
                Assert.AreEqual(expected,inputs[i].Value);
                i++;
            }

        }

        [Then(@"outputs as")]
        public void ThenOutputsAs(Table table)
        {
            ServiceServiceTestViewModel serviceServiceTest = GetTestFrameworkFromContext();
            var outputs = serviceServiceTest.SelectedServiceTest.Outputs;
            Assert.AreNotEqual(0, outputs.Count);
            var i = 0;
            foreach (var tableRow in table.Rows)
            {
                Assert.AreEqual(tableRow["Variable Name"], outputs[i].Variable);
                var expected = tableRow["Value"];
                if (string.IsNullOrEmpty(expected))
                {
                    expected = null;
                }
                Assert.AreEqual(expected, outputs[i].Value);
                i++;
            }
        }

        [Then(@"save is disabled")]
        public void ThenSaveIsDisabled()
        {
            ServiceServiceTestViewModel serviceTest = GetTestFrameworkFromContext();
            Assert.IsFalse(serviceTest.CanSave);
            Assert.IsFalse(serviceTest.SaveCommand.CanExecute(null));
        }

        [Then(@"test status is pending")]
        public void ThenTestStatusIsPending()
        {
            ServiceServiceTestViewModel serviceTest = GetTestFrameworkFromContext();
            Assert.IsTrue(serviceTest.SelectedServiceTest.TestPending);
            
        }

        [Then(@"test is enabled")]
        public void ThenTestIsEnabled()
        {
            ServiceServiceTestViewModel serviceTest = GetTestFrameworkFromContext();
            Assert.IsTrue(serviceTest.SelectedServiceTest.Enabled);
        }


        ServiceServiceTestViewModel GetTestFrameworkFromContext()
        {
            ServiceServiceTestViewModel serviceServiceTest;
            if (ScenarioContext.TryGetValue("testFramework", out serviceServiceTest))
            {
                return serviceServiceTest;
            }
            Assert.Fail("Test Framework ViewModel not found");
            return null;
        }
    }
}
