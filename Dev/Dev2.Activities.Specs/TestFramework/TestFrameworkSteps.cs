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
                    ScenarioContext.Add(workflowName, resourceModel);
                    ScenarioContext.Add("dataListViewModel", dataListViewModel);
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
                var testFramework = new TestViewModel(resourceModel);
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
            TestViewModel test = GetTestFrameworkFromContext();
            Assert.AreEqual(0,test.Tests.Count);
        }


        [When(@"I click New Test")]
        public void WhenIClickNewTest()
        {
            TestViewModel test = GetTestFrameworkFromContext();
            test.CreateTestCommand.Execute(null);
            
        }


        [Then(@"a new test is added")]
        public void ThenANewTestIsAdded()
        {
            TestViewModel test = GetTestFrameworkFromContext();
            Assert.AreNotEqual(0, test.Tests.Count);
        }


        [Then(@"test name starts with ""(.*)""")]
        public void ThenTestNameStartsWith(string testName)
        {
            TestViewModel test = GetTestFrameworkFromContext();
            Assert.AreEqual(testName,test.SelectedTest.TestName);
        }

        [Then(@"username is blank")]
        public void ThenUsernameIsBlank()
        {
            TestViewModel test = GetTestFrameworkFromContext();
            Assert.AreEqual("", test.SelectedTest.UserName);
        }


        TestViewModel GetTestFrameworkFromContext()
        {
            TestViewModel test;
            if (ScenarioContext.TryGetValue("test", out test))
            {
                return test;
            }
            Assert.Fail("Test Framework ViewModel not found");
            return null;
        }
    }
}
