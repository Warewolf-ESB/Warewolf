using System;
using System.Linq;
using Dev2.Common;
using Dev2.Common.Interfaces.Infrastructure.Events;
using Dev2.Data.Binary_Objects;
using Dev2.Data.Util;
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
            if (scenarioContext == null) throw new ArgumentNullException(nameof(scenarioContext));
            ScenarioContext = scenarioContext;
        }

        ScenarioContext ScenarioContext { get; }

        [Given(@"I have ""(.*)"" with inputs as")]
        public void GivenIHaveWithInputsAs(string workflowName, Table inputVariables)
        {
            var mockCon = new Mock<IEnvironmentConnection>();
            mockCon.Setup(connection => connection.IsConnected).Returns(true);
            mockCon.Setup(connection => connection.ServerEvents).Returns(new Mock<IEventPublisher>().Object);
            IEnvironmentModel model = new EnvironmentModel(GlobalConstants.ServerWorkspaceID,mockCon.Object);
            
            var resourceModel = new ResourceModel(model)
            {
                ResourceName = workflowName,
                DisplayName = workflowName,
                DataList = ""
            };
            
            var datalistViewModel = new DataListViewModel();
            datalistViewModel.InitializeDataListViewModel(resourceModel);
            foreach(var variablesRow in inputVariables.Rows)
            {
                AddVariables(variablesRow["Input Var Name"], datalistViewModel, enDev2ColumnArgumentDirection.Input);
            }
            datalistViewModel.WriteToResourceModel();
            ScenarioContext.Add(workflowName,resourceModel);
            ScenarioContext.Add($"{workflowName}dataListViewModel",datalistViewModel);
        }

        private static void AddVariables(string variableName, DataListViewModel datalistViewModel, enDev2ColumnArgumentDirection ioDirection)
        {
            
            if(DataListUtil.IsValueScalar(variableName))
            {
                var scalarName = DataListUtil.RemoveLanguageBrackets(variableName);
                var scalarItemModel = new ScalarItemModel(scalarName, ioDirection);
                if(!scalarItemModel.HasError)
                {
                    datalistViewModel.ScalarCollection.Add(scalarItemModel);
                }
            }
            if(DataListUtil.IsValueRecordsetWithFields(variableName))
            {
                var rsName = DataListUtil.ExtractRecordsetNameFromValue(variableName);
                var fieldName = DataListUtil.ExtractFieldNameOnlyFromValue(variableName);
                var rs = datalistViewModel.RecsetCollection.FirstOrDefault(model => model.Name == rsName);
                if(rs == null)
                {
                    var recordSetItemModel = new RecordSetItemModel(rsName);
                    datalistViewModel.RecsetCollection.Add(recordSetItemModel);
                    recordSetItemModel.Children.Add(new RecordSetFieldItemModel(fieldName,
                        recordSetItemModel, ioDirection));
                }
                else
                {
                    var recordSetFieldItemModel = rs.Children.FirstOrDefault(model => model.Name == fieldName);
                    if(recordSetFieldItemModel == null)
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
            if(ScenarioContext.TryGetValue(workflowName,out resourceModel))
            {
                DataListViewModel dataListViewModel;
                if(ScenarioContext.TryGetValue($"{workflowName}dataListViewModel",out dataListViewModel))
                {
                    foreach (var variablesRow in outputVariables.Rows)
                    {
                        AddVariables(variablesRow["Ouput Var Name"], dataListViewModel, enDev2ColumnArgumentDirection.Output);
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
        [When(@"the test builder is open with ""(.*)""")]
        [Then(@"the test builder is open with ""(.*)""")]
        public void GivenTheTestBuilderIsOpenWith(string workflowName)
        {
            ResourceModel resourceModel;
            if (ScenarioContext.TryGetValue(workflowName, out resourceModel))
            {
                var testFramework = new ServiceTestViewModel(resourceModel);
                Assert.IsNotNull(testFramework);
                Assert.IsNotNull(testFramework.ResourceModel);
                ScenarioContext.Add("testFramework",testFramework);
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
            Assert.AreEqual(expectedTabHeader,serviceTest.DisplayName);
        }

        [Given(@"there are no tests")]
        [When(@"there are no tests")]
        [Then(@"there are no tests")]
        public void GivenThereAreNoTests()
        {
            ServiceTestViewModel serviceTest = GetTestFrameworkFromContext();
            var testsAreDummy = serviceTest.Tests.All(model => model.GetType() == typeof(DummyServiceTest));
            Assert.IsTrue(testsAreDummy);
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
            ServiceTestViewModel serviceTest = GetTestFrameworkFromContext();
            Assert.AreNotEqual(0, serviceTest.Tests.Count);
        }

        [Then(@"there are (.*) tests")]
        public void ThenThereAreTests(int testCount)
        {
            ServiceTestViewModel serviceTest = GetTestFrameworkFromContext();
            Assert.AreEqual(testCount, serviceTest.Tests.Count);
        }

        [Then(@"test name starts with ""(.*)""")]
        public void ThenTestNameStartsWith(string testName)
        {
            ServiceTestViewModel serviceTest = GetTestFrameworkFromContext();
            Assert.AreEqual(testName,serviceTest.SelectedServiceTest.TestName);
        }

        [Then(@"""(.*)"" is selected")]
        public void ThenIsSelected(string testName)
        {
            ServiceTestViewModel serviceTest = GetTestFrameworkFromContext();
            Assert.AreEqual(testName, serviceTest.SelectedServiceTest.TestName);
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
            Assert.AreNotEqual(0,inputs.Count);
            var i = 0;
            foreach(var tableRow in table.Rows)
            {
                Assert.AreEqual(tableRow["Variable Name"],inputs[i].Variable);
                var expected = tableRow["Value"];
//                if (string.IsNullOrEmpty(expected))
//                {
//                    expected = null;
//                }
                Assert.AreEqual(expected,inputs[i].Value);
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

        [Then(@"save is disabled")]
        public void ThenSaveIsDisabled()
        {
            ServiceTestViewModel serviceTest = GetTestFrameworkFromContext();
            Assert.IsFalse(serviceTest.CanSave);
        }
        [Then(@"save is enabled")]
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

        [When(@"I save")]
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
